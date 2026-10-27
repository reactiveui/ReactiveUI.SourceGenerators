// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ReactiveUI.Binding.Observables;

namespace ReactiveUI.Binding.Tests.Observables;

/// <summary>Unit tests for <see cref="EventObservable{T}"/>.</summary>
public class EventObservableTests
{
    /// <summary>The initial value produced by the event observable's getter.</summary>
    private const string InitialValue = "initial";

    /// <summary>The expected number of emitted values when two notifications are produced.</summary>
    private const int ExpectedTwoEmissions = 2;

    /// <summary>Verifies that constructor throws ArgumentNullException when addHandler is null.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Constructor_NullAddHandler_ThrowsArgumentNullException()
    {
        var action = static () => new EventObservable<string>(
            null!,
            static _ => { },
            static () => "test",
            true);

        await Assert.That(action).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Verifies that constructor throws ArgumentNullException when removeHandler is null.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Constructor_NullRemoveHandler_ThrowsArgumentNullException()
    {
        var action = static () => new EventObservable<string>(
            static _ => { },
            null!,
            static () => "test",
            true);

        await Assert.That(action).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Verifies that constructor throws ArgumentNullException when getter is null.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Constructor_NullGetter_ThrowsArgumentNullException()
    {
        var action = static () => new EventObservable<string>(
            static _ => { },
            static _ => { },
            null!,
            true);

        await Assert.That(action).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Verifies that Subscribe throws ArgumentNullException when observer is null.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Subscribe_NullObserver_ThrowsArgumentNullException()
    {
        var observable = new EventObservable<string>(
            static _ => { },
            static _ => { },
            static () => "test",
            true);

        var action = () => observable.Subscribe(null!);

        await Assert.That(action).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Verifies that Subscribe emits the current value immediately.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Subscribe_EmitsInitialValue()
    {
        const string value = InitialValue;
        var results = new List<string>();

        var observable = new EventObservable<string>(
            static _ => { },
            static _ => { },
            static () => value,
            false);

        _ = observable.Subscribe(new AnonymousObserver<string>(results.Add, static _ => { }, static () => { }));

        await Assert.That(results).Count().IsEqualTo(1);
        await Assert.That(results[0]).IsEqualTo(InitialValue);
    }

    /// <summary>Verifies that a getter throwing on subscribe propagates and removes the handler through the remove delegate.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Subscribe_GetterThrows_RemovesHandler()
    {
        EventHandler? handler = null;
        var fail = true;
        var results = new List<string>();

        var observable = new EventObservable<string>(
            h => handler += h,
            h => handler -= h,
            () => fail ? throw new InvalidOperationException("getter") : InitialValue,
            false);

        var action = () => observable.Subscribe(new AnonymousObserver<string>(results.Add, static _ => { }, static () => { }));
        await Assert.That(action).ThrowsExactly<InvalidOperationException>();

        fail = false;
        handler?.Invoke(this, EventArgs.Empty);

        await Assert.That(handler).IsNull();
        await Assert.That(results).IsEmpty();
    }

    /// <summary>Verifies that event handler invocation emits updated values.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Subscribe_EventFired_EmitsNewValue()
    {
        EventHandler? handler = null;
        var value = InitialValue;
        var results = new List<string>();

        var observable = new EventObservable<string>(
            h => handler = h,
            h => handler = null,
            () => value,
            false);

        _ = observable.Subscribe(new AnonymousObserver<string>(results.Add, static _ => { }, static () => { }));

        value = "updated";
        handler?.Invoke(this, EventArgs.Empty);

        await Assert.That(results).Count().IsEqualTo(ExpectedTwoEmissions);
        await Assert.That(results[1]).IsEqualTo("updated");
    }

    /// <summary>Verifies that distinct-until-changed suppresses duplicate values.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Subscribe_DistinctUntilChanged_SuppressesDuplicates()
    {
        EventHandler? handler = null;
        var value = "same";
        var results = new List<string>();

        var observable = new EventObservable<string>(
            h => handler = h,
            h => handler = null,
            () => value,
            true);

        _ = observable.Subscribe(new AnonymousObserver<string>(results.Add, static _ => { }, static () => { }));

        // Fire event with same value — should be suppressed
        handler?.Invoke(this, EventArgs.Empty);

        await Assert.That(results).Count().IsEqualTo(1);

        // Fire event with different value — should emit
        value = "different";
        handler?.Invoke(this, EventArgs.Empty);

        await Assert.That(results).Count().IsEqualTo(ExpectedTwoEmissions);
        await Assert.That(results[1]).IsEqualTo("different");
    }

    /// <summary>Verifies that without distinct-until-changed, duplicate values are emitted.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Subscribe_NoDistinct_EmitsDuplicates()
    {
        EventHandler? handler = null;
        const string value = "same";
        var results = new List<string>();

        var observable = new EventObservable<string>(
            h => handler = h,
            h => handler = null,
            static () => value,
            false);

        _ = observable.Subscribe(new AnonymousObserver<string>(results.Add, static _ => { }, static () => { }));

        // Fire event with same value — should emit
        handler?.Invoke(this, EventArgs.Empty);

        await Assert.That(results).Count().IsEqualTo(ExpectedTwoEmissions);
    }

    /// <summary>Verifies that Dispose unsubscribes the event handler.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Dispose_UnsubscribesHandler()
    {
        EventHandler? handler = null;
        const string value = InitialValue;
        var results = new List<string>();

        var observable = new EventObservable<string>(
            h => handler = h,
            h =>
            {
                if (h != handler)
                {
                    return;
                }

                handler = null;
            },
            static () => value,
            false);

        var sub = observable.Subscribe(new AnonymousObserver<string>(results.Add, static _ => { }, static () => { }));

        sub.Dispose();

        await Assert.That(handler).IsNull();
    }

    /// <summary>Verifies that events fired after dispose are ignored.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Dispose_EventAfterDispose_Ignored()
    {
        EventHandler? handler = null;
        var value = InitialValue;
        var results = new List<string>();

        var observable = new EventObservable<string>(
            h => handler = h,
            static _ => { },
            () => value,
            false);

        var sub = observable.Subscribe(new AnonymousObserver<string>(results.Add, static _ => { }, static () => { }));

        // Keep reference before dispose clears it
        var savedHandler = handler;
        sub.Dispose();

        value = "after-dispose";
        savedHandler?.Invoke(this, EventArgs.Empty);

        await Assert.That(results).Count().IsEqualTo(1);
    }

    /// <summary>Verifies that double dispose is safe.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Dispose_CalledTwice_IsSafe()
    {
        var removeCount = 0;
        var observable = new EventObservable<string>(
            static _ => { },
            _ => removeCount++,
            static () => "test",
            false);

        var sub = observable.Subscribe(new AnonymousObserver<string>(static _ => { }, static _ => { }, static () => { }));

        sub.Dispose();
        sub.Dispose();

        await Assert.That(removeCount).IsEqualTo(1);
    }

    /// <summary>Anonymous observer for testing.</summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="onNext">Called for each emitted value.</param>
    /// <param name="onError">Called when the sequence faults.</param>
    /// <param name="onCompleted">Called when the sequence completes.</param>
    private sealed class AnonymousObserver<T>(Action<T> onNext, Action<Exception> onError, Action onCompleted) : IObserver<T>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnCompleted() => onCompleted();

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnError(Exception error) => onError(error);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnNext(T value) => onNext(value);
    }
}
