// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace System.Linq
{
    public static partial class AsyncEnumerable
    {
        /// <summary>Generates a sequence of integral numbers within a specified range.</summary>
        /// <param name="start">The value of the first integer in the sequence.</param>
        /// <param name="count">The number of sequential integers to generate.</param>
        /// <returns>An <see cref="IAsyncEnumerable{T}"/> that contains a range of sequential integral numbers.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is less than 0</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="start"/> + <paramref name="count"/> -1 is larger than <see cref="int.MaxValue"/>.</exception>
        public static IAsyncEnumerable<int> Range(int start, int count)
        {
            long endInclusive = ((long)start) + count - 1;
            if (count < 0 || endInclusive > int.MaxValue)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(count));
            }

            return count == 0 ?
                Empty<int>() :
                Impl(start, count);

            static async IAsyncEnumerable<int> Impl(int start, int count)
            {
                for (int i = 0; i < count; i++)
                {
                    yield return start + i;
                }
            }
        }
    }
}
