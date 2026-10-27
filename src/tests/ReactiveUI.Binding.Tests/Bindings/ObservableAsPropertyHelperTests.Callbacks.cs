// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Subjects;

namespace ReactiveUI.Binding.Tests.Bindings;

/// <summary>
/// Tests for <see cref="ObservableAsPropertyHelper{T}"/> covering the ordering of the changing/changed callbacks,
/// the <see cref="ObservableAsPropertyHelper{T}"/> <c>Create</c> factories used by generated code, the
/// <see cref="ObservableAsPropertyHelper{T}.ThrownExceptions"/> stream, and completion.
/// </summary>
public sealed partial class ObservableAsPropertyHelperTests
{
    /// <summary>The changing callback observes the old value still in effect, and the changed callback observes the new one.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OnChanging_ObservesOldValue_OnChanged_ObservesNewValueAlreadyStored()
    {
        const int UpdatedValue = 2;
        var source = new Subject<int>();
        ObservableAsPropertyHelper<int>? fixture = null;
        var changingSawValue = -1;
        var changedSawValue = -1;

        fixture = new(
            source,
            _ => changedSawValue = fixture!.Value,
            _ => changingSawValue = fixture!.Value,
            SuppliedValue,
            deferSubscription: true);

        _ = fixture.Value; // activates without raising callbacks

        source.OnNext(UpdatedValue);

        await Assert.That(changingSawValue).IsEqualTo(SuppliedValue);
        await Assert.That(changedSawValue).IsEqualTo(UpdatedValue);
        await Assert.That(fixture.Value).IsEqualTo(UpdatedValue);
    }

    /// <summary>The value-seeded <see cref="ObservableAsPropertyHelper{T}"/> <c>Create</c> overload passes the owner to both callbacks.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Create_WithInitialValue_PassesOwnerToOnChangedAndOnChanging()
    {
        const int PushedValue = 2;
        var owner = new object();
        var source = new Subject<int>();
        object? changedOwner = null;
        object? changingOwner = null;
        var changedValue = -1;

        using var fixture = ObservableAsPropertyHelper<int>.Create(
            source,
            owner,
            (o, v) =>
            {
                changedOwner = o;
                changedValue = v;
            },
            (o, _) => changingOwner = o,
            0,
            deferSubscription: false,
            scheduler: null);

        source.OnNext(PushedValue);

        await Assert.That(changedOwner).IsSameReferenceAs(owner);
        await Assert.That(changingOwner).IsSameReferenceAs(owner);
        await Assert.That(changedValue).IsEqualTo(PushedValue);
    }

    /// <summary>The factory-based <see cref="ObservableAsPropertyHelper{T}"/> <c>Create</c> overload passes the owner and evaluates the factory.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Create_WithInitialValueFactory_PassesOwnerAndEvaluatesFactory()
    {
        var owner = new object();
        var source = new Subject<int>();
        object? changedOwner = null;
        var accessed = false;

        using var fixture = ObservableAsPropertyHelper<int>.Create(
            source,
            owner,
            (o, _) => changedOwner = o,
            onChanging: null,
            getInitialValue: GetInitialValue,
            deferSubscription: false,
            scheduler: null);

        await Assert.That(accessed).IsTrue();
        await Assert.That(fixture.Value).IsEqualTo(SuppliedValue);
        await Assert.That(changedOwner).IsSameReferenceAs(owner);
        return;

        int GetInitialValue()
        {
            accessed = true;
            return SuppliedValue;
        }
    }

    /// <summary>A null owner is rejected by the value-seeded <see cref="ObservableAsPropertyHelper{T}"/> <c>Create</c> overload.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Create_WithInitialValue_NullOwner_ThrowsArgumentNullException() =>
        await Assert.That(static () => ObservableAsPropertyHelper<int>.Create(
            NeverSource,
            null!,
            static (_, _) => { },
            null,
            0,
            false,
            null)).Throws<ArgumentNullException>();

    /// <summary>A null onChanged is rejected by the value-seeded <see cref="ObservableAsPropertyHelper{T}"/> <c>Create</c> overload.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Create_WithInitialValue_NullOnChanged_ThrowsArgumentNullException() =>
        await Assert.That(static () => ObservableAsPropertyHelper<int>.Create(
            NeverSource,
            new(),
            null!,
            null,
            0,
            false,
            null)).Throws<ArgumentNullException>();

    /// <summary>A null owner is rejected by the factory-seeded <see cref="ObservableAsPropertyHelper{T}"/> <c>Create</c> overload.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Create_WithInitialValueFactory_NullOwner_ThrowsArgumentNullException() =>
        await Assert.That(static () => ObservableAsPropertyHelper<int>.Create(
            NeverSource,
            null!,
            static (_, _) => { },
            null,
            static () => 0,
            false,
            null)).Throws<ArgumentNullException>();

    /// <summary>A null onChanged is rejected by the factory-seeded <see cref="ObservableAsPropertyHelper{T}"/> <c>Create</c> overload.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Create_WithInitialValueFactory_NullOnChanged_ThrowsArgumentNullException() =>
        await Assert.That(static () => ObservableAsPropertyHelper<int>.Create(
            NeverSource,
            new(),
            null!,
            null,
            static () => 0,
            false,
            null)).Throws<ArgumentNullException>();

    /// <summary>With no onChanging supplied, the <see cref="ObservableAsPropertyHelper{T}"/> <c>Create</c> factory only invokes onChanged.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Create_WithNoOnChanging_OnlyInvokesOnChanged()
    {
        const int ExpectedDeliveryCount = 2;
        var owner = new object();
        var source = new Subject<int>();
        var changedCount = 0;

        using var fixture = ObservableAsPropertyHelper<int>.Create(
            source,
            owner,
            (_, _) => changedCount++,
            onChanging: null,
            0,
            deferSubscription: false,
            scheduler: null);

        source.OnNext(1);

        // The initial value delivery plus the one source value.
        await Assert.That(changedCount).IsEqualTo(ExpectedDeliveryCount);
    }

    /// <summary>With no thrown-exceptions observer, a source error is rethrown on the thread that produced it.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ThrownExceptions_NoObserver_RethrowsOnProducingThread()
    {
        var source = new Subject<int>();
        using var fixture = new ObservableAsPropertyHelper<int>(source, static _ => { }, 0);

        await Assert.That(() => source.OnError(new InvalidOperationException("die"))).Throws<InvalidOperationException>();
    }

    /// <summary>A source error reaches a subscribed <see cref="ObservableAsPropertyHelper{T}.ThrownExceptions"/> observer instead of being rethrown.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ThrownExceptions_WithObserver_ReceivesError_ValueRetainsLast()
    {
        var source = new Subject<int>();
        using var fixture = new ObservableAsPropertyHelper<int>(source, static _ => { }, 0);
        var errors = new List<Exception>();

        source.OnNext(SuppliedValue);
        using var subscription = fixture.ThrownExceptions.Subscribe(errors.Add);

        var error = new InvalidOperationException("die");
        await Assert.That(() => source.OnError(error)).ThrowsNothing();

        await Assert.That(errors.Count).IsEqualTo(1);
        await Assert.That(errors[0]).IsSameReferenceAs(error);
        await Assert.That(fixture.Value).IsEqualTo(SuppliedValue);
    }

    /// <summary>Disposing the only <see cref="ObservableAsPropertyHelper{T}.ThrownExceptions"/> subscription restores the rethrow behaviour.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ThrownExceptions_AfterSubscriptionDisposed_RethrowsAgain()
    {
        var source = new Subject<int>();
        using var fixture = new ObservableAsPropertyHelper<int>(source, static _ => { }, 0);
        var errors = new List<Exception>();

        var subscription = fixture.ThrownExceptions.Subscribe(errors.Add);
        subscription.Dispose();

        await Assert.That(() => source.OnError(new InvalidOperationException("die again"))).Throws<InvalidOperationException>();
        await Assert.That(errors).IsEmpty();
    }

    /// <summary>Source completion is ignored: the value is retained and nothing throws.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Completion_IsIgnored_ValueRetainsLastAndNoExceptionThrown()
    {
        var source = new Subject<int>();
        using var fixture = new ObservableAsPropertyHelper<int>(source, static _ => { }, 0);

        source.OnNext(SuppliedValue);

        await Assert.That(() => source.OnCompleted()).ThrowsNothing();
        await Assert.That(fixture.Value).IsEqualTo(SuppliedValue);
    }
}
