// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace System.Buffers
{
    /// <summary>
    /// Provides an ArrayPool implementation meant to be used as the singleton returned from ArrayPool.Shared.
    /// </summary>
    /// <remarks>
    /// The implementation uses a tiered caching scheme, with a small per-thread cache for each array size, followed
    /// by a cache per array size shared by all threads, split into per-core stacks meant to be used by threads
    /// running on that core.  Locks are used to protect each per-core stack, because a thread can migrate after
    /// checking its processor number, because multiple threads could interleave on the same core, and because
    /// a thread is allowed to check other core's buckets if its core's bucket is empty/full.
    /// </remarks>
    internal sealed partial class SharedArrayPool<T> : ArrayPool<T>
    {
        /// <summary>The number of buckets (array sizes) in the pool, one for each array length, starting from length 16.</summary>
        private const int NumBuckets = 27; // Utilities.SelectBucketIndex(1024 * 1024 * 1024 + 1)

        /// <summary>A per-thread array of arrays, to cache one array per array size per thread.</summary>
        [ThreadStatic]
        private static ThreadLocalArray[]? t_tlsBuckets;
        /// <summary>Used to keep track of all thread local buckets for trimming if needed.</summary>
        private readonly ConditionalWeakTable<ThreadLocalArray[], object?> _allTlsBuckets = new ConditionalWeakTable<ThreadLocalArray[], object?>();
        /// <summary>
        /// An array of per-core partitions. The slots are lazily initialized to avoid creating
        /// lots of overhead for unused array sizes.
        /// </summary>
        private readonly Partitions?[] _buckets = new Partitions[NumBuckets];
        /// <summary>Whether the callback to trim arrays in response to memory pressure has been created.</summary>
        private int _trimCallbackCreated;

        /// <summary>Allocate a new <see cref="Partitions"/> and try to store it into the <see cref="_buckets"/> array.</summary>
        private Partitions CreatePerCorePartitions(int bucketIndex)
        {
            var inst = new Partitions();
            return Interlocked.CompareExchange(ref _buckets[bucketIndex], inst, null) ?? inst;
        }

        /// <summary>Gets an ID for the pool to use with events.</summary>
        private int Id => GetHashCode();

        public override T[] Rent(int minimumLength)
        {
            ArrayPoolEventSource log = ArrayPoolEventSource.Log;
            T[]? buffer;

            // Get the bucket number for the array length. The result may be out of range of buckets,
            // either for too large a value or for 0 and negative values.
            int bucketIndex = Utilities.SelectBucketIndex(minimumLength);

            // First, try to get an array from TLS if possible.
            ThreadLocalArray[]? tlsBuckets = t_tlsBuckets;
            if (tlsBuckets is not null && (uint)bucketIndex < (uint)tlsBuckets.Length)
            {
                buffer = tlsBuckets[bucketIndex].Array;
                if (buffer is not null)
                {
                    tlsBuckets[bucketIndex].Array = null;
                    if (log.IsEnabled())
                    {
                        log.BufferRented(buffer.GetHashCode(), buffer.Length, Id, bucketIndex);
                    }
                    return buffer;
                }
            }

            // Next, try to get an array from one of the partitions.
            Partitions?[] perCoreBuckets = _buckets;
            if ((uint)bucketIndex < (uint)perCoreBuckets.Length)
            {
                Partitions? b = perCoreBuckets[bucketIndex];
                if (b is not null)
                {
                    buffer = b.TryPop();
                    if (buffer is not null)
                    {
                        if (log.IsEnabled())
                        {
                            log.BufferRented(buffer.GetHashCode(), buffer.Length, Id, bucketIndex);
                        }
                        return buffer;
                    }
                }

                // No buffer available.  Ensure the length we'll allocate matches that of a bucket
                // so we can later return it.
                minimumLength = Utilities.GetMaxSizeForBucket(bucketIndex);
            }
            else if (minimumLength == 0)
            {
                // We allow requesting zero-length arrays (even though pooling such an array isn't valuable)
                // as it's a valid length array, and we want the pool to be usable in general instead of using
                // `new`, even for computed lengths. But, there's no need to log the empty array.  Our pool is
                // effectively infinite for empty arrays and we'll never allocate for rents and never store for returns.
                return Array.Empty<T>();
            }
            else
            {
                ArgumentOutOfRangeException.ThrowIfNegative(minimumLength);
            }

            buffer = GC.AllocateUninitializedArray<T>(minimumLength);
            if (log.IsEnabled())
            {
                int bufferId = buffer.GetHashCode();
                log.BufferRented(bufferId, buffer.Length, Id, ArrayPoolEventSource.NoBucketId);
                log.BufferAllocated(bufferId, buffer.Length, Id, ArrayPoolEventSource.NoBucketId, bucketIndex >= _buckets.Length ?
                    ArrayPoolEventSource.BufferAllocatedReason.OverMaximumSize :
                    ArrayPoolEventSource.BufferAllocatedReason.PoolExhausted);
            }
            return buffer;
        }

        public override void Return(T[] array, bool clearArray = false)
        {
            if (array is null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            }

            // Determine with what bucket this array length is associated
            int bucketIndex = Utilities.SelectBucketIndex(array.Length);

            // Make sure our TLS buckets are initialized.  Technically we could avoid doing
            // this if the array being returned is erroneous or too large for the pool, but the
            // former condition is an error we don't need to optimize for, and the latter is incredibly
            // rare, given a max size of 1B elements.
            ThreadLocalArray[] tlsBuckets = t_tlsBuckets ?? InitializeTlsBucketsAndTrimming();

            bool haveBucket = false;
            bool returned = true;
            if ((uint)bucketIndex < (uint)tlsBuckets.Length)
            {
                haveBucket = true;

                // Clear the array if the user requested it.
                if (clearArray)
                {
                    Array.Clear(array);
                }

                // Check to see if the buffer is the correct size for this bucket.
                if (array.Length != Utilities.GetMaxSizeForBucket(bucketIndex))
                {
                    throw new ArgumentException(SR.ArgumentException_BufferNotFromPool, nameof(array));
                }

                // Store the array into the TLS bucket.  If there's already an array in it,
                // push that array down into the partitions, preferring to keep the latest
                // one in TLS for better locality.
                ref ThreadLocalArray tla = ref tlsBuckets[bucketIndex];
                T[]? prev = tla.Array;
                tla = new ThreadLocalArray(array);
                if (prev is not null)
                {
                    Partitions partitionsForArraySize = _buckets[bucketIndex] ?? CreatePerCorePartitions(bucketIndex);
                    returned = partitionsForArraySize.TryPush(prev);
                }
            }

            // Log that the buffer was returned
            ArrayPoolEventSource log = ArrayPoolEventSource.Log;
            if (log.IsEnabled() && array.Length != 0)
            {
                log.BufferReturned(array.GetHashCode(), array.Length, Id);
                if (!(haveBucket & returned))
                {
                    log.BufferDropped(array.GetHashCode(), array.Length, Id,
                        haveBucket ? bucketIndex : ArrayPoolEventSource.NoBucketId,
                        haveBucket ? ArrayPoolEventSource.BufferDroppedReason.Full : ArrayPoolEventSource.BufferDroppedReason.OverMaximumSize);
                }
            }
        }

        public bool Trim()
        {
            int currentMilliseconds = Environment.TickCount;
            Utilities.MemoryPressure pressure = Utilities.GetMemoryPressure();

            // Log that we're trimming.
            ArrayPoolEventSource log = ArrayPoolEventSource.Log;
            if (log.IsEnabled())
            {
                log.BufferTrimPoll(currentMilliseconds, (int)pressure);
            }

            // Trim each of the per-core buckets.
            Partitions?[] perCoreBuckets = _buckets;
            for (int i = 0; i < perCoreBuckets.Length; i++)
            {
                perCoreBuckets[i]?.Trim(currentMilliseconds, Id, pressure, Utilities.GetMaxSizeForBucket(i));
            }

            // Trim each of the TLS buckets. Note that threads may be modifying their TLS slots concurrently with
            // this trimming happening. We do not force synchronization with those operations, so we accept the fact
            // that we may end up firing a trimming event even if an array wasn't trimmed, and potentially
            // trim an array we didn't need to.  Both of these should be rare occurrences.

            // Under high pressure, release all thread locals.
            if (pressure == Utilities.MemoryPressure.High)
            {
                if (!log.IsEnabled())
                {
                    foreach (KeyValuePair<ThreadLocalArray[], object?> tlsBuckets in _allTlsBuckets)
                    {
                        Array.Clear(tlsBuckets.Key);
                    }
                }
                else
                {
                    foreach (KeyValuePair<ThreadLocalArray[], object?> tlsBuckets in _allTlsBuckets)
                    {
                        ThreadLocalArray[] buckets = tlsBuckets.Key;
                        for (int i = 0; i < buckets.Length; i++)
                        {
                            if (Interlocked.Exchange(ref buckets[i].Array, null) is T[] buffer)
                            {
                                log.BufferTrimmed(buffer.GetHashCode(), buffer.Length, Id);
                            }
                        }
                    }
                }
            }
            else
            {
                // Otherwise, release thread locals based on how long we've observed them to be stored. This time is
                // approximate, with the time set not when the array is stored but when we see it during a Trim, so it
                // takes at least two Trim calls (and thus two gen2 GCs) to drop an array, unless we're in high memory
                // pressure. These values have been set arbitrarily; we could tune them in the future.
                uint millisecondsThreshold = pressure switch
                {
                    Utilities.MemoryPressure.Medium => 15_000,
                    _ => 30_000,
                };

                foreach (KeyValuePair<ThreadLocalArray[], object?> tlsBuckets in _allTlsBuckets)
                {
                    ThreadLocalArray[] buckets = tlsBuckets.Key;
                    for (int i = 0; i < buckets.Length; i++)
                    {
                        if (buckets[i].Array is null)
                        {
                            continue;
                        }

                        // We treat 0 to mean it hasn't yet been seen in a Trim call. In the very rare case where Trim records 0,
                        // it'll take an extra Trim call to remove the array.
                        int lastSeen = buckets[i].MillisecondsTimeStamp;
                        if (lastSeen == 0)
                        {
                            buckets[i].MillisecondsTimeStamp = currentMilliseconds;
                        }
                        else if ((currentMilliseconds - lastSeen) >= millisecondsThreshold)
                        {
                            // Time noticeably wrapped, or we've surpassed the threshold.
                            // Clear out the array, and log its being trimmed if desired.
                            if (Interlocked.Exchange(ref buckets[i].Array, null) is T[] buffer &&
                                log.IsEnabled())
                            {
                                log.BufferTrimmed(buffer.GetHashCode(), buffer.Length, Id);
                            }
                        }
                    }
                }
            }

            return true;
        }

        private ThreadLocalArray[] InitializeTlsBucketsAndTrimming()
        {
            Debug.Assert(t_tlsBuckets is null, $"Non-null {nameof(t_tlsBuckets)}");

            var tlsBuckets = new ThreadLocalArray[NumBuckets];
            t_tlsBuckets = tlsBuckets;

            _allTlsBuckets.Add(tlsBuckets, null);
            if (Interlocked.Exchange(ref _trimCallbackCreated, 1) == 0)
            {
                Gen2GcCallback.Register(s => ((SharedArrayPool<T>)s).Trim(), this);
            }

            return tlsBuckets;
        }

        /// <summary>Provides a collection of partitions, each of which is a pool of arrays.</summary>
        private sealed class Partitions
        {
            /// <summary>The partitions.</summary>
            private readonly List<T[]>[] _perCore;
            /// <summary>Fallback global queue used when an individual partition is exhausted.</summary>
            private readonly List<T[]> _global;

            /// <summary>Initializes the partitions.</summary>
            public Partitions()
            {
                // Create one partition per core.
                var partitions = new List<T[]>[SharedArrayPoolStatics.s_partitionCount];
                for (int i = 0; i < partitions.Length; i++)
                {
                    partitions[i] = new List<T[]>(SharedArrayPoolStatics.s_maxArraysPerPartition);
                }
                _perCore = partitions;

                // Create a global partition.
                _global = new List<T[]>(SharedArrayPoolStatics.s_maxArraysInGlobalQueue);
            }

            /// <summary>
            /// Try to push the array into any partition with available space, starting with partition associated with the current core.
            /// If all partitions are full, the array will be dropped.
            /// </summary>
            public bool TryPush(T[] array)
            {
                // Get the partition for the current core.
                int index = (int)((uint)Thread.GetCurrentProcessorId() % (uint)SharedArrayPoolStatics.s_partitionCount); // mod by constant in tier 1
                List<T[]> partition = _perCore[index];

                // Try to take a lock on the partition.  Since this is per core, contention should be rare.
                // If there is contention, it's likely because the partition is currently being trimmed.
                // Regardless, just skip it if we can't get access.
                if (Monitor.TryEnter(partition))
                {
                    try
                    {
                        if (partition.Count < SharedArrayPoolStatics.s_maxArraysPerPartition)
                        {
                            partition.Add(array);
                            return true;
                        }
                    }
                    finally
                    {
                        Monitor.Exit(partition);
                    }
                }

                // We either couldn't access the local partition or it was full.
                // Either way, try to push to the global partition. If someone else
                // currently has it locked, just skip it.
                if (Monitor.TryEnter(_global))
                {
                    try
                    {
                        if (_global.Count < SharedArrayPoolStatics.s_maxArraysInGlobalQueue)
                        {
                            _global.Add(array);
                            return true;
                        }
                    }
                    finally
                    {
                        Monitor.Exit(_global);
                    }
                }

                // Fail to push.
                return false;
            }

            /// <summary>
            /// Try to pop an array from any partition with available arrays, starting with partition associated with the current core.
            /// If all partitions are empty, null is returned.
            /// </summary>
            public T[]? TryPop()
            {
                // Get the partition for the current core.
                int index = (int)((uint)Thread.GetCurrentProcessorId() % (uint)SharedArrayPoolStatics.s_partitionCount); // mod by constant in tier 1
                List<T[]> partition = _perCore[index];

                // Try to take a lock on the partition.  Since this is per core, contention should be rare.
                // If there is contention, it's likely because the partition is currently being trimmed.
                // If we can't get access, just fail the whole pop operation. (We could try the global queue
                // even if popping fails, but it's not worth the extra code.)
                if (Monitor.TryEnter(partition))
                {
                    try
                    {
                        // If there's anything in the local partition, remove and return it.
                        if (CollectionsMarshal.TryRemoveLast(partition, out T[] array))
                        {
                            return array;
                        }

                        // Otherwise, try to access the global partition.  We do so while still holding
                        // the lock on the local partition, because we'd like to minimize further access
                        // to the global queue, and so if the local partition was empty, we not only
                        // want to take 1 array from the global partition, we want to transfer some number
                        // of additional arrays from the global partition to the local one, which requires
                        // holding both locks.  We never try to acquire a local partition lock while holding
                        // the global partition lock, but we also don't block in the case of contention, so
                        // lock inversion wouldn't result in deadlock, anyway.
                        if (Monitor.TryEnter(_global))
                        {
                            try
                            {
                                // If there's at least one array in the global partition. Take it.
                                if (CollectionsMarshal.TryRemoveLast(_global, out array))
                                {
                                    int globalCount = _global.Count;

                                    // Now see if we can transfer any additional arrays from the global partition to the local.
                                    int extra = globalCount / SharedArrayPoolStatics.s_partitionCount;
                                    if (extra > 0)
                                    {
                                        extra = Math.Min(extra, SharedArrayPoolStatics.s_maxArraysPerPartition);

                                        Debug.Assert(partition.Count == 0);
                                        CollectionsMarshal.SetCount(partition, extra);
                                        CollectionsMarshal.AsSpan(_global).Slice(globalCount - extra).CopyTo(CollectionsMarshal.AsSpan(partition));
                                        CollectionsMarshal.SetCount(_global, globalCount - extra);
                                    }

                                    // Return the array we got.
                                    return array;
                                }
                            }
                            finally
                            {
                                Monitor.Exit(_global);
                            }
                        }
                    }
                    finally
                    {
                        Monitor.Exit(partition);
                    }
                }

                return null;
            }

            public void Trim(int currentMilliseconds, int id, Utilities.MemoryPressure pressure, int bucketSize)
            {
                _ = currentMilliseconds;
                _ = id;
                _ = pressure;
                _ = bucketSize;
                _ = _perCore.Length;

                // TODO:

                //Partition[] partitions = _partitions;
                //for (int i = 0; i < partitions.Length; i++)
                //{
                //    partitions[i].Trim(currentMilliseconds, id, pressure, bucketSize);
                //}
            }
        }

        /// <summary>Wrapper for arrays stored in ThreadStatic buckets.</summary>
        private struct ThreadLocalArray
        {
            /// <summary>The stored array.</summary>
            public T[]? Array;
            /// <summary>Environment.TickCount timestamp for when this array was observed by Trim.</summary>
            public int MillisecondsTimeStamp;

            public ThreadLocalArray(T[] array)
            {
                Array = array;
                MillisecondsTimeStamp = 0;
            }
        }
    }

    internal static class SharedArrayPoolStatics
    {
        /// <summary>Number of partitions to employ.</summary>
        internal static readonly int s_partitionCount = GetPartitionCount();
        /// <summary>The maximum number of arrays per array size to store per partition.</summary>
        internal static readonly int s_maxArraysPerPartition = GetMaxArraysPerPartition();
        /// <summary>The maximum number of arrays to store in the fallback global queue.</summary>
        /// <remarks>This will always be a power of 2.</remarks>
        internal static readonly int s_maxArraysInGlobalQueue = (int)BitOperations.RoundUpToPowerOf2((uint)GetMaxArraysInGlobalQueue());

        /// <summary>Gets the maximum number of partitions to shard arrays into.</summary>
        /// <remarks>Defaults to int.MaxValue.  Whatever value is returned will end up being clamped to <see cref="Environment.ProcessorCount"/>.</remarks>
        private static int GetPartitionCount()
        {
            int partitionCount = TryGetInt32EnvironmentVariable("DOTNET_SYSTEM_BUFFERS_SHAREDARRAYPOOL_MAXPARTITIONCOUNT", out int result) && result > 0 ?
                result :
                int.MaxValue; // no limit other than processor count
            return Math.Min(partitionCount, Environment.ProcessorCount);
        }

        /// <summary>Gets the maximum number of arrays of a given size allowed to be cached per partition.</summary>
        /// <returns>Defaults to 16. This does not factor in or impact the number of arrays cached per thread in TLS (currently only 1).</returns>
        private static int GetMaxArraysPerPartition()
        {
            return TryGetInt32EnvironmentVariable("DOTNET_SYSTEM_BUFFERS_SHAREDARRAYPOOL_MAXARRAYSPERPARTITION", out int result) && result > 0 ?
                result :
                16; // arbitrary limit
        }

        /// <summary>Gets the maximum number of arrays of a given size stored in a global cache available to all threads.</summary>
        /// <returns>Defaults to 1024.</returns>
        private static int GetMaxArraysInGlobalQueue()
        {
            return TryGetInt32EnvironmentVariable("DOTNET_SYSTEM_BUFFERS_SHAREDARRAYPOOL_MAXGLOBALQUEUEARRAYS", out int result) && result > 0 ?
                result :
                1024; // arbitrary limit
        }

        /// <summary>Look up an environment variable and try to parse it as an Int32.</summary>
        /// <remarks>This avoids using anything that might in turn recursively use the ArrayPool.</remarks>
        private static bool TryGetInt32EnvironmentVariable(string variable, out int result)
        {
            // Avoid globalization stack, as it might in turn be using ArrayPool.

            if (Environment.GetEnvironmentVariableCore_NoArrayPool(variable) is string envVar &&
                envVar.Length is > 0 and <= 32) // arbitrary limit that allows for some spaces around the maximum length of a non-negative Int32 (10 digits)
            {
                ReadOnlySpan<char> value = envVar.AsSpan().Trim(' ');
                if (!value.IsEmpty && value.Length <= 10)
                {
                    long tempResult = 0;
                    foreach (char c in value)
                    {
                        uint digit = (uint)(c - '0');
                        if (digit > 9)
                        {
                            goto Fail;
                        }

                        tempResult = tempResult * 10 + digit;
                    }

                    if (tempResult is >= 0 and <= int.MaxValue)
                    {
                        result = (int)tempResult;
                        return true;
                    }
                }
            }

        Fail:
            result = 0;
            return false;
        }
    }
}
