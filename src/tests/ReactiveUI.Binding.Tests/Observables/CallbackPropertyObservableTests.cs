// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ReactiveUI.Binding.Observables;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.Observables;

/// <summary>Unit tests for <see cref="CallbackPropertyObservable{TSource, TValue}"/>.</summary>
public class CallbackPropertyObservableTests
{
    /// <summary>The value the source holds before it changes.</summary>
    private const string FirstValue = "first";

    /// <summary>The value the source holds after it changes.</summary>
    private const string SecondValue = "second";

    /// <summary>A subscriber gets the current value, then the value again each time the platform notifies.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Subscribe_DeliversTheCurrentValueThenEachNotifiedChange()
    {
        var control = new NotifyingControl { Value = FirstValue };
        var observer = new RecordingObserver<string>();

        using var subscription = Observe(control, distinct: false).Subscribe(observer);
        control.Value = SecondValue;
        control.Notify();

        await Assert.That(observer.Values).IsEquivalentTo([FirstValue, SecondValue]);
    }

    /// <summary>Distinct delivery drops a notification that leaves the value unchanged.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Subscribe_Distinct_DropsAnUnchangedValue()
    {
        var control = new NotifyingControl { Value = FirstValue };
        var observer = new RecordingObserver<string>();

        using var subscription = Observe(control, distinct: true).Subscribe(observer);
        control.Notify();
        control.Value = SecondValue;
        control.Notify();

        await Assert.That(observer.Values).IsEquivalentTo([FirstValue, SecondValue]);
    }

    /// <summary>Without distinct delivery an unchanged value is delivered again.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Subscribe_NotDistinct_DeliversAnUnchangedValue()
    {
        var control = new NotifyingControl { Value = FirstValue };
        var observer = new RecordingObserver<string>();

        using var subscription = Observe(control, distinct: false).Subscribe(observer);
        control.Notify();

        await Assert.That(observer.Values).IsEquivalentTo([FirstValue, FirstValue]);
    }

    /// <summary>Disposing detaches from the platform once, and a late notification reaches no one.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Dispose_DetachesOnceAndStopsDelivery()
    {
        var control = new NotifyingControl { Value = FirstValue };
        var observer = new RecordingObserver<string>();

        var subscription = Observe(control, distinct: false).Subscribe(observer);
        var callback = control.Callback!;
        subscription.Dispose();
        subscription.Dispose();
        control.Value = SecondValue;
        callback();

        using (Assert.Multiple())
        {
            await Assert.That(control.Detachments).IsEqualTo(1);
            await Assert.That(observer.Values).IsEquivalentTo([FirstValue]);
        }
    }

    /// <summary>A getter that fails on the first read detaches the subscription before the failure propagates.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Subscribe_GetterThrows_DetachesAndRethrows()
    {
        var control = new NotifyingControl();
        var observable = new CallbackPropertyObservable<NotifyingControl, string>(
            control,
            static (source, notify) => source.Attach(notify),
            static _ => throw new InvalidOperationException("read failed"),
            false);

        using (Assert.Multiple())
        {
            await Assert.That(() => _ = observable.Subscribe(new RecordingObserver<string>())).Throws<InvalidOperationException>();
            await Assert.That(control.Detachments).IsEqualTo(1);
        }
    }

    /// <summary>A null observer has nowhere to deliver values, so the call is rejected.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Subscribe_NullObserver_Throws() =>
        await Assert.That(static () => _ = Observe(new(), false).Subscribe(null!)).Throws<ArgumentNullException>();

    /// <summary>Without a way to attach to the platform there is nothing to observe, so construction is rejected.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Constructor_NullSubscribe_Throws() =>
        await Assert.That(static () => _ = new CallbackPropertyObservable<NotifyingControl, string>(new NotifyingControl(), null!, static control => control.Value, false))
            .Throws<ArgumentNullException>();

    /// <summary>Without a getter there is nothing to read, so construction is rejected.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Constructor_NullGetter_Throws() =>
        await Assert.That(static () => _ = new CallbackPropertyObservable<NotifyingControl, string>(new NotifyingControl(), static (source, notify) => source.Attach(notify), null!, false))
            .Throws<ArgumentNullException>();

    /// <summary>Observes a control's value through its notification callback.</summary>
    /// <param name="control">The control to observe.</param>
    /// <param name="distinct">Whether an unchanged value is dropped.</param>
    /// <returns>The observation.</returns>
    private static CallbackPropertyObservable<NotifyingControl, string> Observe(NotifyingControl control, bool distinct) =>
        new(control, static (source, notify) => source.Attach(notify), static source => source.Value, distinct);

    /// <summary>A control whose platform notification is a single callback.</summary>
    private sealed class NotifyingControl
    {
        /// <summary>Gets or sets the value.</summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>Gets the attached callback, or null.</summary>
        public Action? Callback { get; private set; }

        /// <summary>Gets how many times the callback was detached.</summary>
        public int Detachments { get; private set; }

        /// <summary>Attaches the callback and returns what detaches it.</summary>
        /// <param name="callback">The callback to attach.</param>
        /// <returns>The detachment.</returns>
        public ActionDisposable Attach(Action callback)
        {
            Callback = callback;
            return new(() =>
            {
                Callback = null;
                Detachments++;
            });
        }

        /// <summary>Raises the platform notification.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Notify() => Callback?.Invoke();
    }
}
