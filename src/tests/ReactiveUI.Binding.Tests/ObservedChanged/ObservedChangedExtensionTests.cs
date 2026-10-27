// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.Expressions;
using ReactiveUI.Binding.ObservableForProperty;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.ObservedChanged;

/// <summary>Tests for ObservedChangedMixins extension methods (Value, GetPropertyName, GetValue, GetValueOrDefault).</summary>
public class ObservedChangedExtensionTests
{
    /// <summary>The expected number of emissions after the initial value plus one change.</summary>
    private const int ExpectedTwoEmissions = 2;

    /// <summary>Verifies that the Value extension converts observed changes to values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Value_ConvertsObservedChangesToValues()
    {
        EnsureInitialized();

        var fixture = new TestFixture { IsNotNullString = "Start" };
        var values = new List<string>();

        using var sub = fixture.ObservableForProperty(x => x.IsNotNullString, skipInitial: false)
            .Value()
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0]).IsEqualTo("Start");

        fixture.IsNotNullString = "End";

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(ExpectedTwoEmissions);
        await Assert.That(values[1]).IsEqualTo("End");
    }

    /// <summary>Verifies that GetPropertyName returns the correct property path.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetPropertyName_ReturnsCorrectPath()
    {
        Expression<Func<TestFixture, string>> expr = x => x.IsNotNullString;
        var body = Reflection.Rewrite(expr.Body);
        var change = new ObservedChange<TestFixture, string>(new(), body, "value");

        var name = change.GetPropertyName();
        await Assert.That(name).IsEqualTo("IsNotNullString");
    }

    /// <summary>Verifies that GetPropertyName returns a dotted path for nested properties.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetPropertyName_Nested_ReturnsDottedPath()
    {
        Expression<Func<HostTestFixture, string>> expr = x => x.Child!.IsOnlyOneWord;
        var body = Reflection.Rewrite(expr.Body);
        var change = new ObservedChange<HostTestFixture, string>(new(), body, "value");

        var name = change.GetPropertyName();
        await Assert.That(name).IsEqualTo("Child.IsOnlyOneWord");
    }

    /// <summary>Verifies that GetValue evaluates the expression chain to get the current value.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetValue_EvaluatesExpressionChain()
    {
        var fixture = new TestFixture { IsNotNullString = "Evaluated" };
        Expression<Func<TestFixture, string>> expr = x => x.IsNotNullString;
        var body = Reflection.Rewrite(expr.Body);

        // Create a change with default value to force expression evaluation
        var change = new ObservedChange<TestFixture, string>(fixture, body, default!);
        var value = change.GetValue();

        await Assert.That(value).IsEqualTo("Evaluated");
    }

    /// <summary>Verifies that GetValueOrDefault returns default when the chain contains a null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetValueOrDefault_NullChain_ReturnsDefault()
    {
        var fixture = new HostTestFixture(); // Child is null
        Expression<Func<HostTestFixture, string>> expr = x => x.Child!.IsOnlyOneWord;
        var body = Reflection.Rewrite(expr.Body);

        var change = new ObservedChange<HostTestFixture, string>(fixture, body, default!);
        var value = change.GetValueOrDefault();

        await Assert.That(value).IsNull();
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
