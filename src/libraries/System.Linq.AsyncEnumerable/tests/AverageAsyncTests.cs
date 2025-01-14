// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace System.Linq.Tests
{
    public class AverageAsyncTests : AsyncEnumerableTests
    {
        [Fact]
        public void InvalidInputs_Throws()
        {
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.AverageAsync((IAsyncEnumerable<int>)null));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.AverageAsync((IAsyncEnumerable<long>)null));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.AverageAsync((IAsyncEnumerable<float>)null));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.AverageAsync((IAsyncEnumerable<double>)null));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.AverageAsync((IAsyncEnumerable<decimal>)null));

            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.AverageAsync((IAsyncEnumerable<int?>)null));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.AverageAsync((IAsyncEnumerable<long?>)null));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.AverageAsync((IAsyncEnumerable<float?>)null));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.AverageAsync((IAsyncEnumerable<double?>)null));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.AverageAsync((IAsyncEnumerable<decimal?>)null));

            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.AverageAsync<string>(null, s => (int)42));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.AverageAsync<string>(null, s => (long)42));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.AverageAsync<string>(null, s => (float)42));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.AverageAsync<string>(null, s => (double)42));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.AverageAsync<string>(null, s => (decimal)42));
            AssertExtensions.Throws<ArgumentNullException>("selector", () => AsyncEnumerable.AverageAsync(AsyncEnumerable.Empty<string>(), (Func<string, int>)null));
            AssertExtensions.Throws<ArgumentNullException>("selector", () => AsyncEnumerable.AverageAsync(AsyncEnumerable.Empty<string>(), (Func<string, long>)null));
            AssertExtensions.Throws<ArgumentNullException>("selector", () => AsyncEnumerable.AverageAsync(AsyncEnumerable.Empty<string>(), (Func<string, float>)null));
            AssertExtensions.Throws<ArgumentNullException>("selector", () => AsyncEnumerable.AverageAsync(AsyncEnumerable.Empty<string>(), (Func<string, double>)null));
            AssertExtensions.Throws<ArgumentNullException>("selector", () => AsyncEnumerable.AverageAsync(AsyncEnumerable.Empty<string>(), (Func<string, decimal>)null));

            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.AverageAsync<string>(null, async (s, ct) => 42));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.AverageAsync<string>(null, async (s, ct) => 42L));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.AverageAsync<string>(null, async (s, ct) => 42f));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.AverageAsync<string>(null, async (s, ct) => 42.0));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.AverageAsync<string>(null, async (s, ct) => 42m));
            AssertExtensions.Throws<ArgumentNullException>("selector", () => AsyncEnumerable.AverageAsync(AsyncEnumerable.Empty<string>(), (Func<string, CancellationToken, ValueTask<int>>)null));
            AssertExtensions.Throws<ArgumentNullException>("selector", () => AsyncEnumerable.AverageAsync(AsyncEnumerable.Empty<string>(), (Func<string, CancellationToken, ValueTask<long>>)null));
            AssertExtensions.Throws<ArgumentNullException>("selector", () => AsyncEnumerable.AverageAsync(AsyncEnumerable.Empty<string>(), (Func<string, CancellationToken, ValueTask<float>>)null));
            AssertExtensions.Throws<ArgumentNullException>("selector", () => AsyncEnumerable.AverageAsync(AsyncEnumerable.Empty<string>(), (Func<string, CancellationToken, ValueTask<double>>)null));
            AssertExtensions.Throws<ArgumentNullException>("selector", () => AsyncEnumerable.AverageAsync(AsyncEnumerable.Empty<string>(), (Func<string, CancellationToken, ValueTask<decimal>>)null));

            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.AverageAsync<string>(null, s => (int?)42));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.AverageAsync<string>(null, s => (long?)42));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.AverageAsync<string>(null, s => (float?)42));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.AverageAsync<string>(null, s => (double?)42));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.AverageAsync<string>(null, s => (decimal?)42));
            AssertExtensions.Throws<ArgumentNullException>("selector", () => AsyncEnumerable.AverageAsync(AsyncEnumerable.Empty<string>(), (Func<string, int?>)null));
            AssertExtensions.Throws<ArgumentNullException>("selector", () => AsyncEnumerable.AverageAsync(AsyncEnumerable.Empty<string>(), (Func<string, long?>)null));
            AssertExtensions.Throws<ArgumentNullException>("selector", () => AsyncEnumerable.AverageAsync(AsyncEnumerable.Empty<string>(), (Func<string, float?>)null));
            AssertExtensions.Throws<ArgumentNullException>("selector", () => AsyncEnumerable.AverageAsync(AsyncEnumerable.Empty<string>(), (Func<string, double?>)null));
            AssertExtensions.Throws<ArgumentNullException>("selector", () => AsyncEnumerable.AverageAsync(AsyncEnumerable.Empty<string>(), (Func<string, decimal?>)null));

            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.AverageAsync<string>(null, async (s, ct) => (int?)42));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.AverageAsync<string>(null, async (s, ct) => (long?)42));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.AverageAsync<string>(null, async (s, ct) => (float?)42));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.AverageAsync<string>(null, async (s, ct) => (double?)42));
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.AverageAsync<string>(null, async (s, ct) => (decimal?)42));
            AssertExtensions.Throws<ArgumentNullException>("selector", () => AsyncEnumerable.AverageAsync(AsyncEnumerable.Empty<string>(), (Func<string, CancellationToken, ValueTask<int?>>)null));
            AssertExtensions.Throws<ArgumentNullException>("selector", () => AsyncEnumerable.AverageAsync(AsyncEnumerable.Empty<string>(), (Func<string, CancellationToken, ValueTask<long?>>)null));
            AssertExtensions.Throws<ArgumentNullException>("selector", () => AsyncEnumerable.AverageAsync(AsyncEnumerable.Empty<string>(), (Func<string, CancellationToken, ValueTask<float?>>)null));
            AssertExtensions.Throws<ArgumentNullException>("selector", () => AsyncEnumerable.AverageAsync(AsyncEnumerable.Empty<string>(), (Func<string, CancellationToken, ValueTask<double?>>)null));
            AssertExtensions.Throws<ArgumentNullException>("selector", () => AsyncEnumerable.AverageAsync(AsyncEnumerable.Empty<string>(), (Func<string, CancellationToken, ValueTask<decimal?>>)null));
        }
        [Fact]
        public async Task EmptyInputs_NonNullable_Throws()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await AsyncEnumerable.AverageAsync(AsyncEnumerable.Empty<int>()));
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await AsyncEnumerable.AverageAsync(AsyncEnumerable.Empty<long>()));
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await AsyncEnumerable.AverageAsync(AsyncEnumerable.Empty<float>()));
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await AsyncEnumerable.AverageAsync(AsyncEnumerable.Empty<double>()));
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await AsyncEnumerable.AverageAsync(AsyncEnumerable.Empty<decimal>()));

            Assert.Null(await AsyncEnumerable.AverageAsync(AsyncEnumerable.Empty<int?>()));
            Assert.Null(await AsyncEnumerable.AverageAsync(AsyncEnumerable.Empty<long?>()));
            Assert.Null(await AsyncEnumerable.AverageAsync(AsyncEnumerable.Empty<float?>()));
            Assert.Null(await AsyncEnumerable.AverageAsync(AsyncEnumerable.Empty<double?>()));
            Assert.Null(await AsyncEnumerable.AverageAsync(AsyncEnumerable.Empty<decimal?>()));

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await AsyncEnumerable.AverageAsync(AsyncEnumerable.Empty<string>(), s => (int)42));
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await AsyncEnumerable.AverageAsync(AsyncEnumerable.Empty<string>(), s => (long)42));
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await AsyncEnumerable.AverageAsync(AsyncEnumerable.Empty<string>(), s => (float)42));
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await AsyncEnumerable.AverageAsync(AsyncEnumerable.Empty<string>(), s => (double)42));
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await AsyncEnumerable.AverageAsync(AsyncEnumerable.Empty<string>(), s => (decimal)42));

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await AsyncEnumerable.AverageAsync(AsyncEnumerable.Empty<string>(), async (s, ct) => (int)42));
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await AsyncEnumerable.AverageAsync(AsyncEnumerable.Empty<string>(), async (s, ct) => (long)42));
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await AsyncEnumerable.AverageAsync(AsyncEnumerable.Empty<string>(), async (s, ct) => (float)42));
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await AsyncEnumerable.AverageAsync(AsyncEnumerable.Empty<string>(), async (s, ct) => (double)42));
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await AsyncEnumerable.AverageAsync(AsyncEnumerable.Empty<string>(), async (s, ct) => (decimal)42));

            Assert.Null(await AsyncEnumerable.AverageAsync(AsyncEnumerable.Empty<string>(), s => (int?)42));
            Assert.Null(await AsyncEnumerable.AverageAsync(AsyncEnumerable.Empty<string>(), s => (long?)42));
            Assert.Null(await AsyncEnumerable.AverageAsync(AsyncEnumerable.Empty<string>(), s => (float?)42));
            Assert.Null(await AsyncEnumerable.AverageAsync(AsyncEnumerable.Empty<string>(), s => (double?)42));
            Assert.Null(await AsyncEnumerable.AverageAsync(AsyncEnumerable.Empty<string>(), s => (decimal?)42));

            Assert.Null(await AsyncEnumerable.AverageAsync(AsyncEnumerable.Empty<string>(), async (s, ct) => (int?)42));
            Assert.Null(await AsyncEnumerable.AverageAsync(AsyncEnumerable.Empty<string>(), async (s, ct) => (long?)42));
            Assert.Null(await AsyncEnumerable.AverageAsync(AsyncEnumerable.Empty<string>(), async (s, ct) => (float?)42));
            Assert.Null(await AsyncEnumerable.AverageAsync(AsyncEnumerable.Empty<string>(), async (s, ct) => (double?)42));
            Assert.Null(await AsyncEnumerable.AverageAsync(AsyncEnumerable.Empty<string>(), async (s, ct) => (decimal?)42));
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
                Assert.Equal(values.Select(i => (int)i).Average(), await source.Select(i => (int)i).AverageAsync());
                Assert.Equal(values.Select(i => (long)i).Average(), await source.Select(i => (long)i).AverageAsync());
                Assert.Equal(values.Select(i => (float)i).Average(), await source.Select(i => (float)i).AverageAsync());
                Assert.Equal(values.Select(i => (double)i).Average(), await source.Select(i => (double)i).AverageAsync());
                Assert.Equal(values.Select(i => (decimal)i).Average(), await source.Select(i => (decimal)i).AverageAsync());

                Assert.Equal(values.Select(i => (int?)i).Average(), await source.Select(i => (int?)i).AverageAsync());
                Assert.Equal(values.Select(i => (long?)i).Average(), await source.Select(i => (long?)i).AverageAsync());
                Assert.Equal(values.Select(i => (float?)i).Average(), await source.Select(i => (float?)i).AverageAsync());
                Assert.Equal(values.Select(i => (double?)i).Average(), await source.Select(i => (double?)i).AverageAsync());
                Assert.Equal(values.Select(i => (decimal?)i).Average(), await source.Select(i => (decimal?)i).AverageAsync());

                Assert.Equal(values.Select(i => (int?)i).Average(), await source.SelectMany<int, int?>(i => [i, null]).AverageAsync());
                Assert.Equal(values.Select(i => (long?)i).Average(), await source.SelectMany<int, long?>(i => [i, null]).AverageAsync());
                Assert.Equal(values.Select(i => (float?)i).Average(), await source.SelectMany<int, float?>(i => [i, null]).AverageAsync());
                Assert.Equal(values.Select(i => (double?)i).Average(), await source.SelectMany<int, double?>(i => [i, null]).AverageAsync());
                Assert.Equal(values.Select(i => (decimal?)i).Average(), await source.SelectMany<int, decimal?>(i => [i, null]).AverageAsync());

                CultureInfo ci = CultureInfo.InvariantCulture;

                Assert.Equal(values.Select(i => ((int)i).ToString(ci)).Average(s => int.Parse(s, ci)), await source.Select(i => ((int)i).ToString(ci)).AverageAsync(s => int.Parse(s, ci)));
                Assert.Equal(values.Select(i => ((long)i).ToString(ci)).Average(s => long.Parse(s, ci)), await source.Select(i => ((long)i).ToString(ci)).AverageAsync(s => long.Parse(s, ci)));
                Assert.Equal(values.Select(i => ((float)i).ToString(ci)).Average(s => float.Parse(s, ci)), await source.Select(i => ((float)i).ToString(ci)).AverageAsync(s => float.Parse(s, ci)));
                Assert.Equal(values.Select(i => ((double)i).ToString(ci)).Average(s => double.Parse(s, ci)), await source.Select(i => ((double)i).ToString(ci)).AverageAsync(s => double.Parse(s, ci)));
                Assert.Equal(values.Select(i => ((decimal)i).ToString(ci)).Average(s => decimal.Parse(s, ci)), await source.Select(i => ((decimal)i).ToString(ci)).AverageAsync(s => decimal.Parse(s, ci)));

                Assert.Equal(values.Select(i => ((int)i).ToString(ci)).Average(s => int.Parse(s, ci)), await source.Select(i => ((int)i).ToString(ci)).AverageAsync(s => (int?)int.Parse(s, ci)));
                Assert.Equal(values.Select(i => ((long)i).ToString(ci)).Average(s => long.Parse(s, ci)), await source.Select(i => ((long)i).ToString(ci)).AverageAsync(s => (long?)long.Parse(s, ci)));
                Assert.Equal(values.Select(i => ((float)i).ToString(ci)).Average(s => float.Parse(s, ci)), await source.Select(i => ((float)i).ToString(ci)).AverageAsync(s => (float?)float.Parse(s, ci)));
                Assert.Equal(values.Select(i => ((double)i).ToString(ci)).Average(s => double.Parse(s, ci)), await source.Select(i => ((double)i).ToString(ci)).AverageAsync(s => (double?)double.Parse(s, ci)));
                Assert.Equal(values.Select(i => ((decimal)i).ToString(ci)).Average(s => decimal.Parse(s, ci)), await source.Select(i => ((decimal)i).ToString(ci)).AverageAsync(s => (decimal?)decimal.Parse(s, ci)));

                Assert.Equal(values.Select(i => ((int)i).ToString(ci)).Average(s => int.Parse(s, ci)), await source.Select(i => ((int)i).ToString(ci)).AverageAsync(async (s, ct) => int.Parse(s, ci)));
                Assert.Equal(values.Select(i => ((long)i).ToString(ci)).Average(s => long.Parse(s, ci)), await source.Select(i => ((long)i).ToString(ci)).AverageAsync(async (s, ct) => long.Parse(s, ci)));
                Assert.Equal(values.Select(i => ((float)i).ToString(ci)).Average(s => float.Parse(s, ci)), await source.Select(i => ((float)i).ToString(ci)).AverageAsync(async (s, ct) => float.Parse(s, ci)));
                Assert.Equal(values.Select(i => ((double)i).ToString(ci)).Average(s => double.Parse(s, ci)), await source.Select(i => ((double)i).ToString(ci)).AverageAsync(async (s, ct) => double.Parse(s, ci)));
                Assert.Equal(values.Select(i => ((decimal)i).ToString(ci)).Average(s => decimal.Parse(s, ci)), await source.Select(i => ((decimal)i).ToString(ci)).AverageAsync(async (s, ct) => decimal.Parse(s, ci)));

                Assert.Equal(values.Select(i => ((int)i).ToString(ci)).Average(s => int.Parse(s, ci)), await source.Select(i => ((int)i).ToString(ci)).AverageAsync(async (s, ct) => (int?)int.Parse(s, ci)));
                Assert.Equal(values.Select(i => ((long)i).ToString(ci)).Average(s => long.Parse(s, ci)), await source.Select(i => ((long)i).ToString(ci)).AverageAsync(async (s, ct) => (long?)long.Parse(s, ci)));
                Assert.Equal(values.Select(i => ((float)i).ToString(ci)).Average(s => float.Parse(s, ci)), await source.Select(i => ((float)i).ToString(ci)).AverageAsync(async (s, ct) => (float?)float.Parse(s, ci)));
                Assert.Equal(values.Select(i => ((double)i).ToString(ci)).Average(s => double.Parse(s, ci)), await source.Select(i => ((double)i).ToString(ci)).AverageAsync(async (s, ct) => (double?)double.Parse(s, ci)));
                Assert.Equal(values.Select(i => ((decimal)i).ToString(ci)).Average(s => decimal.Parse(s, ci)), await source.Select(i => ((decimal)i).ToString(ci)).AverageAsync(async (s, ct) => (decimal?)decimal.Parse(s, ci)));
            }
        }

        [Fact]
        public async Task Cancellation_Cancels()
        {
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource(2, 4).Select(i => (int)i).AverageAsync(new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource(2, 4).Select(i => (long)i).AverageAsync(new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource(2, 4).Select(i => (float)i).AverageAsync(new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource(2, 4).Select(i => (double)i).AverageAsync(new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource(2, 4).Select(i => (decimal)i).AverageAsync(new CancellationToken(true)));

            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource(2, 4).Select(i => (int?)i).AverageAsync(new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource(2, 4).Select(i => (long?)i).AverageAsync(new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource(2, 4).Select(i => (float?)i).AverageAsync(new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource(2, 4).Select(i => (double?)i).AverageAsync(new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource(2, 4).Select(i => (decimal?)i).AverageAsync(new CancellationToken(true)));

            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource("2", "4").AverageAsync(s => int.Parse(s), new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource("2", "4").AverageAsync(s => long.Parse(s), new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource("2", "4").AverageAsync(s => float.Parse(s), new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource("2", "4").AverageAsync(s => double.Parse(s), new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource("2", "4").AverageAsync(s => decimal.Parse(s), new CancellationToken(true)));

            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource("2", "4").AverageAsync(s => (int?)int.Parse(s), new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource("2", "4").AverageAsync(s => (long?)long.Parse(s), new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource("2", "4").AverageAsync(s => (float?)float.Parse(s), new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource("2", "4").AverageAsync(s => (double?)double.Parse(s), new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource("2", "4").AverageAsync(s => (decimal?)decimal.Parse(s), new CancellationToken(true)));

            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource("2", "4").AverageAsync(async (s, ct) => int.Parse(s), new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource("2", "4").AverageAsync(async (s, ct) => long.Parse(s), new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource("2", "4").AverageAsync(async (s, ct) => float.Parse(s), new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource("2", "4").AverageAsync(async (s, ct) => double.Parse(s), new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource("2", "4").AverageAsync(async (s, ct) => decimal.Parse(s), new CancellationToken(true)));

            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource("2", "4").AverageAsync(async (s, ct) => (int?)int.Parse(s), new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource("2", "4").AverageAsync(async (s, ct) => (long?)long.Parse(s), new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource("2", "4").AverageAsync(async (s, ct) => (float?)float.Parse(s), new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource("2", "4").AverageAsync(async (s, ct) => (double?)double.Parse(s), new CancellationToken(true)));
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateSource("2", "4").AverageAsync(async (s, ct) => (decimal?)decimal.Parse(s), new CancellationToken(true)));
        }

        [Fact]
        public async Task InterfaceCalls_ExpectedCounts()
        {
            await Validate(source => source.Select(i => (int)i).AverageAsync());
            await Validate(source => source.Select(i => (long)i).AverageAsync());
            await Validate(source => source.Select(i => (float)i).AverageAsync());
            await Validate(source => source.Select(i => (double)i).AverageAsync());
            await Validate(source => source.Select(i => (decimal)i).AverageAsync());

            await Validate(source => source.Select(i => (int?)i).AverageAsync());
            await Validate(source => source.Select(i => (long?)i).AverageAsync());
            await Validate(source => source.Select(i => (float?)i).AverageAsync());
            await Validate(source => source.Select(i => (double?)i).AverageAsync());
            await Validate(source => source.Select(i => (decimal?)i).AverageAsync());

            CultureInfo ci = CultureInfo.InvariantCulture;

            await Validate(source => source.Select(i => i.ToString(ci)).AverageAsync(s => int.Parse(s, ci)));
            await Validate(source => source.Select(i => i.ToString(ci)).AverageAsync(s => long.Parse(s, ci)));
            await Validate(source => source.Select(i => i.ToString(ci)).AverageAsync(s => float.Parse(s, ci)));
            await Validate(source => source.Select(i => i.ToString(ci)).AverageAsync(s => double.Parse(s, ci)));
            await Validate(source => source.Select(i => i.ToString(ci)).AverageAsync(s => decimal.Parse(s, ci)));

            await Validate(source => source.Select(i => i.ToString(ci)).AverageAsync(s => (int?)int.Parse(s, ci)));
            await Validate(source => source.Select(i => i.ToString(ci)).AverageAsync(s => (long?)long.Parse(s, ci)));
            await Validate(source => source.Select(i => i.ToString(ci)).AverageAsync(s => (float?)float.Parse(s, ci)));
            await Validate(source => source.Select(i => i.ToString(ci)).AverageAsync(s => (double?)double.Parse(s, ci)));
            await Validate(source => source.Select(i => i.ToString(ci)).AverageAsync(s => (decimal?)decimal.Parse(s, ci)));

            await Validate(source => source.Select(i => i.ToString(ci)).AverageAsync(async (s, ct) => int.Parse(s, ci)));
            await Validate(source => source.Select(i => i.ToString(ci)).AverageAsync(async (s, ct) => long.Parse(s, ci)));
            await Validate(source => source.Select(i => i.ToString(ci)).AverageAsync(async (s, ct) => float.Parse(s, ci)));
            await Validate(source => source.Select(i => i.ToString(ci)).AverageAsync(async (s, ct) => double.Parse(s, ci)));
            await Validate(source => source.Select(i => i.ToString(ci)).AverageAsync(async (s, ct) => decimal.Parse(s, ci)));

            await Validate(source => source.Select(i => i.ToString(ci)).AverageAsync(async (s, ct) => (int?)int.Parse(s, ci)));
            await Validate(source => source.Select(i => i.ToString(ci)).AverageAsync(async (s, ct) => (long?)long.Parse(s, ci)));
            await Validate(source => source.Select(i => i.ToString(ci)).AverageAsync(async (s, ct) => (float?)float.Parse(s, ci)));
            await Validate(source => source.Select(i => i.ToString(ci)).AverageAsync(async (s, ct) => (double?)double.Parse(s, ci)));
            await Validate(source => source.Select(i => i.ToString(ci)).AverageAsync(async (s, ct) => (decimal?)decimal.Parse(s, ci)));

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
