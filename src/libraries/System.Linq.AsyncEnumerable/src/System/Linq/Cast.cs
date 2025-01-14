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
        /// Casts the elements of an <see cref="IAsyncEnumerable{T}"/> to the specified type.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source.</typeparam>
        /// <typeparam name="TResult">The type to cast the elements of source to.</typeparam>
        /// <param name="source">The <see cref="IAsyncEnumerable{TSource}"/> that contains the elements to be cast to type <typeparamref name="TResult"/>.</param>
        /// <returns>An <see cref="IAsyncEnumerable{TResult}"/> that contains each element of the source sequence cast to the <typeparamref name="TResult"/> type.</returns>
        public static IAsyncEnumerable<TResult> Cast<TSource, TResult>(
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
                    yield return (TResult)(object)item!;
                }
            }
        }
    }
}
