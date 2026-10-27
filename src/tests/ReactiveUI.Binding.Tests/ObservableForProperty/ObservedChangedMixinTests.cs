// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Expressions;
using ReactiveUI.Binding.ObservableForProperty;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.ObservableForProperty;

/// <summary>Tests for the <see cref="ObservedChangedMixins"/> class.</summary>
public class ObservedChangedMixinTests
{
    /// <summary>The value assigned through the observed change under test.</summary>
    private const string NewValue = "NewValue";

    /// <summary>The age assigned to the sample view model.</summary>
    private const int SampleAge = 42;

    /// <summary>A directly-populated change value used across tests.</summary>
    private const string DirectValue = "Direct";

    /// <summary>A sample integer value carried by an observed change.</summary>
    private const int SampleIntValue = 99;

    /// <summary>The expected number of mapped values from a two-element change stream.</summary>
    private const int ExpectedTwoValues = 2;

    /// <summary>Verifies that GetPropertyName returns the correct property name from an observed change.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetPropertyName_SimpleProperty_ReturnsName()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);

        var change = new ObservedChange<TestViewModel, string>(new(), body, "Hello");

        var name = change.GetPropertyName();

        await Assert.That(name).IsEqualTo("Name");
    }

    /// <summary>Verifies that GetPropertyName returns a dotted path for nested properties.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetPropertyName_NestedProperty_ReturnsDottedPath()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Address!.City;
        var body = Reflection.Rewrite(expr.Body);

        var change = new ObservedChange<TestViewModel, string>(new(), body, "Seattle");

        var name = change.GetPropertyName();

        await Assert.That(name).IsEqualTo("Address.City");
    }

    /// <summary>Verifies that GetValue returns the value when it's already populated.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetValue_WithPopulatedValue_ReturnsValue()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);

        var change = new ObservedChange<TestViewModel, string>(new() { Name = "Test" }, body, DirectValue);

        var value = change.GetValue();

        await Assert.That(value).IsEqualTo(DirectValue);
    }

    /// <summary>Verifies that GetValue evaluates the expression chain when value is default.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetValue_WithDefaultValue_EvaluatesChain()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);

        var vm = new TestViewModel { Name = "Evaluated" };
        var change = new ObservedChange<TestViewModel, string>(vm, body, default!);

        var value = change.GetValue();

        await Assert.That(value).IsEqualTo("Evaluated");
    }

    /// <summary>Verifies that GetValueOrDefault returns default when chain is broken.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetValueOrDefault_NullInChain_ReturnsDefault()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Address!.City;
        var body = Reflection.Rewrite(expr.Body);

        var vm = new TestViewModel(); // Address is null
        var change = new ObservedChange<TestViewModel, string>(vm, body, default!);

        var value = change.GetValueOrDefault();

        await Assert.That(value).IsNull();
    }

    /// <summary>Verifies that GetPropertyName throws for null input.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetPropertyName_NullItem_ThrowsArgumentNullException()
    {
        IObservedChange<TestViewModel, string>? nullChange = null;

        await Assert.That(() => nullChange!.GetPropertyName())
            .ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Verifies that GetValue throws for null input.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetValue_NullItem_ThrowsArgumentNullException()
    {
        IObservedChange<TestViewModel, string>? nullChange = null;

        await Assert.That(() => nullChange!.GetValue())
            .ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Verifies that GetValueOrDefault throws for null input.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetValueOrDefault_NullItem_ThrowsArgumentNullException()
    {
        IObservedChange<TestViewModel, string>? nullChange = null;

        await Assert.That(() => nullChange!.GetValueOrDefault())
            .ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Verifies that GetValue returns the populated value for a non-default value.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetValue_NonDefaultValue_ReturnsDirectValue()
    {
        Expression<Func<TestViewModel, int>> expr = x => x.Age;
        var body = Reflection.Rewrite(expr.Body);

        var vm = new TestViewModel { Age = SampleAge };
        var change = new ObservedChange<TestViewModel, int>(vm, body, SampleIntValue);

        var value = change.GetValue();

        await Assert.That(value).IsEqualTo(SampleIntValue);
    }

    /// <summary>Verifies that GetValueOrDefault returns the value when it's non-default.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetValueOrDefault_WithNonDefaultValue_ReturnsValue()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);

        var change = new ObservedChange<TestViewModel, string>(new() { Name = "Test" }, body, DirectValue);

        var value = change.GetValueOrDefault();

        await Assert.That(value).IsEqualTo(DirectValue);
    }

    /// <summary>Verifies that GetValue throws when the expression chain is broken (null intermediate).</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetValue_BrokenChain_ThrowsException()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Address!.City;
        var body = Reflection.Rewrite(expr.Body);

        var vm = new TestViewModel(); // Address is null, chain is broken
        var change = new ObservedChange<TestViewModel, string>(vm, body, default!);

        await Assert.That(() => change.GetValue())
            .ThrowsExactly<InvalidOperationException>();
    }

    /// <summary>Verifies that Value() observable extension maps a stream of changes to values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Value_MapsChangesToValues()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);

        var change1 = new ObservedChange<TestViewModel, string>(new(), body, "First");
        var change2 = new ObservedChange<TestViewModel, string>(new(), body, "Second");

        var stream = new[] { change1, change2 }.ToObservable();
        var values = new List<string>();

        using var sub = stream.Value().Subscribe(values.Add);

        await Assert.That(values).Count().IsEqualTo(ExpectedTwoValues);
        await Assert.That(values[0]).IsEqualTo("First");
        await Assert.That(values[1]).IsEqualTo("Second");
    }

    /// <summary>Verifies that SetValueToProperty applies the change to the target object.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SetValueToProperty_ValidTarget_SetsValue()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);

        var change = new ObservedChange<TestViewModel, string>(new(), body, NewValue);
        var target = new TestViewModel { Name = "OldValue" };

        change.SetValueToProperty(target, x => x.Name);

        await Assert.That(target.Name).IsEqualTo(NewValue);
    }

    /// <summary>Verifies that SetValueToProperty does nothing when target is null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SetValueToProperty_NullTarget_DoesNotThrow()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);

        var change = new ObservedChange<TestViewModel, string>(new(), body, NewValue);
        TestViewModel? target = null;

        // Should not throw
        change.SetValueToProperty(target!, x => x.Name);

        await Assert.That(target).IsNull();
    }
}
