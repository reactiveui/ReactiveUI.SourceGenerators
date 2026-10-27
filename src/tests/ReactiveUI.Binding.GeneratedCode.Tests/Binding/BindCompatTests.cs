// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.GeneratedCode.TestModels.Scenarios;
using ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

namespace ReactiveUI.Binding.GeneratedCode.Tests.Binding;

/// <summary>Tests that the Bind compat alias (view-first two-way syntax) works correctly at runtime.</summary>
public class BindCompatTests
{
    /// <summary>The initial property value used across the binding tests.</summary>
    private const string InitialPropertyValue = "Hello";

    /// <summary>The value the view model changes to.</summary>
    private const string ChangedPropertyValue = "World";

    /// <summary>The value the view writes back.</summary>
    private const string ViewPropertyValue = "FromView";

    /// <summary>Verifies that Bind syncs the initial value from view model to view.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Bind_SyncsInitialValue()
    {
        var vm = new TestViewModel { Name = InitialPropertyValue };
        var view = new TestView();

        using var binding = BindCompatScenarios.StringProperty(view, vm);

        await Assert.That(view.DisplayName).IsEqualTo(InitialPropertyValue);
    }

    /// <summary>Verifies that Bind syncs changes from view model to view.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Bind_SyncsSourceToView()
    {
        var vm = new TestViewModel { Name = InitialPropertyValue };
        var view = new TestView();

        using var binding = BindCompatScenarios.StringProperty(view, vm);

        vm.Name = ChangedPropertyValue;

        await Assert.That(view.DisplayName).IsEqualTo(ChangedPropertyValue);
    }

    /// <summary>Verifies that Bind syncs changes from view back to view model.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Bind_SyncsViewToSource()
    {
        var vm = new TestViewModel { Name = InitialPropertyValue };
        var view = new TestView();

        using var binding = BindCompatScenarios.StringProperty(view, vm);

        view.DisplayName = ViewPropertyValue;

        await Assert.That(vm.Name).IsEqualTo(ViewPropertyValue);
    }

    /// <summary>Verifies that disposing the Bind binding stops syncing.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Bind_Disposal_StopsSyncing()
    {
        var vm = new TestViewModel { Name = InitialPropertyValue };
        var view = new TestView();

        var binding = BindCompatScenarios.StringProperty(view, vm);
        binding.Dispose();

        vm.Name = "AfterDisposal";

        await Assert.That(view.DisplayName).IsEqualTo(InitialPropertyValue);
    }

    /// <summary>Verifies that Bind leaves the view untouched while the view model path passes through null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Bind_NullIntermediate_LeavesViewUntouched()
    {
        var vm = new TestViewModel();
        var view = new TestView { DisplayName = InitialPropertyValue };

        using var binding = BindCompatScenarios.ChildName(view, vm);
        await Assert.That(view.DisplayName).IsEqualTo(InitialPropertyValue);

        var child = new TestViewModel { Name = ChangedPropertyValue };
        vm.Child = child;
        await Assert.That(view.DisplayName).IsEqualTo(ChangedPropertyValue);

        vm.Child = null;
        await Assert.That(view.DisplayName).IsEqualTo(ChangedPropertyValue);

        view.DisplayName = ViewPropertyValue;
        await Assert.That(child.Name).IsEqualTo(ChangedPropertyValue);
    }
}
