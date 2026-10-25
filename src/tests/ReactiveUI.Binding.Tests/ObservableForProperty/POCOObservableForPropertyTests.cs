// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Expressions;
using ReactiveUI.Binding.ObservableForProperty;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.ObservableForProperty;

/// <summary>Tests for the <see cref="POCOObservableForProperty"/> class.</summary>
public class POCOObservableForPropertyTests
{
    /// <summary>The name of the property the POCO tests observe.</summary>
    private const string ValuePropertyName = "Value";

    /// <summary>Verifies affinity is always 1.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetAffinityForObject_AnyType_Returns1()
    {
        var sut = new POCOObservableForProperty();

        var affinity = sut.GetAffinityForObject(typeof(PocoModel), ValuePropertyName);

        await Assert.That(affinity).IsEqualTo(1);
    }

    /// <summary>Verifies affinity is 1 even for INPC types (POCO is the fallback).</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetAffinityForObject_INPCType_StillReturns1()
    {
        var sut = new POCOObservableForProperty();

        var affinity = sut.GetAffinityForObject(typeof(TestViewModel), "Name");

        await Assert.That(affinity).IsEqualTo(1);
    }

    /// <summary>Verifies that GetNotificationForProperty emits exactly one value on subscription.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetNotificationForProperty_EmitsOneValue()
    {
        var sut = new POCOObservableForProperty();
        var model = new PocoModel { Value = "Hello" };

        Expression<Func<PocoModel, string>> expr = x => x.Value;
        var body = Reflection.Rewrite(expr.Body);

        var count = 0;
        using var sub = sut.GetNotificationForProperty(model, body, ValuePropertyName)
            .Take(1)
            .Subscribe(_ => count++);

        await Assert.That(count).IsEqualTo(1);
    }

    /// <summary>Verifies that GetNotificationForProperty does not emit more than once even when property changes.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetNotificationForProperty_NoFurtherEmissions()
    {
        var sut = new POCOObservableForProperty();
        var model = new PocoModel { Value = "Hello" };

        Expression<Func<PocoModel, string>> expr = x => x.Value;
        var body = Reflection.Rewrite(expr.Body);

        var values = new List<IObservedChange<object, object?>>();
        using var sub = sut.GetNotificationForProperty(model, body, ValuePropertyName)
            .Take(1)
            .Subscribe(values.Add);

        model.Value = "World";

        // Only the initial value should have been emitted
        await Assert.That(values.Count).IsEqualTo(1);
    }
}
