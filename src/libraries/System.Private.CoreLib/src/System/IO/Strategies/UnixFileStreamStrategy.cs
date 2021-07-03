// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

namespace System.IO.Strategies
{
    internal sealed partial class UnixFileStreamStrategy : OSFileStreamStrategy
    {
        internal UnixFileStreamStrategy(SafeFileHandle handle, FileAccess access, FileShare share) :
            base(handle, access, share)
        {
        }

        internal UnixFileStreamStrategy(string path, FileMode mode, FileAccess access, FileShare share, FileOptions options, long preallocationSize) :
            base(path, mode, access, share, options, preallocationSize)
        {
        }

        internal override bool IsAsync => _fileHandle.IsAsync;

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
            ReadAsync(new Memory<byte>(buffer, offset, count), cancellationToken).AsTask();

        public override ValueTask<int> ReadAsync(Memory<byte> destination, CancellationToken cancellationToken)
        {
            if (!CanRead)
            {
                ThrowHelper.ThrowNotSupportedException_UnreadableStream();
            }

            long positionBefore = -1;
            if (CanSeek)
            {
                long len = Length;

                if (len == 0)
                {
                    return Read0LengthFileAsync(destination, cancellationToken);
                }

                positionBefore = _filePosition;
                if (positionBefore + destination.Length > len)
                {
                    destination = positionBefore <= len ?
                        destination.Slice(0, (int)(len - positionBefore)) :
                        default;
                }

                _filePosition += destination.Length;
            }

            return RandomAccess.ReadAtOffsetAsync(_fileHandle, destination, positionBefore, cancellationToken);

            ValueTask<int> Read0LengthFileAsync(Memory<byte> destination, CancellationToken cancellationToken)
            {
                // Some special file systems, e.g. procfs, report files as being regular and seekable
                // but always return a zero length.  Thus if Length is 0, we can't trust that to mean
                // we should trim the destination buffer to 0 in order to not advance beyond the end
                // of the file.  Instead, for this case we fall back to just invoking Read asynchronously,
                // as it doesn't employ such trimming and instead updates the position after the
                // read completes.  The primary reason for updating the position before with async
                // operations is to support concurrently running operations, but doing that would typically
                // require knowing the length to be able to partition the read, and if you query Length
                // to determine that, you wouldn't know how big to make each partition.

                return new ValueTask<int>(Task.Factory.StartNew(static state =>
                {
                    var args = ((UnixFileStreamStrategy thisRef, Memory<byte> destination))state!;
                    return args.thisRef.Read(args.destination.Span);
                }, (this, destination), cancellationToken, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default));
            }
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
            WriteAsync(new ReadOnlyMemory<byte>(buffer, offset, count), cancellationToken).AsTask();

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> source, CancellationToken cancellationToken)
        {
            if (!CanWrite)
            {
                ThrowHelper.ThrowNotSupportedException_UnwritableStream();
            }

            long positionBefore = -1;
            if (CanSeek)
            {
                positionBefore = _filePosition;
                _filePosition += source.Length;
                UpdateLengthOnChangePosition();
            }

#pragma warning disable CA2012 // The analyzer doesn't know the internal AsValueTask is safe
            return RandomAccess.WriteAtOffsetAsync(_fileHandle, source, positionBefore, cancellationToken).AsValueTask();
#pragma warning restore CA2012
        }

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            // Queue a single call to the synchronous CopyTo.  The base CopyToAsync employs an asynchronous read/write
            // loop, which means we'll be paying to queue each individual read/write rather than one queueing for everything.
            return Task.Factory.StartNew(static state =>
            {
                var args = ((UnixFileStreamStrategy thisRef, Stream destination, int bufferSize))state!;
                args.thisRef.CopyTo(args.destination, args.bufferSize);
            }, (this, destination, bufferSize), cancellationToken, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
        }
    }
}
