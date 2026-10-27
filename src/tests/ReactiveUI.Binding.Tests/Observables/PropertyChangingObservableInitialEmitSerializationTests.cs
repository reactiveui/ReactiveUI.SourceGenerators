// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding.Observables;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.Observables;

/// <summary>
/// Tests that <see cref="PropertyChangingObservable{T}"/> serializes the initial emit it performs while
/// the subscription is still being built against <see cref="INotifyPropertyChanging.PropertyChanging"/>
/// notifications arriving at the same time.
/// </summary>
/// <remarks>
/// The window under test is the gap between the constructor reading the property and delivering that
/// read downstream. A thread can be descheduled there, and these tests force that schedule by driving
/// the competing writes from inside the property read itself.
/// </remarks>
public class PropertyChangingObservableInitialEmitSerializationTests
{
    /// <summary>The value the second competing write stores.</summary>
    private const int SecondWrite = 2;

    /// <summary>
    /// A source that raises before it writes - the conventional shape - still lets a stale initial emit
    /// land after a newer one once two writes pass through the constructor's read-to-emit gap. The
    /// serialization is therefore load-bearing on this type, not merely a consistency measure.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Subscribe_ConventionalSourceWritesDuringInitialEmit_DoesNotDeliverAStaleValueLast()
    {
        var source = new RaiseThenWriteViewModel { Version = 0 };
        var recorder = new EmissionRecorder<int>();

        using var competitor = new InitialEmitCompetitor(version => source.Version = version);

        var observable = new PropertyChangingObservable<int>(
            source,
            nameof(RaiseThenWriteViewModel.Version),
            competitor.CreateContendedRead(() => source.Version));

        using (observable.Subscribe(recorder))
        {
            competitor.WaitForCompletion();

            // The competing emits are held behind the initial emit, so the initial 0 lands first and the
            // pre-change values that follow it only ever move forward.
            await AssertSequence(recorder.Snapshot(), 0, 0, 1);
        }
    }

    /// <summary>
    /// A source that writes before raising its before-change event - the shape the change calls
    /// atypical - is the case the serialization is claimed to guard. Without it the handler emits both
    /// written values before the initial emit lands, leaving the oldest value last.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Subscribe_AtypicalSourceWritesBeforeRaising_DoesNotDeliverAStaleValueLast()
    {
        var source = new WriteThenRaiseViewModel { Version = 0 };
        var recorder = new EmissionRecorder<int>();

        using var competitor = new InitialEmitCompetitor(version => source.Version = version);

        var observable = new PropertyChangingObservable<int>(
            source,
            nameof(WriteThenRaiseViewModel.Version),
            competitor.CreateContendedRead(() => source.Version));

        using (observable.Subscribe(recorder))
        {
            competitor.WaitForCompletion();

            await AssertSequence(recorder.Snapshot(), 0, 1, SecondWrite);
        }
    }

    /// <summary>
    /// The initial emit stays unconditional on an ordinary subscribe, including for a value equal to the
    /// default for its type, and every subscriber receives its own.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Subscribe_NoConcurrentNotification_EmitsTheInitialValueToEverySubscriber()
    {
        var source = new RaiseThenWriteViewModel { Version = 0 };
        var observable = new PropertyChangingObservable<int>(
            source,
            nameof(RaiseThenWriteViewModel.Version),
            static x => ((RaiseThenWriteViewModel)x).Version);

        var first = new EmissionRecorder<int>();
        var second = new EmissionRecorder<int>();

        using (observable.Subscribe(first))
        {
            using (observable.Subscribe(second))
            {
                await AssertSequence(first.Snapshot(), 0);
                await AssertSequence(second.Snapshot(), 0);
            }
        }
    }

    /// <summary>Asserts that a recorded emission sequence matches the expected one exactly.</summary>
    /// <param name="actual">The recorded emissions.</param>
    /// <param name="expected">The emissions the subscription is required to produce, in order.</param>
    /// <returns>A <see cref="Task"/> representing the assertion.</returns>
    private static async Task AssertSequence(IReadOnlyList<int> actual, params int[] expected)
    {
        await Assert.That(actual).Count().IsEqualTo(expected.Length);

        for (var index = 0; index < expected.Length; index++)
        {
            await Assert.That(actual[index]).IsEqualTo(expected[index]);
        }
    }

    /// <summary>
    /// Drives two competing property writes from inside the subscription's initial property read, so
    /// both notifications fall in the constructor's read-to-emit gap.
    /// </summary>
    private sealed class InitialEmitCompetitor : IDisposable
    {
        /// <summary>
        /// How long to give the competing thread to complete its emits while the initial read and emit
        /// are still in progress. A serialized subscription blocks it for the whole window, so the wait
        /// always expires; an unserialized one lets it run to completion in microseconds.
        /// </summary>
        private const int InterleaveWindowMilliseconds = 500;

        /// <summary>Signals that the competing thread is running and about to write.</summary>
        private readonly ManualResetEventSlim _started = new(false);

        /// <summary>The thread performing the competing writes.</summary>
        private readonly Thread _thread;

        /// <summary>
        /// The latch claiming the right to drive the contention: zero until a caller claims it, one
        /// afterwards, so later reads run plainly. It is an interlocked integer rather than a boolean
        /// because the read it guards is the very code a subscription that stopped serializing would let
        /// two threads into at once, where a check-then-set would start the thread twice.
        /// </summary>
        private int _contended;

        /// <summary>Initializes a new instance of the <see cref="InitialEmitCompetitor"/> class.</summary>
        /// <param name="write">Writes the observed property.</param>
        public InitialEmitCompetitor(Action<int> write) => _thread = new(() =>
        {
            _started.Set();
            write(1);
            write(SecondWrite);
        }) { IsBackground = true };

        /// <summary>
        /// Builds a property read that, the first time it runs, releases the competing thread and holds
        /// for it before returning the value read on entry.
        /// </summary>
        /// <param name="read">Reads the current property value.</param>
        /// <returns>The property read to hand to the observable.</returns>
        public Func<INotifyPropertyChanging, int> CreateContendedRead(Func<int> read) => source =>
        {
            var valueOnEntry = read();

            if (Interlocked.Exchange(ref _contended, 1) != 0)
            {
                return valueOnEntry;
            }

            _thread.Start();
            _started.Wait();
            _ = _thread.Join(InterleaveWindowMilliseconds);

            return valueOnEntry;
        };

        /// <summary>Waits for the competing writes to finish.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WaitForCompletion() => _thread.Join();

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => _started.Dispose();
    }

    /// <summary>A view model with the conventional before-change ordering: raise, then write.</summary>
    private sealed class RaiseThenWriteViewModel : INotifyPropertyChanging
    {
        /// <inheritdoc/>
        public event PropertyChangingEventHandler? PropertyChanging;

        /// <summary>Gets or sets the observed property, raising the event before the write lands.</summary>
        public int Version
        {
            get;
            set
            {
                PropertyChanging?.Invoke(this, new(nameof(Version)));
                field = value;
            }
        }
    }

    /// <summary>A view model that writes before raising its before-change event, which is atypical.</summary>
    private sealed class WriteThenRaiseViewModel : INotifyPropertyChanging
    {
        /// <inheritdoc/>
        public event PropertyChangingEventHandler? PropertyChanging;

        /// <summary>Gets or sets the observed property, raising the event after the write has landed.</summary>
        public int Version
        {
            get;
            set
            {
                field = value;
                PropertyChanging?.Invoke(this, new(nameof(Version)));
            }
        }
    }
}
