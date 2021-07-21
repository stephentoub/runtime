// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

namespace System.Runtime.CompilerServices
{
    /// <summary>Provides an awaiter for a <see cref="ValueTask"/>.</summary>
    public readonly struct ValueTaskAwaiter : ICriticalNotifyCompletion, IStateMachineBoxAwareAwaiter
    {
        /// <summary>Shim used to invoke an <see cref="Action"/> passed as the state argument to a <see cref="Action{Object}"/>.</summary>
        internal static readonly Action<object?> s_invokeActionDelegate = static state =>
        {
            if (!(state is Action action))
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.state);
                return;
            }

            action();
        };

        /// <summary>The value being awaited.</summary>
        private readonly ValueTask _value;

        /// <summary>Initializes the awaiter.</summary>
        /// <param name="value">The value to be awaited.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ValueTaskAwaiter(in ValueTask value) => _value = value;

        /// <summary>Gets whether the <see cref="ValueTask"/> has completed.</summary>
        public bool IsCompleted
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _value.IsCompleted;
        }

        /// <summary>Gets the result of the ValueTask.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetResult() => _value.ThrowIfCompletedUnsuccessfully();

        /// <summary>Schedules the continuation action for this ValueTask.</summary>
        public void OnCompleted(Action continuation)
        {
            IValueTaskSource? source = _value._source;

            if (source is not null)
            {
                source.OnCompleted(s_invokeActionDelegate, continuation, _value._token, ValueTaskSourceOnCompletedFlags.UseSchedulingContext | ValueTaskSourceOnCompletedFlags.FlowExecutionContext);
            }
            else
            {
                Task.CompletedTask.GetAwaiter().OnCompleted(continuation);
            }
        }

        /// <summary>Schedules the continuation action for this ValueTask.</summary>
        public void UnsafeOnCompleted(Action continuation)
        {
            IValueTaskSource? source = _value._source;

            if (source is not null)
            {
                source.OnCompleted(s_invokeActionDelegate, continuation, _value._token, ValueTaskSourceOnCompletedFlags.UseSchedulingContext);
            }
            else
            {
                Task.CompletedTask.GetAwaiter().UnsafeOnCompleted(continuation);
            }
        }

        void IStateMachineBoxAwareAwaiter.AwaitUnsafeOnCompleted(IAsyncStateMachineBox box)
        {
            IValueTaskSource? source = _value._source;

            if (source is Task t)
            {
                TaskAwaiter.UnsafeOnCompletedInternal(t, box, continueOnCapturedContext: true);
            }
            else if (source is not null)
            {
                source.OnCompleted(ThreadPool.s_invokeAsyncStateMachineBox, box, _value._token, ValueTaskSourceOnCompletedFlags.UseSchedulingContext);
            }
            else
            {
                TaskAwaiter.UnsafeOnCompletedInternal(Task.CompletedTask, box, continueOnCapturedContext: true);
            }
        }
    }

    /// <summary>Provides an awaiter for a <see cref="ValueTask{TResult}"/>.</summary>
    public readonly struct ValueTaskAwaiter<TResult> : ICriticalNotifyCompletion, IStateMachineBoxAwareAwaiter
    {
        /// <summary>The value being awaited.</summary>
        private readonly ValueTask<TResult> _value;

        /// <summary>Initializes the awaiter.</summary>
        /// <param name="value">The value to be awaited.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ValueTaskAwaiter(in ValueTask<TResult> value) => _value = value;

        /// <summary>Gets whether the <see cref="ValueTask{TResult}"/> has completed.</summary>
        public bool IsCompleted
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _value.IsCompleted;
        }

        /// <summary>Gets the result of the ValueTask.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TResult GetResult() => _value.Result;

        /// <summary>Schedules the continuation action for this ValueTask.</summary>
        public void OnCompleted(Action continuation)
        {
            IValueTaskSource<TResult>? source = _value._source;

            if (source is not null)
            {
                source.OnCompleted(ValueTaskAwaiter.s_invokeActionDelegate, continuation, _value._token, ValueTaskSourceOnCompletedFlags.UseSchedulingContext | ValueTaskSourceOnCompletedFlags.FlowExecutionContext);
            }
            else
            {
                Task.CompletedTask.GetAwaiter().OnCompleted(continuation);
            }
        }

        /// <summary>Schedules the continuation action for this ValueTask.</summary>
        public void UnsafeOnCompleted(Action continuation)
        {
            IValueTaskSource<TResult>? source = _value._source;

            if (source is not null)
            {
                source.OnCompleted(ValueTaskAwaiter.s_invokeActionDelegate, continuation, _value._token, ValueTaskSourceOnCompletedFlags.UseSchedulingContext);
            }
            else
            {
                Task.CompletedTask.GetAwaiter().UnsafeOnCompleted(continuation);
            }
        }

        void IStateMachineBoxAwareAwaiter.AwaitUnsafeOnCompleted(IAsyncStateMachineBox box)
        {
            IValueTaskSource<TResult>? source = _value._source;

            if (source is Task<TResult> t)
            {
                TaskAwaiter.UnsafeOnCompletedInternal(t, box, continueOnCapturedContext: true);
            }
            else if (source is not null)
            {
                source.OnCompleted(ThreadPool.s_invokeAsyncStateMachineBox, box, _value._token, ValueTaskSourceOnCompletedFlags.UseSchedulingContext);
            }
            else
            {
                TaskAwaiter.UnsafeOnCompletedInternal(Task.CompletedTask, box, continueOnCapturedContext: true);
            }
        }
    }

    /// <summary>Internal interface used to enable optimizations from <see cref="AsyncTaskMethodBuilder"/>.</summary>>
    internal interface IStateMachineBoxAwareAwaiter
    {
        /// <summary>Invoked to set <see cref="ITaskCompletionAction.Invoke"/> of the <paramref name="box"/> as the awaiter's continuation.</summary>
        /// <param name="box">The box object.</param>
        void AwaitUnsafeOnCompleted(IAsyncStateMachineBox box);
    }
}
