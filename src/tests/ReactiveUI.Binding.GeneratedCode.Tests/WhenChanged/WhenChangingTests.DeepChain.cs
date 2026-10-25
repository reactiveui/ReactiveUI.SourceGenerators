// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.GeneratedCode.TestModels.Scenarios;
using ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

namespace ReactiveUI.Binding.GeneratedCode.Tests.WhenChanged;

/// <summary>Edge case tests for WhenChanging covering deep chains, multi-property change emission, and disposal scenarios.</summary>
public partial class WhenChangingTests
{
    /// <summary>The initial city value used by deep-chain tests.</summary>
    private const string InitialCity = "Seattle";

    /// <summary>The city the first change moves to, which the second change then leaves behind.</summary>
    private const string ReplacementCity = "Portland";

    /// <summary>The replacement value for the integer property in two-property tests.</summary>
    private const int ReplacementIntValue = 2;

    /// <summary>Verifies that deep chain WhenChanging emits the initial nested property value.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepChain_EmitsInitialValue()
    {
        var vm = new BigViewModel();
        vm.Address.City = InitialCity;
        var values = new List<string>();

        using var sub = WhenChangingScenarios.DeepChain_AddressCity(vm)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0]).IsEqualTo(InitialCity);
    }

    /// <summary>
    /// A before-change chain reports the value a property is about to leave behind, and reports it only when it
    /// differs from the one already reported. The first change is about to leave the value the initial emission
    /// already carried, so it says nothing; the second is about to leave a value nobody has seen.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepChain_EmitsTheValueEachNestedChangeLeavesBehind()
    {
        var vm = new BigViewModel();
        vm.Address.City = InitialCity;
        var values = new List<string>();

        using var sub = WhenChangingScenarios.DeepChain_AddressCity(vm)
            .Subscribe(values.Add);

        vm.Address.City = ReplacementCity;
        vm.Address.City = "Boston";

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(MinEmissionsAfterChange);
        await Assert.That(values[0]).IsEqualTo(InitialCity);
        await Assert.That(values[1]).IsEqualTo(ReplacementCity);
    }

    /// <summary>Verifies that WhenChanging two-property emits when either property is about to change.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TwoProperties_EmitsOnEitherChange()
    {
        var vm = new BigViewModel { Prop1 = "A", Prop2 = 1 };
        var values = new List<PropertyValues<string, int>>();

        using var sub = WhenChangingScenarios.TwoProperties(vm)
            .Subscribe(values.Add);

        // Initial emission
        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0].Property1).IsEqualTo("A");
        await Assert.That(values[0].Property2).IsEqualTo(1);

        // Change Prop1 — should emit before-change values
        vm.Prop1 = "B";

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(MinEmissionsAfterChange);

        // The before-change value for Prop1 should still be "A"
        await Assert.That(values[1].Property1).IsEqualTo("A");
        await Assert.That(values[1].Property2).IsEqualTo(1);
    }

    /// <summary>Verifies that disposing the WhenChanging deep chain subscription stops listening.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepChain_Disposal_StopsListening()
    {
        var vm = new BigViewModel();
        vm.Address.City = InitialCity;
        var values = new List<string>();

        var sub = WhenChangingScenarios.DeepChain_AddressCity(vm)
            .Subscribe(values.Add);

        sub.Dispose();

        vm.Address.City = "Portland";

        await Assert.That(values.Count).IsEqualTo(1);
        await Assert.That(values[0]).IsEqualTo(InitialCity);
    }

    /// <summary>Verifies that WhenChanging multi-property emits sequential before-change values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TwoProperties_SequentialChanges()
    {
        var vm = new BigViewModel { Prop1 = "A", Prop2 = 1 };
        var values = new List<PropertyValues<string, int>>();

        using var sub = WhenChangingScenarios.TwoProperties(vm)
            .Subscribe(values.Add);

        vm.Prop1 = "B";
        vm.Prop2 = ReplacementIntValue;

        // Should have at least 3 emissions: initial + 2 changes
        await Assert.That(values.Count).IsGreaterThanOrEqualTo(MinEmissionsAfterTwoChanges);
    }
}
