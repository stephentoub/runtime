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
        /// <summary>
        /// Filters the elements of a <see cref="IAsyncEnumerable{TSource}"/> based on a specified type <typeparamref name="TResult"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source.</typeparam>
        /// <typeparam name="TResult">The type to filter the elements of the sequence on.</typeparam>
        /// <param name="source">The <see cref="IAsyncEnumerable{T}"/> whose elements to filter.</param>
        /// <returns>An <see cref="IAsyncEnumerable{T}"/> that contains elements from the input sequence of type <typeparamref name="TResult"/>.</returns>
        public static IAsyncEnumerable<TResult> OfType<TSource, TResult>(
            this IAsyncEnumerable<TSource> source)
        {
            ThrowHelper.ThrowIfNull(source);

            return Impl(source, default);

            static async IAsyncEnumerable<TResult> Impl(
                IAsyncEnumerable<TSource> source,
                [EnumeratorCancellation] CancellationToken cancellationToken)
            {
                await foreach (TSource item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
                {
                    if (item is TResult target)
                    {
                        yield return target;
                    }
                }
            }
        }
    }
}
