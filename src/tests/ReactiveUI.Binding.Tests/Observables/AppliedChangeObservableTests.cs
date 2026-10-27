// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ReactiveUI.Binding.Observables;

namespace ReactiveUI.Binding.Tests.Observables;

/// <summary>Tests for <see cref="AppliedChangeObservable"/>, the changes a two-way binding wrote.</summary>
public class AppliedChangeObservableTests
{
    /// <summary>A value a binding reports having written.</summary>
    private const string FirstValue = "first";

    /// <summary>A second value, used where the order changes arrive in matters.</summary>
    private const string SecondValue = "second";

    /// <summary>What an observer present for the first change alone receives.</summary>
    private static readonly string[] FirstOnly = [FirstValue];

    /// <summary>What an observer present for both changes receives.</summary>
    private static readonly string[] FirstThenSecond = [FirstValue, SecondValue];

    /// <summary>What an observer that joined after the first change receives.</summary>
    private static readonly string[] SecondOnly = [SecondValue];

    /// <summary>The public boundary is needed only while a subscriber is present.</summary>
    /// <returns>The asynchronous test operation.</returns>
    [Test]
    public async Task HasObservers_TracksSubscriptionLifetime()
    {
        var changes = new AppliedChangeObservable();
        await Assert.That(changes.HasObservers).IsFalse();
        var subscription = changes.Subscribe(new RecordingObserver());
        await Assert.That(changes.HasObservers).IsTrue();
        subscription.Dispose();
        await Assert.That(changes.HasObservers).IsFalse();
    }

    /// <summary>Reporting a change with nobody watching is what a binding usually does.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OnNext_WithNoObservers_ReportsToNobodyAndDoesNotThrow()
    {
        var changes = new AppliedChangeObservable();

        await Assert.That(() => changes.OnNext(new(FirstValue, true))).ThrowsNothing();
    }

    /// <summary>Every observer sees the change, which is what sharing one upstream subscription is for.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OnNext_WithTwoObservers_ReportsToBoth()
    {
        var changes = new AppliedChangeObservable();
        var first = new RecordingObserver();
        var second = new RecordingObserver();

        using (changes.Subscribe(first))
        using (changes.Subscribe(second))
        {
            changes.OnNext(new(FirstValue, true));
        }

        await Assert.That(first.Received).IsEquivalentTo(FirstOnly);
        await Assert.That(second.Received).IsEquivalentTo(FirstOnly);
    }

    /// <summary>An observer that has unsubscribed hears nothing further, and the others carry on.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Dispose_OfOneSubscription_LeavesTheOthersReporting()
    {
        var changes = new AppliedChangeObservable();
        var dropped = new RecordingObserver();
        var kept = new RecordingObserver();

        var droppedSubscription = changes.Subscribe(dropped);
        using var keptSubscription = changes.Subscribe(kept);

        changes.OnNext(new(FirstValue, true));
        droppedSubscription.Dispose();
        changes.OnNext(new(SecondValue, false));

        await Assert.That(dropped.Received).IsEquivalentTo(FirstOnly);
        await Assert.That(kept.Received).IsEquivalentTo(FirstThenSecond);
    }

    /// <summary>Disposing twice drops one place, not two.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Dispose_CalledTwice_DropsOnePlace()
    {
        var changes = new AppliedChangeObservable();
        var kept = new RecordingObserver();

        var droppedSubscription = changes.Subscribe(new RecordingObserver());
        using var keptSubscription = changes.Subscribe(kept);

        droppedSubscription.Dispose();
        droppedSubscription.Dispose();
        changes.OnNext(new(FirstValue, true));

        await Assert.That(kept.Received).IsEquivalentTo(FirstOnly);
    }

    /// <summary>
    /// A change already being delivered walks the observers it started with, so a subscription taken from
    /// inside a delivery joins the next one rather than disturbing this one.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Subscribe_DuringDelivery_JoinsTheNextChange()
    {
        var changes = new AppliedChangeObservable();
        var joining = new RecordingObserver();
        var subscriptions = new List<IDisposable>();

        using var first = changes.Subscribe(new DelegateObserver(() => subscriptions.Add(changes.Subscribe(joining))));

        changes.OnNext(new(FirstValue, true));
        changes.OnNext(new(SecondValue, false));

        foreach (var subscription in subscriptions)
        {
            subscription.Dispose();
        }

        await Assert.That(joining.Received).IsEquivalentTo(SecondOnly);
    }

    /// <summary>Dropping an observer that was never watching leaves the ones that are alone.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Remove_OfAnObserverThatNeverSubscribed_LeavesTheOthersReporting()
    {
        var changes = new AppliedChangeObservable();
        var kept = new RecordingObserver();

        using var keptSubscription = changes.Subscribe(kept);

        changes.Remove(new RecordingObserver());
        changes.OnNext(new(FirstValue, true));

        await Assert.That(kept.Received).IsEquivalentTo(FirstOnly);
    }

    /// <summary>An observer is required, so asking to watch without one is refused.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Subscribe_WithNoObserver_ThrowsArgumentNullException()
    {
        var changes = new AppliedChangeObservable();

        await Assert.That(() => changes.Subscribe(null!)).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Records the values a binding reported having written.</summary>
    private sealed class RecordingObserver : IObserver<BindingChange>
    {
        /// <summary>Gets the values reported, in order.</summary>
        public List<string> Received { get; } = [];

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnNext(BindingChange value) => Received.Add((string)value.Value!);

        /// <inheritdoc/>
        public void OnError(Exception error)
        {
        }

        /// <inheritdoc/>
        public void OnCompleted()
        {
        }
    }

    /// <summary>Runs an action on every change, used to subscribe from inside a delivery.</summary>
    /// <param name="onNext">The action to run.</param>
    private sealed class DelegateObserver(Action onNext) : IObserver<BindingChange>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnNext(BindingChange value) => onNext();

        /// <inheritdoc/>
        public void OnError(Exception error)
        {
        }

        /// <inheritdoc/>
        public void OnCompleted()
        {
        }
    }
}
