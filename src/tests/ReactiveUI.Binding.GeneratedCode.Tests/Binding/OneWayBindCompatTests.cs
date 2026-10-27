// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.GeneratedCode.TestModels.Scenarios;
using ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

namespace ReactiveUI.Binding.GeneratedCode.Tests.Binding;

/// <summary>Tests that the OneWayBind compat alias (view-first syntax) works correctly at runtime.</summary>
public class OneWayBindCompatTests
{
    /// <summary>The initial property value used across the binding tests.</summary>
    private const string InitialPropertyValue = "Hello";

    /// <summary>The value the view model changes to.</summary>
    private const string ChangedPropertyValue = "World";

    /// <summary>Verifies that OneWayBind syncs the initial value from view model to view.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OneWayBind_SyncsInitialValue()
    {
        var vm = new TestViewModel { Name = InitialPropertyValue };
        var view = new TestView();

        using var binding = OneWayBindCompatScenarios.StringProperty(view, vm);

        await Assert.That(view.DisplayName).IsEqualTo(InitialPropertyValue);
    }

    /// <summary>Verifies that OneWayBind syncs changes from view model to view.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OneWayBind_SyncsOnSourceChange()
    {
        var vm = new TestViewModel { Name = InitialPropertyValue };
        var view = new TestView();

        using var binding = OneWayBindCompatScenarios.StringProperty(view, vm);

        vm.Name = ChangedPropertyValue;

        await Assert.That(view.DisplayName).IsEqualTo(ChangedPropertyValue);
    }

    /// <summary>Verifies that disposing the OneWayBind binding stops syncing.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OneWayBind_Disposal_StopsSyncing()
    {
        var vm = new TestViewModel { Name = InitialPropertyValue };
        var view = new TestView();

        var binding = OneWayBindCompatScenarios.StringProperty(view, vm);
        binding.Dispose();

        vm.Name = "AfterDisposal";

        await Assert.That(view.DisplayName).IsEqualTo(InitialPropertyValue);
    }

    /// <summary>Verifies that OneWayBind leaves the view untouched while the path passes through null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OneWayBind_NullIntermediate_LeavesViewUntouched()
    {
        var vm = new TestViewModel();
        var view = new TestView { DisplayName = InitialPropertyValue };

        using var binding = OneWayBindCompatScenarios.ChildName(view, vm);
        await Assert.That(view.DisplayName).IsEqualTo(InitialPropertyValue);

        vm.Child = new() { Name = ChangedPropertyValue };
        await Assert.That(view.DisplayName).IsEqualTo(ChangedPropertyValue);

        vm.Child = null;
        await Assert.That(view.DisplayName).IsEqualTo(ChangedPropertyValue);
    }
}
