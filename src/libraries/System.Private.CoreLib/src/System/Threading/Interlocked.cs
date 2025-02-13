// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CS8500 // takes the address of, gets the size of, or declares a pointer to a managed type ('T')

namespace System.Threading
{
    /// <summary>Provides atomic operations for variables that are shared by multiple threads.</summary>
    public static partial class Interlocked
    {
        #region Increment
        /// <summary>Increments a specified variable and stores the result, as an atomic operation.</summary>
        /// <param name="location">The variable whose value is to be incremented.</param>
        /// <returns>The incremented value.</returns>
        /// <exception cref="NullReferenceException">The address of location is a null pointer.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static uint Increment(ref uint location) =>
            Add(ref location, 1);

        /// <summary>Increments a specified variable and stores the result, as an atomic operation.</summary>
        /// <param name="location">The variable whose value is to be incremented.</param>
        /// <returns>The incremented value.</returns>
        /// <exception cref="NullReferenceException">The address of location is a null pointer.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static ulong Increment(ref ulong location) =>
            Add(ref location, 1);
        #endregion

        #region Decrement
        /// <summary>Decrements a specified variable and stores the result, as an atomic operation.</summary>
        /// <param name="location">The variable whose value is to be decremented.</param>
        /// <returns>The decremented value.</returns>
        /// <exception cref="NullReferenceException">The address of location is a null pointer.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static uint Decrement(ref uint location) =>
            (uint)Add(ref Unsafe.As<uint, int>(ref location), -1);

        /// <summary>Decrements a specified variable and stores the result, as an atomic operation.</summary>
        /// <param name="location">The variable whose value is to be decremented.</param>
        /// <returns>The decremented value.</returns>
        /// <exception cref="NullReferenceException">The address of location is a null pointer.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static ulong Decrement(ref ulong location) =>
            (ulong)Add(ref Unsafe.As<ulong, long>(ref location), -1);
        #endregion

        #region Exchange
        /// <summary>Sets a 8-bit signed integer to a specified value and returns the original value, as an atomic operation.</summary>
        /// <param name="location1">The variable to set to the specified value.</param>
        /// <param name="value">The value to which the <paramref name="location1"/> parameter is set.</param>
        /// <returns>The original value of <paramref name="location1"/>.</returns>
        /// <exception cref="NullReferenceException">The address of location1 is a null pointer.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static sbyte Exchange(ref sbyte location1, sbyte value) =>
            (sbyte)Exchange(ref Unsafe.As<sbyte, byte>(ref location1), (byte)value);

        /// <summary>Sets a 16-bit unsigned integer to a specified value and returns the original value, as an atomic operation.</summary>
        /// <param name="location1">The variable to set to the specified value.</param>
        /// <param name="value">The value to which the <paramref name="location1"/> parameter is set.</param>
        /// <returns>The original value of <paramref name="location1"/>.</returns>
        /// <exception cref="NullReferenceException">The address of location1 is a null pointer.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short Exchange(ref short location1, short value) =>
            (short)Exchange(ref Unsafe.As<short, ushort>(ref location1), (ushort)value);

        /// <summary>Sets a 8-bit unsigned integer to a specified value and returns the original value, as an atomic operation.</summary>
        /// <param name="location1">The variable to set to the specified value.</param>
        /// <param name="value">The value to which the <paramref name="location1"/> parameter is set.</param>
        /// <returns>The original value of <paramref name="location1"/>.</returns>
        /// <exception cref="NullReferenceException">The address of location1 is a null pointer.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Exchange(ref byte location1, byte value)
        {
#if (MONO && (TARGET_AMD64 || TARGET_ARM64 || TARGET_WASM)) || (!MONO && (TARGET_X86 || TARGET_AMD64 || TARGET_ARM64))
            return Exchange(ref location1, value); // Must expand intrinsic
#else
            // this relies on GC keeping 4B alignment for refs and on subtracting to such alignment being in the same object
            nuint offset = Unsafe.OpportunisticMisalignment(ref location1, sizeof(uint));
            ref uint alignedRef = ref Unsafe.As<byte, uint>(ref Unsafe.SubtractByteOffset(ref location1, offset));
            int bitOffset =
                (int)((BitConverter.IsLittleEndian ? offset : sizeof(uint) - offset - sizeof(byte)) * 8); // to bit offset
            Debug.Assert(bitOffset is 0 or 8 or 16 or 24);
            uint mask = ~((uint)byte.MaxValue << bitOffset);
            uint shiftedValue = (uint)value << bitOffset;

            // this doesn't need to be volatile since CompareExchange will update stale values
            uint originalValue = alignedRef;
            uint newValue;
            do
            {
                // make sure the ref is still aligned
                Debug.Assert(Unsafe.IsOpportunisticallyAligned(ref alignedRef, sizeof(uint)));
                newValue = originalValue & mask | shiftedValue;
            } while (originalValue !=
                     (originalValue = CompareExchange(ref alignedRef, newValue, originalValue)));

            // verify the GC hasn't broken the ref
            Debug.Assert((nuint)Unsafe.ByteOffset(ref Unsafe.As<uint, byte>(ref alignedRef), ref location1) == offset);
            return (byte)(originalValue >> bitOffset);
#endif
        }

        /// <summary>Sets a 16-bit signed integer to a specified value and returns the original value, as an atomic operation.</summary>
        /// <param name="location1">The variable to set to the specified value.</param>
        /// <param name="value">The value to which the <paramref name="location1"/> parameter is set.</param>
        /// <returns>The original value of <paramref name="location1"/>.</returns>
        /// <exception cref="NullReferenceException">The address of location1 is a null pointer.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static ushort Exchange(ref ushort location1, ushort value)
        {
#if ((MONO && (TARGET_AMD64 || TARGET_ARM64 || TARGET_WASM)) || !MONO && (TARGET_X86 || TARGET_AMD64 || TARGET_ARM64))
            return Exchange(ref location1, value); // Must expand intrinsic
#else
            // this relies on GC keeping 4B alignment for refs and on subtracting to such alignment being in the same object
            nuint offset = Unsafe.OpportunisticMisalignment(ref location1, sizeof(uint));
            ref uint alignedRef = ref Unsafe.As<ushort, uint>(ref Unsafe.SubtractByteOffset(ref location1, offset));
            int bitOffset =
                (int)((BitConverter.IsLittleEndian ? offset : sizeof(uint) - offset - sizeof(ushort)) * 8); // to bit offset
            Debug.Assert(bitOffset is 0 or 16);
            uint mask = ~((uint)ushort.MaxValue << bitOffset);
            uint shiftedValue = (uint)value << bitOffset;

            // this doesn't need to be volatile since CompareExchange will update stale values
            uint originalValue = alignedRef;
            uint newValue;
            do
            {
                // make sure the ref is still aligned
                Debug.Assert(Unsafe.IsOpportunisticallyAligned(ref alignedRef, sizeof(uint)));
                newValue = originalValue & mask | shiftedValue;
            } while (originalValue !=
                     (originalValue = CompareExchange(ref alignedRef, newValue, originalValue)));

            // verify the GC hasn't broken the ref
            Debug.Assert((nuint)Unsafe.ByteOffset(ref Unsafe.As<uint, ushort>(ref alignedRef), ref location1) == offset);
            return (ushort)(originalValue >> bitOffset);
#endif
        }

        /// <summary>Sets a 32-bit unsigned integer to a specified value and returns the original value, as an atomic operation.</summary>
        /// <param name="location1">The variable to set to the specified value.</param>
        /// <param name="value">The value to which the <paramref name="location1"/> parameter is set.</param>
        /// <returns>The original value of <paramref name="location1"/>.</returns>
        /// <exception cref="NullReferenceException">The address of location1 is a null pointer.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static uint Exchange(ref uint location1, uint value) =>
            (uint)Exchange(ref Unsafe.As<uint, int>(ref location1), (int)value);

        /// <summary>Sets a 64-bit unsigned integer to a specified value and returns the original value, as an atomic operation.</summary>
        /// <param name="location1">The variable to set to the specified value.</param>
        /// <param name="value">The value to which the <paramref name="location1"/> parameter is set.</param>
        /// <returns>The original value of <paramref name="location1"/>.</returns>
        /// <exception cref="NullReferenceException">The address of location1 is a null pointer.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static ulong Exchange(ref ulong location1, ulong value) =>
            (ulong)Exchange(ref Unsafe.As<ulong, long>(ref location1), (long)value);

        /// <summary>Sets a single-precision floating point number to a specified value and returns the original value, as an atomic operation.</summary>
        /// <param name="location1">The variable to set to the specified value.</param>
        /// <param name="value">The value to which the <paramref name="location1"/> parameter is set.</param>
        /// <returns>The original value of <paramref name="location1"/>.</returns>
        /// <exception cref="NullReferenceException">The address of location1 is a null pointer.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Exchange(ref float location1, float value)
            => Unsafe.BitCast<int, float>(Exchange(ref Unsafe.As<float, int>(ref location1), Unsafe.BitCast<float, int>(value)));

        /// <summary>Sets a double-precision floating point number to a specified value and returns the original value, as an atomic operation.</summary>
        /// <param name="location1">The variable to set to the specified value.</param>
        /// <param name="value">The value to which the <paramref name="location1"/> parameter is set.</param>
        /// <returns>The original value of <paramref name="location1"/>.</returns>
        /// <exception cref="NullReferenceException">The address of location1 is a null pointer.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Exchange(ref double location1, double value)
            => Unsafe.BitCast<long, double>(Exchange(ref Unsafe.As<double, long>(ref location1), Unsafe.BitCast<double, long>(value)));

        /// <summary>Sets a platform-specific handle or pointer to a specified value and returns the original value, as an atomic operation.</summary>
        /// <param name="location1">The variable to set to the specified value.</param>
        /// <param name="value">The value to which the <paramref name="location1"/> parameter is set.</param>
        /// <returns>The original value of <paramref name="location1"/>.</returns>
        /// <exception cref="NullReferenceException">The address of location1 is a null pointer.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntPtr Exchange(ref IntPtr location1, IntPtr value)
        {
#pragma warning disable CA2020 // Prevent from behavioral change
#if TARGET_64BIT
            return (IntPtr)Interlocked.Exchange(ref Unsafe.As<IntPtr, long>(ref location1), (long)value);
#else
            return (IntPtr)Exchange(ref Unsafe.As<IntPtr, int>(ref location1), (int)value);
#endif
#pragma warning restore CA2020
        }

        /// <summary>Sets a platform-specific handle or pointer to a specified value and returns the original value, as an atomic operation.</summary>
        /// <param name="location1">The variable to set to the specified value.</param>
        /// <param name="value">The value to which the <paramref name="location1"/> parameter is set.</param>
        /// <returns>The original value of <paramref name="location1"/>.</returns>
        /// <exception cref="NullReferenceException">The address of location1 is a null pointer.</exception>
        [Intrinsic]
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UIntPtr Exchange(ref UIntPtr location1, UIntPtr value)
        {
#if TARGET_64BIT
            return (UIntPtr)Interlocked.Exchange(ref Unsafe.As<UIntPtr, long>(ref location1), (long)value);
#else
            return (UIntPtr)Exchange(ref Unsafe.As<UIntPtr, int>(ref location1), (int)value);
#endif
        }

        /// <summary>Sets a variable of the specified type <typeparamref name="T"/> to a specified value and returns the original value, as an atomic operation.</summary>
        /// <param name="location1">The variable to set to the specified value.</param>
        /// <param name="value">The value to which the <paramref name="location1"/> parameter is set.</param>
        /// <returns>The original value of <paramref name="location1"/>.</returns>
        /// <exception cref="NullReferenceException">The address of location1 is a null pointer.</exception>
        /// <exception cref="NotSupportedException">An unsupported <typeparamref name="T"/> is specified.</exception>
        /// <typeparam name="T">
        /// The type to be used for <paramref name="location1"/> and <paramref name="value"/>.
        /// This type must be a reference type, an enum type (i.e. typeof(T).IsEnum is true), or a primitive type (i.e. typeof(T).IsPrimitive is true).
        /// </typeparam>
        [Intrinsic]
        [return: NotNullIfNotNull(nameof(location1))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe T Exchange<T>([NotNullIfNotNull(nameof(value))] ref T location1, T value)
        {
            // Handle all reference types with CompareExchange(ref object, ...).
            if (!typeof(T).IsValueType)
            {
                object? result = Exchange(ref Unsafe.As<T, object?>(ref location1), value);
                return Unsafe.As<object?, T>(ref result);
            }

            // Handle everything else with a CompareExchange overload for the unsigned integral type of the corresponding size.
            // Only primitive types and enum types (which are backed by primitive types) are supported.
            if (!typeof(T).IsPrimitive && !typeof(T).IsEnum)
            {
                throw new NotSupportedException(SR.NotSupported_ReferenceEnumOrPrimitiveTypeRequired);
            }

            if (sizeof(T) == 1)
            {
                return Unsafe.BitCast<byte, T>(
                    Exchange(
                        ref Unsafe.As<T, byte>(ref location1),
                        Unsafe.BitCast<T, byte>(value)));
            }

            if (sizeof(T) == 2)
            {
                return Unsafe.BitCast<ushort, T>(
                    Exchange(
                        ref Unsafe.As<T, ushort>(ref location1),
                        Unsafe.BitCast<T, ushort>(value)));
            }

            if (sizeof(T) == 4)
            {
                return Unsafe.BitCast<int, T>(
                    Exchange(
                        ref Unsafe.As<T, int>(ref location1),
                        Unsafe.BitCast<T, int>(value)));
            }

            Debug.Assert(sizeof(T) == 8);
            return Unsafe.BitCast<long, T>(
                Exchange(
                    ref Unsafe.As<T, long>(ref location1),
                    Unsafe.BitCast<T, long>(value)));
        }
        #endregion

        #region CompareExchange
        /// <summary>Compares two 8-bit signed integers for equality and, if they are equal, replaces the first value.</summary>
        /// <param name="location1">The destination, whose value is compared with <paramref name="comparand"/> and possibly replaced.</param>
        /// <param name="value">The value that replaces the destination value if the comparison results in equality.</param>
        /// <param name="comparand">The value that is compared to the value at <paramref name="location1"/>.</param>
        /// <returns>The original value in <paramref name="location1"/>.</returns>
        /// <exception cref="NullReferenceException">The address of <paramref name="location1"/> is a null pointer.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static sbyte CompareExchange(ref sbyte location1, sbyte value, sbyte comparand) =>
            (sbyte)CompareExchange(ref Unsafe.As<sbyte, byte>(ref location1), (byte)value, (byte)comparand);

        /// <summary>Compares two 16-bit unsigned integers for equality and, if they are equal, replaces the first value.</summary>
        /// <param name="location1">The destination, whose value is compared with <paramref name="comparand"/> and possibly replaced.</param>
        /// <param name="value">The value that replaces the destination value if the comparison results in equality.</param>
        /// <param name="comparand">The value that is compared to the value at <paramref name="location1"/>.</param>
        /// <returns>The original value in <paramref name="location1"/>.</returns>
        /// <exception cref="NullReferenceException">The address of <paramref name="location1"/> is a null pointer.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short CompareExchange(ref short location1, short value, short comparand) =>
            (short)CompareExchange(ref Unsafe.As<short, ushort>(ref location1), (ushort)value, (ushort)comparand);

        /// <summary>Compares two 8-bit unsigned integers for equality and, if they are equal, replaces the first value.</summary>
        /// <param name="location1">The destination, whose value is compared with <paramref name="comparand"/> and possibly replaced.</param>
        /// <param name="value">The value that replaces the destination value if the comparison results in equality.</param>
        /// <param name="comparand">The value that is compared to the value at <paramref name="location1"/>.</param>
        /// <returns>The original value in <paramref name="location1"/>.</returns>
        /// <exception cref="NullReferenceException">The address of <paramref name="location1"/> is a null pointer.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte CompareExchange(ref byte location1, byte value, byte comparand)
        {
#if (MONO && (TARGET_ARM64 || TARGET_AMD64 || TARGET_WASM)) || (!MONO && (TARGET_X86 || TARGET_AMD64 || TARGET_ARM64))
            return CompareExchange(ref location1, value, comparand); // Must expand intrinsic
#else
            // this relies on GC keeping 4B alignment for refs and on subtracting to such alignment being in the same object
            nuint offset = Unsafe.OpportunisticMisalignment(ref location1, sizeof(uint));
            ref uint alignedRef = ref Unsafe.As<byte, uint>(ref Unsafe.SubtractByteOffset(ref location1, offset));
            int bitOffset =
                (int)((BitConverter.IsLittleEndian ? offset : sizeof(uint) - offset - sizeof(byte)) * 8); // to bit offset
            Debug.Assert(bitOffset is 0 or 8 or 16 or 24);
            uint mask = ~((uint)byte.MaxValue << bitOffset);
            uint shiftedValue = (uint)value << bitOffset;
            uint shiftedComparand = (uint)comparand << bitOffset;

            // this doesn't need to be volatile since CompareExchange will update stale values
            uint originalValue = alignedRef;
            uint fullComparand, newValue;
            do
            {
                // make sure the ref is still aligned
                Debug.Assert(Unsafe.IsOpportunisticallyAligned(ref alignedRef, sizeof(uint)));
                uint otherMemory = originalValue & mask;
                fullComparand = otherMemory | shiftedComparand;
                newValue = otherMemory | shiftedValue;
            } while (originalValue !=
                     (originalValue = CompareExchange(ref alignedRef, newValue, fullComparand)));

            // verify the GC hasn't broken the ref
            Debug.Assert((nuint)Unsafe.ByteOffset(ref Unsafe.As<uint, byte>(ref alignedRef), ref location1) == offset);
            return (byte)(originalValue >> bitOffset);
#endif
        }

        /// <summary>Compares two 16-bit signed integers for equality and, if they are equal, replaces the first value.</summary>
        /// <param name="location1">The destination, whose value is compared with <paramref name="comparand"/> and possibly replaced.</param>
        /// <param name="value">The value that replaces the destination value if the comparison results in equality.</param>
        /// <param name="comparand">The value that is compared to the value at <paramref name="location1"/>.</param>
        /// <returns>The original value in <paramref name="location1"/>.</returns>
        /// <exception cref="NullReferenceException">The address of <paramref name="location1"/> is a null pointer.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static ushort CompareExchange(ref ushort location1, ushort value, ushort comparand)
        {
#if (MONO && (TARGET_ARM64 || TARGET_AMD64 || TARGET_WASM)) || (!MONO && (TARGET_X86 || TARGET_AMD64 || TARGET_ARM64))
            return CompareExchange(ref location1, value, comparand); // Must expand intrinsic
#else
            // this relies on GC keeping 4B alignment for refs and on subtracting to such alignment being in the same object
            nuint offset = Unsafe.OpportunisticMisalignment(ref location1, sizeof(uint));
            ref uint alignedRef = ref Unsafe.As<ushort, uint>(ref Unsafe.SubtractByteOffset(ref location1, offset));
            int bitOffset =
                (int)((BitConverter.IsLittleEndian ? offset : sizeof(uint) - offset - sizeof(ushort)) * 8); // to bit offset
            Debug.Assert(bitOffset is 0 or 16);
            uint mask = ~((uint)ushort.MaxValue << bitOffset);
            uint shiftedValue = (uint)value << bitOffset;
            uint shiftedComparand = (uint)comparand << bitOffset;

            // this doesn't need to be volatile since CompareExchange will update stale values
            uint originalValue = alignedRef;
            uint fullComparand, newValue;
            do
            {
                // make sure the ref is still aligned
                Debug.Assert(Unsafe.IsOpportunisticallyAligned(ref alignedRef, sizeof(uint)));
                uint otherMemory = originalValue & mask;
                fullComparand = otherMemory | shiftedComparand;
                newValue = otherMemory | shiftedValue;
            } while (originalValue !=
                     (originalValue = CompareExchange(ref alignedRef, newValue, fullComparand)));

            // verify the GC hasn't broken the ref
            Debug.Assert((nuint)Unsafe.ByteOffset(ref Unsafe.As<uint, ushort>(ref alignedRef), ref location1) == offset);
            return (ushort)(originalValue >> bitOffset);
#endif
        }

        /// <summary>Compares two 32-bit unsigned integers for equality and, if they are equal, replaces the first value.</summary>
        /// <param name="location1">The destination, whose value is compared with <paramref name="comparand"/> and possibly replaced.</param>
        /// <param name="value">The value that replaces the destination value if the comparison results in equality.</param>
        /// <param name="comparand">The value that is compared to the value at <paramref name="location1"/>.</param>
        /// <returns>The original value in <paramref name="location1"/>.</returns>
        /// <exception cref="NullReferenceException">The address of <paramref name="location1"/> is a null pointer.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static uint CompareExchange(ref uint location1, uint value, uint comparand) =>
            (uint)CompareExchange(ref Unsafe.As<uint, int>(ref location1), (int)value, (int)comparand);

        /// <summary>Compares two 64-bit unsigned integers for equality and, if they are equal, replaces the first value.</summary>
        /// <param name="location1">The destination, whose value is compared with <paramref name="comparand"/> and possibly replaced.</param>
        /// <param name="value">The value that replaces the destination value if the comparison results in equality.</param>
        /// <param name="comparand">The value that is compared to the value at <paramref name="location1"/>.</param>
        /// <returns>The original value in <paramref name="location1"/>.</returns>
        /// <exception cref="NullReferenceException">The address of <paramref name="location1"/> is a null pointer.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static ulong CompareExchange(ref ulong location1, ulong value, ulong comparand) =>
            (ulong)CompareExchange(ref Unsafe.As<ulong, long>(ref location1), (long)value, (long)comparand);

        /// <summary>Compares two single-precision floating point numbers for equality and, if they are equal, replaces the first value.</summary>
        /// <param name="location1">The destination, whose value is compared with <paramref name="comparand"/> and possibly replaced.</param>
        /// <param name="value">The value that replaces the destination value if the comparison results in equality.</param>
        /// <param name="comparand">The value that is compared to the value at <paramref name="location1"/>.</param>
        /// <returns>The original value in <paramref name="location1"/>.</returns>
        /// <exception cref="NullReferenceException">The address of <paramref name="location1"/> is a null pointer.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float CompareExchange(ref float location1, float value, float comparand)
            => Unsafe.BitCast<int, float>(CompareExchange(ref Unsafe.As<float, int>(ref location1), Unsafe.BitCast<float, int>(value), Unsafe.BitCast<float, int>(comparand)));

        /// <summary>Compares two double-precision floating point numbers for equality and, if they are equal, replaces the first value.</summary>
        /// <param name="location1">The destination, whose value is compared with <paramref name="comparand"/> and possibly replaced.</param>
        /// <param name="value">The value that replaces the destination value if the comparison results in equality.</param>
        /// <param name="comparand">The value that is compared to the value at <paramref name="location1"/>.</param>
        /// <returns>The original value in <paramref name="location1"/>.</returns>
        /// <exception cref="NullReferenceException">The address of <paramref name="location1"/> is a null pointer.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double CompareExchange(ref double location1, double value, double comparand)
            => Unsafe.BitCast<long, double>(CompareExchange(ref Unsafe.As<double, long>(ref location1), Unsafe.BitCast<double, long>(value), Unsafe.BitCast<double, long>(comparand)));

        /// <summary>Compares two platform-specific handles or pointers for equality and, if they are equal, replaces the first one.</summary>
        /// <param name="location1">The destination <see cref="IntPtr"/>, whose value is compared with the value of <paramref name="comparand"/> and possibly replaced by <paramref name="value"/>.</param>
        /// <param name="value">The <see cref="IntPtr"/> that replaces the destination value if the comparison results in equality.</param>
        /// <param name="comparand">The <see cref="IntPtr"/> that is compared to the value at <paramref name="location1"/>.</param>
        /// <returns>The original value in <paramref name="location1"/>.</returns>
        /// <exception cref="NullReferenceException">The address of <paramref name="location1"/> is a null pointer.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntPtr CompareExchange(ref IntPtr location1, IntPtr value, IntPtr comparand)
        {
#pragma warning disable CA2020 // Prevent from behavioral change
#if TARGET_64BIT
            return (IntPtr)Interlocked.CompareExchange(ref Unsafe.As<IntPtr, long>(ref location1), (long)value, (long)comparand);
#else
            return (IntPtr)CompareExchange(ref Unsafe.As<IntPtr, int>(ref location1), (int)value, (int)comparand);
#endif
#pragma warning restore CA2020
        }

        /// <summary>Compares two platform-specific handles or pointers for equality and, if they are equal, replaces the first one.</summary>
        /// <param name="location1">The destination <see cref="UIntPtr"/>, whose value is compared with the value of <paramref name="comparand"/> and possibly replaced by <paramref name="value"/>.</param>
        /// <param name="value">The <see cref="UIntPtr"/> that replaces the destination value if the comparison results in equality.</param>
        /// <param name="comparand">The <see cref="UIntPtr"/> that is compared to the value at <paramref name="location1"/>.</param>
        /// <returns>The original value in <paramref name="location1"/>.</returns>
        /// <exception cref="NullReferenceException">The address of <paramref name="location1"/> is a null pointer.</exception>
        [Intrinsic]
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UIntPtr CompareExchange(ref UIntPtr location1, UIntPtr value, UIntPtr comparand)
        {
#if TARGET_64BIT
            return (UIntPtr)Interlocked.CompareExchange(ref Unsafe.As<UIntPtr, long>(ref location1), (long)value, (long)comparand);
#else
            return (UIntPtr)CompareExchange(ref Unsafe.As<UIntPtr, int>(ref location1), (int)value, (int)comparand);
#endif
        }

        /// <summary>Compares two instances of the specified type <typeparamref name="T"/> for equality and, if they are equal, replaces the first one.</summary>
        /// <param name="location1">The destination, whose value is compared with <paramref name="comparand"/> and possibly replaced.</param>
        /// <param name="value">The value that replaces the destination value if the comparison results in equality.</param>
        /// <param name="comparand">The object that is compared to the value at <paramref name="location1"/>.</param>
        /// <returns>The original value in <paramref name="location1"/>.</returns>
        /// <exception cref="NullReferenceException">The address of <paramref name="location1"/> is a null pointer.</exception>
        /// <exception cref="NotSupportedException">An unsupported <typeparamref name="T"/> is specified.</exception>
        /// <typeparam name="T">
        /// The type to be used for <paramref name="location1"/>, <paramref name="value"/>, and <paramref name="comparand"/>.
        /// This type must be a reference type, an enum type (i.e. typeof(T).IsEnum is true), or a primitive type (i.e. typeof(T).IsPrimitive is true).
        /// </typeparam>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [return: NotNullIfNotNull(nameof(location1))]
        public static unsafe T CompareExchange<T>(ref T location1, T value, T comparand)
        {
            // Handle all reference types with CompareExchange(ref object, ...).
            if (!typeof(T).IsValueType)
            {
                object? result = CompareExchange(ref Unsafe.As<T, object?>(ref location1), value, comparand);
                return Unsafe.As<object?, T>(ref result);
            }

            // Handle everything else with a CompareExchange overload for the unsigned integral type of the corresponding size.
            // Only primitive types and enum types (which are backed by primitive types) are supported.
            if (!typeof(T).IsPrimitive && !typeof(T).IsEnum)
            {
                throw new NotSupportedException(SR.NotSupported_ReferenceEnumOrPrimitiveTypeRequired);
            }

            if (sizeof(T) == 1)
            {
                return Unsafe.BitCast<byte, T>(
                    CompareExchange(
                        ref Unsafe.As<T, byte>(ref location1),
                        Unsafe.BitCast<T, byte>(value),
                        Unsafe.BitCast<T, byte>(comparand)));
            }

            if (sizeof(T) == 2)
            {
                return Unsafe.BitCast<ushort, T>(
                    CompareExchange(
                        ref Unsafe.As<T, ushort>(ref location1),
                        Unsafe.BitCast<T, ushort>(value),
                        Unsafe.BitCast<T, ushort>(comparand)));
            }

            if (sizeof(T) == 4)
            {
                return Unsafe.BitCast<int, T>(
                    CompareExchange(
                        ref Unsafe.As<T, int>(ref location1),
                        Unsafe.BitCast<T, int>(value),
                        Unsafe.BitCast<T, int>(comparand)));
            }

            Debug.Assert(sizeof(T) == 8);
            return Unsafe.BitCast<long, T>(
                CompareExchange(
                    ref Unsafe.As<T, long>(ref location1),
                    Unsafe.BitCast<T, long>(value),
                    Unsafe.BitCast<T, long>(comparand)));
        }
        #endregion

        #region Add
        /// <summary>Adds two 32-bit unsigned integers and replaces the first integer with the sum, as an atomic operation.</summary>
        /// <param name="location1">A variable containing the first value to be added. The sum of the two values is stored in <paramref name="location1"/>.</param>
        /// <param name="value">The value to be added to the integer at <paramref name="location1"/>.</param>
        /// <returns>The new value stored at <paramref name="location1"/>.</returns>
        /// <exception cref="NullReferenceException">The address of <paramref name="location1"/> is a null pointer.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static uint Add(ref uint location1, uint value) =>
            (uint)Add(ref Unsafe.As<uint, int>(ref location1), (int)value);

        /// <summary>Adds two 64-bit unsigned integers and replaces the first integer with the sum, as an atomic operation.</summary>
        /// <param name="location1">A variable containing the first value to be added. The sum of the two values is stored in <paramref name="location1"/>.</param>
        /// <param name="value">The value to be added to the integer at <paramref name="location1"/>.</param>
        /// <returns>The new value stored at <paramref name="location1"/>.</returns>
        /// <exception cref="NullReferenceException">The address of <paramref name="location1"/> is a null pointer.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static ulong Add(ref ulong location1, ulong value) =>
            (ulong)Add(ref Unsafe.As<ulong, long>(ref location1), (long)value);
        #endregion

        #region Read
        /// <summary>Returns a 64-bit unsigned value, loaded as an atomic operation.</summary>
        /// <param name="location">The 64-bit value to be loaded.</param>
        /// <returns>The loaded value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static ulong Read(ref readonly ulong location) =>
            CompareExchange(ref Unsafe.AsRef(in location), 0, 0);
        #endregion

        #region And
        /// <summary>Bitwise "ands" two 32-bit signed integers and replaces the first integer with the result, as an atomic operation.</summary>
        /// <param name="location1">A variable containing the first value to be combined. The result is stored in <paramref name="location1"/>.</param>
        /// <param name="value">The value to be combined with the integer at <paramref name="location1"/>.</param>
        /// <returns>The original value in <paramref name="location1"/>.</returns>
        /// <exception cref="NullReferenceException">The address of <paramref name="location1"/> is a null pointer.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int And(ref int location1, int value)
        {
            int current = location1;
            while (true)
            {
                int newValue = current & value;
                int oldValue = CompareExchange(ref location1, newValue, current);
                if (oldValue == current)
                {
                    return oldValue;
                }
                current = oldValue;
            }
        }

        /// <summary>Bitwise "ands" two 32-bit unsigned integers and replaces the first integer with the result, as an atomic operation.</summary>
        /// <param name="location1">A variable containing the first value to be combined. The result is stored in <paramref name="location1"/>.</param>
        /// <param name="value">The value to be combined with the integer at <paramref name="location1"/>.</param>
        /// <returns>The original value in <paramref name="location1"/>.</returns>
        /// <exception cref="NullReferenceException">The address of <paramref name="location1"/> is a null pointer.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static uint And(ref uint location1, uint value) =>
            (uint)And(ref Unsafe.As<uint, int>(ref location1), (int)value);

        /// <summary>Bitwise "ands" two 64-bit signed integers and replaces the first integer with the result, as an atomic operation.</summary>
        /// <param name="location1">A variable containing the first value to be combined. The result is stored in <paramref name="location1"/>.</param>
        /// <param name="value">The value to be combined with the integer at <paramref name="location1"/>.</param>
        /// <returns>The original value in <paramref name="location1"/>.</returns>
        /// <exception cref="NullReferenceException">The address of <paramref name="location1"/> is a null pointer.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long And(ref long location1, long value)
        {
            long current = location1;
            while (true)
            {
                long newValue = current & value;
                long oldValue = CompareExchange(ref location1, newValue, current);
                if (oldValue == current)
                {
                    return oldValue;
                }
                current = oldValue;
            }
        }

        /// <summary>Bitwise "ands" two 64-bit unsigned integers and replaces the first integer with the result, as an atomic operation.</summary>
        /// <param name="location1">A variable containing the first value to be combined. The result is stored in <paramref name="location1"/>.</param>
        /// <param name="value">The value to be combined with the integer at <paramref name="location1"/>.</param>
        /// <returns>The original value in <paramref name="location1"/>.</returns>
        /// <exception cref="NullReferenceException">The address of <paramref name="location1"/> is a null pointer.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static ulong And(ref ulong location1, ulong value) =>
            (ulong)And(ref Unsafe.As<ulong, long>(ref location1), (long)value);
        #endregion

        #region Or
        /// <summary>Bitwise "ors" two 32-bit signed integers and replaces the first integer with the result, as an atomic operation.</summary>
        /// <param name="location1">A variable containing the first value to be combined. The result is stored in <paramref name="location1"/>.</param>
        /// <param name="value">The value to be combined with the integer at <paramref name="location1"/>.</param>
        /// <returns>The original value in <paramref name="location1"/>.</returns>
        /// <exception cref="NullReferenceException">The address of <paramref name="location1"/> is a null pointer.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Or(ref int location1, int value)
        {
            int current = location1;
            while (true)
            {
                int newValue = current | value;
                int oldValue = CompareExchange(ref location1, newValue, current);
                if (oldValue == current)
                {
                    return oldValue;
                }
                current = oldValue;
            }
        }

        /// <summary>Bitwise "ors" two 32-bit unsigned integers and replaces the first integer with the result, as an atomic operation.</summary>
        /// <param name="location1">A variable containing the first value to be combined. The result is stored in <paramref name="location1"/>.</param>
        /// <param name="value">The value to be combined with the integer at <paramref name="location1"/>.</param>
        /// <returns>The original value in <paramref name="location1"/>.</returns>
        /// <exception cref="NullReferenceException">The address of <paramref name="location1"/> is a null pointer.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static uint Or(ref uint location1, uint value) =>
            (uint)Or(ref Unsafe.As<uint, int>(ref location1), (int)value);

        /// <summary>Bitwise "ors" two 64-bit signed integers and replaces the first integer with the result, as an atomic operation.</summary>
        /// <param name="location1">A variable containing the first value to be combined. The result is stored in <paramref name="location1"/>.</param>
        /// <param name="value">The value to be combined with the integer at <paramref name="location1"/>.</param>
        /// <returns>The original value in <paramref name="location1"/>.</returns>
        /// <exception cref="NullReferenceException">The address of <paramref name="location1"/> is a null pointer.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Or(ref long location1, long value)
        {
            long current = location1;
            while (true)
            {
                long newValue = current | value;
                long oldValue = CompareExchange(ref location1, newValue, current);
                if (oldValue == current)
                {
                    return oldValue;
                }
                current = oldValue;
            }
        }

        /// <summary>Bitwise "ors" two 64-bit unsigned integers and replaces the first integer with the result, as an atomic operation.</summary>
        /// <param name="location1">A variable containing the first value to be combined. The result is stored in <paramref name="location1"/>.</param>
        /// <param name="value">The value to be combined with the integer at <paramref name="location1"/>.</param>
        /// <returns>The original value in <paramref name="location1"/>.</returns>
        /// <exception cref="NullReferenceException">The address of <paramref name="location1"/> is a null pointer.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static ulong Or(ref ulong location1, ulong value) =>
            (ulong)Or(ref Unsafe.As<ulong, long>(ref location1), (long)value);
        #endregion

        #region MemoryBarrier
        /// <summary>
        /// Synchronizes memory access as follows:
        /// The processor that executes the current thread cannot reorder instructions in such a way that memory accesses before
        /// the call to <see cref="MemoryBarrier"/> execute after memory accesses that follow the call to <see cref="MemoryBarrier"/>.
        /// </summary>
        [Intrinsic]
        public static void MemoryBarrier() => MemoryBarrier();
        #endregion
    }
}
