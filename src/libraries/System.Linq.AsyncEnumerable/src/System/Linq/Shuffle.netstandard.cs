// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq
{
    public static partial class AsyncEnumerable
    {
        private sealed partial class ShuffleIterator<TSource>
        {
            private static void Shuffle(Span<TSource> span)
            {
                Random random = GetSharedRandom();
                int n = span.Length;
                for (int i = 0; i < n - 1; i++)
                {
                    int j = random.Next(i, n);
                    if (j != i)
                    {
                        TSource temp = span[i];
                        span[i] = span[j];
                        span[j] = temp;
                    }
                }
            }

            private static void Shuffle(List<TSource> list)
            {
                Random random = GetSharedRandom();
                int n = list.Count;
                for (int i = 0; i < n - 1; i++)
                {
                    int j = random.Next(i, n);
                    if (j != i)
                    {
                        TSource temp = list[i];
                        list[i] = list[j];
                        list[j] = temp;
                    }
                }
            }

            [ThreadStatic]
            private static Random? t_random;

            private static Random GetSharedRandom() =>
                t_random ??= new Random(Environment.TickCount ^ Environment.CurrentManagedThreadId);
        }
    }
}
