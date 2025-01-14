// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq
{
    public static partial class AsyncEnumerable
    {
        /// <summary>Creates a new <see cref="IObservable{T}"/> with the data from the <see cref="IAsyncEnumerable{T}"/>.</summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">An <see cref="IAsyncEnumerable{T}"/> of the elements to enumerate.</param>
        /// <returns>An <see cref="IObservable{T}"/> that pushes the data from the <paramref name="source"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// Each <see cref="IObserver{T}"/> subscribed to the observer will iterate through the <paramref name="source"/>.
        /// </remarks>
        public static IObservable<TSource> ToObservable<TSource>(
            this IAsyncEnumerable<TSource> source)
        {
            ThrowHelper.ThrowIfNull(source);

            return new AsyncEnumerableObservable<TSource>(source);
        }

        private sealed class AsyncEnumerableObservable<TSource>(IAsyncEnumerable<TSource> source) : IObservable<TSource>
        {
            public IDisposable Subscribe(IObserver<TSource> observer)
            {
                CancellationTokenDisposable d = new();
                _ = Impl(source, observer, d.Token); // fire and forget, invoked synchronously
                return d;

                static async Task Impl(
                    IAsyncEnumerable<TSource> source,
                    IObserver<TSource> observer,
                    CancellationToken cancellationToken)
                {
                    try
                    {
                        await foreach (TSource element in source.WithCancellation(cancellationToken).ConfigureAwait(false))
                        {
                            observer.OnNext(element);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex is not OperationCanceledException ||
                            !cancellationToken.IsCancellationRequested)
                        {
                            observer.OnError(ex);
                            return;
                        }
                    }

                    observer.OnCompleted();
                }
            }

            private sealed class CancellationTokenDisposable : IDisposable
            {
                private readonly CancellationTokenSource _cts = new();

                public CancellationToken Token => _cts.Token;

                public void Dispose() => _cts.Cancel();
            }
        }
    }
}
