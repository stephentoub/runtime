// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq
{
    public static partial class AsyncEnumerable
    {
        /// <summary>Shuffles the order of the elements of a sequence.</summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">A sequence of values to shuffle.</param>
        /// <returns>A sequence whose elements correspond to those of the input sequence in randomized order.</returns>
        /// <remarks>Randomization is performed using a non-cryptographically-secure random number generator.</remarks>
        public static IAsyncEnumerable<TSource> Shuffle<TSource>(
            this IAsyncEnumerable<TSource> source)
        {
            ThrowHelper.ThrowIfNull(source);
            
            return
                source.IsKnownEmpty() ? Empty<TSource>() :
                new ShuffleIterator<TSource>(source);
        }

        private sealed partial class ShuffleIterator<TSource>(IAsyncEnumerable<TSource> source) : Iterator<TSource>
        {
            private readonly IAsyncEnumerable<TSource> _source = source;

            public override async IAsyncEnumerator<TSource> GetAsyncEnumerator(CancellationToken cancellationToken)
            {
                TSource[] array = await _source.ToArrayAsync(cancellationToken).ConfigureAwait(false);
                Shuffle(array);

                for (int i = 0; i < array.Length; i++)
                {
                    yield return array[i];
                }
            }

            public override async ValueTask<TSource[]> ToArrayAsync(CancellationToken cancellationToken)
            {
                TSource[] array = await _source.ToArrayAsync(cancellationToken).ConfigureAwait(false);
                Shuffle(array);
                return array;
            }

            public override async ValueTask<List<TSource>> ToListAsync(CancellationToken cancellationToken)
            {
                List<TSource> list = await _source.ToListAsync(cancellationToken).ConfigureAwait(false);
                Shuffle(list);
                return list;
            }

            public override ValueTask<int> CountAsync(CancellationToken cancellationToken) =>
                _source.CountAsync(cancellationToken);

            public override async ValueTask<TSource> FirstAsync(CancellationToken cancellationToken)
            {
                (List<TSource>? list, _) = await SampleToListAsync(_source, 1, cancellationToken).ConfigureAwait(false);
                if (list is null)
                {
                    ThrowHelper.ThrowNoElementsException();
                }

                return list[0];
            }

            public override async ValueTask<TSource?> FirstOrDefaultAsync(CancellationToken cancellationToken)
            {
                (List<TSource>? list, _) = await SampleToListAsync(_source, 1, cancellationToken).ConfigureAwait(false);
                return list is not null ? list[0] : default;
            }

            public override ValueTask<TSource> LastAsync(CancellationToken cancellationToken) =>
                FirstAsync(cancellationToken);

            public override ValueTask<TSource?> LastOrDefaultAsync(CancellationToken cancellationToken) =>
                FirstOrDefaultAsync(cancellationToken);

            public override async ValueTask<TSource> ElementAtAsync(int index, CancellationToken cancellationToken)
            {
                (List<TSource>? list, int totalElementCount) = await SampleToListAsync(_source, 1, cancellationToken).ConfigureAwait(false);
                if (list is null || index >= totalElementCount)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException(nameof(index));
                }

                return list[0];
            }

            public override async ValueTask<TSource?> ElementAtOrDefaultAsync(int index, CancellationToken cancellationToken)
            {
                (List<TSource>? list, int totalElementCount) = await SampleToListAsync(_source, 1, cancellationToken).ConfigureAwait(false);
                return list is not null && index < totalElementCount ? list[0] : default;
            }

            public override ValueTask<bool> AnyAsync(CancellationToken cancellationToken) => _source.AnyAsync(cancellationToken);

            /// <summary>Uses reservoir sampling to randomly select <paramref name="takeCount"/> elements from <paramref name="source"/>.</summary>
            private static async ValueTask<(List<TSource>?, int totalElementCount)> SampleToListAsync(
                IAsyncEnumerable<TSource> source, int takeCount, CancellationToken cancellationToken)
            {
                List<TSource>? reservoir = null;
                int totalElementCount = 0;
                IAsyncEnumerator<TSource> e = source.GetAsyncEnumerator(cancellationToken);
                try
                {
                    if (await e.MoveNextAsync().ConfigureAwait(false))
                    {
                        // Fill the reservoir with the first takeCount elements from the source.
                        // If we can't fill it, just return what we get.
                        reservoir = new List<TSource>(Math.Min(takeCount, 4)) { e.Current };
                        while (reservoir.Count < takeCount)
                        {
                            if (!await e.MoveNextAsync().ConfigureAwait(false))
                            {
                                totalElementCount = reservoir.Count;
                                goto ReturnReservoir;
                            }

                            reservoir.Add(e.Current);
                        }

                        // For each subsequent element in the source, randomly replace an element in the
                        // reservoir with a decreasing probability.
                        int i = takeCount;
                        while (await e.MoveNextAsync().ConfigureAwait(false))
                        {
                            checked { i++; }
                            long r = GetSharedRandom().Next(i);
                            if (r < takeCount)
                            {
                                reservoir[(int)r] = e.Current;
                            }
                        }

                        totalElementCount = i;
                    }
                }
                finally
                {
                    await e.DisposeAsync().ConfigureAwait(false);
                }

            ReturnReservoir:
                if (reservoir is not null)
                {
                    // Ensure that elements in the reservoir are in random order. The sampling helped
                    // to ensure we got a uniform distribution from the source into the reservoir, but
                    // it didn't randomize the order of the reservoir itself; this is especially relevant
                    // to the elements initially added into the reservoir.
                    Shuffle(reservoir);
                }

                return (reservoir, totalElementCount);
            }
        }
    }
}
