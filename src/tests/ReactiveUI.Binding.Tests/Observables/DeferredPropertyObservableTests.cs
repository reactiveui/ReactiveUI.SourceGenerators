// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Observables;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.Observables;

/// <summary>Unit tests for <see cref="DeferredPropertyObservable{TSource, TValue}"/>.</summary>
public class DeferredPropertyObservableTests
{
    /// <summary>The value the source holds before it changes.</summary>
    private const string FirstValue = "first";

    /// <summary>The value the source holds after it changes.</summary>
    private const string SecondValue = "second";

    /// <summary>Each subscriber gets the property's value as it stands when that subscriber arrives.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Subscribe_ReadsThePropertyWhenEachSubscriberArrives()
    {
        var source = new Holder { Value = FirstValue };
        var observable = new DeferredPropertyObservable<Holder, string>(source, static holder => holder.Value);
        var first = new List<string>();
        var second = new List<string>();

        using var firstSubscription = observable.Subscribe(new RecordingObserver<string>(first.Add));
        source.Value = SecondValue;
        using var secondSubscription = observable.Subscribe(new RecordingObserver<string>(second.Add));

        using (Assert.Multiple())
        {
            await Assert.That(first).IsEquivalentTo([FirstValue]);
            await Assert.That(second).IsEquivalentTo([SecondValue]);
        }
    }

    /// <summary>A null observer has nowhere to deliver the value, so the call is rejected.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Subscribe_NullObserver_Throws()
    {
        var observable = new DeferredPropertyObservable<Holder, string>(new Holder(), static holder => holder.Value);

        await Assert.That(() => _ = observable.Subscribe(null!)).Throws<ArgumentNullException>();
    }

    /// <summary>Without a getter there is nothing to read, so construction is rejected.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Constructor_NullGetter_Throws() =>
        await Assert.That(static () => _ = new DeferredPropertyObservable<Holder, string>(new Holder(), null!)).Throws<ArgumentNullException>();

    /// <summary>A plain object with a property that raises nothing.</summary>
    private sealed class Holder
    {
        /// <summary>Gets or sets the value.</summary>
        public string Value { get; set; } = string.Empty;
    }
}
