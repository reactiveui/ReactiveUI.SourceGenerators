// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding.Observables;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.Observables;

/// <summary>
/// Tests that <see cref="PropertyObservable{T}"/> serializes the initial emit it performs while the
/// subscription is still being built against <see cref="INotifyPropertyChanged.PropertyChanged"/>
/// notifications arriving at the same time.
/// </summary>
/// <remarks>
/// Every interleaving here is forced rather than raced for: the source's event accessor, the property
/// read and the downstream observer are each used as a hook to drive a notification into one specific
/// point of the subscription's construction. Those are the same points a thread can be descheduled at,
/// so the schedules are real ones - they are only made to happen every run instead of occasionally.
/// </remarks>
public class PropertyObservableInitialEmitSerializationTests
{
    /// <summary>The property value present before any competing write.</summary>
    private const string InitialName = "Alice";

    /// <summary>The property value a competing thread writes.</summary>
    private const string ReplacementName = "Bob";

    /// <summary>The property value written after a read has failed.</summary>
    private const string ThirdName = "Carol";

    /// <summary>How long the initial emit gives a competing thread to hand off its notification.</summary>
    private const int InterleaveWindowMilliseconds = 500;

    /// <summary>
    /// Subscriptions the unforced sweep builds. Sized from measurement: against unserialized code this
    /// count reports the defect on every target framework with a wide margin, where a tenth of it misses
    /// on some of them because the early iterations run before the loop is fully optimized.
    /// </summary>
    private const int SweepIterations = 10_000;

    /// <summary>
    /// The reported defect: a notification landing after the handler is attached but before the
    /// constructor has read and emitted delivers the same value twice, breaking the
    /// no-consecutive-duplicates contract the caller asked for with <c>distinctUntilChanged: true</c>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Subscribe_PropertyChangedRaisedOnAnotherThreadWhileAttaching_EmitsTheValueOnce()
    {
        var source = new HookedViewModel { Name = InitialName };
        var recorder = new EmissionRecorder<string?>();

        // Drive a whole mutation through the source the instant the handler is attached, so the handler
        // has already emitted by the time the constructor reads the property.
        source.AfterHandlerAttached = () => RunToCompletionOnAnotherThread(() => source.Name = ReplacementName);

        var observable = new PropertyObservable<string?>(
            source,
            nameof(HookedViewModel.Name),
            static x => ((HookedViewModel)x).Name,
            distinctUntilChanged: true);

        using (observable.Subscribe(recorder))
        {
            await AssertNoErrors(recorder);
            await AssertSequence(recorder.Snapshot(), ReplacementName);
        }
    }

    /// <summary>
    /// The same defect reached re-entrantly rather than across threads: the property read the
    /// constructor performs for its initial emit itself raises
    /// <see cref="INotifyPropertyChanged.PropertyChanged"/>, so the handler runs part-way through
    /// construction on the subscribing thread. This also pins that a notification raised on the emitting
    /// thread is handed to the running emit rather than nested inside it.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Subscribe_PropertyChangedRaisedReentrantlyDuringInitialRead_EmitsTheValueOnce()
    {
        var source = new HookedViewModel { Name = InitialName };
        var recorder = new EmissionRecorder<string?>();
        var raised = false;

        string? ReadAndNotifyOnce(INotifyPropertyChanged instance)
        {
            if (!raised)
            {
                raised = true;
                source.RaisePropertyChanged(nameof(HookedViewModel.Name));
            }

            return ((HookedViewModel)instance).Name;
        }

        var observable = new PropertyObservable<string?>(
            source,
            nameof(HookedViewModel.Name),
            ReadAndNotifyOnce,
            distinctUntilChanged: true);

        using (observable.Subscribe(recorder))
        {
            await AssertNoErrors(recorder);
            await AssertSequence(recorder.Snapshot(), InitialName);
        }
    }

    /// <summary>
    /// A competing writer hands off its notification while the initial observer waits for that writer.
    /// The delivery gate emits the latest value after the initial observer returns.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Subscribe_PropertyChangedRaisedOnAnotherThreadDuringInitialEmit_HandsOffWithoutOverlap()
    {
        var source = new HookedViewModel { Name = InitialName };
        using var competitorStarted = new ManualResetEventSlim(false);
        Thread? competitor = null;
        var competitorFinishedDuringInitialEmit = false;

        // Runs from inside the downstream call of the initial emit, which is the window no other emit may enter.
        var recorder = new EmissionRecorder<string?>
        {
            OnFirstValue = () =>
            {
                competitor = new(() =>
                {
                    competitorStarted.Set();
                    source.Name = ReplacementName;
                }) { IsBackground = true };

                competitor.Start();
                competitorStarted.Wait();
                competitorFinishedDuringInitialEmit = competitor.Join(InterleaveWindowMilliseconds);
            },
        };

        var observable = new PropertyObservable<string?>(
            source,
            nameof(HookedViewModel.Name),
            static instance => ((HookedViewModel)instance).Name,
            distinctUntilChanged: true);

        using (observable.Subscribe(recorder))
        {
            competitor!.Join();

            await AssertNoErrors(recorder);
            await Assert.That(competitorFinishedDuringInitialEmit).IsTrue();
            await Assert.That(recorder.MaxConcurrentEmissions).IsEqualTo(1);
            await AssertSequence(recorder.Snapshot(), InitialName, ReplacementName);
        }
    }

    /// <summary>An observer that changes the property it observes gets the change after its own call returns.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task OnNext_ObserverChangesTheObservedProperty_DeliversTheChangeAfterTheObserverReturns()
    {
        var source = new HookedViewModel { Name = InitialName };
        var recorder = new EmissionRecorder<string?> { OnFirstValue = () => source.Name = ReplacementName };

        var observable = new PropertyObservable<string?>(
            source,
            nameof(HookedViewModel.Name),
            static x => ((HookedViewModel)x).Name,
            distinctUntilChanged: true);

        using (observable.Subscribe(recorder))
        {
            await AssertNoErrors(recorder);
            await Assert.That(recorder.MaxConcurrentEmissions).IsEqualTo(1);
            await AssertSequence(recorder.Snapshot(), InitialName, ReplacementName);
        }
    }

    /// <summary>
    /// A read that throws reaches the thread that raised the change, and the subscription keeps working:
    /// the next change still emits.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task OnPropertyChanged_ReadThrows_TheNextChangeStillEmits()
    {
        var source = new HookedViewModel { Name = InitialName };
        var recorder = new EmissionRecorder<string?>();
        var failNextRead = false;

        string? ReadOrFail(INotifyPropertyChanged instance)
        {
            if (!failNextRead)
            {
                return ((HookedViewModel)instance).Name;
            }

            failNextRead = false;
            throw new InvalidOperationException(nameof(ReadOrFail));
        }

        var observable = new PropertyObservable<string?>(
            source,
            nameof(HookedViewModel.Name),
            ReadOrFail,
            distinctUntilChanged: true);

        using (observable.Subscribe(recorder))
        {
            failNextRead = true;
            await Assert.That(() => source.Name = ReplacementName).ThrowsExactly<InvalidOperationException>();

            source.Name = ThirdName;

            await AssertSequence(recorder.Snapshot(), InitialName, ThirdName);
        }
    }

    /// <summary>
    /// The initial emit stays unconditional on an ordinary subscribe, including when the value equals
    /// the default for its type. The change applies the distinct-until-changed test to the initial emit
    /// as well, so this guards the first emission against being swallowed by that new test.
    /// </summary>
    /// <param name="distinctUntilChanged">Whether the subscription suppresses consecutive duplicates.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task Subscribe_ValueIsTheTypeDefault_StillEmitsTheInitialValue(bool distinctUntilChanged)
    {
        var source = new HookedViewModel { Name = null, Count = 0 };

        var nameRecorder = new EmissionRecorder<string?>();
        var nameObservable = new PropertyObservable<string?>(
            source,
            nameof(HookedViewModel.Name),
            static x => ((HookedViewModel)x).Name,
            distinctUntilChanged);

        var countRecorder = new EmissionRecorder<int>();
        var countObservable = new PropertyObservable<int>(
            source,
            nameof(HookedViewModel.Count),
            static x => ((HookedViewModel)x).Count,
            distinctUntilChanged);

        using (nameObservable.Subscribe(nameRecorder))
        {
            using (countObservable.Subscribe(countRecorder))
            {
                await Assert.That(nameRecorder.Snapshot()).Count().IsEqualTo(1);
                await Assert.That(nameRecorder.Snapshot()[0]).IsNull();
                await AssertSequence(countRecorder.Snapshot(), 0);
            }
        }
    }

    /// <summary>
    /// Each subscription carries its own distinct-until-changed state, so a later subscriber still
    /// receives the initial value even though an existing subscriber has already been given it, and a
    /// re-subscription after disposal receives it again.
    /// </summary>
    /// <param name="distinctUntilChanged">Whether the subscription suppresses consecutive duplicates.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task Subscribe_ValueAlreadyDeliveredToAnotherSubscriber_StillEmitsTheInitialValue(bool distinctUntilChanged)
    {
        var source = new HookedViewModel { Name = InitialName };
        var observable = new PropertyObservable<string?>(
            source,
            nameof(HookedViewModel.Name),
            static x => ((HookedViewModel)x).Name,
            distinctUntilChanged);

        var first = new EmissionRecorder<string?>();
        var second = new EmissionRecorder<string?>();
        var afterDisposal = new EmissionRecorder<string?>();

        using (observable.Subscribe(first))
        {
            using (observable.Subscribe(second))
            {
                await AssertSequence(first.Snapshot(), InitialName);
                await AssertSequence(second.Snapshot(), InitialName);
            }
        }

        using (observable.Subscribe(afterDisposal))
        {
            await AssertSequence(afterDisposal.Snapshot(), InitialName);
        }
    }

    /// <summary>
    /// An unforced sweep over the same window, reaching interleavings the forced tests do not enumerate.
    /// A competing thread writes a fresh value continuously while a subscription is built against it, so
    /// the subscribe always lands mid-storm, and no subscriber may ever see two equal values in a row.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Subscribe_ConcurrentWritesThroughoutSubscription_NeverEmitsConsecutiveDuplicates()
    {
        var iterationsThatEmitted = 0;
        var iterationsWithDuplicate = 0;

        for (var iteration = 0; iteration < SweepIterations; iteration++)
        {
            var source = new HookedViewModel { Count = 0 };
            using var competitorStarted = new ManualResetEventSlim(false);
            var stopped = 0;

            var competitor = new Thread(() =>
            {
                var next = 0;
                competitorStarted.Set();
                while (Volatile.Read(ref stopped) == 0)
                {
                    source.Count = ++next;
                }
            }) { IsBackground = true };

            competitor.Start();
            competitorStarted.Wait();

            var recorder = new EmissionRecorder<int>();
            var observable = new PropertyObservable<int>(
                source,
                nameof(HookedViewModel.Count),
                static x => ((HookedViewModel)x).Count,
                distinctUntilChanged: true);

            using (observable.Subscribe(recorder))
            {
                Volatile.Write(ref stopped, 1);
                competitor.Join();
            }

            var observed = recorder.Snapshot();
            if (observed.Count > 0)
            {
                iterationsThatEmitted++;
            }

            for (var index = 1; index < observed.Count; index++)
            {
                if (observed[index] != observed[index - 1])
                {
                    continue;
                }

                iterationsWithDuplicate++;
                break;
            }
        }

        await Assert.That(iterationsWithDuplicate).IsEqualTo(0);
        await Assert.That(iterationsThatEmitted).IsEqualTo(SweepIterations);
    }

    /// <summary>
    /// A source raises its event off a handler snapshot taken before the subscription attached, so the
    /// notification for that write never reaches the new handler. On the conventional write-then-raise
    /// setter the value is still not lost: the constructor reads the property after attaching, and that
    /// read is the backstop.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Subscribe_HandlerAttachesAfterTheRaiserSnapshotsItsDelegate_StillObservesTheWrittenValue()
    {
        var source = new ConventionalRaiseOrderViewModel { Name = InitialName };
        var recorder = new EmissionRecorder<string?>();
        using var subscriptions = new DisposableBag();

        source.BeforeRaise = () => subscriptions.Add(new PropertyObservable<string?>(
            source,
            nameof(ConventionalRaiseOrderViewModel.Name),
            static x => ((ConventionalRaiseOrderViewModel)x).Name,
            distinctUntilChanged: true).Subscribe(recorder));

        source.Name = ReplacementName;

        await Assert.That(source.HandlerWasInvoked).IsFalse();
        await AssertSequence(recorder.Snapshot(), ReplacementName);
    }

    /// <summary>
    /// The same missed notification against a source that raises before it writes. The backstop read
    /// runs too early there, so the subscriber is left holding the pre-write value - a gap owned by the
    /// source's ordering rather than by the subscription, and one no amount of serialization closes.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Subscribe_SourceRaisesPropertyChangedBeforeWritingTheField_ObservesThePreWriteValue()
    {
        var source = new RaiseBeforeWriteViewModel { Name = InitialName };
        var recorder = new EmissionRecorder<string?>();
        using var subscriptions = new DisposableBag();

        source.BeforeRaise = () => subscriptions.Add(new PropertyObservable<string?>(
            source,
            nameof(RaiseBeforeWriteViewModel.Name),
            static x => ((RaiseBeforeWriteViewModel)x).Name,
            distinctUntilChanged: true).Subscribe(recorder));

        source.Name = ReplacementName;

        await Assert.That(source.HandlerWasInvoked).IsFalse();
        await Assert.That(source.Name).IsEqualTo(ReplacementName);
        await AssertSequence(recorder.Snapshot(), InitialName);
    }

    /// <summary>Asserts that nothing was pushed to the observer's error channel.</summary>
    /// <typeparam name="T">The emitted element type.</typeparam>
    /// <param name="recorder">The recorder to inspect.</param>
    /// <returns>A <see cref="Task"/> representing the assertion.</returns>
    private static async Task AssertNoErrors<T>(EmissionRecorder<T> recorder) =>
        await Assert.That(recorder.ErrorSnapshot()).IsEmpty();

    /// <summary>Asserts that a recorded emission sequence matches the expected one exactly.</summary>
    /// <typeparam name="T">The emitted element type.</typeparam>
    /// <param name="actual">The recorded emissions.</param>
    /// <param name="expected">The emissions the subscription is required to produce, in order.</param>
    /// <returns>A <see cref="Task"/> representing the assertion.</returns>
    private static async Task AssertSequence<T>(IReadOnlyList<T> actual, params T[] expected)
    {
        await Assert.That(actual).Count().IsEqualTo(expected.Length);

        for (var index = 0; index < expected.Length; index++)
        {
            await Assert.That(actual[index]).IsEqualTo(expected[index]);
        }
    }

    /// <summary>Runs an action on a dedicated thread and waits for it to finish.</summary>
    /// <param name="action">The work to run away from the calling thread.</param>
    private static void RunToCompletionOnAnotherThread(Action action)
    {
        var thread = new Thread(action.Invoke) { IsBackground = true };
        thread.Start();
        thread.Join();
    }

    /// <summary>
    /// A view model whose event accessor exposes the instant a handler becomes attached, which is where
    /// the subscribe-time window opens.
    /// </summary>
    private sealed class HookedViewModel : INotifyPropertyChanged
    {
        /// <summary>Serializes handler registration against deregistration.</summary>
        private readonly Lock _handlerGate = new();

        /// <summary>The registered handlers.</summary>
        private PropertyChangedEventHandler? _handlers;

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged
        {
            add
            {
                lock (_handlerGate)
                {
                    _handlers += value;
                }

                var hook = AfterHandlerAttached;
                AfterHandlerAttached = null;
                hook?.Invoke();
            }

            remove
            {
                lock (_handlerGate)
                {
                    _handlers -= value;
                }
            }
        }

        /// <summary>Gets or sets a one-shot action run once a handler is added, from the adding thread.</summary>
        public Action? AfterHandlerAttached { get; set; }

        /// <summary>Gets or sets the observed reference-typed property, written before the event is raised.</summary>
        public string? Name
        {
            get;
            set
            {
                field = value;
                RaisePropertyChanged(nameof(Name));
            }
        }

        /// <summary>Gets or sets a value-typed property used to check the default-valued initial emit.</summary>
        public int Count
        {
            get;
            set
            {
                field = value;
                RaisePropertyChanged(nameof(Count));
            }
        }

        /// <summary>Raises <see cref="PropertyChanged"/> without writing anything.</summary>
        /// <param name="propertyName">The property to report.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RaisePropertyChanged(string propertyName) => _handlers?.Invoke(this, new(propertyName));
    }

    /// <summary>
    /// A view model with the conventional raise order - write the field, snapshot the handlers, raise -
    /// exposing the point between the snapshot and the raise.
    /// </summary>
    private sealed class ConventionalRaiseOrderViewModel : INotifyPropertyChanged
    {
        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>Gets or sets a one-shot action run after the handler snapshot is taken and before it is invoked.</summary>
        public Action? BeforeRaise { get; set; }

        /// <summary>Gets a value indicating whether the raise reached any handler.</summary>
        public bool HandlerWasInvoked { get; private set; }

        /// <summary>Gets or sets the observed property.</summary>
        public string? Name
        {
            get;
            set
            {
                field = value;

                var snapshot = PropertyChanged;
                var hook = BeforeRaise;
                BeforeRaise = null;
                hook?.Invoke();

                if (snapshot is null)
                {
                    return;
                }

                HandlerWasInvoked = true;
                snapshot(this, new(nameof(Name)));
            }
        }
    }

    /// <summary>
    /// A view model that raises before it writes, which is the ordering under which the subscription's
    /// post-attach read cannot serve as a backstop.
    /// </summary>
    private sealed class RaiseBeforeWriteViewModel : INotifyPropertyChanged
    {
        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>Gets or sets a one-shot action run after the handler snapshot is taken and before it is invoked.</summary>
        public Action? BeforeRaise { get; set; }

        /// <summary>Gets a value indicating whether the raise reached any handler.</summary>
        public bool HandlerWasInvoked { get; private set; }

        /// <summary>Gets or sets the observed property.</summary>
        public string? Name
        {
            get;
            set
            {
                var snapshot = PropertyChanged;
                var hook = BeforeRaise;
                BeforeRaise = null;
                hook?.Invoke();

                if (snapshot is not null)
                {
                    HandlerWasInvoked = true;
                    snapshot(this, new(nameof(Name)));
                }

                field = value;
            }
        }
    }
}
