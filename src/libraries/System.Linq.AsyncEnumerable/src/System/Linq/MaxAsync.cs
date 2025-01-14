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
        /// <summary>Returns the maximum value in a sequence of values.</summary>
        /// <param name="source">A sequence of values to determine the maximum value of.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
        /// <returns>The maximum value in the sequence.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
        public static ValueTask<int> MaxAsync(
            this IAsyncEnumerable<int> source,
            CancellationToken cancellationToken = default)
        {
            ThrowHelper.ThrowIfNull(source);

            return Impl(source, cancellationToken);

            static async ValueTask<int> Impl(
                IAsyncEnumerable<int> source,
                CancellationToken cancellationToken)
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
                        if (x > value)
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

        /// <summary>Returns the maximum value in a sequence of values.</summary>
        /// <param name="source">A sequence of values to determine the maximum value of.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
        /// <returns>The maximum value in the sequence.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
        public static ValueTask<long> MaxAsync(
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
                        if (x > value)
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

        /// <summary>Returns the maximum value in a sequence of values.</summary>
        /// <param name="source">A sequence of values to determine the maximum value of.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
        /// <returns>The maximum value in the sequence.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
        public static ValueTask<float> MaxAsync(
            this IAsyncEnumerable<float> source,
            CancellationToken cancellationToken = default)
        {
            ThrowHelper.ThrowIfNull(source);

            return Impl(source, cancellationToken);

            static async ValueTask<float> Impl(
                IAsyncEnumerable<float> source,
                CancellationToken cancellationToken)
            {
                float value;

                IAsyncEnumerator<float> e = source.GetAsyncEnumerator(cancellationToken);
                try
                {
                    if (!await e.MoveNextAsync().ConfigureAwait(false))
                    {
                        ThrowHelper.ThrowNoElementsException();
                    }

                    // NaN is ordered less than all other values. We need to do explicit checks to ensure this,
                    // but once we've found a value that is not NaN we need no longer worry about it,
                    // so first loop until such a value is found (or not, as the case may be).
                    value = e.Current;
                    while (float.IsNaN(value))
                    {
                        if (!await e.MoveNextAsync().ConfigureAwait(false))
                        {
                            return value;
                        }

                        value = e.Current;
                    }

                    while (await e.MoveNextAsync().ConfigureAwait(false))
                    {
                        float x = e.Current;
                        if (x > value)
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

        /// <summary>Returns the maximum value in a sequence of values.</summary>
        /// <param name="source">A sequence of values to determine the maximum value of.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
        /// <returns>The maximum value in the sequence.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
        public static ValueTask<double> MaxAsync(
            this IAsyncEnumerable<double> source,
            CancellationToken cancellationToken = default)
        {
            ThrowHelper.ThrowIfNull(source);

            return Impl(source, cancellationToken);

            static async ValueTask<double> Impl(
                IAsyncEnumerable<double> source,
                CancellationToken cancellationToken)
            {
                double value;

                IAsyncEnumerator<double> e = source.GetAsyncEnumerator(cancellationToken);
                try
                {
                    if (!await e.MoveNextAsync().ConfigureAwait(false))
                    {
                        ThrowHelper.ThrowNoElementsException();
                    }

                    // NaN is ordered less than all other values. We need to do explicit checks to ensure this,
                    // but once we've found a value that is not NaN we need no longer worry about it,
                    // so first loop until such a value is found (or not, as the case may be).
                    value = e.Current;
                    while (double.IsNaN(value))
                    {
                        if (!await e.MoveNextAsync().ConfigureAwait(false))
                        {
                            return value;
                        }

                        value = e.Current;
                    }

                    while (await e.MoveNextAsync().ConfigureAwait(false))
                    {
                        double x = e.Current;
                        if (x > value)
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

        /// <summary>Returns the maximum value in a sequence of values.</summary>
        /// <param name="source">A sequence of values to determine the maximum value of.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
        /// <returns>The maximum value in the sequence.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
        public static ValueTask<decimal> MaxAsync(
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
                        if (x > value)
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

        /// <summary>Returns the maximum value in a sequence of nullable values.</summary>
        /// <param name="source">A sequence of nullable values to determine the maximum value of.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
        /// <returns>The maximum value in the sequence.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
        public static ValueTask<int?> MaxAsync(
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
                    if (value is null || x > value)
                    {
                        value = x;
                    }
                }

                return value;
            }
        }

        /// <summary>Returns the maximum value in a sequence of nullable values.</summary>
        /// <param name="source">A sequence of nullable values to determine the maximum value of.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
        /// <returns>The maximum value in the sequence.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
        public static ValueTask<long?> MaxAsync(
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
                    if (value is null || x > value)
                    {
                        value = x;
                    }
                }

                return value;
            }
        }

        /// <summary>Returns the maximum value in a sequence of nullable values.</summary>
        /// <param name="source">A sequence of nullable values to determine the maximum value of.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
        /// <returns>The maximum value in the sequence.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
        public static ValueTask<float?> MaxAsync(
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

                    if (value is null || x > value || float.IsNaN(value.GetValueOrDefault()))
                    {
                        value = x;
                    }
                }

                return value;
            }
        }

        /// <summary>Returns the maximum value in a sequence of nullable values.</summary>
        /// <param name="source">A sequence of nullable values to determine the maximum value of.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
        /// <returns>The maximum value in the sequence.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
        public static ValueTask<double?> MaxAsync(
            this IAsyncEnumerable<double?> source,
            CancellationToken cancellationToken = default)
        {
            ThrowHelper.ThrowIfNull(source);

            return Impl(source.WithCancellation(cancellationToken).ConfigureAwait(false));

            static async ValueTask<double?> Impl(
                ConfiguredCancelableAsyncEnumerable<double?> source)
            {
                float? value = null;
                await foreach (float? x in source)
                {
                    if (x is null)
                    {
                        continue;
                    }

                    if (value is null || x > value || float.IsNaN((float)value))
                    {
                        value = x;
                    }
                }

                return value;
            }
        }

        /// <summary>Returns the maximum value in a sequence of nullable values.</summary>
        /// <param name="source">A sequence of nullable values to determine the maximum value of.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
        /// <returns>The maximum value in the sequence.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
        public static ValueTask<decimal?> MaxAsync(
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
                    if (value is null || x > value)
                    {
                        value = x;
                    }
                }

                return value;
            }
        }

        /// <summary>Returns the maximum value in a generic sequence.</summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values to determine the maximum value of.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
        /// <returns>The maximum value in the sequence.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">No object in <paramref name="source" /> implements the <see cref="IComparable" /> or <see cref="IComparable{T}" /> interface (via the returned task).</exception>
        /// <remarks>
        /// <para>If type <typeparamref name="TSource" /> implements <see cref="IComparable{T}" />, the <see cref="MaxAsync{TSource}(IAsyncEnumerable{TSource}, IComparer{TSource}?, CancellationToken)" /> method uses that implementation to compare values. Otherwise, if type <typeparamref name="TSource" /> implements <see cref="IComparable" />, that implementation is used to compare values.</para>
        /// <para>If <typeparamref name="TSource" /> is a reference type and the source sequence is empty or contains only values that are <see langword="null" />, this method returns <see langword="null" />.</para>
        /// </remarks>
        public static ValueTask<TSource?> MaxAsync<TSource>(
            this IAsyncEnumerable<TSource> source,
            CancellationToken cancellationToken = default) =>
            MaxAsync(source, (IComparer<TSource>?)null, cancellationToken);

        /// <summary>Returns the maximum value in a generic sequence.</summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values to determine the maximum value of.</param>
        /// <param name="comparer">The <see cref="IComparer{T}" /> to compare values.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
        /// <returns>The maximum value in the sequence.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">No object in <paramref name="source" /> implements the <see cref="IComparable" /> or <see cref="IComparable{T}" /> interface (via the returned task).</exception>
        /// <remarks>
        /// <para>If type <typeparamref name="TSource" /> implements <see cref="IComparable{T}" />, the <see cref="MaxAsync{TSource}(IAsyncEnumerable{TSource}, IComparer{TSource}?, CancellationToken)" /> method uses that implementation to compare values. Otherwise, if type <typeparamref name="TSource" /> implements <see cref="IComparable" />, that implementation is used to compare values.</para>
        /// <para>If <typeparamref name="TSource" /> is a reference type and the source sequence is empty or contains only values that are <see langword="null" />, this method returns <see langword="null" />.</para>
        /// </remarks>
        public static ValueTask<TSource?> MaxAsync<TSource>(
            this IAsyncEnumerable<TSource> source,
            IComparer<TSource>? comparer,
            CancellationToken cancellationToken = default)
        {
            ThrowHelper.ThrowIfNull(source);

            return Impl(source, comparer ?? Comparer<TSource>.Default, cancellationToken);

            static async ValueTask<TSource?> Impl(
                IAsyncEnumerable<TSource> source,
                IComparer<TSource> comparer,
                CancellationToken cancellationToken)
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
                            if (next is not null && comparer.Compare(next, value) > 0)
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
                                if (Comparer<TSource>.Default.Compare(next, value) > 0)
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
                                if (comparer.Compare(next, value) > 0)
                                {
                                    value = next;
                                }
                            }
                        }
                    }
                }
                finally
                {
                    await e.DisposeAsync().ConfigureAwait(false);
                }

                return value;
            }
        }

        /// <summary>Invokes a transform function on each element of a generic sequence and returns the maximum resulting value.</summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TResult">The type of the value returned by selector.</typeparam>
        /// <param name="source">A sequence of values to determine the maximum value of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
        /// <returns>The maximum value in the sequence.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
        public static ValueTask<TResult?> MaxAsync<TSource, TResult>(
            this IAsyncEnumerable<TSource> source,
            Func<TSource, TResult> selector,
            CancellationToken cancellationToken = default) =>
            source.Select(selector).MaxAsync(null, cancellationToken);

        /// <summary>Invokes a transform function on each element of a generic sequence and returns the maximum resulting value.</summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TResult">The type of the value returned by selector.</typeparam>
        /// <param name="source">A sequence of values to determine the maximum value of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
        /// <returns>The maximum value in the sequence.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
        public static ValueTask<TResult?> MaxAsync<TSource, TResult>(
            this IAsyncEnumerable<TSource> source,
            Func<TSource, CancellationToken, ValueTask<TResult>> selector,
            CancellationToken cancellationToken = default) =>
            source.Select(selector).MaxAsync(null, cancellationToken);
    }
}
