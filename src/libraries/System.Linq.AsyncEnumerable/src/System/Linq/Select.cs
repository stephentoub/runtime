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
        /// <summary>Projects each element of a sequence into a new form.</summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TResult">The type of the value returned by selector.</typeparam>
        /// <param name="source">A sequence of values to invoke a transform function on.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>
        /// An <see cref="IAsyncEnumerable{T}"/> whose elements are the result of
        /// invoking the transform function on each element of source.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="selector"/> is <see langword="null"/>.</exception>
        public static IAsyncEnumerable<TResult> Select<TSource, TResult>( // satisfies the C# query-expression pattern
            this IAsyncEnumerable<TSource> source,
            Func<TSource, TResult> selector)
        {
            ThrowHelper.ThrowIfNull(source);
            ThrowHelper.ThrowIfNull(selector);

            return
                source.IsKnownEmpty() ? Empty<TResult>() :
                source is Iterator<TSource> iterator ? iterator.Select(source, selector) :
                new SelectIterator<TSource, TResult>(source, selector);
        }

        private sealed class SelectIterator<TSource, TResult>(IAsyncEnumerable<TSource> source, Func<TSource, TResult> selector) : Iterator<TResult>
        {
            private readonly IAsyncEnumerable<TSource> _source = source;
            private readonly Func<TSource, TResult> _selector = selector;

            public override async IAsyncEnumerator<TResult> GetAsyncEnumerator(CancellationToken cancellationToken)
            {
                await foreach (TSource element in _source.WithCancellation(cancellationToken).ConfigureAwait(false))
                {
                    yield return _selector(element);
                }
            }
        }

        /// <summary>Projects each element of a sequence into a new form.</summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TResult">The type of the value returned by selector.</typeparam>
        /// <param name="source">A sequence of values to invoke a transform function on.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
        /// <returns>
        /// An <see cref="IAsyncEnumerable{T}"/> whose elements are the result of
        /// invoking the transform function on each element of source.
        /// </returns>
        private static async IAsyncEnumerable<TResult> SelectEnumerable<TSource, TResult>(
            IAsyncEnumerable<TSource> source,
            Func<TSource, TResult> selector,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (TSource element in source.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                yield return selector(element);
            }
        }

        /// <summary>Projects each element of a sequence into a new form.</summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TResult">The type of the value returned by selector.</typeparam>
        /// <param name="source">A sequence of values to invoke a transform function on.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>
        /// An <see cref="IAsyncEnumerable{T}"/> whose elements are the result of
        /// invoking the transform function on each element of source.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="selector"/> is <see langword="null"/>.</exception>
        public static IAsyncEnumerable<TResult> Select<TSource, TResult>(
            this IAsyncEnumerable<TSource> source,
            Func<TSource, CancellationToken, ValueTask<TResult>> selector)
        {
            ThrowHelper.ThrowIfNull(source);
            ThrowHelper.ThrowIfNull(selector);

            return
                source.IsKnownEmpty() ? Empty<TResult>() :
                Impl(source, selector, default);

            static async IAsyncEnumerable<TResult> Impl(
                IAsyncEnumerable<TSource> source,
                Func<TSource, CancellationToken, ValueTask<TResult>> selector,
                [EnumeratorCancellation] CancellationToken cancellationToken)
            {
                await foreach (TSource element in source.WithCancellation(cancellationToken).ConfigureAwait(false))
                {
                    yield return await selector(element, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        /// <summary>Projects each element of a sequence into a new form by incorporating the element's index.</summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TResult">The type of the value returned by selector.</typeparam>
        /// <param name="source">A sequence of values to invoke a transform function on.</param>
        /// <param name="selector">
        /// A transform function to apply to each element; the second parameter of
        /// the function represents the index of the source element.
        /// </param>
        /// <returns>
        /// An <see cref="IAsyncEnumerable{T}"/> whose elements are the result of
        /// invoking the transform function on each element of source.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="selector"/> is <see langword="null"/>.</exception>
        public static IAsyncEnumerable<TResult> Select<TSource, TResult>(
            this IAsyncEnumerable<TSource> source,
            Func<TSource, int, TResult> selector)
        {
            ThrowHelper.ThrowIfNull(source);
            ThrowHelper.ThrowIfNull(selector);

            return
                source.IsKnownEmpty() ? Empty<TResult>() :
                Impl(source, selector, default);

            static async IAsyncEnumerable<TResult> Impl(
                IAsyncEnumerable<TSource> source,
                Func<TSource, int, TResult> selector,
                [EnumeratorCancellation] CancellationToken cancellationToken)
            {
                int index = -1;
                await foreach (TSource element in source.WithCancellation(cancellationToken).ConfigureAwait(false))
                {
                    yield return selector(element, checked(++index));
                }
            }
        }

        /// <summary>Projects each element of a sequence into a new form by incorporating the element's index.</summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TResult">The type of the value returned by selector.</typeparam>
        /// <param name="source">A sequence of values to invoke a transform function on.</param>
        /// <param name="selector">
        /// A transform function to apply to each element; the second parameter of
        /// the function represents the index of the source element.
        /// </param>
        /// <returns>
        /// An <see cref="IAsyncEnumerable{T}"/> whose elements are the result of
        /// invoking the transform function on each element of source.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="selector"/> is <see langword="null"/>.</exception>
        public static IAsyncEnumerable<TResult> Select<TSource, TResult>(
            this IAsyncEnumerable<TSource> source,
            Func<TSource, int, CancellationToken, ValueTask<TResult>> selector)
        {
            ThrowHelper.ThrowIfNull(source);
            ThrowHelper.ThrowIfNull(selector);

            return
                source.IsKnownEmpty() ? Empty<TResult>() :
                Impl(source, selector, default);

            static async IAsyncEnumerable<TResult> Impl(
                IAsyncEnumerable<TSource> source,
                Func<TSource, int, CancellationToken, ValueTask<TResult>> selector,
                [EnumeratorCancellation] CancellationToken cancellationToken)
            {
                int index = -1;
                await foreach (TSource element in source.WithCancellation(cancellationToken).ConfigureAwait(false))
                {
                    yield return await selector(element, checked(++index), cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }
}
