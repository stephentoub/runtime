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
            // NOTE:
            // In System.Linq, the base Iterator class is used both to flow information between query operators
            // and to implement hand-rolled iterator state machines. The latter is complicated, and is even more
            // complicated for IAsyncEnumerator. Here for AsyncEnumerable, we focus only on the former. Operators
            // that want to employ such specialization derive a type from this class in order to add state, but
            // then implement the state machine by overriding GetEnumerator using yield. This is much simpler, but
            // it does have the downside of incurring an extra allocation. By default, the C# compiler will implement
            // an iterator method that returns an IEnumerable by reusing the same state machine class for the both
            // the IEnumerable and the first IEnumerator it produces. By not using an iterator method for producing
            // the IAsyncEnumerable here, we miss out on that optimization. Thus, we only do this when the gains are
            // so significant that it generally significantly outweighs the cost of the extra allocation. And that
            // primarily happens when we can change the algorithmic complexity of the operation, rather than just
            // shaving off a few cycles here and there.

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
