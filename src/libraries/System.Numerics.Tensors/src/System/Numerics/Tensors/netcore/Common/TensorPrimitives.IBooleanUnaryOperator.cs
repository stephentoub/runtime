// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace System.Numerics.Tensors
{
    public static unsafe partial class TensorPrimitives
    {
        /// <summary>Unary operator that produces a Boolean result for each element.</summary>
        /// <remarks>For vector-based methods, the Boolean result is either all-bits-set or zero.</remarks>
        private interface IBooleanUnaryOperator<T>
        {
            static abstract bool Vectorizable { get; }
            static abstract bool Invoke(T x);
            static abstract Vector128<T> Invoke(Vector128<T> x);
            static abstract Vector256<T> Invoke(Vector256<T> x);
            static abstract Vector512<T> Invoke(Vector512<T> x);
        }

        private interface IAnyAllAggregator
        {
            static abstract bool DefaultResult { get; }
            static abstract bool ShouldEarlyExit(bool result);
        }

        private readonly struct AnyAggregator : IAnyAllAggregator
        {
            public static bool DefaultResult => false;
            public static bool ShouldEarlyExit(bool result) => result;
        }

        private readonly struct AllAggregator : IAnyAllAggregator
        {
            public static bool DefaultResult => true;
            public static bool ShouldEarlyExit(bool result) => !result;
        }

        private static bool All<TInput, TOperator>(ReadOnlySpan<TInput> x)
            where TOperator : IBooleanUnaryOperator<TInput> =>
            AggregateAnyAll<TInput, TOperator, AllAggregator>(x);

        private static bool Any<TInput, TOperator>(ReadOnlySpan<TInput> x)
            where TOperator : IBooleanUnaryOperator<TInput> =>
            AggregateAnyAll<TInput, TOperator, AnyAggregator>(x);

        private static bool AggregateAnyAll<TInput, TOperator, TAnyAll>(ReadOnlySpan<TInput> x)
            where TOperator : IBooleanUnaryOperator<TInput>
            where TAnyAll : IAnyAllAggregator
        {
            Debug.Assert(!x.IsEmpty);

            for (int i = 0; i < x.Length; i++)
            {
                bool result = TOperator.Invoke(x[i]);
                if (TAnyAll.ShouldEarlyExit(result))
                {
                    return result;
                }
            }

            return TAnyAll.DefaultResult;
        }

        /// <summary>Performs an element-wise operation on <paramref name="x"/> and writes the results to <paramref name="destination"/>.</summary>
        /// <typeparam name="TInput">The element input type.</typeparam>
        /// <typeparam name="TUnaryOperator">Specifies the operation to perform on each element loaded from <paramref name="x"/>.</typeparam>
        /// <remarks>This should only be used when it's known that TInput/TOutput are vectorizable and the size of TInput is twice that of TOutput.</remarks>
        private static void InvokeSpanIntoSpan<TInput, TUnaryOperator>(
            ReadOnlySpan<TInput> x, Span<bool> destination)
            where TUnaryOperator : struct, IBooleanUnaryOperator<TInput>
        {
            if (x.Length > destination.Length)
            {
                ThrowHelper.ThrowArgument_DestinationTooShort();
            }

            ref TInput xRef = ref MemoryMarshal.GetReference(x);
            ref bool destinationRef = ref MemoryMarshal.GetReference(destination);
            int i = 0; //, twoVectorsFromEnd;

            if (Vector512.IsHardwareAccelerated && TUnaryOperator.Vectorizable)
            {
            }

            if (Vector256.IsHardwareAccelerated && TUnaryOperator.Vectorizable)
            {
            }

            if (Vector128.IsHardwareAccelerated && TUnaryOperator.Vectorizable)
            {
                //Debug.Assert(Vector128<TInput>.IsSupported);
                //Debug.Assert(Vector128<TOutput>.IsSupported);

                //twoVectorsFromEnd = x.Length - (Vector128<TInput>.Count * 2);
                //if (i <= twoVectorsFromEnd)
                //{
                //    // Loop handling two input vectors / one output vector at a time.
                //    do
                //    {
                //        TUnaryOperator.Invoke(
                //            Vector128.LoadUnsafe(ref xRef, (uint)i),
                //            Vector128.LoadUnsafe(ref xRef, (uint)(i + Vector128<TInput>.Count))).StoreUnsafe(ref destinationRef, (uint)i);

                //        i += Vector128<TInput>.Count * 2;
                //    }
                //    while (i <= twoVectorsFromEnd);

                //    // Handle any remaining elements with final vectors.
                //    if (i != x.Length)
                //    {
                //        i = x.Length - (Vector128<TInput>.Count * 2);

                //        TUnaryOperator.Invoke(
                //            Vector128.LoadUnsafe(ref xRef, (uint)i),
                //            Vector128.LoadUnsafe(ref xRef, (uint)(i + Vector128<TInput>.Count))).StoreUnsafe(ref destinationRef, (uint)i);
                //    }

                //    return;
                //}
            }

            while (i < x.Length)
            {
                Unsafe.Add(ref destinationRef, i) = TUnaryOperator.Invoke(Unsafe.Add(ref xRef, i));
                i++;
            }
        }
    }
}
