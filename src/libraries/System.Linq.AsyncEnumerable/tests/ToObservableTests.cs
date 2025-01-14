// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Xunit;

namespace System.Linq.Tests
{
    public class ToObservableTests : AsyncEnumerableTests
    {
        [Fact]
        public void InvalidInputs_Throws()
        {
            AssertExtensions.Throws<ArgumentNullException>("source", () => AsyncEnumerable.ToObservable<int>(null));
        }

        [Fact]
        public async Task AllElementsPropagated()
        {
            foreach (int length in new[] { 0, 1, 1024 })
            {
                foreach (IAsyncEnumerable<int> source in CreateSources(Enumerable.Range(0, length).ToArray()))
                {
                    IObservable<int> observable = source.ToObservable();
                    Assert.NotNull(observable);

                    var subscriber = new ChannelObserver<int>();
                    using IDisposable d = observable.Subscribe(subscriber);
                    Assert.NotNull(d);

                    await AssertEqual(
                        source,
                        subscriber.ReadAllAsync());
                }
            }
        }

        [Fact]
        public async Task DisposeRegistration_Unregisters()
        {
            TaskCompletionSource<bool> iteratorRunning = new();
            TaskCompletionSource<bool> iteratorWaiting = new();

            IObservable<int> observable = YieldAfterSignal(iteratorRunning, iteratorWaiting.Task).ToObservable();
            Assert.NotNull(observable);
            Assert.False(iteratorRunning.Task.IsCompleted);

            var subscriber = new ChannelObserver<int>();
            IDisposable d = observable.Subscribe(subscriber);
            Assert.NotNull(d);
            Assert.True(iteratorRunning.Task.IsCompleted);

            d.Dispose();
            iteratorWaiting.SetResult(true);

            Assert.Equal(0, await subscriber.ReadAllAsync().CountAsync());
        }

        [Fact]
        public async Task OnError_Propagates()
        {
            TaskCompletionSource<bool> iteratorRunning = new();
            TaskCompletionSource<bool> iteratorWaiting = new();

            IObservable<int> observable = YieldAfterSignal(iteratorRunning, iteratorWaiting.Task).ToObservable();
            Assert.NotNull(observable);
            Assert.False(iteratorRunning.Task.IsCompleted);

            var subscriber = new ChannelObserver<int>();
            using IDisposable d = observable.Subscribe(subscriber);
            Assert.NotNull(d);
            Assert.True(iteratorRunning.Task.IsCompleted);

            iteratorWaiting.SetException(new FormatException());

            await Assert.ThrowsAsync<FormatException>(async () => await subscriber.ReadAllAsync().CountAsync());
        }

        private static async IAsyncEnumerable<int> YieldAfterSignal(
            TaskCompletionSource<bool> tcs, Task t, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            tcs.SetResult(true);
            await t;
            cancellationToken.ThrowIfCancellationRequested();
            yield return 42;
        }

        private sealed class ChannelObserver<T> : IObserver<T>
        {
            private readonly Channel<T> _channel = Channel.CreateUnbounded<T>();

            public IAsyncEnumerable<T> ReadAllAsync() => _channel.Reader.ReadAllAsync();

            public void OnCompleted() => _channel.Writer.Complete();
            public void OnError(Exception error) => _channel.Writer.Complete(error);
            public void OnNext(T value) => _channel.Writer.WriteAsync(value).AsTask().GetAwaiter().GetResult();
        }
    }
}
