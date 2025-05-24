// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

namespace System.Threading.Channels
{
    internal sealed class AsyncOperationQueue<T>
    {
        private AsyncOperation<T>? _head;
        private AsyncOperation<T>? _tail;

        public AsyncOperationQueue(object syncObj) => SyncObj = syncObj;

        public object SyncObj { get; }

        public bool IsEmpty
        {
            get
            {
                Debug.Assert((_head is null) == (_tail is null));

                return _head is null;
            }
        }

        public bool TryDequeue([NotNullWhen(true)] out AsyncOperation<T>? op)
        {
            if (_head is { } head)
            {
                _head = (AsyncOperation<T>?)head.Next;

                if (head == _tail)
                {
                    Debug.Assert(head.Next is null && head.Previous is null);
                    _tail = null;
                }
                else
                {
                    Debug.Assert(head.Next is not null);
                    head.Next.Previous = null;

                    head.Next = head.Previous = null;
                }

                op = head;
                op.Parent = null;
                return true;
            }

            op = null;
            return false;
        }

        public void Enqueue(AsyncOperation<T> op)
        {
            Debug.Assert(op.Next is null && op.Previous is null);

            if (_head is null)
            {
                Debug.Assert(_tail is null);
                _head = _tail = op;
            }
            else
            {
                Debug.Assert(_tail is not null);
                op.Previous = _tail;
                _tail.Next = op;
                _tail = op;
            }

            op.Parent = this;
        }

        public void Remove(AsyncOperation<T> op)
        {
            if (op.Next is { } next)
            {
                next.Previous = op.Previous;
            }

            if (op.Previous is { } previous)
            {
                previous.Next = op.Next;
            }

            if (_head == op)
            {
                _head = op.Next;
            }

            if (_tail == op)
            {
                _tail = op.Previous;
            }

            op.Next = op.Previous = null;
            op.Parent = null;
        }
    }
}
