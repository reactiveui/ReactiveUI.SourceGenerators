// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using ReactiveUI.Binding.GeneratedCode.TestModels.Scenarios;
using ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

namespace ReactiveUI.Binding.GeneratedCode.Tests.Binding;

/// <summary>Covers one-way bindings whose source is a chain, and repeated bindings onto one target.</summary>
public partial class BindOneWayTests
{
    /// <summary>The initial deep-chain city value.</summary>
    private const string InitialCity = "Seattle";

    /// <summary>The replacement deep-chain city value.</summary>
    private const string ReplacementCity = "Portland";

    /// <summary>The pre-set source property value.</summary>
    private const string ValueSetBeforeBinding = "PreSet";

    /// <summary>The number of rapid sequential changes in the rapid-changes test.</summary>
    private const int RapidChangeCount = 100;

    /// <summary>Verifies that BindOneWay with a deep chain source property syncs the initial value.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_DeepChainSource_SyncsInitialValue()
    {
        var source = new BigViewModel();
        source.Address.City = InitialCity;
        var target = new BigView();
        using var binding = BindOneWayScenarios.DeepChainProperty(source, target);
        await Assert.That(target.ViewProp1).IsEqualTo(InitialCity);
    }

    /// <summary>Verifies that BindOneWay with a deep chain source property syncs nested changes.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_DeepChainSource_SyncsNestedChanges()
    {
        var source = new BigViewModel();
        source.Address.City = InitialCity;
        var target = new BigView();
        using var binding = BindOneWayScenarios.DeepChainProperty(source, target);
        source.Address.City = ReplacementCity;
        await Assert.That(target.ViewProp1).IsEqualTo(ReplacementCity);
    }

    /// <summary>Verifies that BindOneWay with a deep chain re-subscribes on intermediate replacement.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_DeepChainSource_IntermediateReplacement()
    {
        var source = new BigViewModel();
        source.Address.City = InitialCity;
        var target = new BigView();
        using var binding = BindOneWayScenarios.DeepChainProperty(source, target);
        source.Address = new() { City = ReplacementCity, };
        await Assert.That(target.ViewProp1).IsEqualTo(ReplacementCity);

        // Further changes on the new address
        source.Address.City = "Eugene";
        await Assert.That(target.ViewProp1).IsEqualTo("Eugene");
    }

    /// <summary>Verifies that multiple BindOneWay bindings to the same target property both function correctly (last write wins).</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_MultipleBindingsToSameTarget()
    {
        var source = new BigViewModel { Prop1 = "From1", Prop5 = "From5", };
        var target = new BigView();

        // Both bind to the same target: ViewProp1 and ViewProp5
        using var binding1 = BindOneWayScenarios.StringProperty(source, target);
        using var binding2 = BindOneWayScenarios.StringProperty5(source, target);

        // Each binding independently syncs its own target
        await Assert.That(target.ViewProp1).IsEqualTo("From1");
        await Assert.That(target.ViewProp5).IsEqualTo("From5");
        source.Prop1 = "Updated1";
        await Assert.That(target.ViewProp1).IsEqualTo("Updated1");
        source.Prop5 = "Updated5";
        await Assert.That(target.ViewProp5).IsEqualTo("Updated5");
    }

    /// <summary>Verifies that disposing a BindOneWay with deep chain source stops syncing.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_DeepChainSource_Disposal()
    {
        var source = new BigViewModel();
        source.Address.City = InitialCity;
        var target = new BigView();
        var binding = BindOneWayScenarios.DeepChainProperty(source, target);
        await Assert.That(target.ViewProp1).IsEqualTo(InitialCity);
        binding.Dispose();
        source.Address.City = ReplacementCity;
        await Assert.That(target.ViewProp1).IsEqualTo(InitialCity);
    }

    /// <summary>
    /// Verifies that setting a source property before establishing the binding
    /// correctly syncs the pre-set value to the target on subscription.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_SourcePropertySetBeforeBinding_SyncsOnSubscription()
    {
        var source = new BigViewModel { Prop1 = ValueSetBeforeBinding, };
        var target = new BigView();

        // Property is already set before binding is created
        using var binding = BindOneWayScenarios.StringProperty(source, target);
        await Assert.That(target.ViewProp1).IsEqualTo(ValueSetBeforeBinding);
    }

    /// <summary>Verifies that BindOneWay handles rapid sequential changes without missing any.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_RapidChanges_AllSynced()
    {
        var source = new BigViewModel { Prop1 = "Start", };
        var target = new BigView();
        using var binding = BindOneWayScenarios.StringProperty(source, target);
        for (var i = 0; i < RapidChangeCount; i++)
        {
            source.Prop1 = $"Value_{i}";
        }

        await Assert.That(target.ViewProp1).IsEqualTo("Value_99");
    }
}
