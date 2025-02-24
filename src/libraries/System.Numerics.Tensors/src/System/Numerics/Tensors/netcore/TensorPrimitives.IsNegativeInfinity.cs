// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace System.Numerics.Tensors
{
    public static partial class TensorPrimitives
    {
        /// <summary>Computes for each value in the specified tensor whether it's negative infinity.</summary>
        /// <param name="x">The tensor, represented as a span.</param>
        /// <param name="destination">The destination tensor, represented as a span.</param>
        /// <exception cref="ArgumentException">Destination is too short.</exception>
        /// <exception cref="ArgumentException"><paramref name="x"/> and <paramref name="destination"/> reference overlapping memory locations and do not begin at the same location.</exception>
        /// <remarks>
        /// This method effectively computes <c><paramref name="destination" />[i] = <typeparamref name="T"/>.IsNegativeInfinity(<paramref name="x" />[i])</c>.
        /// </remarks>
        public static void IsNegativeInfinity<T>(ReadOnlySpan<T> x, Span<bool> destination)
            where T : INumberBase<T> =>
            InvokeSpanIntoSpan<T, IsNegativeInfinityOperator<T>>(x, destination);

        /// <summary>Computes whether all of the values in the specified tensor are negative infinity.</summary>
        /// <param name="x">The tensor, represented as a span.</param>
        /// <returns><see langword="true"/> if all of the values in <paramref name="x"/> are negative infinity; otherwise, <see langword="false"/>.</returns>
        public static bool IsNegativeInfinityAll<T>(ReadOnlySpan<T> x)
            where T : INumberBase<T> =>
            !x.IsEmpty &&
            MayBeNegativeInfinity<T>() && All<T, IsNegativeInfinityOperator<T>>(x);

        /// <summary>Computes whether any of the values in the specified tensor is negative infinity.</summary>
        /// <param name="x">The tensor, represented as a span.</param>
        /// <returns><see langword="true"/> if any of the values in <paramref name="x"/> is negative infinity; otherwise, <see langword="false"/>.</returns>
        public static bool IsNegativeInfinityAny<T>(ReadOnlySpan<T> x)
            where T : INumberBase<T> =>
            !x.IsEmpty &&
            MayBeNegativeInfinity<T>() && Any<T, IsNegativeInfinityOperator<T>>(x);

        /// <summary>Gets whether any value could be complex.</summary>
        private static bool MayBeNegativeInfinity<T>() =>
            typeof(T) != typeof(decimal) &&
            typeof(T) != typeof(sbyte) && typeof(T) != typeof(byte) &&
            typeof(T) != typeof(short) && typeof(T) != typeof(ushort) && typeof(T) != typeof(char) &&
            typeof(T) != typeof(int) && typeof(T) != typeof(uint) &&
            typeof(T) != typeof(long) && typeof(T) != typeof(ulong) &&
            typeof(T) != typeof(nint) && typeof(T) != typeof(nuint) &&
            typeof(T) != typeof(Int128) && typeof(T) != typeof(UInt128);

        /// <summary>T.IsNegativeInfinity(x)</summary>
        private readonly struct IsNegativeInfinityOperator<T> : IBooleanUnaryOperator<T>
            where T : INumberBase<T>
        {
            public static bool Vectorizable => true;

            public static bool Invoke(T x) => T.IsNegativeInfinity(x);

            public static Vector128<T> Invoke(Vector128<T> x)
            {
#if NET10_0_OR_GREATER
                return Vector128.IsNegativeInfinity(x);
#else
                if (typeof(T) == typeof(float))
                {
                    return Vector128.Equals(x, Vector128.Create(float.NegativeInfinity).As<float, T>());
                }

                if (typeof(T) == typeof(double))
                {
                    return Vector128.Equals(x, Vector128.Create(double.NegativeInfinity).As<double, T>());
                }

                return Vector128<T>.Zero;
#endif
            }

            public static Vector256<T> Invoke(Vector256<T> x)
            {
#if NET10_0_OR_GREATER
                return Vector256.IsNegativeInfinity(x);
#else
                if (typeof(T) == typeof(float))
                {
                    return Vector256.Equals(x, Vector256.Create(float.NegativeInfinity).As<float, T>());
                }

                if (typeof(T) == typeof(double))
                {
                    return Vector256.Equals(x, Vector256.Create(double.NegativeInfinity).As<double, T>());
                }

                return Vector256<T>.Zero;
#endif
            }

            public static Vector512<T> Invoke(Vector512<T> x)
            {
#if NET10_0_OR_GREATER
                return Vector512.IsNegativeInfinity(x);
#else
                if (typeof(T) == typeof(float))
                {
                    return Vector512.Equals(x, Vector512.Create(float.NegativeInfinity).As<float, T>());
                }

                if (typeof(T) == typeof(double))
                {
                    return Vector512.Equals(x, Vector512.Create(double.NegativeInfinity).As<double, T>());
                }

                return Vector512<T>.Zero;
#endif
            }
        }
    }
}
