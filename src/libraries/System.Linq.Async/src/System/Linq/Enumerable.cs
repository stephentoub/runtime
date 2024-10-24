// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace System.Linq
{
    public static partial class AsyncEnumerable
    {
        /// <summary>Returns an empty <see cref="IAsyncEnumerable{TResult}"/>.</summary>
        /// <typeparam name="TResult">The type to assign to the type parameter of the returned generic <see cref="IAsyncEnumerable{T}"/>.</typeparam>
        public static IAsyncEnumerable<TResult> Empty<TResult>() =>
            EmptyAsyncEnumerable<TResult>.Instance;

        /// <summary>Returns an <see cref="IAsyncEnumerable{TSource}"/> for the provided <see cref="IEnumerable{T}"/> <paramref name="source"/>.</summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The sequence to type as <see cref="IAsyncEnumerable{TSource}"/>.</param>
        /// <returns>An <see cref="IAsyncEnumerable{T}"/> for the source.</returns>
        public static IAsyncEnumerable<TSource> AsAsyncEnumerable<TSource>(this IEnumerable<TSource> source)
        {
            ArgumentNullException.ThrowIfNull(source);

            if (source is IAsyncEnumerable<TSource> ae)
            {
                return ae;
            }

            if (source is TSource[] { Length: 0 })
            {
                return EmptyAsyncEnumerable<TSource>.Instance;
            }

            return new EnumerableAsyncEnumerable<TSource>(source);
        }

        private class EnumerableAsyncEnumerable<TSource>(IEnumerable<TSource> source) : IAsyncEnumerable<TSource>
        {
            public IEnumerable<TSource> Source { get; } = source;

            async IAsyncEnumerator<TSource> IAsyncEnumerable<TSource>.GetAsyncEnumerator(CancellationToken cancellationToken)
            {
                foreach (TSource item in Source)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    yield return item;
                }
            }
        }

        /// <summary>An implementation for an empty <see cref="IAsyncEnumerable{T}"/>.</summary>
        /// <typeparam name="TSource">The type of the elements.</typeparam>
        /// <remarks>Intended to be used only via the singleton <see cref="Instance"/>.</remarks>
        private sealed class EmptyAsyncEnumerable<TSource> : EnumerableAsyncEnumerable<TSource>, IAsyncEnumerable<TSource>, IAsyncEnumerator<TSource>
        {
            private EmptyAsyncEnumerable() : base(Array.Empty<TSource>()) { }

            public static EmptyAsyncEnumerable<TSource> Instance { get; } = new();

            IAsyncEnumerator<TSource> IAsyncEnumerable<TSource>.GetAsyncEnumerator(CancellationToken cancellationToken) => this;

            ValueTask IAsyncDisposable.DisposeAsync() => ValueTask.CompletedTask;

            ValueTask<bool> IAsyncEnumerator<TSource>.MoveNextAsync() => ValueTask.FromResult(false);

            TSource IAsyncEnumerator<TSource>.Current => throw new NotSupportedException();
        }

        /// <summary>Gets whether the async enumerable is an empty array.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsEmptyArray<TSource>(IAsyncEnumerable<TSource> source) =>
            source is EmptyAsyncEnumerable<TSource>;
    }
}
