// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.GeneratedCode.TestModels.Scenarios;
using ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

namespace ReactiveUI.Binding.GeneratedCode.Tests.Binding;

/// <summary>Tests that the source-generator-generated BindTwoWay code works correctly at runtime.</summary>
public partial class BindTwoWayTests
{
    /// <summary>The initial string property value used across the binding tests.</summary>
    private const string InitialPropertyValue = "Hello";

    /// <summary>The initial integer property test value.</summary>
    private const int IntValue = 42;

    /// <summary>The replacement integer property test value.</summary>
    private const int ReplacementIntValue = 100;

    /// <summary>The second replacement integer property test value.</summary>
    private const int SecondIntValue = 200;

    /// <summary>The value written to the view side to prove the target-to-source direction.</summary>
    private const string FromTargetValue = "FromTarget";

    /// <summary>Verifies that BindTwoWay syncs the initial value from source to target.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StringProperty_SyncsSourceToTarget()
    {
        var source = new BigViewModel { Prop1 = InitialPropertyValue };
        var target = new BigView();

        using var binding = BindTwoWayScenarios.StringProperty(source, target);

        await Assert.That(target.ViewProp1).IsEqualTo(InitialPropertyValue);

        source.Prop1 = "World";

        await Assert.That(target.ViewProp1).IsEqualTo("World");
    }

    /// <summary>Verifies that BindTwoWay syncs changes from target back to source.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StringProperty_SyncsTargetToSource()
    {
        var source = new BigViewModel { Prop1 = InitialPropertyValue };
        var target = new BigView();

        using var binding = BindTwoWayScenarios.StringProperty(source, target);

        target.ViewProp1 = FromTargetValue;

        await Assert.That(source.Prop1).IsEqualTo(FromTargetValue);
    }

    /// <summary>Verifies that BindTwoWay syncs int property in both directions.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IntProperty_SyncsBothDirections()
    {
        var source = new BigViewModel { Prop2 = IntValue };
        var target = new BigView();

        using var binding = BindTwoWayScenarios.IntProperty(source, target);

        await Assert.That(target.ViewProp2).IsEqualTo(IntValue);

        source.Prop2 = ReplacementIntValue;
        await Assert.That(target.ViewProp2).IsEqualTo(ReplacementIntValue);

        target.ViewProp2 = SecondIntValue;
        await Assert.That(source.Prop2).IsEqualTo(SecondIntValue);
    }

    /// <summary>Verifies that disposing the two-way binding stops syncing in both directions.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Disposal_StopsSyncing()
    {
        var source = new BigViewModel { Prop1 = InitialPropertyValue };
        var target = new BigView();

        var binding = BindTwoWayScenarios.StringProperty(source, target);
        await Assert.That(target.ViewProp1).IsEqualTo(InitialPropertyValue);

        binding.Dispose();

        source.Prop1 = "AfterDisposal";
        await Assert.That(target.ViewProp1).IsEqualTo(InitialPropertyValue);

        target.ViewProp1 = FromTargetValue;
        await Assert.That(source.Prop1).IsEqualTo("AfterDisposal");
    }
}
