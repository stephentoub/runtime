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
        /// <summary>Provides extensions for <see cref="IAsyncEnumerable{TSource}"/>.</summary>
        /// <param name="source">The <see cref="IAsyncEnumerable{Object}"/> that contains the elements to be cast to type <typeparamref name="TResult"/>.</param>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        extension<TSource>(IAsyncEnumerable<TSource> source)
        {
            /// <summary>
            /// Casts the elements of an <see cref="IAsyncEnumerable{Object}"/> to the specified type.
            /// </summary>
            /// <typeparam name="TResult">The type to cast the elements of source to.</typeparam>
            /// <returns>An <see cref="IAsyncEnumerable{TResult}"/> that contains each element of the source sequence cast to the <typeparamref name="TResult"/> type.</returns>
            public IAsyncEnumerable<TResult> Cast<TResult>() // satisfies the C# query-expression pattern
            {
                ThrowHelper.ThrowIfNull(source);

                return
                    source.IsKnownEmpty() ? Empty<TResult>() :
                    source as IAsyncEnumerable<TResult> ??
                    Impl(source, default);

                static async IAsyncEnumerable<TResult> Impl(
                    IAsyncEnumerable<TSource?> source,
                    [EnumeratorCancellation] CancellationToken cancellationToken)
                {
                    await foreach (object? item in source.WithCancellation(cancellationToken))
                    {
                        yield return (TResult)item!;
                    }
                }
            }
        }
    }
}
