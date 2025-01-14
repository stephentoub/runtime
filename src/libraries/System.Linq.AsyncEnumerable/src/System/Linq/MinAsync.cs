// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq
{
    public static partial class AsyncEnumerable
    {
        /// <summary>Returns the minimum value in a sequence of values.</summary>
        /// <param name="source">A sequence of values to determine the minimum value of.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
        /// <returns>The minimum value in the sequence.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
        public static ValueTask<int> MinAsync(
            this IAsyncEnumerable<int> source,
            CancellationToken cancellationToken = default)
        {
            ThrowHelper.ThrowIfNull(source);

            return Impl(source, cancellationToken);

            static async ValueTask<int> Impl(IAsyncEnumerable<int> source, CancellationToken cancellationToken)
            {
                IAsyncEnumerator<int> e = source.GetAsyncEnumerator(cancellationToken);
                try
                {
                    if (!await e.MoveNextAsync().ConfigureAwait(false))
                    {
                        ThrowHelper.ThrowNoElementsException();
                    }

                    int value = e.Current;

                    while (await e.MoveNextAsync().ConfigureAwait(false))
                    {
                        int x = e.Current;
                        if (x < value)
                        {
                            value = x;
                        }
                    }

                    return value;
                }
                finally
                {
                    await e.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        /// <summary>Returns the minimum value in a sequence of values.</summary>
        /// <param name="source">A sequence of values to determine the minimum value of.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
        /// <returns>The minimum value in the sequence.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
        public static ValueTask<long> MinAsync(
            this IAsyncEnumerable<long> source,
            CancellationToken cancellationToken = default)
        {
            ThrowHelper.ThrowIfNull(source);

            return Impl(source, cancellationToken);

            static async ValueTask<long> Impl(
                IAsyncEnumerable<long> source,
                CancellationToken cancellationToken)
            {
                IAsyncEnumerator<long> e = source.GetAsyncEnumerator(cancellationToken);
                try
                {
                    if (!await e.MoveNextAsync().ConfigureAwait(false))
                    {
                        ThrowHelper.ThrowNoElementsException();
                    }

                    long value = e.Current;

                    while (await e.MoveNextAsync().ConfigureAwait(false))
                    {
                        long x = e.Current;
                        if (x < value)
                        {
                            value = x;
                        }
                    }

                    return value;
                }
                finally
                {
                    await e.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        /// <summary>Returns the minimum value in a sequence of values.</summary>
        /// <param name="source">A sequence of values to determine the minimum value of.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
        /// <returns>The minimum value in the sequence.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
        public static ValueTask<float> MinAsync(
            this IAsyncEnumerable<float> source,
            CancellationToken cancellationToken = default)
        {
            ThrowHelper.ThrowIfNull(source);

            return Impl(source, cancellationToken);

            static async ValueTask<float> Impl(
                IAsyncEnumerable<float> source,
                CancellationToken cancellationToken)
            {
                IAsyncEnumerator<float> e = source.GetAsyncEnumerator(cancellationToken);
                try
                {
                    if (!await e.MoveNextAsync().ConfigureAwait(false))
                    {
                        ThrowHelper.ThrowNoElementsException();
                    }

                    float value = e.Current;
                    if (float.IsNaN(value))
                    {
                        return value;
                    }

                    while (await e.MoveNextAsync().ConfigureAwait(false))
                    {
                        float x = e.Current;
                        if (x < value)
                        {
                            value = x;
                        }

                        // Normally NaN < anything is false, as is anything < NaN
                        // However, this leads to some irksome outcomes in Min and Max.
                        // If we use those semantics then Min(NaN, 5.0) is NaN, but
                        // Min(5.0, NaN) is 5.0!  To fix this, we impose a total
                        // ordering where NaN is smaller than every value, including
                        // negative infinity. Not testing for NaN therefore isn't an option, but since we
                        // can't find a smaller value, we can short-circuit.
                        else if (float.IsNaN(x))
                        {
                            return x;
                        }
                    }

                    return value;

                }
                finally
                {
                    await e.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        /// <summary>Returns the minimum value in a sequence of values.</summary>
        /// <param name="source">A sequence of values to determine the minimum value of.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
        /// <returns>The minimum value in the sequence.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
        public static ValueTask<double> MinAsync(
            this IAsyncEnumerable<double> source,
            CancellationToken cancellationToken = default)
        {
            ThrowHelper.ThrowIfNull(source);

            return Impl(source, cancellationToken);

            static async ValueTask<double> Impl(
                IAsyncEnumerable<double> source,
                CancellationToken cancellationToken)
            {
                IAsyncEnumerator<double> e = source.GetAsyncEnumerator(cancellationToken);
                try
                {
                    if (!await e.MoveNextAsync().ConfigureAwait(false))
                    {
                        ThrowHelper.ThrowNoElementsException();
                    }

                    double value = e.Current;
                    if (double.IsNaN(value))
                    {
                        return value;
                    }

                    while (await e.MoveNextAsync().ConfigureAwait(false))
                    {
                        double x = e.Current;
                        if (x < value)
                        {
                            value = x;
                        }

                        // Normally NaN < anything is false, as is anything < NaN
                        // However, this leads to some irksome outcomes in Min and Max.
                        // If we use those semantics then Min(NaN, 5.0) is NaN, but
                        // Min(5.0, NaN) is 5.0!  To fix this, we impose a total
                        // ordering where NaN is smaller than every value, including
                        // negative infinity. Not testing for NaN therefore isn't an option, but since we
                        // can't find a smaller value, we can short-circuit.
                        else if (double.IsNaN(x))
                        {
                            return x;
                        }
                    }

                    return value;

                }
                finally
                {
                    await e.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        /// <summary>Returns the minimum value in a sequence of values.</summary>
        /// <param name="source">A sequence of values to determine the minimum value of.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
        /// <returns>The minimum value in the sequence.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
        public static ValueTask<decimal> MinAsync(
            this IAsyncEnumerable<decimal> source,
            CancellationToken cancellationToken = default)
        {
            ThrowHelper.ThrowIfNull(source);

            return Impl(source, cancellationToken);

            static async ValueTask<decimal> Impl(
                IAsyncEnumerable<decimal> source,
                CancellationToken cancellationToken)
            {
                IAsyncEnumerator<decimal> e = source.GetAsyncEnumerator(cancellationToken);
                try
                {
                    if (!await e.MoveNextAsync().ConfigureAwait(false))
                    {
                        ThrowHelper.ThrowNoElementsException();
                    }

                    decimal value = e.Current;

                    while (await e.MoveNextAsync().ConfigureAwait(false))
                    {
                        decimal x = e.Current;
                        if (x < value)
                        {
                            value = x;
                        }
                    }

                    return value;
                }
                finally
                {
                    await e.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        /// <summary>Returns the minimum value in a sequence of nullable values.</summary>
        /// <param name="source">A sequence of nullable values to determine the minimum value of.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
        /// <returns>The minimum value in the sequence.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
        public static ValueTask<int?> MinAsync(
            this IAsyncEnumerable<int?> source,
            CancellationToken cancellationToken = default)
        {
            ThrowHelper.ThrowIfNull(source);

            return Impl(source.WithCancellation(cancellationToken).ConfigureAwait(false));

            static async ValueTask<int?> Impl(
                ConfiguredCancelableAsyncEnumerable<int?> source)
            {
                int? value = null;
                await foreach (int? x in source)
                {
                    if (value is null || x < value)
                    {
                        value = x;
                    }
                }

                return value;
            }
        }

        /// <summary>Returns the minimum value in a sequence of nullable values.</summary>
        /// <param name="source">A sequence of nullable values to determine the minimum value of.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
        /// <returns>The minimum value in the sequence.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
        public static ValueTask<long?> MinAsync(
            this IAsyncEnumerable<long?> source,
            CancellationToken cancellationToken = default)
        {
            ThrowHelper.ThrowIfNull(source);

            return Impl(source.WithCancellation(cancellationToken).ConfigureAwait(false));

            static async ValueTask<long?> Impl(
                ConfiguredCancelableAsyncEnumerable<long?> source)
            {
                long? value = null;
                await foreach (long? x in source)
                {
                    if (value is null || x < value)
                    {
                        value = x;
                    }
                }

                return value;
            }
        }

        /// <summary>Returns the minimum value in a sequence of nullable values.</summary>
        /// <param name="source">A sequence of nullable values to determine the minimum value of.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
        /// <returns>The minimum value in the sequence.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
        public static ValueTask<float?> MinAsync(
            this IAsyncEnumerable<float?> source,
            CancellationToken cancellationToken = default)
        {
            ThrowHelper.ThrowIfNull(source);

            return Impl(source.WithCancellation(cancellationToken).ConfigureAwait(false));

            static async ValueTask<float?> Impl(
                ConfiguredCancelableAsyncEnumerable<float?> source)
            {
                float? value = null;
                await foreach (float? x in source)
                {
                    if (x is null)
                    {
                        continue;
                    }

                    if (value == null || x < value || float.IsNaN(x.GetValueOrDefault()))
                    {
                        value = x;
                    }
                }

                return value;
            }
        }

        /// <summary>Returns the minimum value in a sequence of nullable values.</summary>
        /// <param name="source">A sequence of nullable values to determine the minimum value of.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
        /// <returns>The minimum value in the sequence.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
        public static ValueTask<double?> MinAsync(
            this IAsyncEnumerable<double?> source,
            CancellationToken cancellationToken = default)
        {
            ThrowHelper.ThrowIfNull(source);

            return Impl(source.WithCancellation(cancellationToken).ConfigureAwait(false));

            static async ValueTask<double?> Impl(
                ConfiguredCancelableAsyncEnumerable<double?> source)
            {
                double? value = null;
                await foreach (double? x in source)
                {
                    if (x is null)
                    {
                        continue;
                    }

                    if (value == null || x < value || double.IsNaN(x.GetValueOrDefault()))
                    {
                        value = x;
                    }
                }

                return value;
            }
        }

        /// <summary>Returns the minimum value in a sequence of nullable values.</summary>
        /// <param name="source">A sequence of nullable values to determine the minimum value of.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
        /// <returns>The minimum value in the sequence.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
        public static ValueTask<decimal?> MinAsync(
            this IAsyncEnumerable<decimal?> source,
            CancellationToken cancellationToken = default)
        {
            ThrowHelper.ThrowIfNull(source);

            return Impl(source.WithCancellation(cancellationToken).ConfigureAwait(false));

            static async ValueTask<decimal?> Impl(
                ConfiguredCancelableAsyncEnumerable<decimal?> source)
            {
                decimal? value = null;
                await foreach (decimal? x in source)
                {
                    if (value is null || x < value)
                    {
                        value = x;
                    }
                }

                return value;
            }
        }

        /// <summary>Returns the minimum value in a generic sequence.</summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values to determine the minimum value of.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
        /// <returns>The minimum value in the sequence.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">No object in <paramref name="source" /> implements the <see cref="IComparable" /> or <see cref="IComparable{T}" /> interface.</exception>
        /// <remarks>
        /// <para>If type <typeparamref name="TSource" /> implements <see cref="IComparable{T}" />, the <see cref="MinAsync{TSource}(IAsyncEnumerable{TSource}, IComparer{TSource}?, CancellationToken)" /> method uses that implementation to compare values. Otherwise, if type <typeparamref name="TSource" /> implements <see cref="IComparable" />, that implementation is used to compare values.</para>
        /// <para>If <typeparamref name="TSource" /> is a reference type and the source sequence is empty or contains only values that are <see langword="null" />, this method returns <see langword="null" />.</para>
        /// </remarks>
        public static ValueTask<TSource?> MinAsync<TSource>(
            this IAsyncEnumerable<TSource> source,
            CancellationToken cancellationToken = default) =>
            MinAsync(source, (IComparer<TSource>?)null, cancellationToken);

        /// <summary>Returns the minimum value in a generic sequence.</summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values to determine the minimum value of.</param>
        /// <param name="comparer">The <see cref="IComparer{T}" /> to compare values.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
        /// <returns>The minimum value in the sequence.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">No object in <paramref name="source" /> implements the <see cref="IComparable" /> or <see cref="IComparable{T}" /> interface.</exception>
        /// <remarks>
        /// <para>If type <typeparamref name="TSource" /> implements <see cref="IComparable{T}" />, the <see cref="MinAsync{TSource}(IAsyncEnumerable{TSource}, IComparer{TSource}?, CancellationToken)" /> method uses that implementation to compare values. Otherwise, if type <typeparamref name="TSource" /> implements <see cref="IComparable" />, that implementation is used to compare values.</para>
        /// <para>If <typeparamref name="TSource" /> is a reference type and the source sequence is empty or contains only values that are <see langword="null" />, this method returns <see langword="null" />.</para>
        /// </remarks>
        public static ValueTask<TSource?> MinAsync<TSource>(
            this IAsyncEnumerable<TSource> source,
            IComparer<TSource>? comparer,
            CancellationToken cancellationToken = default)
        {
            ThrowHelper.ThrowIfNull(source);

            return Impl(source, comparer ?? Comparer<TSource>.Default, cancellationToken);

            static async ValueTask<TSource?> Impl(IAsyncEnumerable<TSource> source, IComparer<TSource> comparer, CancellationToken cancellationToken)
            {
                TSource? value = default;
                IAsyncEnumerator<TSource> e = source.GetAsyncEnumerator(cancellationToken);
                try
                {
                    if (value is null)
                    {
                        do
                        {
                            if (!await e.MoveNextAsync().ConfigureAwait(false))
                            {
                                return value;
                            }

                            value = e.Current;
                        }
                        while (value is null);

                        while (await e.MoveNextAsync().ConfigureAwait(false))
                        {
                            TSource next = e.Current;
                            if (next is not null && comparer.Compare(next, value) < 0)
                            {
                                value = next;
                            }
                        }
                    }
                    else
                    {
                        if (!await e.MoveNextAsync().ConfigureAwait(false))
                        {
                            ThrowHelper.ThrowNoElementsException();
                        }

                        value = e.Current;
                        if (comparer == Comparer<TSource>.Default)
                        {
                            while (await e.MoveNextAsync().ConfigureAwait(false))
                            {
                                TSource next = e.Current;
                                if (Comparer<TSource>.Default.Compare(next, value) < 0)
                                {
                                    value = next;
                                }
                            }
                        }
                        else
                        {
                            while (await e.MoveNextAsync().ConfigureAwait(false))
                            {
                                TSource next = e.Current;
                                if (comparer.Compare(next, value) < 0)
                                {
                                    value = next;
                                }
                            }
                        }
                    }

                    return value;
                }
                finally
                {
                    await e.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        /// <summary>Invokes a transform function on each element of a generic sequence and returns the minimum resulting value.</summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TResult">The type of the value returned by selector.</typeparam>
        /// <param name="source">A sequence of values to determine the minimum value of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
        /// <returns>The minimum value in the sequence.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
        public static ValueTask<TResult?> MinAsync<TSource, TResult>(
            this IAsyncEnumerable<TSource> source,
            Func<TSource, TResult> selector,
            CancellationToken cancellationToken = default) =>
            source.Select(selector).MinAsync(null, cancellationToken);

        /// <summary>Invokes a transform function on each element of a generic sequence and returns the minimum resulting value.</summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TResult">The type of the value returned by selector.</typeparam>
        /// <param name="source">A sequence of values to determine the minimum value of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
        /// <returns>The minimum value in the sequence.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
        public static ValueTask<TResult?> MinAsync<TSource, TResult>(
            this IAsyncEnumerable<TSource> source,
            Func<TSource, CancellationToken, ValueTask<TResult>> selector,
            CancellationToken cancellationToken = default) =>
            source.Select(selector).MinAsync(null, cancellationToken);
    }
}
