// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

// TODO: Compiler currently fails when putting XML comment on extension type
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace System.Linq
{
    public static partial class AsyncEnumerable
    {
        extension<TSource>(IAsyncEnumerable<TSource> source)
        {
            /// <summary>
            /// Casts the elements of an <see cref="IAsyncEnumerable{TSource}"/> to the specified <typeparamref name="TResult"/> type.
            /// </summary>
            /// <typeparam name="TResult">The type to cast the elements of the source to.</typeparam>
            /// <returns>An <see cref="IAsyncEnumerable{TResult}"/> that contains each element of the source sequence cast to the <typeparamref name="TResult"/> type.</returns>
            public IAsyncEnumerable<
#nullable disable // there's no way to annotate the connection of the nullability of TResult to that of TSource
                TResult
#nullable restore
                > Cast<TResult>() // satisfies the C# query-expression pattern
            {
                ArgumentNullException.ThrowIfNull(source);

                return
                    source.IsKnownEmpty() ? Empty<TResult>() :
                    source as IAsyncEnumerable<TResult> ??
                    Impl(source, default);

                static async IAsyncEnumerable<TResult> Impl(
                    IAsyncEnumerable<TSource> source,
                    [EnumeratorCancellation] CancellationToken cancellationToken)
                {
                    await foreach (TSource item in source.WithCancellation(cancellationToken))
                    {
                        yield return (TResult)(object)item!;
                    }
                }
            }
        }
    }
}
