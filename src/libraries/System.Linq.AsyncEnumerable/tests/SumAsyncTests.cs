// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace System.Linq.Tests
{
    public class SumAsyncTests : AsyncEnumerableTests
    {
        [Fact]
        public void InvalidInputs_Throws()
        {
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.SumAsync((IAsyncEnumerable<int>)null));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.SumAsync((IAsyncEnumerable<long>)null));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.SumAsync((IAsyncEnumerable<float>)null));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.SumAsync((IAsyncEnumerable<double>)null));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.SumAsync((IAsyncEnumerable<decimal>)null));

            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.SumAsync((IAsyncEnumerable<int?>)null));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.SumAsync((IAsyncEnumerable<long?>)null));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.SumAsync((IAsyncEnumerable<float?>)null));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.SumAsync((IAsyncEnumerable<double?>)null));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.SumAsync((IAsyncEnumerable<decimal?>)null));

            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.SumAsync<string>(null, s => (int)42));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.SumAsync<string>(null, s => (long)42));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.SumAsync<string>(null, s => (float)42));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.SumAsync<string>(null, s => (double)42));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.SumAsync<string>(null, s => (decimal)42));
            AssertExtensions.Throws<ArgumentNullException>("selector", () => AsyncEnumerable.SumAsync(AsyncEnumerable.Empty<string>(), (Func<string, int>)null));
            AssertExtensions.Throws<ArgumentNullException>("selector", () => AsyncEnumerable.SumAsync(AsyncEnumerable.Empty<string>(), (Func<string, long>)null));
            AssertExtensions.Throws<ArgumentNullException>("selector", () => AsyncEnumerable.SumAsync(AsyncEnumerable.Empty<string>(), (Func<string, float>)null));
            AssertExtensions.Throws<ArgumentNullException>("selector", () => AsyncEnumerable.SumAsync(AsyncEnumerable.Empty<string>(), (Func<string, double>)null));
            AssertExtensions.Throws<ArgumentNullException>("selector", () => AsyncEnumerable.SumAsync(AsyncEnumerable.Empty<string>(), (Func<string, decimal>)null));

            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.SumAsync<string>(null, async (s, ct) => (int)42));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.SumAsync<string>(null, async (s, ct) => (long)42));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.SumAsync<string>(null, async (s, ct) => (float)42));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.SumAsync<string>(null, async (s, ct) => (double)42));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.SumAsync<string>(null, async (s, ct) => (decimal)42));
            AssertExtensions.Throws<ArgumentNullException>("selector", () => AsyncEnumerable.SumAsync(AsyncEnumerable.Empty<string>(), (Func<string, CancellationToken, ValueTask<int>>)null));
            AssertExtensions.Throws<ArgumentNullException>("selector", () => AsyncEnumerable.SumAsync(AsyncEnumerable.Empty<string>(), (Func<string, CancellationToken, ValueTask<long>>)null));
            AssertExtensions.Throws<ArgumentNullException>("selector", () => AsyncEnumerable.SumAsync(AsyncEnumerable.Empty<string>(), (Func<string, CancellationToken, ValueTask<float>>)null));
            AssertExtensions.Throws<ArgumentNullException>("selector", () => AsyncEnumerable.SumAsync(AsyncEnumerable.Empty<string>(), (Func<string, CancellationToken, ValueTask<double>>)null));
            AssertExtensions.Throws<ArgumentNullException>("selector", () => AsyncEnumerable.SumAsync(AsyncEnumerable.Empty<string>(), (Func<string, CancellationToken, ValueTask<decimal>>)null));

            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.SumAsync<string>(null, s => (int?)42));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.SumAsync<string>(null, s => (long?)42));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.SumAsync<string>(null, s => (float?)42));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.SumAsync<string>(null, s => (double?)42));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.SumAsync<string>(null, s => (decimal?)42));
            AssertExtensions.Throws<ArgumentNullException>("selector", () => AsyncEnumerable.SumAsync(AsyncEnumerable.Empty<string>(), (Func<string, int?>)null));
            AssertExtensions.Throws<ArgumentNullException>("selector", () => AsyncEnumerable.SumAsync(AsyncEnumerable.Empty<string>(), (Func<string, long?>)null));
            AssertExtensions.Throws<ArgumentNullException>("selector", () => AsyncEnumerable.SumAsync(AsyncEnumerable.Empty<string>(), (Func<string, float?>)null));
            AssertExtensions.Throws<ArgumentNullException>("selector", () => AsyncEnumerable.SumAsync(AsyncEnumerable.Empty<string>(), (Func<string, double?>)null));
            AssertExtensions.Throws<ArgumentNullException>("selector", () => AsyncEnumerable.SumAsync(AsyncEnumerable.Empty<string>(), (Func<string, decimal?>)null));

            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.SumAsync<string>(null, async (s, ct) => (int?)42));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.SumAsync<string>(null, async (s, ct) => (long?)42));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.SumAsync<string>(null, async (s, ct) => (float?)42));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.SumAsync<string>(null, async (s, ct) => (double?)42));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.SumAsync<string>(null, async (s, ct) => (decimal?)42));
            AssertExtensions.Throws<ArgumentNullException>("selector", () => AsyncEnumerable.SumAsync(AsyncEnumerable.Empty<string>(), (Func<string, CancellationToken, ValueTask<int?>>)null));
            AssertExtensions.Throws<ArgumentNullException>("selector", () => AsyncEnumerable.SumAsync(AsyncEnumerable.Empty<string>(), (Func<string, CancellationToken, ValueTask<long?>>)null));
            AssertExtensions.Throws<ArgumentNullException>("selector", () => AsyncEnumerable.SumAsync(AsyncEnumerable.Empty<string>(), (Func<string, CancellationToken, ValueTask<float?>>)null));
            AssertExtensions.Throws<ArgumentNullException>("selector", () => AsyncEnumerable.SumAsync(AsyncEnumerable.Empty<string>(), (Func<string, CancellationToken, ValueTask<double?>>)null));
            AssertExtensions.Throws<ArgumentNullException>("selector", () => AsyncEnumerable.SumAsync(AsyncEnumerable.Empty<string>(), (Func<string, CancellationToken, ValueTask<decimal?>>)null));
        }
        [Fact]
        public async Task EmptyInputs_NonNullable_Throws()
        {
            Assert.Equal(0, await AsyncEnumerable.SumAsync(AsyncEnumerable.Empty<int>()));
            Assert.Equal(0, await AsyncEnumerable.SumAsync(AsyncEnumerable.Empty<long>()));
            Assert.Equal(0, await AsyncEnumerable.SumAsync(AsyncEnumerable.Empty<float>()));
            Assert.Equal(0, await AsyncEnumerable.SumAsync(AsyncEnumerable.Empty<double>()));
            Assert.Equal(0, await AsyncEnumerable.SumAsync(AsyncEnumerable.Empty<decimal>()));

            Assert.Equal(0, await AsyncEnumerable.SumAsync(AsyncEnumerable.Empty<int?>()));
            Assert.Equal(0, await AsyncEnumerable.SumAsync(AsyncEnumerable.Empty<long?>()));
            Assert.Equal(0, await AsyncEnumerable.SumAsync(AsyncEnumerable.Empty<float?>()));
            Assert.Equal(0, await AsyncEnumerable.SumAsync(AsyncEnumerable.Empty<double?>()));
            Assert.Equal(0, await AsyncEnumerable.SumAsync(AsyncEnumerable.Empty<decimal?>()));

            Assert.Equal(0, await AsyncEnumerable.SumAsync(AsyncEnumerable.Empty<string>(), s => (int)42));
            Assert.Equal(0, await AsyncEnumerable.SumAsync(AsyncEnumerable.Empty<string>(), s => (long)42));
            Assert.Equal(0, await AsyncEnumerable.SumAsync(AsyncEnumerable.Empty<string>(), s => (float)42));
            Assert.Equal(0, await AsyncEnumerable.SumAsync(AsyncEnumerable.Empty<string>(), s => (double)42));
            Assert.Equal(0, await AsyncEnumerable.SumAsync(AsyncEnumerable.Empty<string>(), s => (decimal)42));

            Assert.Equal(0, await AsyncEnumerable.SumAsync(AsyncEnumerable.Empty<string>(), async (s, ct) => (int)42));
            Assert.Equal(0, await AsyncEnumerable.SumAsync(AsyncEnumerable.Empty<string>(), async (s, ct) => (long)42));
            Assert.Equal(0, await AsyncEnumerable.SumAsync(AsyncEnumerable.Empty<string>(), async (s, ct) => (float)42));
            Assert.Equal(0, await AsyncEnumerable.SumAsync(AsyncEnumerable.Empty<string>(), async (s, ct) => (double)42));
            Assert.Equal(0, await AsyncEnumerable.SumAsync(AsyncEnumerable.Empty<string>(), async (s, ct) => (decimal)42));

            Assert.Equal(0, await AsyncEnumerable.SumAsync(AsyncEnumerable.Empty<string>(), s => (int?)42));
            Assert.Equal(0, await AsyncEnumerable.SumAsync(AsyncEnumerable.Empty<string>(), s => (long?)42));
            Assert.Equal(0, await AsyncEnumerable.SumAsync(AsyncEnumerable.Empty<string>(), s => (float?)42));
            Assert.Equal(0, await AsyncEnumerable.SumAsync(AsyncEnumerable.Empty<string>(), s => (double?)42));
            Assert.Equal(0, await AsyncEnumerable.SumAsync(AsyncEnumerable.Empty<string>(), s => (decimal?)42));

            Assert.Equal(0, await AsyncEnumerable.SumAsync(AsyncEnumerable.Empty<string>(), async (s, ct) => (int?)42));
            Assert.Equal(0, await AsyncEnumerable.SumAsync(AsyncEnumerable.Empty<string>(), async (s, ct) => (long?)42));
            Assert.Equal(0, await AsyncEnumerable.SumAsync(AsyncEnumerable.Empty<string>(), async (s, ct) => (float?)42));
            Assert.Equal(0, await AsyncEnumerable.SumAsync(AsyncEnumerable.Empty<string>(), async (s, ct) => (double?)42));
            Assert.Equal(0, await AsyncEnumerable.SumAsync(AsyncEnumerable.Empty<string>(), async (s, ct) => (decimal?)42));
        }

        [Theory]
        [InlineData(new int[] { 0 })]
        [InlineData(new int[] { 1 })]
        [InlineData(new int[] { 2, 4, 8 })]
        [InlineData(new int[] { -1, 2, 5, 6, 7, 8 })]
        [InlineData(new int[] { -int.MaxValue, int.MaxValue })]
        [InlineData(new int[] { -1, -2, -3 })]
        public async Task VariousValues_MatchesEnumerable(int[] values)
        {
            foreach (IAsyncEnumerable<int> source in CreateSources(values))
            {
                Assert.Equal(values.Select(i => (int)i).Sum(), await source.Select(i => (int)i).SumAsync());
                Assert.Equal(values.Select(i => (long)i).Sum(), await source.Select(i => (long)i).SumAsync());
                Assert.Equal(values.Select(i => (float)i).Sum(), await source.Select(i => (float)i).SumAsync());
                Assert.Equal(values.Select(i => (double)i).Sum(), await source.Select(i => (double)i).SumAsync());
                Assert.Equal(values.Select(i => (decimal)i).Sum(), await source.Select(i => (decimal)i).SumAsync());

                Assert.Equal(values.Select(i => (int?)i).Sum(), await source.Select(i => (int?)i).SumAsync());
                Assert.Equal(values.Select(i => (long?)i).Sum(), await source.Select(i => (long?)i).SumAsync());
                Assert.Equal(values.Select(i => (float?)i).Sum(), await source.Select(i => (float?)i).SumAsync());
                Assert.Equal(values.Select(i => (double?)i).Sum(), await source.Select(i => (double?)i).SumAsync());
                Assert.Equal(values.Select(i => (decimal?)i).Sum(), await source.Select(i => (decimal?)i).SumAsync());

                Assert.Equal(values.Select(i => (int?)i).Sum(), await source.SelectMany<int, int?>(i => [i, null]).SumAsync());
                Assert.Equal(values.Select(i => (long?)i).Sum(), await source.SelectMany<int, long?>(i => [i, null]).SumAsync());
                Assert.Equal(values.Select(i => (float?)i).Sum(), await source.SelectMany<int, float?>(i => [i, null]).SumAsync());
                Assert.Equal(values.Select(i => (double?)i).Sum(), await source.SelectMany<int, double?>(i => [i, null]).SumAsync());
                Assert.Equal(values.Select(i => (decimal?)i).Sum(), await source.SelectMany<int, decimal?>(i => [i, null]).SumAsync());

                CultureInfo ci = CultureInfo.InvariantCulture;

                Assert.Equal(values.Select(i => ((int)i).ToString(ci)).Sum(s => int.Parse(s, ci)), await source.Select(i => ((int)i).ToString(ci)).SumAsync(s => int.Parse(s, ci)));
                Assert.Equal(values.Select(i => ((long)i).ToString(ci)).Sum(s => long.Parse(s, ci)), await source.Select(i => ((long)i).ToString(ci)).SumAsync(s => long.Parse(s, ci)));
                Assert.Equal(values.Select(i => ((float)i).ToString(ci)).Sum(s => float.Parse(s, ci)), await source.Select(i => ((float)i).ToString(ci)).SumAsync(s => float.Parse(s, ci)));
                Assert.Equal(values.Select(i => ((double)i).ToString(ci)).Sum(s => double.Parse(s, ci)), await source.Select(i => ((double)i).ToString(ci)).SumAsync(s => double.Parse(s, ci)));
                Assert.Equal(values.Select(i => ((decimal)i).ToString(ci)).Sum(s => decimal.Parse(s, ci)), await source.Select(i => ((decimal)i).ToString(ci)).SumAsync(s => decimal.Parse(s, ci)));

                Assert.Equal(values.Select(i => ((int)i).ToString(ci)).Sum(s => int.Parse(s, ci)), await source.Select(i => ((int)i).ToString(ci)).SumAsync(s => (int?)int.Parse(s, ci)));
                Assert.Equal(values.Select(i => ((long)i).ToString(ci)).Sum(s => long.Parse(s, ci)), await source.Select(i => ((long)i).ToString(ci)).SumAsync(s => (long?)long.Parse(s, ci)));
                Assert.Equal(values.Select(i => ((float)i).ToString(ci)).Sum(s => float.Parse(s, ci)), await source.Select(i => ((float)i).ToString(ci)).SumAsync(s => (float?)float.Parse(s, ci)));
                Assert.Equal(values.Select(i => ((double)i).ToString(ci)).Sum(s => double.Parse(s, ci)), await source.Select(i => ((double)i).ToString(ci)).SumAsync(s => (double?)double.Parse(s, ci)));
                Assert.Equal(values.Select(i => ((decimal)i).ToString(ci)).Sum(s => decimal.Parse(s, ci)), await source.Select(i => ((decimal)i).ToString(ci)).SumAsync(s => (decimal?)decimal.Parse(s, ci)));

                Assert.Equal(values.Select(i => ((int)i).ToString(ci)).Sum(s => int.Parse(s, ci)), await source.Select(i => ((int)i).ToString(ci)).SumAsync(async (s, ct) => int.Parse(s, ci)));
                Assert.Equal(values.Select(i => ((long)i).ToString(ci)).Sum(s => long.Parse(s, ci)), await source.Select(i => ((long)i).ToString(ci)).SumAsync(async (s, ct) => long.Parse(s, ci)));
                Assert.Equal(values.Select(i => ((float)i).ToString(ci)).Sum(s => float.Parse(s, ci)), await source.Select(i => ((float)i).ToString(ci)).SumAsync(async (s, ct) => float.Parse(s, ci)));
                Assert.Equal(values.Select(i => ((double)i).ToString(ci)).Sum(s => double.Parse(s, ci)), await source.Select(i => ((double)i).ToString(ci)).SumAsync(async (s, ct) => double.Parse(s, ci)));
                Assert.Equal(values.Select(i => ((decimal)i).ToString(ci)).Sum(s => decimal.Parse(s, ci)), await source.Select(i => ((decimal)i).ToString(ci)).SumAsync(async (s, ct) => decimal.Parse(s, ci)));

                Assert.Equal(values.Select(i => ((int)i).ToString(ci)).Sum(s => int.Parse(s, ci)), await source.Select(i => ((int)i).ToString(ci)).SumAsync(async (s, ct) => int.Parse(s, ci)));
                Assert.Equal(values.Select(i => ((long)i).ToString(ci)).Sum(s => long.Parse(s, ci)), await source.Select(i => ((long)i).ToString(ci)).SumAsync(async (s, ct) => long.Parse(s, ci)));
                Assert.Equal(values.Select(i => ((float)i).ToString(ci)).Sum(s => float.Parse(s, ci)), await source.Select(i => ((float)i).ToString(ci)).SumAsync(async (s, ct) => float.Parse(s, ci)));
                Assert.Equal(values.Select(i => ((double)i).ToString(ci)).Sum(s => double.Parse(s, ci)), await source.Select(i => ((double)i).ToString(ci)).SumAsync(async (s, ct) => double.Parse(s, ci)));
                Assert.Equal(values.Select(i => ((decimal)i).ToString(ci)).Sum(s => decimal.Parse(s, ci)), await source.Select(i => ((decimal)i).ToString(ci)).SumAsync(async (s, ct) => decimal.Parse(s, ci)));
            }
        }

        [Fact]
        public async Task Cancellation_Cancels()
        {
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource(2, 4).Select(i => (int)i).SumAsync(new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource(2, 4).Select(i => (long)i).SumAsync(new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource(2, 4).Select(i => (float)i).SumAsync(new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource(2, 4).Select(i => (double)i).SumAsync(new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource(2, 4).Select(i => (decimal)i).SumAsync(new CancellationToken(true)));

            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource(2, 4).Select(i => (int?)i).SumAsync(new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource(2, 4).Select(i => (long?)i).SumAsync(new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource(2, 4).Select(i => (float?)i).SumAsync(new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource(2, 4).Select(i => (double?)i).SumAsync(new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource(2, 4).Select(i => (decimal?)i).SumAsync(new CancellationToken(true)));

            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource("2", "4").SumAsync(s => int.Parse(s), new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource("2", "4").SumAsync(s => long.Parse(s), new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource("2", "4").SumAsync(s => float.Parse(s), new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource("2", "4").SumAsync(s => double.Parse(s), new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource("2", "4").SumAsync(s => decimal.Parse(s), new CancellationToken(true)));

            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource("2", "4").SumAsync(s => (int?)int.Parse(s), new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource("2", "4").SumAsync(s => (long?)long.Parse(s), new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource("2", "4").SumAsync(s => (float?)float.Parse(s), new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource("2", "4").SumAsync(s => (double?)double.Parse(s), new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource("2", "4").SumAsync(s => (decimal?)decimal.Parse(s), new CancellationToken(true)));

            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource("2", "4").SumAsync(async (s, ct) => int.Parse(s), new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource("2", "4").SumAsync(async (s, ct) => long.Parse(s), new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource("2", "4").SumAsync(async (s, ct) => float.Parse(s), new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource("2", "4").SumAsync(async (s, ct) => double.Parse(s), new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource("2", "4").SumAsync(async (s, ct) => decimal.Parse(s), new CancellationToken(true)));

            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource("2", "4").SumAsync(async (s, ct) => (int?)int.Parse(s), new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource("2", "4").SumAsync(async (s, ct) => (long?)long.Parse(s), new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource("2", "4").SumAsync(async (s, ct) => (float?)float.Parse(s), new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource("2", "4").SumAsync(async (s, ct) => (double?)double.Parse(s), new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource("2", "4").SumAsync(async (s, ct) => (decimal?)decimal.Parse(s), new CancellationToken(true)));
        }

        [Fact]
        public async Task InterfaceCalls_ExpectedCounts()
        {
            await Validate(source => source.Select(i => (int)i).SumAsync());
            await Validate(source => source.Select(i => (long)i).SumAsync());
            await Validate(source => source.Select(i => (float)i).SumAsync());
            await Validate(source => source.Select(i => (double)i).SumAsync());
            await Validate(source => source.Select(i => (decimal)i).SumAsync());

            await Validate(source => source.Select(i => (int?)i).SumAsync());
            await Validate(source => source.Select(i => (long?)i).SumAsync());
            await Validate(source => source.Select(i => (float?)i).SumAsync());
            await Validate(source => source.Select(i => (double?)i).SumAsync());
            await Validate(source => source.Select(i => (decimal?)i).SumAsync());

            CultureInfo ci = CultureInfo.InvariantCulture;

            await Validate(source => source.Select(i => i.ToString(ci)).SumAsync(s => int.Parse(s, ci)));
            await Validate(source => source.Select(i => i.ToString(ci)).SumAsync(s => long.Parse(s, ci)));
            await Validate(source => source.Select(i => i.ToString(ci)).SumAsync(s => float.Parse(s, ci)));
            await Validate(source => source.Select(i => i.ToString(ci)).SumAsync(s => double.Parse(s, ci)));
            await Validate(source => source.Select(i => i.ToString(ci)).SumAsync(s => decimal.Parse(s, ci)));

            await Validate(source => source.Select(i => i.ToString(ci)).SumAsync(s => (int?)int.Parse(s, ci)));
            await Validate(source => source.Select(i => i.ToString(ci)).SumAsync(s => (long?)long.Parse(s, ci)));
            await Validate(source => source.Select(i => i.ToString(ci)).SumAsync(s => (float?)float.Parse(s, ci)));
            await Validate(source => source.Select(i => i.ToString(ci)).SumAsync(s => (double?)double.Parse(s, ci)));
            await Validate(source => source.Select(i => i.ToString(ci)).SumAsync(s => (decimal?)decimal.Parse(s, ci)));

            await Validate(source => source.Select(i => i.ToString(ci)).SumAsync(async (s, ct) => int.Parse(s, ci)));
            await Validate(source => source.Select(i => i.ToString(ci)).SumAsync(async (s, ct) => long.Parse(s, ci)));
            await Validate(source => source.Select(i => i.ToString(ci)).SumAsync(async (s, ct) => float.Parse(s, ci)));
            await Validate(source => source.Select(i => i.ToString(ci)).SumAsync(async (s, ct) => double.Parse(s, ci)));
            await Validate(source => source.Select(i => i.ToString(ci)).SumAsync(async (s, ct) => decimal.Parse(s, ci)));

            await Validate(source => source.Select(i => i.ToString(ci)).SumAsync(async (s, ct) => int.Parse(s, ci)));
            await Validate(source => source.Select(i => i.ToString(ci)).SumAsync(async (s, ct) => long.Parse(s, ci)));
            await Validate(source => source.Select(i => i.ToString(ci)).SumAsync(async (s, ct) => float.Parse(s, ci)));
            await Validate(source => source.Select(i => i.ToString(ci)).SumAsync(async (s, ct) => double.Parse(s, ci)));
            await Validate(source => source.Select(i => i.ToString(ci)).SumAsync(async (s, ct) => decimal.Parse(s, ci)));

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
