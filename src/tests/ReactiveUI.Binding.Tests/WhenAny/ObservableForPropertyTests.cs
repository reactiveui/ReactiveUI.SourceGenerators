// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.ObservableForProperty;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.WhenAny;

/// <summary>Tests for ObservableForProperty extension methods using the runtime fallback path.</summary>
public class ObservableForPropertyTests
{
    /// <summary>The age assigned to the sample view model.</summary>
    private const int SampleAge = 42;

    /// <summary>The name of the property observed by the string-based overload tests.</summary>
    private const string ObservedPropertyName = "IsNotNullString";

    /// <summary>The initial property value used across tests.</summary>
    private const string InitialValue = "Initial";

    /// <summary>The expected number of emitted changes when two notifications are produced.</summary>
    private const int ExpectedTwoChanges = 2;

    /// <summary>The replacement leaf value used in deep-chain tests.</summary>
    private const int ReplacementLeafValue = 99;

    /// <summary>Verifies that a single property emits on change.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_EmitsOnChange()
    {
        EnsureInitialized();

        var fixture = new TestFixture();
        var changes = new List<IObservedChange<TestFixture, string>>();

        using var sub = fixture.ObservableForProperty(x => x.IsNotNullString)
            .Subscribe(changes.Add);

        fixture.IsNotNullString = "Bar";
        fixture.IsNotNullString = "Baz";

        await Assert.That(changes.Count).IsEqualTo(ExpectedTwoChanges);
        await Assert.That(changes[0].Value).IsEqualTo("Bar");
        await Assert.That(changes[1].Value).IsEqualTo("Baz");
    }

    /// <summary>Verifies that ObservableForProperty skips the initial value by default.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_SkipsInitialByDefault()
    {
        EnsureInitialized();

        var fixture = new TestFixture { IsNotNullString = InitialValue };
        var changes = new List<IObservedChange<TestFixture, string>>();

        using var sub = fixture.ObservableForProperty(x => x.IsNotNullString)
            .Subscribe(changes.Add);

        // Should not have emitted the initial value
        await Assert.That(changes.Count).IsEqualTo(0);
    }

    /// <summary>Verifies that ObservableForProperty emits the initial value when skipInitial is false.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_EmitsInitialWhenRequested()
    {
        EnsureInitialized();

        var fixture = new TestFixture { IsNotNullString = InitialValue };
        var changes = new List<IObservedChange<TestFixture, string>>();

        using var sub = fixture.ObservableForProperty(x => x.IsNotNullString, skipInitial: false)
            .Subscribe(changes.Add);

        await Assert.That(changes.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(changes[0].Value).IsEqualTo(InitialValue);
    }

    /// <summary>Verifies that a deep property chain emits when the leaf property changes.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepChain_EmitsOnLeafChange()
    {
        EnsureInitialized();

        var fixture = new HostTestFixture { Child = new() { IsOnlyOneWord = "Foo" } };
        var changes = new List<IObservedChange<HostTestFixture, string>>();

        using var sub = fixture.ObservableForProperty(x => x.Child!.IsOnlyOneWord, skipInitial: false)
            .Subscribe(changes.Add);

        await Assert.That(changes.Count).IsGreaterThanOrEqualTo(1);

        fixture.Child.IsOnlyOneWord = "Bar";

        await Assert.That(changes.Count).IsGreaterThanOrEqualTo(ExpectedTwoChanges);
    }

    /// <summary>Verifies that a deep chain resubscribes when an intermediate object changes.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepChain_ResubscribesOnIntermediateChange()
    {
        EnsureInitialized();

        var fixture = new HostTestFixture { Child = new() { IsOnlyOneWord = "Foo" } };
        var changes = new List<IObservedChange<HostTestFixture, string>>();

        using var sub = fixture.ObservableForProperty(x => x.Child!.IsOnlyOneWord, skipInitial: false)
            .Subscribe(changes.Add);

        var initialCount = changes.Count;

        // Replace the intermediate object entirely
        fixture.Child = new() { IsOnlyOneWord = "Baz" };

        await Assert.That(changes.Count).IsGreaterThan(initialCount);
    }

    /// <summary>Verifies that a deep chain handles null intermediate objects gracefully.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepChain_ResubscribesAfterNullThenRestore()
    {
        EnsureInitialized();

        var fixture = new HostTestFixture { Child = new() { IsOnlyOneWord = "Foo" } };
        var changes = new List<IObservedChange<HostTestFixture, string>>();

        using var sub = fixture.ObservableForProperty(x => x.Child!.IsOnlyOneWord, skipInitial: false)
            .Subscribe(changes.Add);

        var countAfterInitial = changes.Count;

        // Set intermediate to null
        fixture.Child = null;

        // Restore with a new value
        fixture.Child = new() { IsOnlyOneWord = "Bar" };

        await Assert.That(changes.Count).IsGreaterThan(countAfterInitial);
    }

    /// <summary>Verifies that a four-level deep chain triggers updates at any level.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FourLevelDeepChain_AnyLevelTriggersUpdate()
    {
        EnsureInitialized();

        var fixture = new ObjChain1 { Chain2 = new() { Chain3 = new() { Host = new() { SomeOtherParam = SampleAge } } } };
        var values = new List<IObservedChange<ObjChain1, int>>();

        using var sub = fixture.ObservableForProperty(
                x => x.Chain2!.Chain3!.Host!.SomeOtherParam,
                skipInitial: false)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);

        // Change the leaf value
        fixture.Chain2!.Chain3!.Host!.SomeOtherParam = ReplacementLeafValue;

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(ExpectedTwoChanges);
    }

    /// <summary>Verifies that ObservableForProperty works with plain INPC objects.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WorksWithINPCObjects()
    {
        EnsureInitialized();

        var obj = new NonReactiveINotifyPropertyChangedObject();
        var changes = new List<IObservedChange<NonReactiveINotifyPropertyChangedObject, string>>();

        using var sub = obj.ObservableForProperty(x => x.InpcProperty)
            .Subscribe(changes.Add);

        obj.InpcProperty = "Hello";

        await Assert.That(changes.Count).IsEqualTo(1);
        await Assert.That(changes[0].Value).IsEqualTo("Hello");
    }

    /// <summary>Verifies that the string-based overload of ObservableForProperty works.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StringOverload_ObservesProperty()
    {
        EnsureInitialized();

        var fixture = new TestFixture();
        var changes = new List<IObservedChange<TestFixture, string>>();

        using var sub = fixture.ObservableForProperty<TestFixture, string>(ObservedPropertyName)
            .Subscribe(changes.Add);

        fixture.IsNotNullString = "Test";

        await Assert.That(changes.Count).IsEqualTo(1);
        await Assert.That(changes[0].Value).IsEqualTo("Test");
    }

    /// <summary>Verifies that ObservableForProperty deduplicates same values by default.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsDistinct_DeduplicatesSameValues()
    {
        EnsureInitialized();

        var fixture = new TestFixture { IsNotNullString = "A" };
        var changes = new List<IObservedChange<TestFixture, string>>();

        using var sub = fixture.ObservableForProperty(x => x.IsNotNullString)
            .Subscribe(changes.Add);

        fixture.IsNotNullString = "B";
        fixture.IsNotNullString = "B"; // Same value — should be deduped by equality check in setter

        await Assert.That(changes.Count).IsEqualTo(1);
    }

    /// <summary>Verifies that the string-based overload emits the initial value when skipInitial is false.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StringOverload_SkipInitialFalse_EmitsInitialValue()
    {
        EnsureInitialized();

        var fixture = new TestFixture { IsNotNullString = InitialValue };
        var changes = new List<IObservedChange<TestFixture, string>>();

        using var sub = fixture.ObservableForProperty<TestFixture, string>(ObservedPropertyName, skipInitial: false)
            .Subscribe(changes.Add);

        await Assert.That(changes.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(changes[0].Value).IsEqualTo(InitialValue);
    }

    /// <summary>Verifies that the string-based overload works with isDistinct false.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StringOverload_IsDistinctFalse_EmitsAll()
    {
        EnsureInitialized();

        var fixture = new TestFixture();
        var changes = new List<IObservedChange<TestFixture, string>>();

        using var sub = fixture.ObservableForProperty<TestFixture, string>(ObservedPropertyName, beforeChange: false, skipInitial: false, isDistinct: false)
            .Subscribe(changes.Add);

        fixture.IsNotNullString = "A";
        fixture.IsNotNullString = "B";

        await Assert.That(changes.Count).IsGreaterThanOrEqualTo(ExpectedTwoChanges);
    }

    /// <summary>Verifies that the string-based overload works with beforeChange true.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StringOverload_BeforeChange_EmitsBeforePropertyChanges()
    {
        EnsureInitialized();

        var fixture = new TestFixture();
        var changes = new List<IObservedChange<TestFixture, string>>();

        using var sub = fixture.ObservableForProperty<TestFixture, string>(ObservedPropertyName, true)
            .Subscribe(changes.Add);

        fixture.IsNotNullString = "Changed";

        await Assert.That(changes.Count).IsGreaterThanOrEqualTo(1);
    }

    /// <summary>Verifies that the expression-based overload works with isDistinct false.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExpressionOverload_IsDistinctFalse_EmitsAll()
    {
        EnsureInitialized();

        var fixture = new TestFixture();
        var changes = new List<IObservedChange<TestFixture, string>>();

        using var sub = fixture.ObservableForProperty(x => x.IsNotNullString, beforeChange: false, skipInitial: false, isDistinct: false)
            .Subscribe(changes.Add);

        fixture.IsNotNullString = "A";
        fixture.IsNotNullString = "B";

        await Assert.That(changes.Count).IsGreaterThanOrEqualTo(ExpectedTwoChanges);
    }

    /// <summary>Verifies that the expression-based overload works with beforeChange true.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExpressionOverload_BeforeChange_EmitsBeforePropertyChanges()
    {
        EnsureInitialized();

        var fixture = new TestFixture();
        var changes = new List<IObservedChange<TestFixture, string>>();

        using var sub = fixture.ObservableForProperty(x => x.IsNotNullString, true)
            .Subscribe(changes.Add);

        fixture.IsNotNullString = "Changed";

        await Assert.That(changes.Count).IsGreaterThanOrEqualTo(1);
    }

    /// <summary>
    /// Verifies that the string-based overload handles a non-existent property name gracefully.
    /// Covers the catch block at ReactiveNotifyPropertyChangedMixins lines 65-68 where
    /// Expression.Property throws and falls back to the parameter expression.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StringOverload_NonExistentProperty_DoesNotThrow()
    {
        EnsureInitialized();

        var fixture = new TestFixture();
        var changes = new List<IObservedChange<TestFixture, string>>();

        // "NonExistentProperty" doesn't exist on TestFixture, so Expression.Property will throw
        // and the catch block will set expr = parameter (a fallback).
        using var sub = fixture.ObservableForProperty<TestFixture, string>("NonExistentProperty")
            .Subscribe(changes.Add);

        // The observable should be created without throwing
        await Assert.That(changes.Count).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies that ObservableForProperty by name returns default when the property cannot be found via reflection.
    /// Covers ReactiveNotifyPropertyChangedMixins.cs line 82 (prop is null path in GetCurrentValue).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StringOverload_PropertyNotFoundViaReflection_ReturnsDefault()
    {
        EnsureInitialized();

        // Use an object where we observe a property name that exists in the
        // interface/expression parameter but does not exist on the runtime type.
        // ObservableForProperty<TSender, TValue>("NonExistentProperty") -
        // The factory won't fire since no PropertyChanged event for that name gets raised,
        // but the skipInitial=false path calls GetCurrentValue which will find prop is null.
        var fixture = new TestFixture();

        string? receivedValue = null;
        var received = false;
        using var sub = fixture.ObservableForProperty<TestFixture, string?>(
                "NonExistentProperty",
                skipInitial: false)
            .Subscribe(x =>
            {
                receivedValue = x.Value;
                received = true;
            });

        await Assert.That(received).IsTrue();
        await Assert.That(receivedValue).IsNull();
    }

    /// <summary>
    /// Verifies that the SubscribeToExpressionChain val cast branch is exercised.
    /// Covers ReactiveNotifyPropertyChangedMixins.cs line 211 (val is not null and val is not TValue).
    /// When val is null, the condition evaluates differently.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SubscribeToExpressionChain_NullValue_PassesThroughAsDefault()
    {
        EnsureInitialized();

        var fixture = new HostTestFixture { Child = null };

        var values = new List<IObservedChange<HostTestFixture, TestFixture?>>();

        using var sub = fixture.ObservableForProperty(
                x => x.Child,
                beforeChange: false,
                skipInitial: false,
                isDistinct: false)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);

        // The first value should be null since Child is null
        await Assert.That(values[0].Value).IsNull();
    }

    /// <summary>Resets and initializes the ReactiveUI binding infrastructure for testing.</summary>
    internal static void EnsureInitialized()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        _ = builder.WithCoreServices();
        _ = builder.BuildApp();
    }
}
