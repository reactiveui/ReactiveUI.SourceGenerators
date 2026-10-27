// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ReactiveUI.Binding.Observables;

namespace ReactiveUI.Binding.Tests.Observables;

/// <summary>Unit tests for <see cref="UnchangingPropertyObservable{T}"/>.</summary>
public class UnchangingPropertyObservableTests
{
    /// <summary>The value the observation under test carries.</summary>
    private const string ObservedValue = "only";

    /// <summary>The single emission every subscriber is expected to receive.</summary>
    private static readonly string[] _expectedEmissions = [ObservedValue];

    /// <summary>The property's value reaches a subscriber as soon as it subscribes.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Subscribe_EmitsTheValueItWasBuiltWith()
    {
        var values = new List<string>();
        var observable = new UnchangingPropertyObservable<string>(ObservedValue);

        using var subscription = observable.Subscribe(new RecordingObserver<string>(values));

        await Assert.That(values).IsEquivalentTo(_expectedEmissions);
    }

    /// <summary>
    /// The observation never completes. A property that cannot notify has not ended, and a binding that saw
    /// completion would tear itself down while the property is still in use.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Subscribe_DoesNotComplete()
    {
        var observer = new RecordingObserver<string>([]);
        var observable = new UnchangingPropertyObservable<string>(ObservedValue);

        using var subscription = observable.Subscribe(observer);

        await Assert.That(observer.Completed).IsFalse();
    }

    /// <summary>Each subscriber is given the value, so a second one is not left waiting.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Subscribe_SecondSubscriber_AlsoReceivesTheValue()
    {
        var first = new List<string>();
        var second = new List<string>();
        var observable = new UnchangingPropertyObservable<string>(ObservedValue);

        using var firstSubscription = observable.Subscribe(new RecordingObserver<string>(first));
        using var secondSubscription = observable.Subscribe(new RecordingObserver<string>(second));

        await Assert.That(first).IsEquivalentTo(_expectedEmissions);
        await Assert.That(second).IsEquivalentTo(_expectedEmissions);
    }

    /// <summary>Disposing the handle is what a caller does with it, and it holds nothing to release.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Dispose_CalledAgain_DoesNotThrow()
    {
        var observable = new UnchangingPropertyObservable<int>(1);
        var subscription = observable.Subscribe(new RecordingObserver<int>([]));
        subscription.Dispose();

        await Assert.That(subscription.Dispose).ThrowsNothing();
    }

    /// <summary>A null observer has nowhere to deliver the value, so the call is rejected.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Subscribe_NullObserver_Throws()
    {
        var observable = new UnchangingPropertyObservable<string>(ObservedValue);

        await Assert.That(() => _ = observable.Subscribe(null!)).Throws<ArgumentNullException>();
    }

    /// <summary>An observer that records what it is given.</summary>
    /// <typeparam name="T">The observed value type.</typeparam>
    /// <param name="values">The list the observer appends each value to.</param>
    private sealed class RecordingObserver<T>(List<T> values) : IObserver<T>
    {
        /// <summary>Gets a value indicating whether the observation reported completion.</summary>
        public bool Completed { get; private set; }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnCompleted() => Completed = true;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnError(Exception error) => throw error;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnNext(T value) => values.Add(value);
    }
}
