// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace System.Linq.Tests
{
    public class ShuffleTests : AsyncEnumerableTests
    {
        [Fact]
        public void InvalidInputs_Throws()
        {
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.Shuffle<int>(null));
        }

        [Fact]
        public void Empty_ProducesEmpty() // validating an optimization / implementation detail
        {
            Assert.Same(AsyncEnumerable.Empty<string>(), AsyncEnumerable.Empty<string>().Shuffle());
        }

        [Theory]
        [InlineData(new int[0])]
        [InlineData(new int[] { 1 })]
        [InlineData(new int[] { 2, 4, 8 })]
        [InlineData(new int[] { -1, 2, 5, 6, 7, 8 })]
        public async Task VariousValues_ContainsAllInputValues(int[] values)
        {
            foreach (IAsyncEnumerable<int> source in CreateSources(values))
            {
                int[] shuffled = await source.Shuffle().Iterate().ToArrayAsync();
                Array.Sort(shuffled);
                Assert.Equal(values, shuffled);
            }
        }

        [Theory]
        [InlineData(new int[0])]
        [InlineData(new int[] { 1 })]
        [InlineData(new int[] { 2, 4, 8 })]
        [InlineData(new int[] { -1, 2, 5, 6, 7, 8 })]
        public async Task CountAsync_MatchesLength(int[] values)
        {
            foreach (IAsyncEnumerable<int> source in CreateSources(values))
            {
                Assert.Equal(values.Length, await source.Shuffle().CountAsync());
            }
        }

        [Theory]
        [InlineData(new int[0])]
        [InlineData(new int[] { 1 })]
        [InlineData(new int[] { 2, 4, 8 })]
        [InlineData(new int[] { -1, 2, 5, 6, 7, 8 })]
        public async Task AnyAsync_TrueIfNonEmpty(int[] values)
        {
            foreach (IAsyncEnumerable<int> source in CreateSources(values))
            {
                Assert.Equal(values.Length != 0, await source.Shuffle().AnyAsync());
            }
        }

        [Theory]
        [InlineData(new int[0])]
        [InlineData(new int[] { 1 })]
        [InlineData(new int[] { 2, 4, 8 })]
        [InlineData(new int[] { -1, 2, 5, 6, 7, 8 })]
        public async Task ContainsAsync_ExpectedValuesReturnTrue(int[] values)
        {
            foreach (IAsyncEnumerable<int> source in CreateSources(values))
            {
                foreach (int i in values)
                {
                    Assert.True(await source.Shuffle().ContainsAsync(i));
                }

                Assert.False(await source.Shuffle().ContainsAsync(42));
                Assert.False(await source.Shuffle().ContainsAsync(-2));
            }
        }

        [Fact]
        public async Task ToArrayAsync_ElementsAreRandomized()
        {
            // The chance that shuffling a thousand elements produces the same order twice is infinitesimal.
            int length = 1000;
            int[] array = Enumerable.Range(0, length).ToArray();
            foreach (IAsyncEnumerable<int> source in CreateSources(array))
            {
                int[] first = await source.Shuffle().ToArrayAsync();
                int[] second = await source.Shuffle().ToArrayAsync();
                Assert.Equal(length, first.Length);
                Assert.Equal(length, second.Length);

                Assert.NotEqual(first, second);

                Array.Sort(first);
                Array.Sort(second);
                Assert.Equal(array, first);
                Assert.Equal(array, second);
            }
        }

        [Fact]
        public async Task ToListAsync_ElementsAreRandomized()
        {
            // The chance that shuffling a thousand elements produces the same order twice is infinitesimal.
            int length = 1000;
            int[] array = Enumerable.Range(0, length).ToArray();
            foreach (IAsyncEnumerable<int> source in CreateSources(array))
            {
                List<int> first = await source.Shuffle().ToListAsync();
                List<int> second = await source.Shuffle().ToListAsync();
                Assert.Equal(length, first.Count);
                Assert.Equal(length, second.Count);

                Assert.NotEqual(first, second);

                first.Sort();
                second.Sort();
                Assert.Equal(array, first);
                Assert.Equal(array, second);
            }
        }

        [Fact]
        public async Task FirstLastElementAtAsync_Empty_Throw()
        {
            IAsyncEnumerable<int> e = AsyncEnumerable.Empty<int>().Iterate().Shuffle();

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await e.FirstAsync());
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await e.LastAsync());
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await e.ElementAtAsync(0));

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await e.Take(1).FirstAsync());
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await e.Take(1).LastAsync());
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await e.Take(1).ElementAtAsync(0));

            Assert.Equal(42, await e.FirstOrDefaultAsync(42));
            Assert.Equal(42, await e.LastOrDefaultAsync(42));
            Assert.Equal(42, await e.ElementAtOrDefaultAsync(42));

            Assert.Equal(42, await e.Take(1).FirstOrDefaultAsync(42));
            Assert.Equal(42, await e.Take(1).LastOrDefaultAsync(42));
            Assert.Equal(0, await e.Take(1).ElementAtOrDefaultAsync(42));
        }

        [Fact]
        public async Task FirstAsync_LastAsync_GetElementAsync_ProduceRandomElements()
        {
            foreach (var source in CreateSources(Enumerable.Range(0, 100)))
            {
                await AssertRetryAsync(async () => await source.Shuffle().FirstAsync() != await source.Shuffle().FirstAsync());
                await AssertRetryAsync(async () => await source.Shuffle().LastAsync() != await source.Shuffle().LastAsync());
                await AssertRetryAsync(async () => await source.Shuffle().ElementAtAsync(5) != await source.Shuffle().ElementAtAsync(5));

                await AssertRetryAsync(async () => await source.Shuffle().Take(10).FirstAsync() != await source.Shuffle().Take(10).FirstAsync());
                await AssertRetryAsync(async () => await source.Shuffle().Take(10).LastAsync() != await source.Shuffle().Take(10).LastAsync());
                await AssertRetryAsync(async () => await source.Shuffle().Take(10).ElementAtAsync(5) != await source.Shuffle().Take(10).ElementAtAsync(5));

                await AssertRetryAsync(async () => await source.Shuffle().Take(10).Take(5).FirstAsync() != await source.Shuffle().Take(10).Take(5).FirstAsync());
                await AssertRetryAsync(async () => await source.Shuffle().Take(10).Take(5).LastAsync() != await source.Shuffle().Take(10).Take(5).LastAsync());
                await AssertRetryAsync(async () => await source.Shuffle().Take(10).Take(5).ElementAtAsync(3) != await source.Shuffle().Take(10).Take(5).ElementAtAsync(3));
            }

            static async Task AssertRetryAsync(Func<ValueTask<bool>> predicate)
            {
                for (int i = 0; i < 10; i++)
                {
                    if (await predicate())
                    {
                        return;
                    }
                }

                Assert.Fail("Predicate was true for 10 iterations");
            }
        }

        [Fact]
        public async Task ValidateShuffleTakeRandomDistribution()
        {
            const int InputLength = 10;
            const int Iterations = 100_000;
            const double Expected = Iterations / (double)InputLength;

            foreach (int mode in new[] { 0, 1, 2 })
            {
                foreach (IAsyncEnumerable<int> source in CreateSources(Enumerable.Range(0, InputLength)))
                {
                    IAsyncEnumerable<int> selected = source.Shuffle().Take(1);

                    Dictionary<int, int> counts = new();
                    for (int i = 0; i < Iterations; i++)
                    {
                        int value = 0;
                        switch (mode)
                        {
                            case 0:
                                await using (IAsyncEnumerator<int> e = selected.GetAsyncEnumerator())
                                {
                                    await e.MoveNextAsync();
                                    value = e.Current;
                                }
                                break;

                            case 1:
                                value = await selected.FirstAsync();
                                break;

                            case 2:
                                value = (await selected.ToArrayAsync())[0];
                                break;

                            default:
                                Assert.Fail("Invalid mode");
                                break;
                        }

                        counts[value] = counts.TryGetValue(value, out int count) ? count + 1 : 1;
                    }

                    Assert.All(counts, kvp => Assert.InRange(kvp.Value, Expected * 0.85, Expected * 1.15));
                }
            }
        }

        [Fact]
        public async Task Cancellation_Cancels()
        {
            IAsyncEnumerable<int> source = CreateSource(2, 4, 8, 16);
            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await source.Shuffle().WithCancellation(new CancellationToken(true)).ConsumeAsync();
            });
        }

        [Fact]
        public async Task InterfaceCalls_ExpectedCounts()
        {
            TrackingAsyncEnumerable<int> source = CreateSource(2, 4, 8, 16).Track();
            await source.Shuffle().ConsumeAsync();
            Assert.Equal(5, source.MoveNextAsyncCount);
            Assert.Equal(4, source.CurrentCount);
            Assert.Equal(1, source.DisposeAsyncCount);
        }
    }
}
