// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Threading
{
    public static partial class Interlocked
    {
        #region Increment
        /// <summary>Increments a specified variable and stores the result, as an atomic operation.</summary>
        /// <param name="location">The variable whose value is to be incremented.</param>
        /// <returns>The incremented value.</returns>
        /// <exception cref="NullReferenceException">The address of location is a null pointer.</exception>
        public static int Increment(ref int location) =>
            Add(ref location, 1);

        /// <summary>Increments a specified variable and stores the result, as an atomic operation.</summary>
        /// <param name="location">The variable whose value is to be incremented.</param>
        /// <returns>The incremented value.</returns>
        /// <exception cref="NullReferenceException">The address of location is a null pointer.</exception>
        public static long Increment(ref long location) =>
            Add(ref location, 1);
        #endregion

        #region Decrement
        /// <summary>Decrements a specified variable and stores the result, as an atomic operation.</summary>
        /// <param name="location">The variable whose value is to be decremented.</param>
        /// <returns>The decremented value.</returns>
        /// <exception cref="NullReferenceException">The address of location is a null pointer.</exception>
        public static int Decrement(ref int location) =>
            Add(ref location, -1);

        /// <summary>Decrements a specified variable and stores the result, as an atomic operation.</summary>
        /// <param name="location">The variable whose value is to be decremented.</param>
        /// <returns>The decremented value.</returns>
        /// <exception cref="NullReferenceException">The address of location is a null pointer.</exception>
        public static long Decrement(ref long location) =>
            Add(ref location, -1);
        #endregion

        #region Exchange
        /// <summary>Sets a 32-bit signed integer to a specified value and returns the original value, as an atomic operation.</summary>
        /// <param name="location1">The variable to set to the specified value.</param>
        /// <param name="value">The value to which the <paramref name="location1"/> parameter is set.</param>
        /// <returns>The original value of <paramref name="location1"/>.</returns>
        /// <exception cref="NullReferenceException">The address of location1 is a null pointer.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Exchange(ref int location1, int value)
        {
#if TARGET_X86 || TARGET_AMD64 || TARGET_ARM64 || TARGET_RISCV64
            return Exchange(ref location1, value); // Must expand intrinsic
#else
            if (Unsafe.IsNullRef(ref location1))
                ThrowHelper.ThrowNullReferenceException();
            return Exchange32(ref location1, value);
#endif
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern int Exchange32(ref int location1, int value);

        /// <summary>Sets a 64-bit signed integer to a specified value and returns the original value, as an atomic operation.</summary>
        /// <param name="location1">The variable to set to the specified value.</param>
        /// <param name="value">The value to which the <paramref name="location1"/> parameter is set.</param>
        /// <returns>The original value of <paramref name="location1"/>.</returns>
        /// <exception cref="NullReferenceException">The address of location1 is a null pointer.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Exchange(ref long location1, long value)
        {
#if TARGET_AMD64 || TARGET_ARM64 || TARGET_RISCV64
            return Exchange(ref location1, value); // Must expand intrinsic
#else
            if (Unsafe.IsNullRef(ref location1))
                ThrowHelper.ThrowNullReferenceException();
            return Exchange64(ref location1, value);
#endif
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern long Exchange64(ref long location1, long value);

        /// <summary>Sets an object to the specified value and returns a reference to the original object, as an atomic operation.</summary>
        /// <param name="location1">The variable to set to the specified value.</param>
        /// <param name="value">The value to which the <paramref name="location1"/> parameter is set.</param>
        /// <returns>The original value of <paramref name="location1"/>.</returns>
        /// <exception cref="NullReferenceException">The address of location1 is a null pointer.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [return: NotNullIfNotNull(nameof(location1))]
        public static object? Exchange([NotNullIfNotNull(nameof(value))] ref object? location1, object? value)
        {
            if (Unsafe.IsNullRef(ref location1))
                ThrowHelper.ThrowNullReferenceException();
            return ExchangeObject(ref location1, value);
        }

        [return: NotNullIfNotNull(nameof(location1))]
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern object? ExchangeObject([NotNullIfNotNull(nameof(value))] ref object? location1, object? value);

        // The below whole method reduces to a single call to Exchange(ref object, object) but
        // the JIT thinks that it will generate more native code than it actually does.

        /// <summary>Sets a variable of the specified type <typeparamref name="T"/> to a specified value and returns the original value, as an atomic operation.</summary>
        /// <param name="location1">The variable to set to the specified value.</param>
        /// <param name="value">The value to which the <paramref name="location1"/> parameter is set.</param>
        /// <returns>The original value of <paramref name="location1"/>.</returns>
        /// <exception cref="NullReferenceException">The address of location1 is a null pointer.</exception>
        /// <typeparam name="T">The type to be used for <paramref name="location1"/> and <paramref name="value"/>. This type must be a reference type.</typeparam>
        [Intrinsic]
        [return: NotNullIfNotNull(nameof(location1))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Exchange<T>([NotNullIfNotNull(nameof(value))] ref T location1, T value) where T : class? =>
            Unsafe.As<T>(Exchange(ref Unsafe.As<T, object?>(ref location1), value));
#endregion

        #region CompareExchange
        /// <summary>Compares two 32-bit signed integers for equality and, if they are equal, replaces the first value.</summary>
        /// <param name="location1">The destination, whose value is compared with <paramref name="comparand"/> and possibly replaced.</param>
        /// <param name="value">The value that replaces the destination value if the comparison results in equality.</param>
        /// <param name="comparand">The value that is compared to the value at <paramref name="location1"/>.</param>
        /// <returns>The original value in <paramref name="location1"/>.</returns>
        /// <exception cref="NullReferenceException">The address of <paramref name="location1"/> is a null pointer.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CompareExchange(ref int location1, int value, int comparand)
        {
#if TARGET_X86 || TARGET_AMD64 || TARGET_ARM64 || TARGET_RISCV64
            return CompareExchange(ref location1, value, comparand); // Must expand intrinsic
#else
            if (Unsafe.IsNullRef(ref location1))
                ThrowHelper.ThrowNullReferenceException();
            return CompareExchange32(ref location1, value, comparand);
#endif
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern int CompareExchange32(ref int location1, int value, int comparand);

        /// <summary>Compares two 64-bit signed integers for equality and, if they are equal, replaces the first value.</summary>
        /// <param name="location1">The destination, whose value is compared with <paramref name="comparand"/> and possibly replaced.</param>
        /// <param name="value">The value that replaces the destination value if the comparison results in equality.</param>
        /// <param name="comparand">The value that is compared to the value at <paramref name="location1"/>.</param>
        /// <returns>The original value in <paramref name="location1"/>.</returns>
        /// <exception cref="NullReferenceException">The address of <paramref name="location1"/> is a null pointer.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long CompareExchange(ref long location1, long value, long comparand)
        {
#if TARGET_AMD64 || TARGET_ARM64 || TARGET_RISCV64
            return CompareExchange(ref location1, value, comparand); // Must expand intrinsic
#else
            if (Unsafe.IsNullRef(ref location1))
                ThrowHelper.ThrowNullReferenceException();
            return CompareExchange64(ref location1, value, comparand);
#endif
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern long CompareExchange64(ref long location1, long value, long comparand);

        /// <summary>Compares two objects for reference equality and, if they are equal, replaces the first object.</summary>
        /// <param name="location1">The destination object that is compared by reference with <paramref name="comparand"/> and possibly replaced.</param>
        /// <param name="value">The object that replaces the destination object if the reference comparison results in equality.</param>
        /// <param name="comparand">The object that is compared by reference to the object at <paramref name="location1"/>.</param>
        /// <returns>The original value in <paramref name="location1"/>.</returns>
        /// <exception cref="NullReferenceException">The address of <paramref name="location1"/> is a null pointer.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [return: NotNullIfNotNull(nameof(location1))]
        public static object? CompareExchange(ref object? location1, object? value, object? comparand)
        {
            if (Unsafe.IsNullRef(ref location1))
                ThrowHelper.ThrowNullReferenceException();
            return CompareExchangeObject(ref location1, value, comparand);
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        [return: NotNullIfNotNull(nameof(location1))]
        private static extern object? CompareExchangeObject(ref object? location1, object? value, object? comparand);

        // Note that getILIntrinsicImplementationForInterlocked() in vm\jitinterface.cpp replaces
        // the body of the following method with the following IL when T is a reference type:
        //     ldarg.0
        //     ldarg.1
        //     ldarg.2
        //     call System.Threading.Interlocked::CompareExchange(ref Object, Object, Object)
        //     ret
        // The workaround is no longer strictly necessary now that we have Unsafe.As but it does
        // have the advantage of being less sensitive to JIT's inliner decisions.

        /// <summary>Compares two instances of the specified type <typeparamref name="T"/> for equality and, if they are equal, replaces the first one.</summary>
        /// <param name="location1">The destination, whose value is compared with <paramref name="comparand"/> and possibly replaced.</param>
        /// <param name="value">The value that replaces the destination value if the comparison results in equality.</param>
        /// <param name="comparand">The object that is compared to the value at <paramref name="location1"/>.</param>
        /// <returns>The original value in <paramref name="location1"/>.</returns>
        /// <exception cref="NullReferenceException">The address of <paramref name="location1"/> is a null pointer.</exception>
        /// <exception cref="NotSupportedException">An unsupported <typeparamref name="T"/> is specified.</exception>
        /// <typeparam name="T">
        /// The type to be used for <paramref name="location1"/>, <paramref name="value"/>, and <paramref name="comparand"/>.
        /// This type must be a reference type, an enum type, or one of the following value types: <see cref="bool"/>, <see cref="byte"/>,
        /// <see cref="sbyte"/>, <see cref="char"/>, <see cref="short"/>, <see cref="ushort"/>, <see cref="int"/>, <see cref="uint"/>,
        /// <see cref="long"/>, <see cref="ulong"/>, <see cref="nint"/>, or <see cref="nuint"/>.
        /// </typeparam>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [return: NotNullIfNotNull(nameof(location1))]
        public static T CompareExchange<T>(ref T location1, T value, T comparand)
        {
            if (!typeof(T).IsValueType)
            {
                object? result = CompareExchange(ref Unsafe.As<T, object?>(ref location1), value, comparand);
                return Unsafe.As<object, T>(ref result!);
            }

            // We could use the internal RuntimeHelpers.IsBitwiseEquatable, but that would make the set of types supported
            // harder to reason about, and would mean various changes to a type (like adding an Equals override) could then
            // cause Interlocked operations with it to start failing. Alternatively, we could make this support all types
            // of sizes 1, 2, 4, or 8 bytes, and always use bitwise equality semantics even if that's not what the type does,
            // and document that behavior.

            if (typeof(T) == typeof(byte) ||
                typeof(T) == typeof(sbyte) ||
                typeof(T) == typeof(bool) ||
                (typeof(T).IsEnum && Unsafe.SizeOf<T>() == 1))
            {
                return Unsafe.BitCast<byte, T>(CompareExchange(ref Unsafe.As<T, byte>(ref location1), Unsafe.BitCast<T, byte>(value), Unsafe.BitCast<T, byte>(comparand)));
            }

            if (typeof(T) == typeof(short) ||
                typeof(T) == typeof(ushort) ||
                typeof(T) == typeof(char) ||
                (typeof(T).IsEnum && Unsafe.SizeOf<T>() == 2))
            {
                return Unsafe.BitCast<ushort, T>(CompareExchange(ref Unsafe.As<T, ushort>(ref location1), Unsafe.BitCast<T, ushort>(value), Unsafe.BitCast<T, ushort>(comparand)));
            }

            if (typeof(T) == typeof(int) ||
                typeof(T) == typeof(uint) ||
                ((typeof(T) == typeof(nint) || typeof(T) == typeof(nuint)) && IntPtr.Size == 4) ||
                (typeof(T).IsEnum && Unsafe.SizeOf<T>() == 4))
            {
                return Unsafe.BitCast<uint, T>(CompareExchange(ref Unsafe.As<T, uint>(ref location1), Unsafe.BitCast<T, uint>(value), Unsafe.BitCast<T, uint>(comparand)));
            }

            if (typeof(T) == typeof(long) ||
                typeof(T) == typeof(ulong) ||
                ((typeof(T) == typeof(nint) || typeof(T) == typeof(nuint)) && IntPtr.Size == 8) ||
                (typeof(T).IsEnum && Unsafe.SizeOf<T>() == 8))
            {
                return Unsafe.BitCast<ulong, T>(CompareExchange(ref Unsafe.As<T, ulong>(ref location1), Unsafe.BitCast<T, ulong>(value), Unsafe.BitCast<T, ulong>(comparand)));
            }

            throw new NotSupportedException();
        }
        #endregion

        #region Add
        /// <summary>Adds two 32-bit signed integers and replaces the first integer with the sum, as an atomic operation.</summary>
        /// <param name="location1">A variable containing the first value to be added. The sum of the two values is stored in <paramref name="location1"/>.</param>
        /// <param name="value">The value to be added to the integer at <paramref name="location1"/>.</param>
        /// <returns>The new value stored at <paramref name="location1"/>.</returns>
        /// <exception cref="NullReferenceException">The address of <paramref name="location1"/> is a null pointer.</exception>
        public static int Add(ref int location1, int value) =>
            ExchangeAdd(ref location1, value) + value;

        /// <summary>Adds two 64-bit signed integers and replaces the first integer with the sum, as an atomic operation.</summary>
        /// <param name="location1">A variable containing the first value to be added. The sum of the two values is stored in <paramref name="location1"/>.</param>
        /// <param name="value">The value to be added to the integer at <paramref name="location1"/>.</param>
        /// <returns>The new value stored at <paramref name="location1"/>.</returns>
        /// <exception cref="NullReferenceException">The address of <paramref name="location1"/> is a null pointer.</exception>
        public static long Add(ref long location1, long value) =>
            ExchangeAdd(ref location1, value) + value;

        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int ExchangeAdd(ref int location1, int value)
        {
#if TARGET_X86 || TARGET_AMD64 || TARGET_ARM64 || TARGET_RISCV64
            return ExchangeAdd(ref location1, value); // Must expand intrinsic
#else
            if (Unsafe.IsNullRef(ref location1))
                ThrowHelper.ThrowNullReferenceException();
            return ExchangeAdd32(ref location1, value);
#endif
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern int ExchangeAdd32(ref int location1, int value);

        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static long ExchangeAdd(ref long location1, long value)
        {
#if TARGET_AMD64 || TARGET_ARM64 || TARGET_RISCV64
            return ExchangeAdd(ref location1, value); // Must expand intrinsic
#else
            if (Unsafe.IsNullRef(ref location1))
                ThrowHelper.ThrowNullReferenceException();
            return ExchangeAdd64(ref location1, value);
#endif
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern long ExchangeAdd64(ref long location1, long value);
        #endregion

        #region Read
        /// <summary>Returns a 64-bit signed value, loaded as an atomic operation.</summary>
        /// <param name="location">The 64-bit value to be loaded.</param>
        /// <returns>The loaded value.</returns>
        public static long Read(ref readonly long location) =>
            CompareExchange(ref Unsafe.AsRef(in location), 0, 0);
        #endregion

        #region MemoryBarrierProcessWide
        [LibraryImport(RuntimeHelpers.QCall, EntryPoint = "Interlocked_MemoryBarrierProcessWide")]
        private static partial void _MemoryBarrierProcessWide();

        /// <summary>Provides a process-wide memory barrier that ensures that reads and writes from any CPU cannot move across the barrier.</summary>
        public static void MemoryBarrierProcessWide() => _MemoryBarrierProcessWide();
        #endregion
    }
}
