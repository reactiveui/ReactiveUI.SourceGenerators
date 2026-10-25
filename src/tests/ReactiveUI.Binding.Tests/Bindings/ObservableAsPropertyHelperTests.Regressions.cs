// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Subjects;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.Bindings;

/// <summary>Regression tests for helper bugs reported against ReactiveUI and ReactiveUI.SourceGenerators.</summary>
public sealed partial class ObservableAsPropertyHelperTests
{
    /// <summary>A null after a non-null value reads back as null rather than the stale value (ReactiveUI.SourceGenerators #197).</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullAfterNonNull_ReadsBackNull()
    {
        var source = new Subject<string?>();
        var fixture = new ObservableAsPropertyHelper<string?>(source, static _ => { }, (string?)null);

        source.OnNext("first");
        source.OnNext(null);

        await Assert.That(fixture.Value).IsNull();
    }

    /// <summary>Reading a deferred helper disposed before its first read returns its initial value and does not throw (ReactiveUI #2455).</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Deferred_DisposedBeforeFirstRead_ReturnsInitialValueWithoutSubscribing()
    {
        var subscriptions = 0;
        var source = new CountingObservable<int>(() => subscriptions++);
        var fixture = new ObservableAsPropertyHelper<int>(source, static _ => { }, SuppliedValue, true);

        fixture.Dispose();

        await Assert.That(fixture.Value).IsEqualTo(SuppliedValue);
        await Assert.That(subscriptions).IsEqualTo(0);
    }

    /// <summary>A source that emits during Subscribe is subscribed once, even when a callback reads the value (ReactiveUI #814).</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SynchronousSource_ReadingValueFromCallback_SubscribesOnce()
    {
        var subscriptions = 0;
        ObservableAsPropertyHelper<int>? fixture = null;
        var source = new CountingObservable<int>(() => subscriptions++, SuppliedValue);
        var read = -1;

        fixture = new(source, value => read = fixture?.Value ?? value, 0, true);
        var value = fixture.Value;

        // The first read subscribes, and the value the source emits while being subscribed to is delivered before
        // the read returns.
        await Assert.That(subscriptions).IsEqualTo(1);
        await Assert.That(value).IsEqualTo(SuppliedValue);
        await Assert.That(read).IsEqualTo(SuppliedValue);
    }

    /// <summary>With a scheduler, the initial value is readable at once though its notification waits for the scheduler (ReactiveUI #785, #815).</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Scheduler_InitialValueReadableBeforeNotificationRuns()
    {
        var sequencer = new ManualSequencer();
        var notified = new List<int>();
        var fixture = new ObservableAsPropertyHelper<int>(NeverSource, notified.Add, SuppliedValue, sequencer);

        await Assert.That(fixture.Value).IsEqualTo(SuppliedValue);
        await Assert.That(notified).IsEmpty();
    }

    /// <summary>An observable that counts its subscriptions and can emit one value while being subscribed to.</summary>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="onSubscribe">Runs on every subscription.</param>
    /// <param name="emitOnSubscribe">A value to emit synchronously during subscription, or none.</param>
    private sealed class CountingObservable<TValue>(Action onSubscribe, TValue? emitOnSubscribe = default) : IObservable<TValue>
    {
        /// <inheritdoc/>
        public IDisposable Subscribe(IObserver<TValue> observer)
        {
            onSubscribe();
            if (emitOnSubscribe is not null && !EqualityComparer<TValue>.Default.Equals(emitOnSubscribe, default!))
            {
                observer.OnNext(emitOnSubscribe);
            }

            return EmptyDisposable.Instance;
        }
    }
}
