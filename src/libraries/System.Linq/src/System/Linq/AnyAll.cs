// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.Generic;

namespace System.Linq
{
    public static partial class Enumerable
    {
        public static bool Any<TSource>(this IEnumerable<TSource> source)
        {
            if (source is null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.source);
            }

            if (source is ICollection<TSource> gc)
            {
                return gc.Count != 0;
            }

#if !OPTIMIZE_FOR_SIZE
            if (source is Iterator<TSource> iterator)
            {
                int count = iterator.GetCount(onlyIfCheap: true);
                if (count >= 0)
                {
                    return count != 0;
                }

                iterator.TryGetFirst(out bool found);
                return found;
            }
#endif

            if (source is ICollection ngc)
            {
                return ngc.Count != 0;
            }

            using IEnumerator<TSource> e = source.GetEnumerator();
            return e.MoveNext();
        }

        public static bool Any<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            return source is IList<TSource> list ?
                AnyList(list, predicate) :
                AnyEnumerable(source, predicate);

            static bool AnyList(IList<TSource> list, Func<TSource, bool> predicate)
            {
                if (predicate is null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.predicate);
                }

                if (!TryGetSpan(list, out ReadOnlySpan<TSource> span))
                {
                    int count = list.Count;
                    for (int i = 0; i < count; i++)
                    {
                        if (predicate(list[i]))
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < span.Length; i++)
                    {
                        if (predicate(span[i]))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            static bool AnyEnumerable(IEnumerable<TSource> source, Func<TSource, bool> predicate)
            {
                if (source is null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.source);
                }

                if (predicate is null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.predicate);
                }

                foreach (TSource element in source)
                {
                    if (predicate(element))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public static bool All<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            return source is IList<TSource> list ?
                AllList(list, predicate) :
                AllEnumerable(source, predicate);

            static bool AllList(IList<TSource> list, Func<TSource, bool> predicate)
            {
                if (predicate is null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.predicate);
                }

                if (!TryGetSpan(list, out ReadOnlySpan<TSource> span))
                {
                    int count = list.Count;
                    for (int i = 0; i < count; i++)
                    {
                        if (!predicate(list[i]))
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < span.Length; i++)
                    {
                        if (!predicate(span[i]))
                        {
                            return false;
                        }
                    }
                }

                return true;
            }

            static bool AllEnumerable(IEnumerable<TSource> source, Func<TSource, bool> predicate)
            {
                if (source is null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.source);
                }

                if (predicate is null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.predicate);
                }

                foreach (TSource element in source)
                {
                    if (!predicate(element))
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }
}
