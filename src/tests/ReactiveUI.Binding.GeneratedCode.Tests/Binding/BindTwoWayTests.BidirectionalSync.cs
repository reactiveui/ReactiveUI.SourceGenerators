// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using ReactiveUI.Binding.GeneratedCode.TestModels.Scenarios;
using ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

namespace ReactiveUI.Binding.GeneratedCode.Tests.Binding;

/// <summary>Covers a two-way binding settling in both directions, including under rapid alternating writes.</summary>
public partial class BindTwoWayTests
{
    /// <summary>The pre-set source property value.</summary>
    private const string ValueSetBeforeBinding = "PreSet";

    /// <summary>The initial double property test value.</summary>
    private const double InitialMeasurement = 3.14;

    /// <summary>The replacement double property test value.</summary>
    private const double ReplacementMeasurement = 2.71;

    /// <summary>The square root of two double property test value.</summary>
    private const double ThirdMeasurement = 1.41;

    /// <summary>Verifies that BindTwoWay handles rapid back-and-forth changes without infinite loop.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindTwoWay_RapidBackAndForth()
    {
        var source = new BigViewModel { Prop1 = "Initial", };
        var target = new BigView();
        using var binding = BindTwoWayScenarios.StringProperty(source, target);

        // Rapid alternating changes
        source.Prop1 = "A";
        await Assert.That(target.ViewProp1).IsEqualTo("A");
        target.ViewProp1 = "B";
        await Assert.That(source.Prop1).IsEqualTo("B");
        source.Prop1 = "C";
        await Assert.That(target.ViewProp1).IsEqualTo("C");
        target.ViewProp1 = "D";
        await Assert.That(source.Prop1).IsEqualTo("D");
    }

    /// <summary>Verifies that BindTwoWay syncs double property in both directions.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindTwoWay_DoubleProperty_SyncsBothDirections()
    {
        var source = new BigViewModel { Prop3 = InitialMeasurement, };
        var target = new BigView();
        using var binding = BindTwoWayScenarios.DoubleProperty(source, target);
        await Assert.That(target.ViewProp3).IsEqualTo(InitialMeasurement);
        source.Prop3 = ReplacementMeasurement;
        await Assert.That(target.ViewProp3).IsEqualTo(ReplacementMeasurement);
        target.ViewProp3 = ThirdMeasurement;
        await Assert.That(source.Prop3).IsEqualTo(ThirdMeasurement);
    }

    /// <summary>Verifies that BindTwoWay syncs bool property in both directions.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindTwoWay_BoolProperty_SyncsBothDirections()
    {
        var source = new BigViewModel { Prop4 = true, };
        var target = new BigView();
        using var binding = BindTwoWayScenarios.BoolProperty(source, target);
        await Assert.That(target.ViewProp4).IsTrue();
        source.Prop4 = false;
        await Assert.That(target.ViewProp4).IsFalse();
        target.ViewProp4 = true;
        await Assert.That(source.Prop4).IsTrue();
    }

    /// <summary>Verifies that BindTwoWay correctly syncs a pre-set source value and then supports bidirectional changes after binding.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindTwoWay_SourcePropertySetBeforeBinding_SyncsAndBiDirectional()
    {
        var source = new BigViewModel { Prop1 = ValueSetBeforeBinding, };
        var target = new BigView();
        using var binding = BindTwoWayScenarios.StringProperty(source, target);

        // Pre-set value should sync
        await Assert.That(target.ViewProp1).IsEqualTo(ValueSetBeforeBinding);

        // Bidirectional should still work
        target.ViewProp1 = "FromView";
        await Assert.That(source.Prop1).IsEqualTo("FromView");
        source.Prop1 = "FromSource";
        await Assert.That(target.ViewProp1).IsEqualTo("FromSource");
    }
}
