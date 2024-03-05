// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;

namespace System.Linq
{
    public static partial class Enumerable
    {
        public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value) =>
            source is ICollection<TSource> collection ? collection.Contains(value) :
            Contains(source, value, null);

        public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value, IEqualityComparer<TSource>? comparer)
        {
            if (source is null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.source);
            }

            // NOTE: This must _not_ delegate to Contains(source, value), even if comparer is null or EqualityComparer<TSource>.Default (and
            // even if Contains(source, value) were switched to not delegate to this overload when the source isn't an ICollection<T>).
            // While such a comparer is for default equality semantics, the collection itself may not have such semantics, e.g. if source
            // is ICollection<T>, its Contains might use a different definition of equality, such as if the collection is a HashSet<T>
            // constructed with a non-default comparer. In such a case, by calling this overload, the developer is asking to use the provided
            // comparer's semantics rather than whatever the collection's semantics might be.

            if (typeof(TSource).IsValueType && (comparer is null || comparer == EqualityComparer<TSource>.Default))
            {
                // For value types and when using default equality semantics, use EqualityComparer<TValue>.Default.Equals directly to enable
                // devirtualization and possibly inlining of the comparison operation.

                if (source is IList<TSource> list)
                {
                    if (!TryGetSpan(list, out ReadOnlySpan<TSource> span))
                    {
                        int count = list.Count;
                        for (int i = 0; i < count; i++)
                        {
                            if (EqualityComparer<TSource>.Default.Equals(list[i], value))
                            {
                                return true;
                            }
                        }
                    }
                    else
                    {
                        foreach (TSource element in span)
                        {
                            if (EqualityComparer<TSource>.Default.Equals(element, value))
                            {
                                return true;
                            }
                        }
                    }
                }
                else
                {
                    foreach (TSource element in source)
                    {
                        if (EqualityComparer<TSource>.Default.Equals(element, value))
                        {
                            return true;
                        }
                    }
                }
            }
            else
            {
                // TSource is not a value type and/or a custom comparer is being used.

                comparer ??= EqualityComparer<TSource>.Default;

                if (source is IList<TSource> list)
                {
                    if (!TryGetSpan(list, out ReadOnlySpan<TSource> span))
                    {
                        int count = list.Count;
                        for (int i = 0; i < count; i++)
                        {
                            if (comparer.Equals(list[i], value))
                            {
                                return true;
                            }
                        }
                    }
                    else
                    {
                        foreach (TSource element in span)
                        {
                            if (comparer.Equals(element, value))
                            {
                                return true;
                            }
                        }
                    }
                }
                else
                {
                    foreach (TSource element in source)
                    {
                        if (comparer.Equals(element, value))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
