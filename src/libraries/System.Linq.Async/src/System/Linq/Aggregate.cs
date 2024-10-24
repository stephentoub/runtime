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
        /// <summary>Applies an accumulator function over an asynchronous sequence.</summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">An <see cref="IAsyncEnumerable{TSource}"/> to aggregate over.</param>
        /// <param name="func">An accumulator function to be invoked on each element.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>The final accumulator value.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="func"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException"><paramref name="source"/> contains no elements.</exception>"
        public static ValueTask<TSource> AggregateAsync<TSource>(
            this IAsyncEnumerable<TSource> source,
            Func<TSource, TSource, ValueTask<TSource>> func,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(func);

            return Impl(source, func, cancellationToken);

            static async ValueTask<TSource> Impl(IAsyncEnumerable<TSource> source, Func<TSource, TSource, ValueTask<TSource>> func, CancellationToken cancellationToken)
            {
                ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = source.WithCancellation(cancellationToken).ConfigureAwait(false).GetAsyncEnumerator();
                try
                {
                    if (!await e.MoveNextAsync())
                    {
                        ThrowHelper.ThrowNoElementsException();
                    }

                    TSource result = e.Current;
                    while (await e.MoveNextAsync())
                    {
                        result = await func(result, e.Current).ConfigureAwait(false);
                    }

                    return result;
                }
                finally
                {
                    await e.DisposeAsync();
                }
            }
        }

        public static ValueTask<TAccumulate> AggregateAsync<TSource, TAccumulate>(
            this IAsyncEnumerable<TSource> source,
            TAccumulate seed,
            Func<TAccumulate, TSource, ValueTask<TAccumulate>> func,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(func);

            return Impl(source, seed, func, cancellationToken);

            static async ValueTask<TAccumulate> Impl(IAsyncEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, ValueTask<TAccumulate>> func, CancellationToken cancellationToken)
            {
                TAccumulate result = seed;
                await foreach (TSource element in source.WithCancellation(cancellationToken).ConfigureAwait(false))
                {
                    result = await func(result, element).ConfigureAwait(false);
                }

                return result;
            }
        }

        public static ValueTask<TResult> AggregateAsync<TSource, TAccumulate, TResult>(
            this IAsyncEnumerable<TSource> source,
            TAccumulate seed,
            Func<TAccumulate, TSource, ValueTask<TAccumulate>> func,
            Func<TAccumulate, ValueTask<TResult>> resultSelector,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(func);
            ArgumentNullException.ThrowIfNull(resultSelector);

            return Impl(source, seed, func, resultSelector, cancellationToken);

            static async ValueTask<TResult> Impl(
                IAsyncEnumerable<TSource> source,
                TAccumulate seed,
                Func<TAccumulate, TSource, ValueTask<TAccumulate>> func,
                Func<TAccumulate, ValueTask<TResult>> resultSelector,
                CancellationToken cancellationToken)
            {
                TAccumulate result = seed;
                await foreach (TSource element in source.WithCancellation(cancellationToken).ConfigureAwait(false))
                {
                    result = await func(result, element).ConfigureAwait(false);
                }

                return await resultSelector(result).ConfigureAwait(false);
            }
        }
    }
}
