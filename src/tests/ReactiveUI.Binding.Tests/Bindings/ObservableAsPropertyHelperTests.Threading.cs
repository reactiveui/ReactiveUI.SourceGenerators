// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding.Tests.TestModels;
using ReactiveUI.Primitives.Concurrency;

namespace ReactiveUI.Binding.Tests.Bindings;

/// <summary>
/// Tests for how <see cref="ObservableAsPropertyHelper{T}"/> schedules delivery: inline and serialized with no
/// scheduler (or the immediate one), and posted through a supplied <see cref="ISequencer"/> otherwise.
/// </summary>
public sealed partial class ObservableAsPropertyHelperTests
{
    /// <summary>How long a synchronization wait is allowed to run before a deadlocked test fails instead of hanging.</summary>
    private static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds(10);

    /// <summary>With a real scheduler, nothing is delivered until the scheduler runs its pending work.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Scheduler_CustomSequencer_DoesNotDeliverUntilSequencerRuns()
    {
        var scheduler = new ManualSequencer();
        var source = new Subject<int>();
        var received = new List<int>();

        using var fixture = new ObservableAsPropertyHelper<int>(source, received.Add, 0, scheduler);

        // The initial value delivery is scheduled too, so nothing has run yet.
        await Assert.That(received).IsEmpty();

        _ = scheduler.RunPending();
        await Assert.That(received).IsEquivalentTo([0]);

        source.OnNext(SuppliedValue);
        await Assert.That(received).IsEquivalentTo([0]);

        _ = scheduler.RunPending();
        await Assert.That(received).IsEquivalentTo([0, SuppliedValue]);
    }

    /// <summary>The immediate sequencer is treated the same as no scheduler: delivery happens inline.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Scheduler_ImmediateSequencer_IsTreatedAsInlineDelivery()
    {
        var source = new Subject<int>();
        var received = new List<int>();

        using var fixture = new ObservableAsPropertyHelper<int>(source, received.Add, 0, Sequencer.Immediate);

        await Assert.That(received).IsEquivalentTo([0]);

        source.OnNext(SuppliedValue);
        await Assert.That(received).IsEquivalentTo([0, SuppliedValue]);
    }

    /// <summary>
    /// A value produced reentrantly from inside the changed callback is queued behind the delivery in progress
    /// rather than nested inside it, so the callback never runs at more than one level of depth.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Serialization_ReentrantProduceInsideCallback_QueuesRatherThanNesting()
    {
        const int ReentrantValue = 2;
        var source = new Subject<int>();
        var received = new List<int>();
        var depth = 0;
        var maxDepth = 0;

        using var fixture = new ObservableAsPropertyHelper<int>(source, OnChanged, 0);
        received.Clear();

        source.OnNext(1);

        await Assert.That(received).IsEquivalentTo([1, ReentrantValue]);
        await Assert.That(maxDepth).IsEqualTo(1);
        return;

        void OnChanged(int value)
        {
            depth++;
            maxDepth = Math.Max(maxDepth, depth);
            received.Add(value);

            if (value == 1)
            {
                source.OnNext(ReentrantValue);
            }

            depth--;
        }
    }

    /// <summary>
    /// A value produced on another thread while a delivery is running on the first thread is queued rather than
    /// blocking; it is then delivered by the thread already running the delivery, once that delivery finishes.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Serialization_ValueFromAnotherThreadWhileDeliveryRuns_QueuesAndDeliversAfterOnTheDeliveringThread()
    {
        const int SecondThreadValue = 2;
        var source = new DirectObservable<int>();
        var received = new List<(int Value, int ThreadId)>();
        var enteredFirst = new ManualResetEventSlim(false);
        var releaseFirst = new ManualResetEventSlim(false);
        var concurrent = 0;
        var maxConcurrent = 0;

        using var fixture = new ObservableAsPropertyHelper<int>(source, OnChanged, 0);
        received.Clear();

        var firstThread = Task.Run(() => source.Emit(1));

        await Assert.That(enteredFirst.Wait(WaitTimeout)).IsTrue();

        var secondReturned = new ManualResetEventSlim(false);
        var secondThread = Task.Run(() =>
        {
            source.Emit(SecondThreadValue);
            secondReturned.Set();
        });

        // The second thread only enqueues its value behind the running delivery, so it returns without
        // waiting for the first delivery to be released.
        await Assert.That(secondReturned.Wait(WaitTimeout)).IsTrue();

        releaseFirst.Set();
        await firstThread;
        await secondThread;

        await Assert.That(received.ConvertAll(static entry => entry.Value)).IsEquivalentTo([1, SecondThreadValue]);
        await Assert.That(received[0].ThreadId).IsEqualTo(received[1].ThreadId);
        await Assert.That(maxConcurrent).IsEqualTo(1);
        return;

        void OnChanged(int value)
        {
            var now = Interlocked.Increment(ref concurrent);
            maxConcurrent = Math.Max(maxConcurrent, now);

            received.Add((value, Environment.CurrentManagedThreadId));

            if (value == 1)
            {
                enteredFirst.Set();
                _ = releaseFirst.Wait(WaitTimeout);
            }

            _ = Interlocked.Decrement(ref concurrent);
        }
    }

    /// <summary>
    /// A callback that throws propagates the exception to the producing thread and drops any value queued behind
    /// the one that faulted, rather than delivering it out of order after the failure.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Serialization_CallbackThrows_PropagatesAndDropsQueuedValuesButRecoversForLaterValues()
    {
        const int QueuedValue = 2;
        const int RecoveryValue = 3;
        var source = new Subject<int>();
        var received = new List<int>();

        using var fixture = new ObservableAsPropertyHelper<int>(source, OnChanged, 0);

        await Assert.That(() => source.OnNext(1)).Throws<InvalidOperationException>();

        // The queued value was queued reentrantly behind value 1's delivery, but that delivery faulted, so it is dropped.
        await Assert.That(received).IsEquivalentTo([0, 1]);

        source.OnNext(RecoveryValue);
        await Assert.That(received).IsEquivalentTo([0, 1, RecoveryValue]);
        return;

        void OnChanged(int value)
        {
            received.Add(value);

            if (value != 1)
            {
                return;
            }

            source.OnNext(QueuedValue);
            throw new InvalidOperationException("boom");
        }
    }

    /// <summary>An observable that hands calls straight to the observer, with no internal locking of its own.</summary>
    /// <typeparam name="T">The element type.</typeparam>
    private sealed class DirectObservable<T> : IObservable<T>
    {
        /// <summary>The subscribed observer, or null before <see cref="Subscribe"/> is called.</summary>
        private IObserver<T>? _observer;

        /// <inheritdoc/>
        public IDisposable Subscribe(IObserver<T> observer)
        {
            _observer = observer;
            return NoOpDisposable.Instance;
        }

        /// <summary>Pushes a value straight to the subscribed observer's <see cref="IObserver{T}.OnNext"/>.</summary>
        /// <param name="value">The value to push.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Emit(T value) => _observer!.OnNext(value);
    }
}
