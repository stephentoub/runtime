// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.Generic;

namespace System.Linq
{
    public static partial class Enumerable
    {
        public static int Count<TSource>(this IEnumerable<TSource> source)
        {
            if (source is null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.source);
            }

            if (source is ICollection<TSource> collectionoft)
            {
                return collectionoft.Count;
            }

#if !OPTIMIZE_FOR_SIZE
            if (source is Iterator<TSource> iterator)
            {
                return iterator.GetCount(onlyIfCheap: false);
            }
#endif

            if (source is ICollection collection)
            {
                return collection.Count;
            }

            int count = 0;
            using (IEnumerator<TSource> e = source.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    checked { count++; }
                }
            }

            return count;
        }

        public static int Count<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            return source is IList<TSource> list ?
                CountList(list, predicate) :
                CountEnumerable(source, predicate);

            static int CountList(IList<TSource> list, Func<TSource, bool> predicate)
            {
                if (predicate is null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.predicate);
                }

                int count = 0;
                if (!TryGetSpan(list, out ReadOnlySpan<TSource> span))
                {
                    int listCount = list.Count;
                    for (int i = 0; i < listCount; i++)
                    {
                        if (predicate(list[i]))
                        {
                            count++;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < span.Length; i++)
                    {
                        if (predicate(span[i]))
                        {
                            count++;
                        }
                    }
                }

                return count;
            }

            static int CountEnumerable(IEnumerable<TSource> source, Func<TSource, bool> predicate)
            {
                if (source is null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.source);
                }

                if (predicate is null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.predicate);
                }

                int count = 0;
                foreach (TSource element in source)
                {
                    if (predicate(element))
                    {
                        checked { count++; }
                    }
                }

                return count;
            }
        }

        /// <summary>
        ///   Attempts to determine the number of elements in a sequence without forcing an enumeration.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence that contains elements to be counted.</param>
        /// <param name="count">
        ///     When this method returns, contains the count of <paramref name="source" /> if successful,
        ///     or zero if the method failed to determine the count.</param>
        /// <returns>
        ///   <see langword="true" /> if the count of <paramref name="source"/> can be determined without enumeration;
        ///   otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///   The method performs a series of type tests, identifying common subtypes whose
        ///   count can be determined without enumerating; this includes <see cref="ICollection{T}"/>,
        ///   <see cref="ICollection"/> as well as internal types used in the LINQ implementation.
        ///
        ///   The method is typically a constant-time operation, but ultimately this depends on the complexity
        ///   characteristics of the underlying collection implementation.
        /// </remarks>
        public static bool TryGetNonEnumeratedCount<TSource>(this IEnumerable<TSource> source, out int count)
        {
            if (source is null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.source);
            }

            if (source is ICollection<TSource> collectionoft)
            {
                count = collectionoft.Count;
                return true;
            }

#if !OPTIMIZE_FOR_SIZE
            if (source is Iterator<TSource> iterator)
            {
                int c = iterator.GetCount(onlyIfCheap: true);
                if (c >= 0)
                {
                    count = c;
                    return true;
                }
            }
#endif

            if (source is ICollection collection)
            {
                count = collection.Count;
                return true;
            }

            count = 0;
            return false;
        }

        public static long LongCount<TSource>(this IEnumerable<TSource> source)
        {
            if (TryGetNonEnumeratedCount(source, out int int32Count))
            {
                return int32Count;
            }

            long count = 0;
            using (IEnumerator<TSource> e = source.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    checked { count++; }
                }
            }

            return count;
        }

        public static long LongCount<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source is null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.source);
            }

            if (predicate is null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.predicate);
            }

            long count = 0;
            if (source is IList<TSource> list)
            {
                if (!TryGetSpan(list, out ReadOnlySpan<TSource> span))
                {
                    int listCount = list.Count;
                    for (int i = 0; i < listCount; i++)
                    {
                        if (predicate(list[i]))
                        {
                            count++;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < span.Length; i++)
                    {
                        if (predicate(span[i]))
                        {
                            count++;
                        }
                    }
                }
            }
            else
            {
                foreach (TSource element in source)
                {
                    if (predicate(element))
                    {
                        checked { count++; }
                    }
                }
            }

            return count;
        }
    }
}
