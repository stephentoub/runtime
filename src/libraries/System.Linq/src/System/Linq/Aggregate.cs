// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;

namespace System.Linq
{
    public static partial class Enumerable
    {
        public static TSource Aggregate<TSource>(this IEnumerable<TSource> source, Func<TSource, TSource, TSource> func)
        {
            return source is not IList<TSource> list ?
                AggregateEnumerable(source, func) :
                AggregateList(list, func);

            static TSource AggregateList(IList<TSource> list, Func<TSource, TSource, TSource> func)
            {
                if (func is null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.func);
                }

                TSource result;
                if (!TryGetSpan(list, out ReadOnlySpan<TSource> span))
                {
                    int count = list.Count;
                    if (count == 0)
                    {
                        ThrowHelper.ThrowNoElementsException();
                    }

                    result = list[0];
                    for (int i = 1; i < count; i++)
                    {
                        result = func(result, list[i]);
                    }
                }
                else
                {
                    if (span.IsEmpty)
                    {
                        ThrowHelper.ThrowNoElementsException();
                    }

                    result = span[0];
                    for (int i = 1; i < span.Length; i++)
                    {
                        result = func(result, span[i]);
                    }
                }

                return result;
            }

            static TSource AggregateEnumerable(IEnumerable<TSource> source, Func<TSource, TSource, TSource> func)
            {
                if (source is null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.source);
                }

                if (func is null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.func);
                }

                using (IEnumerator<TSource> e = source.GetEnumerator())
                {
                    if (!e.MoveNext())
                    {
                        ThrowHelper.ThrowNoElementsException();
                    }

                    TSource result = e.Current;
                    while (e.MoveNext())
                    {
                        result = func(result, e.Current);
                    }

                    return result;
                }
            }
        }

        public static TAccumulate Aggregate<TSource, TAccumulate>(this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func) =>
            source is IList<TSource> list ?
                AggregateList(list, seed, func) :
                AggregateEnumerable(source, seed, func);

        public static TResult Aggregate<TSource, TAccumulate, TResult>(this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
        {
            if (resultSelector is null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.resultSelector);
            }

            return resultSelector(Aggregate(source, seed, func));
        }

        private static TAccumulate AggregateList<TSource, TAccumulate>(IList<TSource> list, TAccumulate result, Func<TAccumulate, TSource, TAccumulate> func)
        {
            if (func is null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.func);
            }

            if (!TryGetSpan(list, out ReadOnlySpan<TSource> span))
            {
                int count = list.Count;
                for (int i = 0; i < count; i++)
                {
                    result = func(result, list[i]);
                }
            }
            else
            {
                for (int i = 0; i < span.Length; i++)
                {
                    result = func(result, span[i]);
                }
            }

            return result;
        }

        private static TAccumulate AggregateEnumerable<TSource, TAccumulate>(IEnumerable<TSource> source, TAccumulate result, Func<TAccumulate, TSource, TAccumulate> func)
        {
            if (source is null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.source);
            }

            if (func is null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.func);
            }

            foreach (TSource element in source)
            {
                result = func(result, element);
            }

            return result;
        }
    }
}
