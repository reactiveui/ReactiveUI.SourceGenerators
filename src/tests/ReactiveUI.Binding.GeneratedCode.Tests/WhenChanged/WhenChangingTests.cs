// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.GeneratedCode.TestModels.Scenarios;
using ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

namespace ReactiveUI.Binding.GeneratedCode.Tests.WhenChanged;

/// <summary>
/// Tests that the source-generator-generated WhenChanging code works correctly at runtime.
/// WhenChanging emits the property value before the change is applied.
/// </summary>
public partial class WhenChangingTests
{
    /// <summary>The initial name value used by single-property tests.</summary>
    private const string InitialName = "Initial";

    /// <summary>The minimum number of emissions expected after a single change.</summary>
    private const int MinEmissionsAfterChange = 2;

    /// <summary>The minimum number of emissions expected after two changes.</summary>
    private const int MinEmissionsAfterTwoChanges = 3;

    /// <summary>The index of the third emission.</summary>
    private const int ThirdEmissionIndex = 2;

    /// <summary>The two-property test value for the integer property.</summary>
    private const int TwoPropIntValue = 42;

    /// <summary>The three-property test value for the integer property.</summary>
    private const int ThreePropIntValue = 10;

    /// <summary>The four-property test value for the integer property.</summary>
    private const int FourPropIntValue = 20;

    /// <summary>The three-property test value for the double property.</summary>
    private const double ThreePropDoubleValue = 3.14;

    /// <summary>The four-property test value for the double property.</summary>
    private const double FourPropDoubleValue = 2.71;

    /// <summary>The value present before the change, which a before-change observation must report.</summary>
    private const string BeforeValue = "Before";

    /// <summary>Verifies that a single-property WhenChanging emits the initial value.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_EmitsInitialValue()
    {
        var vm = new TestViewModel { Name = InitialName };
        var values = new List<string>();

        using var sub = WhenChangingScenarios.SingleProperty_Name(vm)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0]).IsEqualTo(InitialName);
    }

    /// <summary>
    /// Verifies that a single-property WhenChanging emits when the property is about to change.
    /// The emitted value is the value before the change (the old value).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_EmitsBeforeChange()
    {
        var vm = new TestViewModel { Name = BeforeValue };
        var values = new List<string>();

        using var sub = WhenChangingScenarios.SingleProperty_Name(vm)
            .Subscribe(values.Add);

        vm.Name = "After";

        // WhenChanging emits the value at the time of PropertyChanging event,
        // which is the old value (before the assignment).
        await Assert.That(values.Count).IsGreaterThanOrEqualTo(MinEmissionsAfterChange);
        await Assert.That(values[0]).IsEqualTo(BeforeValue);
        await Assert.That(values[1]).IsEqualTo(BeforeValue);
    }

    /// <summary>Verifies that a single-property WhenChanging emits sequential before-change values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_EmitsSequentialBeforeChangeValues()
    {
        var vm = new TestViewModel { Name = "A" };
        var values = new List<string>();

        using var sub = WhenChangingScenarios.SingleProperty_Name(vm)
            .Subscribe(values.Add);

        vm.Name = "B";
        vm.Name = "C";

        // Initial "A", then "A" before change to "B", then "B" before change to "C"
        await Assert.That(values.Count).IsGreaterThanOrEqualTo(MinEmissionsAfterTwoChanges);
        await Assert.That(values[0]).IsEqualTo("A");
        await Assert.That(values[1]).IsEqualTo("A");
        await Assert.That(values[ThirdEmissionIndex]).IsEqualTo("B");
    }

    /// <summary>Verifies that two-property WhenChanging emits initial values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TwoProperties_EmitsInitialTuple()
    {
        var vm = new BigViewModel { Prop1 = "Hello", Prop2 = TwoPropIntValue };
        var values = new List<PropertyValues<string, int>>();

        using var sub = WhenChangingScenarios.TwoProperties(vm)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0].Property1).IsEqualTo("Hello");
        await Assert.That(values[0].Property2).IsEqualTo(TwoPropIntValue);
    }

    /// <summary>Verifies that three-property WhenChanging emits initial values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ThreeProperties_EmitsInitialValues()
    {
        var vm = new BigViewModel { Prop1 = "X", Prop2 = ThreePropIntValue, Prop3 = ThreePropDoubleValue };
        var values = new List<PropertyValues<string, int, double>>();

        using var sub = WhenChangingScenarios.ThreeProperties(vm)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0].Property1).IsEqualTo("X");
        await Assert.That(values[0].Property2).IsEqualTo(ThreePropIntValue);
        await Assert.That(values[0].Property3).IsEqualTo(ThreePropDoubleValue);
    }

    /// <summary>Verifies that four-property WhenChanging emits initial values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FourProperties_EmitsInitialValues()
    {
        var vm = new BigViewModel { Prop1 = "Y", Prop2 = FourPropIntValue, Prop3 = FourPropDoubleValue, Prop4 = true };
        var values = new List<PropertyValues<string, int, double, bool>>();

        using var sub = WhenChangingScenarios.FourProperties(vm)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0].Property1).IsEqualTo("Y");
        await Assert.That(values[0].Property2).IsEqualTo(FourPropIntValue);
        await Assert.That(values[0].Property3).IsEqualTo(FourPropDoubleValue);
        await Assert.That(values[0].Property4).IsTrue();
    }

    /// <summary>Verifies that disposing the subscription stops listening for changes.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Disposal_StopsListening()
    {
        var vm = new TestViewModel { Name = InitialName };
        var values = new List<string>();

        var sub = WhenChangingScenarios.SingleProperty_Name(vm)
            .Subscribe(values.Add);

        sub.Dispose();

        vm.Name = "AfterDisposal";

        await Assert.That(values.Count).IsEqualTo(1);
        await Assert.That(values[0]).IsEqualTo(InitialName);
    }
}
