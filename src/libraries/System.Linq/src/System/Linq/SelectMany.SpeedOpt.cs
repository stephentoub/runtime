// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;

namespace System.Linq
{
    public static partial class Enumerable
    {
        private sealed partial class SelectManySingleSelectorIterator<TSource, TResult>
        {
            public override int GetCount(bool onlyIfCheap)
            {
                if (onlyIfCheap)
                {
                    return -1;
                }

                int count = 0;
                Func<TSource, IEnumerable<TResult>> selector = _selector;

                if (_source is not IList<TSource> list)
                {
                    foreach (TSource item in _source)
                    {
                        checked { count += selector(item).Count(); }
                    }
                }
                else if (!TryGetSpan(list, out ReadOnlySpan<TSource> span))
                {
                    int listCount = list.Count;
                    for (int i = 0; i < listCount; i++)
                    {
                        checked { count += selector(list[i]).Count(); }
                    }
                }
                else
                {
                    for (int i = 0; i < span.Length; i++)
                    {
                        checked { count += selector(span[i]).Count(); }
                    }
                }

                return count;
            }

            public override TResult[] ToArray()
            {
                SegmentedArrayBuilder<TResult>.ScratchBuffer scratch = default;
                SegmentedArrayBuilder<TResult> builder = new(scratch);

                Func<TSource, IEnumerable<TResult>> selector = _selector;

                if (_source is not IList<TSource> list)
                {
                    foreach (TSource item in _source)
                    {
                        builder.AddRange(selector(item));
                    }
                }
                else if (!TryGetSpan(list, out ReadOnlySpan<TSource> span))
                {
                    int count = list.Count;
                    for (int i = 0; i < count; i++)
                    {
                        builder.AddRange(selector(list[i]));
                    }
                }
                else
                {
                    for (int i = 0; i < span.Length; i++)
                    {
                        builder.AddRange(selector(span[i]));
                    }
                }

                TResult[] result = builder.ToArray();
                builder.Dispose();

                return result;
            }

            public override List<TResult> ToList()
            {
                var result = new List<TResult>();

                Func<TSource, IEnumerable<TResult>> selector = _selector;

                if (_source is not IList<TSource> list)
                {
                    foreach (TSource item in _source)
                    {
                        result.AddRange(selector(item));
                    }
                }
                else if (!TryGetSpan(list, out ReadOnlySpan<TSource> span))
                {
                    int count = list.Count;
                    for (int i = 0; i < count; i++)
                    {
                        result.AddRange(selector(list[i]));
                    }
                }
                else
                {
                    foreach (TSource item in span)
                    {
                        result.AddRange(selector(item));
                    }
                }

                return result;
            }
        }
    }
}
