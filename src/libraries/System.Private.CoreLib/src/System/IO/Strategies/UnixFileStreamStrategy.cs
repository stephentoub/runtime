// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Buffers;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

namespace System.IO.Strategies
{
    internal sealed partial class UnixFileStreamStrategy : OSFileStreamStrategy
    {
        internal UnixFileStreamStrategy(SafeFileHandle handle, FileAccess access, FileShare share) :
            base(handle, access, share)
        {
        }

        internal UnixFileStreamStrategy(string path, FileMode mode, FileAccess access, FileShare share, FileOptions options, long preallocationSize) :
            base(path, mode, access, share, options, preallocationSize)
        {
        }

        internal override bool IsAsync => _fileHandle.IsAsync;

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            ValidateBufferArguments(buffer, offset, count);
            return ReadAsync(new Memory<byte>(buffer, offset, count), cancellationToken).AsTask();
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return ValueTask.FromCanceled<int>(cancellationToken);
            }

            if (MemoryMarshal.TryGetArray(buffer, out ArraySegment<byte> segment))
            {
                return new ValueTask<int>((Task<int>)BeginReadInternal(segment.Array!, segment.Offset, segment.Count, null, null, serializeAsynchronously: true, apm: false));
            }

            return Core(buffer, cancellationToken);

            async ValueTask<int> Core(Memory<byte> buffer, CancellationToken cancellationToken)
            {
                byte[] rentedBuffer = ArrayPool<byte>.Shared.Rent(buffer.Length);
                try
                {
                    int result = await ((Task<int>)BeginReadInternal(rentedBuffer, 0, buffer.Length, null, null, serializeAsynchronously: true, apm: false)).ConfigureAwait(false);
                    new ReadOnlySpan<byte>(rentedBuffer, 0, result).CopyTo(buffer.Span);
                    return result;
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(rentedBuffer);
                }
            }
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
            WriteAsync(new ReadOnlyMemory<byte>(buffer, offset, count), cancellationToken).AsTask();

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return ValueTask.FromCanceled(cancellationToken);
            }

            if (MemoryMarshal.TryGetArray(buffer, out ArraySegment<byte> segment))
            {
                return new ValueTask((Task)BeginWriteInternal(segment.Array!, segment.Offset, segment.Count, null, null, serializeAsynchronously: true, apm: false));
            }

            byte[] rentedBuffer = ArrayPool<byte>.Shared.Rent(buffer.Length);
            buffer.CopyTo(rentedBuffer);

            return Core(rentedBuffer.AsMemory(0, buffer.Length), cancellationToken);

            async ValueTask Core(Memory<byte> buffer, CancellationToken cancellationToken)
            {
                try
                {
                    await ((Task)BeginWriteInternal(rentedBuffer, 0, buffer.Length, null, null, serializeAsynchronously: true, apm: false)).ConfigureAwait(false);
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(rentedBuffer);
                }
            }
        }

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            // Queue a single call to the synchronous CopyTo.  The base CopyToAsync employs an asynchronous read/write
            // loop, which means we'll be paying to queue each individual read/write rather than one queueing for everything.
            return Task.Factory.StartNew(static state =>
            {
                var args = ((UnixFileStreamStrategy thisRef, Stream destination, int bufferSize))state!;
                args.thisRef.CopyTo(args.destination, args.bufferSize);
            }, (this, destination, bufferSize), cancellationToken, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
        }
    }
}
