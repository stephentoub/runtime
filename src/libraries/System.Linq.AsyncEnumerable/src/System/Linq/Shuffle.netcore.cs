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
            private static void Shuffle(Span<TSource> span) => Random.Shared.Shuffle(span);

            private static void Shuffle(List<TSource> list) => Random.Shared.Shuffle(CollectionsMarshal.AsSpan(list));

            private static Random GetSharedRandom() => Random.Shared;
        }
    }
}
