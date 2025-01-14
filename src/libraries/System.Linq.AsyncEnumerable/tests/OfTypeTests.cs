// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace System.Linq.Tests
{
    public class OfTypeTests : AsyncEnumerableTests
    {
        [Fact]
        public void InvalidInputs_Throws()
        {
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.OfType<int, double>(null));
        }

        [Fact]
        public async Task Empty_ProducesEmpty()
        {
            await AssertEqual(AsyncEnumerable.Empty<string>(), AsyncEnumerable.Empty<object>().OfType<object, string>());
            await AssertEqual(AsyncEnumerable.Empty<double>(), AsyncEnumerable.Empty<int>().OfType<int, double>());
        }

        [Fact]
        public async Task NullAndNonNull_SkipsNulls()
        {
            await AssertEqual(["2", "8"], CreateSource("2", null, "8", null).OfType<string, string>());
            await AssertEqual(["2", "8"], CreateSource<object>("2", null, "8", null).OfType<object, string>());
            await AssertEqual([2, 8], CreateSource<object>(2, null, 8, null).OfType<object, int>());
        }

        [Fact]
        public async Task Cancellation_Cancels()
        {
            IAsyncEnumerable<string> source = CreateSource("2", null, "8", null);
            CancellationTokenSource cts = new();
            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await foreach (var item in source.OfType<string, string>().WithCancellation(cts.Token))
                {
                    cts.Cancel();
                }
            });
        }

        [Fact]
        public async Task InterfaceCalls_ExpectedCounts()
        {
            TrackingAsyncEnumerable<int?> source = CreateSource<int?>(2, null, 8, 16).Track();
            await ConsumeAsync(source.OfType<int?, int>());
            Assert.Equal(5, source.MoveNextAsyncCount);
            Assert.Equal(4, source.CurrentCount);
            Assert.Equal(1, source.DisposeAsyncCount);
        }
    }
}
