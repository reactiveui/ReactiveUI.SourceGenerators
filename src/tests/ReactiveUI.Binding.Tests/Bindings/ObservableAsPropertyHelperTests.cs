// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Subjects;
using ReactiveUI.Binding.Tests.TestModels;
using ReactiveUI.Primitives.Concurrency;

namespace ReactiveUI.Binding.Tests.Bindings;

/// <summary>
/// Tests for <see cref="ObservableAsPropertyHelper{T}"/>, which backs a read-only output property with an
/// observable. This part covers construction, the eager and deferred activation paths, and the distinct-until-
/// changed gate on the source.
/// </summary>
public sealed partial class ObservableAsPropertyHelperTests
{
    /// <summary>A generic value supplied to a constructor, for tests that only check it comes back out unchanged.</summary>
    private const int SuppliedValue = 4;

    /// <summary>An observable that never produces a value, used where a test only cares about the initial value.</summary>
    private static readonly Subject<int> NeverSource = new();

    /// <summary>A constructor with no initial value seeds the property's default value.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Constructor_TwoArg_SeedsDefaultValue()
    {
        using var fixture = new ObservableAsPropertyHelper<int>(NeverSource, static _ => { });

        await Assert.That(fixture.Value).IsEqualTo(0);
    }

    /// <summary>A constructor given an initial value seeds it.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Constructor_WithInitialValue_SeedsSuppliedValue()
    {
        using var fixture = new ObservableAsPropertyHelper<int>(NeverSource, static _ => { }, SuppliedValue);

        await Assert.That(fixture.Value).IsEqualTo(SuppliedValue);
    }

    /// <summary>A constructor given an initial value and a scheduler seeds the value regardless of the scheduler.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Constructor_WithInitialValueAndScheduler_SeedsSuppliedValue()
    {
        var scheduler = new ManualSequencer();
        using var fixture = new ObservableAsPropertyHelper<int>(NeverSource, static _ => { }, SuppliedValue, scheduler);

        await Assert.That(fixture.Value).IsEqualTo(SuppliedValue);
    }

    /// <summary>A constructor given an initial value and deferred subscription does not subscribe until read.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Constructor_WithInitialValueAndDeferSubscription_DoesNotSubscribeUntilValueRead()
    {
        var source = new EagerSubscribeObservable();
        using var fixture = new ObservableAsPropertyHelper<int>(source, static _ => { }, SuppliedValue, true);

        await Assert.That(source.WasSubscribed).IsFalse();

        await Assert.That(fixture.Value).IsEqualTo(SuppliedValue);
        await Assert.That(source.WasSubscribed).IsTrue();
    }

    /// <summary>The initial-value, defer and scheduler constructor is usable end to end.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Constructor_WithInitialValueDeferSubscriptionAndScheduler_Usable()
    {
        var scheduler = new ManualSequencer();
        using var fixture = new ObservableAsPropertyHelper<int>(NeverSource, static _ => { }, SuppliedValue, true, scheduler);

        await Assert.That(fixture.IsSubscribed).IsFalse();
        await Assert.That(fixture.Value).IsEqualTo(SuppliedValue);
        await Assert.That(fixture.IsSubscribed).IsTrue();
    }

    /// <summary>The onChanging-only constructor seeds the default value.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Constructor_WithOnChangingOnly_SeedsDefaultValue()
    {
        using var fixture = new ObservableAsPropertyHelper<int>(NeverSource, static _ => { }, static _ => { });

        await Assert.That(fixture.Value).IsEqualTo(0);
    }

    /// <summary>The onChanging-plus-initial-value constructor seeds the supplied value.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Constructor_WithOnChangingAndInitialValue_SeedsSuppliedValue()
    {
        using var fixture = new ObservableAsPropertyHelper<int>(NeverSource, static _ => { }, static _ => { }, SuppliedValue);

        await Assert.That(fixture.Value).IsEqualTo(SuppliedValue);
    }

    /// <summary>The onChanging-plus-initial-value-plus-defer constructor seeds the supplied value without subscribing.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Constructor_WithOnChangingInitialValueAndDefer_SeedsSuppliedValue()
    {
        using var fixture = new ObservableAsPropertyHelper<int>(NeverSource, static _ => { }, static _ => { }, SuppliedValue, true);

        await Assert.That(fixture.IsSubscribed).IsFalse();
        await Assert.That(fixture.Value).IsEqualTo(SuppliedValue);
    }

    /// <summary>The full constructor overload with an initial value, deferred subscription and a scheduler is usable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Constructor_FullOverloadWithSchedulerAndDefer_Usable()
    {
        var scheduler = new ManualSequencer();
        using var fixture = new ObservableAsPropertyHelper<int>(
            NeverSource,
            static _ => { },
            static _ => { },
            SuppliedValue,
            true,
            scheduler);

        await Assert.That(fixture.IsSubscribed).IsFalse();
        await Assert.That(fixture.Value).IsEqualTo(SuppliedValue);
        await Assert.That(fixture.IsSubscribed).IsTrue();
    }

    /// <summary>The initial-value-factory constructor evaluates the factory eagerly.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Constructor_WithGetInitialValueFactory_EvaluatesFactoryEagerly()
    {
        var accessed = false;
        using var fixture = new ObservableAsPropertyHelper<int>(NeverSource, static _ => { }, static _ => { }, GetInitialValue);

        await Assert.That(accessed).IsTrue();
        await Assert.That(fixture.Value).IsEqualTo(SuppliedValue);
        return;

        int GetInitialValue()
        {
            accessed = true;
            return SuppliedValue;
        }
    }

    /// <summary>The initial-value-factory-plus-defer constructor does not evaluate the factory until <see cref="ObservableAsPropertyHelper{T}.Value"/> is read.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Constructor_WithGetInitialValueFactoryAndDefer_DoesNotEvaluateFactoryUntilValueRead()
    {
        var accessed = false;
        using var fixture = new ObservableAsPropertyHelper<int>(NeverSource, static _ => { }, static _ => { }, GetInitialValue, true);

        await Assert.That(accessed).IsFalse();

        await Assert.That(fixture.Value).IsEqualTo(SuppliedValue);
        await Assert.That(accessed).IsTrue();
        return;

        int GetInitialValue()
        {
            accessed = true;
            return SuppliedValue;
        }
    }

    /// <summary>The shorter Func-based initial-value-plus-defer constructor is usable without a changing callback.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Constructor_WithFuncInitialValueAndDeferButNoOnChanging_Usable()
    {
        using var fixture = new ObservableAsPropertyHelper<int>(NeverSource, static _ => { }, static () => SuppliedValue, true);

        await Assert.That(fixture.IsSubscribed).IsFalse();
        await Assert.That(fixture.Value).IsEqualTo(SuppliedValue);
    }

    /// <summary>A null initial-value factory falls back to <c>default(T)</c>.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Constructor_WithNullGetInitialValueFactory_FallsBackToDefault()
    {
        using var fixture = new ObservableAsPropertyHelper<int>(NeverSource, static _ => { }, onChanging: null, getInitialValue: null);

        await Assert.That(fixture.Value).IsEqualTo(0);
    }

    /// <summary><see cref="ObservableAsPropertyHelper{T}.Default()"/> holds the default value and never changes.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Default_NoArguments_ReturnsDefaultValueAndNeverChanges()
    {
        using var fixture = ObservableAsPropertyHelper<int>.Default();

        await Assert.That(fixture.Value).IsEqualTo(0);
        await Assert.That(fixture.IsSubscribed).IsTrue();
    }

    /// <summary><see cref="ObservableAsPropertyHelper{T}.Default(T)"/> holds the supplied value and never changes.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Default_WithInitialValue_ReturnsSuppliedValueAndNeverChanges()
    {
        using var fixture = ObservableAsPropertyHelper<int>.Default(SuppliedValue);

        await Assert.That(fixture.Value).IsEqualTo(SuppliedValue);
    }

    /// <summary><see cref="ObservableAsPropertyHelper{T}.Default(T, ISequencer?)"/> holds the supplied value.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Default_WithInitialValueAndScheduler_ReturnsSuppliedValue()
    {
        using var fixture = ObservableAsPropertyHelper<int>.Default(SuppliedValue, new ManualSequencer());

        await Assert.That(fixture.Value).IsEqualTo(SuppliedValue);
    }

    /// <summary>A null observable is rejected before anything else runs.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Constructor_NullObservable_ThrowsArgumentNullException() =>
        await Assert.That(static () => new ObservableAsPropertyHelper<int>(null!, static _ => { })).Throws<ArgumentNullException>();

    /// <summary>A null onChanged callback is rejected.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Constructor_NullOnChanged_ThrowsArgumentNullException() =>
        await Assert.That(static () => new ObservableAsPropertyHelper<int>(NeverSource, null!)).Throws<ArgumentNullException>();

    /// <summary>The source is subscribed to at construction, before anything reads <see cref="ObservableAsPropertyHelper{T}.Value"/>.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Eager_SubscribesToSourceImmediately()
    {
        var source = new EagerSubscribeObservable();
        using var fixture = new ObservableAsPropertyHelper<int>(source, static _ => { }, 0);

        await Assert.That(source.WasSubscribed).IsTrue();
        await Assert.That(fixture.IsSubscribed).IsTrue();
    }

    /// <summary>Eager construction delivers the initial value through both callbacks before the constructor returns.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Eager_DeliversInitialValueImmediatelyThroughBothCallbacks()
    {
        var changed = new List<int>();
        var changing = new List<int>();
        using var fixture = new ObservableAsPropertyHelper<int>(NeverSource, changed.Add, changing.Add, SuppliedValue);

        await Assert.That(changed).IsEquivalentTo([SuppliedValue]);
        await Assert.That(changing).IsEquivalentTo([SuppliedValue]);
        await Assert.That(fixture.Value).IsEqualTo(SuppliedValue);
    }

    /// <summary><see cref="ObservableAsPropertyHelper{T}.IsSubscribed"/> stays false until <see cref="ObservableAsPropertyHelper{T}.Value"/> is first read.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Deferred_IsSubscribedIsFalseUntilValueIsFirstRead()
    {
        using var fixture = new ObservableAsPropertyHelper<int>(NeverSource, static _ => { }, 0, true);

        await Assert.That(fixture.IsSubscribed).IsFalse();
        _ = fixture.Value;
        await Assert.That(fixture.IsSubscribed).IsTrue();
    }

    /// <summary>Reading the value for the first time under deferred subscription returns the initial value without raising callbacks.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Deferred_ReadingValueSubscribesAndReturnsInitialValueWithoutRaisingCallbacks()
    {
        var changed = new List<int>();
        var changing = new List<int>();
        using var fixture = new ObservableAsPropertyHelper<int>(NeverSource, changed.Add, changing.Add, SuppliedValue, true);

        await Assert.That(fixture.Value).IsEqualTo(SuppliedValue);
        await Assert.That(changed).IsEmpty();
        await Assert.That(changing).IsEmpty();
    }

    /// <summary>A source value equal to the initial value is skipped once the distinct gate is seeded by activation.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Deferred_SkipsASourceValueEqualToInitialValue()
    {
        const int UpdatedValue = 2;
        var source = new Subject<int>();
        var received = new List<int>();
        using var fixture = new ObservableAsPropertyHelper<int>(source, received.Add, SuppliedValue, true);

        _ = fixture.Value;
        await Assert.That(received).IsEmpty();

        source.OnNext(SuppliedValue);
        await Assert.That(received).IsEmpty();

        source.OnNext(UpdatedValue);
        await Assert.That(received).IsEquivalentTo([UpdatedValue]);
    }

    /// <summary>Deferred subscription with an initial-value factory does not access the factory until the value is read.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Deferred_WithFactory_DoesNotAccessFactoryUntilValueIsRead()
    {
        var source = new Subject<int>();
        var accessed = false;

        using var fixture = new ObservableAsPropertyHelper<int>(source, static _ => { }, GetInitialValue, true);

        await Assert.That(accessed).IsFalse();

        await Assert.That(fixture.Value).IsEqualTo(SuppliedValue);
        await Assert.That(accessed).IsTrue();
        return;

        int GetInitialValue()
        {
            accessed = true;
            return SuppliedValue;
        }
    }

    /// <summary>Activating a deferred helper does not raise callbacks even when the source repeats the initial value.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Deferred_WithFactory_ActivatingDoesNotRaiseCallbacksEvenWhenSourceRepeatsIt()
    {
        var source = new Subject<int>();
        var changed = false;
        var changing = false;

        using var fixture = new ObservableAsPropertyHelper<int>(source, OnChanged, OnChanging, GetInitialValue, true);

        _ = fixture.Value;
        source.OnNext(SuppliedValue);

        await Assert.That(changed).IsFalse();
        await Assert.That(changing).IsFalse();
        return;

        static int GetInitialValue() => SuppliedValue;

        void OnChanged(int _) => changed = true;

        void OnChanging(int _) => changing = true;
    }

    /// <summary>Consecutive equal values from the source are collapsed to one delivery.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DistinctUntilChanged_SkipsConsecutiveEqualValues()
    {
        const int UpdatedValue = 2;
        var source = new Subject<int>();
        var received = new List<int>();
        using var fixture = new ObservableAsPropertyHelper<int>(source, received.Add, 0);
        received.Clear();

        source.OnNext(1);
        source.OnNext(1);
        source.OnNext(UpdatedValue);

        await Assert.That(received).IsEquivalentTo([1, UpdatedValue]);
    }

    /// <summary>A value equal to an earlier one, but not the immediately preceding one, still passes the gate.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DistinctUntilChanged_AllowsARepeatedValueAfterADifferentOneInBetween()
    {
        const int UpdatedValue = 2;
        var source = new Subject<int>();
        var received = new List<int>();
        using var fixture = new ObservableAsPropertyHelper<int>(source, received.Add, 0);
        received.Clear();

        source.OnNext(1);
        source.OnNext(UpdatedValue);
        source.OnNext(1);

        await Assert.That(received).IsEquivalentTo([1, UpdatedValue, 1]);
    }

    /// <summary>An observable that records whether, and how many times, it was subscribed to.</summary>
    private sealed class EagerSubscribeObservable : IObservable<int>
    {
        /// <summary>Gets a value indicating whether the observable has been subscribed to.</summary>
        public bool WasSubscribed { get; private set; }

        /// <inheritdoc/>
        public IDisposable Subscribe(IObserver<int> observer)
        {
            WasSubscribed = true;
            return NoOpDisposable.Instance;
        }
    }

    /// <summary>A disposable that does nothing, for observables that do not need to track unsubscription.</summary>
    private sealed class NoOpDisposable : IDisposable
    {
        /// <summary>Gets the shared instance.</summary>
        public static NoOpDisposable Instance { get; } = new();

        /// <inheritdoc/>
        public void Dispose()
        {
        }
    }
}
