// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq
{
    public static partial class AsyncEnumerable
    {
        /// <summary>Provides a base class for iterators that are able to specialize handling of certain subsequent operators.</summary>
        private abstract class Iterator<TSource> : IAsyncEnumerable<TSource>
        {
            public virtual IAsyncEnumerable<TResult> Select<TResult>(
                IAsyncEnumerable<TSource> source,
                Func<TSource, TResult> selector) =>
                SelectEnumerable(source, selector, default);

            public virtual IAsyncEnumerable<TSource> Where(
                IAsyncEnumerable<TSource> source,
                Func<TSource, bool> predicate) =>
                WhereIterator(source, predicate, default);

            public abstract IAsyncEnumerator<TSource> GetAsyncEnumerator(CancellationToken cancellationToken = default);

            public abstract ValueTask<TSource[]> ToArrayAsync(CancellationToken cancellationToken);

            public abstract ValueTask<List<TSource>> ToListAsync(CancellationToken cancellationToken);

            public abstract ValueTask<int> CountAsync(CancellationToken cancellationToken);

            public abstract ValueTask<bool> AnyAsync(CancellationToken cancellationToken);

            public abstract ValueTask<TSource> FirstAsync(CancellationToken cancellationToken);

            public abstract ValueTask<TSource?> FirstOrDefaultAsync(CancellationToken cancellationToken);

            public abstract ValueTask<TSource> LastAsync(CancellationToken cancellationToken);

            public abstract ValueTask<TSource?> LastOrDefaultAsync(CancellationToken cancellationToken);

            public abstract ValueTask<TSource> ElementAtAsync(int index, CancellationToken cancellationToken);

            public abstract ValueTask<TSource?> ElementAtOrDefaultAsync(int index, CancellationToken cancellationToken);

            public abstract ValueTask<bool> ContainsAsync(TSource value, CancellationToken cancellationToken);

            ///// <summary>Creates a new iterator that takes the specified number of elements from this sequence.</summary>
            //public virtual Iterator<TSource>? Take(int count) => new IEnumerableSkipTakeIterator<TSource>(this, 0, count - 1);
        }
    }
}
