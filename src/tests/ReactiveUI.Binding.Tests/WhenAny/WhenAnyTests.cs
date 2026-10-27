// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.WhenAny;

/// <summary>Tests for the WhenAny extension methods which provide IObservedChange context.</summary>
public class WhenAnyTests
{
    /// <summary>The expected number of emitted values after the second notification.</summary>
    private const int ExpectedTwoEmissions = 2;

    /// <summary>Verifies that WhenAny with a single property and selector returns the selector result.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_WithSelector_ReturnsObservedChange()
    {
        EnsureInitialized();

        var fixture = new TestFixture { IsNotNullString = "Initial" };
        var values = new List<string>();

        using var sub = fixture.WhenAnyUnsafe(
                x => x.IsNotNullString,
                static change => change.Value)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0]).IsEqualTo("Initial");

        fixture.IsNotNullString = "Changed";

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(ExpectedTwoEmissions);
        await Assert.That(values[1]).IsEqualTo("Changed");
    }

    /// <summary>Verifies that WhenAny with two properties combines their observed changes.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TwoProperties_CombinesWithSelector()
    {
        EnsureInitialized();

        var fixture = new TestFixture { IsNotNullString = "Hello", IsOnlyOneWord = "World" };
        var values = new List<string>();

        using var sub = fixture.WhenAnyUnsafe(
                x => x.IsNotNullString,
                x => x.IsOnlyOneWord,
                static (c1, c2) => $"{c1.Value} {c2.Value}")
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0]).IsEqualTo("Hello World");
    }

    /// <summary>Verifies that WhenAny returns the current value on subscription.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReturnsCurrentValueOnSubscription()
    {
        EnsureInitialized();

        var fixture = new TestFixture { IsNotNullString = "PreExisting" };
        var values = new List<string>();

        using var sub = fixture.WhenAnyUnsafe(
                x => x.IsNotNullString,
                static change => change.Value)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0]).IsEqualTo("PreExisting");
    }

    /// <summary>Verifies that WhenAny works with plain INPC objects.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WorksWithINotifyPropertyChangedObjects()
    {
        EnsureInitialized();

        var obj = new NonReactiveINotifyPropertyChangedObject { InpcProperty = "Start" };
        var values = new List<string>();

        using var sub = obj.WhenAnyUnsafe(
                x => x.InpcProperty,
                static change => change.Value)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0]).IsEqualTo("Start");

        obj.InpcProperty = "End";

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(ExpectedTwoEmissions);
    }

    /// <summary>Verifies that WhenAny exposes the sender through the observed change.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExposesSenderThroughObservedChange()
    {
        EnsureInitialized();

        var fixture = new TestFixture { IsNotNullString = "Test" };
        IObservedChange<TestFixture, string>? captured = null;

        using var sub = fixture.WhenAnyUnsafe(
                x => x.IsNotNullString,
                static change => change)
            .Subscribe(c => captured = c);

        await Assert.That(captured).IsNotNull();
        await Assert.That(captured!.Sender).IsEqualTo(fixture);
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
