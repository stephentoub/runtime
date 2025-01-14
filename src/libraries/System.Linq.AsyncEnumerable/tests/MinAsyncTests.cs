// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace System.Linq.Tests
{
    public class MinAsyncTests : AsyncEnumerableTests
    {
        [Fact]
        public void InvalidInputs_Throws()
        {
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.MinAsync((IAsyncEnumerable<int>)null));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.MinAsync((IAsyncEnumerable<long>)null));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.MinAsync((IAsyncEnumerable<float>)null));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.MinAsync((IAsyncEnumerable<double>)null));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.MinAsync((IAsyncEnumerable<decimal>)null));

            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.MinAsync((IAsyncEnumerable<int?>)null));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.MinAsync((IAsyncEnumerable<long?>)null));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.MinAsync((IAsyncEnumerable<float?>)null));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.MinAsync((IAsyncEnumerable<double?>)null));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.MinAsync((IAsyncEnumerable<decimal?>)null));

            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.MinAsync<DateTime>(null));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.MinAsync<DateTime>(null, Comparer<DateTime>.Default, default));

            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.MinAsync<string, DateTime>(null, s => DateTime.Parse(s)));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.MinAsync<string, DateTime>(null, async (s, ct) => DateTime.Parse(s)));

            AssertExtensions.Throws<ArgumentNullException>("selector", () => AsyncEnumerable.MinAsync(AsyncEnumerable.Empty<string>(), (Func<string, DateTime>)null));
            AssertExtensions.Throws<ArgumentNullException>("selector", () => AsyncEnumerable.MinAsync(AsyncEnumerable.Empty<string>(), (Func<string, CancellationToken, ValueTask<DateTime>>)null));
        }

        [Fact]
        public async Task EmptyInputs_NonNullable_Throws()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await AsyncEnumerable.MinAsync(AsyncEnumerable.Empty<int>()));
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await AsyncEnumerable.MinAsync(AsyncEnumerable.Empty<long>()));
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await AsyncEnumerable.MinAsync(AsyncEnumerable.Empty<float>()));
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await AsyncEnumerable.MinAsync(AsyncEnumerable.Empty<double>()));
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await AsyncEnumerable.MinAsync(AsyncEnumerable.Empty<decimal>()));

            Assert.Null(await AsyncEnumerable.MinAsync(AsyncEnumerable.Empty<int?>()));
            Assert.Null(await AsyncEnumerable.MinAsync(AsyncEnumerable.Empty<long?>()));
            Assert.Null(await AsyncEnumerable.MinAsync(AsyncEnumerable.Empty<float?>()));
            Assert.Null(await AsyncEnumerable.MinAsync(AsyncEnumerable.Empty<double?>()));
            Assert.Null(await AsyncEnumerable.MinAsync(AsyncEnumerable.Empty<decimal?>()));

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await AsyncEnumerable.MinAsync(AsyncEnumerable.Empty<DateTime>()));
            Assert.Null(await AsyncEnumerable.MinAsync(AsyncEnumerable.Empty<DateTime>(), dt => dt.ToString()));
            Assert.Null(await AsyncEnumerable.MinAsync(AsyncEnumerable.Empty<DateTime>(), async (dt, ct) => dt.ToString()));
        }

        [Theory]
        [InlineData(new int[0])]
        [InlineData(new int[] { 0 })]
        [InlineData(new int[] { 1 })]
        [InlineData(new int[] { 2, 4, 8 })]
        [InlineData(new int[] { -1, 2, 5, 6, 7, 8 })]
        [InlineData(new int[] { -1000, 1000 })]
        [InlineData(new int[] { -1, -2, -3 })]
        public async Task VariousValues_MatchesEnumerable(int[] values)
        {
            foreach (IAsyncEnumerable<int> source in CreateSources(values))
            {
                if (values.Length > 0)
                {
                    Assert.Equal(values.Select(i => (int)i).Min(), await source.Select(i => (int)i).MinAsync());
                    Assert.Equal(values.Select(i => (long)i).Min(), await source.Select(i => (long)i).MinAsync());
                    Assert.Equal(values.Select(i => (float)i).Min(), await source.Select(i => (float)i).MinAsync());
                    Assert.Equal(values.Select(i => (double)i).Min(), await source.Select(i => (double)i).MinAsync());
                    Assert.Equal(values.Select(i => (decimal)i).Min(), await source.Select(i => (decimal)i).MinAsync());

                    Assert.Equal(values.Select(i => TimeSpan.FromSeconds(i)).Min(), await source.Select(i => TimeSpan.FromSeconds(i)).MinAsync());
                    Assert.Equal(values.Min(i => TimeSpan.FromSeconds(i)), await source.MinAsync(i => TimeSpan.FromSeconds(i)));
                    Assert.Equal(values.Min(i => TimeSpan.FromSeconds(i)), await source.MinAsync(async (i, ct) => TimeSpan.FromSeconds(i)));
                }

                Assert.Equal(values.Select(i => (int?)i).Min(), await source.Select(i => (int?)i).MinAsync());
                Assert.Equal(values.Select(i => (long?)i).Min(), await source.Select(i => (long?)i).MinAsync());
                Assert.Equal(values.Select(i => (float?)i).Min(), await source.Select(i => (float?)i).MinAsync());
                Assert.Equal(values.Select(i => (double?)i).Min(), await source.Select(i => (double?)i).MinAsync());
                Assert.Equal(values.Select(i => (decimal?)i).Min(), await source.Select(i => (decimal?)i).MinAsync());

                // With NaNs
                Assert.Equal(
                    new[] { float.NaN, float.NaN }.Concat(values.Select(i => (float)i)).Concat(new[] { float.NaN, float.NaN }).Min(),
                    await new[] { float.NaN, float.NaN }.ToAsyncEnumerable().Concat(source.Select(i => (float)i)).Concat(new[] { float.NaN, float.NaN }.ToAsyncEnumerable()).MinAsync());
                Assert.Equal(
                    new[] { double.NaN, double.NaN }.Concat(values.Select(i => (double)i)).Concat(new[] { double.NaN, double.NaN }).Min(),
                    await new[] { double.NaN, double.NaN }.ToAsyncEnumerable().Concat(source.Select(i => (double)i)).Concat(new[] { double.NaN, double.NaN }.ToAsyncEnumerable()).MinAsync());
                Assert.Equal(
                    new float?[] { float.NaN, float.NaN }.Concat(values.Select(i => (float?)i)).Concat(new float?[] { float.NaN, float.NaN }).Min(),
                    await new float?[] { float.NaN, float.NaN }.ToAsyncEnumerable().Concat(source.Select(i => (float?)i)).Concat(new float?[] { float.NaN, float.NaN }.ToAsyncEnumerable()).MinAsync());
                Assert.Equal(
                    new double?[] { double.NaN, double.NaN }.Concat(values.Select(i => (double?)i)).Concat(new double?[] { double.NaN, double.NaN }).Min(),
                    await new double?[] { double.NaN, double.NaN }.ToAsyncEnumerable().Concat(source.Select(i => (double?)i)).Concat(new double?[] { double.NaN, double.NaN }.ToAsyncEnumerable()).MinAsync());

                // With nulls
                Assert.Equal(
                    new float?[] { null, null }.Concat(values.Select(i => (float?)i)).Concat(new float?[] { null, null }).Min(),
                    await new float?[] { null, null }.ToAsyncEnumerable().Concat(source.Select(i => (float?)i)).Concat(new float?[] { null, null }.ToAsyncEnumerable()).MinAsync());
                Assert.Equal(
                    new double?[] { null, null }.Concat(values.Select(i => (double?)i)).Concat(new double?[] { null, null }).Min(),
                    await new double?[] { null, null }.ToAsyncEnumerable().Concat(source.Select(i => (double?)i)).Concat(new double?[] { null, null }.ToAsyncEnumerable()).MinAsync());
            }
        }

        [Fact]
        public async Task Cancellation_Cancels()
        {
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource(2, 4).Select(i => (int)i).MinAsync(new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource(2, 4).Select(i => (long)i).MinAsync(new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource(2, 4).Select(i => (float)i).MinAsync(new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource(2, 4).Select(i => (double)i).MinAsync(new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource(2, 4).Select(i => (decimal)i).MinAsync(new CancellationToken(true)));

            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource(2, 4).Select(i => (int?)i).MinAsync(new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource(2, 4).Select(i => (long?)i).MinAsync(new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource(2, 4).Select(i => (float?)i).MinAsync(new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource(2, 4).Select(i => (double?)i).MinAsync(new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource(2, 4).Select(i => (decimal?)i).MinAsync(new CancellationToken(true)));

            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource(2, 4).Select(i => TimeSpan.FromSeconds(i)).MinAsync(new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource(2, 4).MinAsync(i => TimeSpan.FromSeconds(i), new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource(2, 4).MinAsync(async (i, ct) => TimeSpan.FromSeconds(i), new CancellationToken(true)));

            var cts = new CancellationTokenSource();
            await CreateSource(2, 4).MinAsync(async (i, ct) =>
            {
                Assert.Equal(cts.Token, ct);
                return TimeSpan.FromSeconds(i);
            }, cts.Token);
        }

        [Fact]
        public async Task InterfaceCalls_ExpectedCounts()
        {
            await Validate(source => source.Select(i => (int)i).MinAsync());
            await Validate(source => source.Select(i => (long)i).MinAsync());
            await Validate(source => source.Select(i => (float)i).MinAsync());
            await Validate(source => source.Select(i => (double)i).MinAsync());
            await Validate(source => source.Select(i => (decimal)i).MinAsync());

            await Validate(source => source.Select(i => (int?)i).MinAsync());
            await Validate(source => source.Select(i => (long?)i).MinAsync());
            await Validate(source => source.Select(i => (float?)i).MinAsync());
            await Validate(source => source.Select(i => (double?)i).MinAsync());
            await Validate(source => source.Select(i => (decimal?)i).MinAsync());

            await Validate(source => source.Select(i => TimeSpan.FromSeconds(i)).MinAsync());
            await Validate(source => source.MinAsync(i => TimeSpan.FromSeconds(i)));
            await Validate(source => source.MinAsync(async (i, ct) => TimeSpan.FromSeconds(i)));

            static async Task Validate<TResult>(Func<IAsyncEnumerable<int>, ValueTask<TResult>> factory)
            {
                TrackingAsyncEnumerable<int> source;

                source = CreateSource(2, 4, 8, 16).Track();
                await factory(source);
                Assert.Equal(5, source.MoveNextAsyncCount);
                Assert.Equal(4, source.CurrentCount);
                Assert.Equal(1, source.DisposeAsyncCount);

                source = CreateSource(2, 4, 8, 16).AppendException(new FormatException()).Track();
                await Assert.ThrowsAsync<FormatException>(async () => await factory(source));
                Assert.Equal(5, source.MoveNextAsyncCount);
                Assert.Equal(4, source.CurrentCount);
                Assert.Equal(1, source.DisposeAsyncCount);
            }
        }
    }
}
