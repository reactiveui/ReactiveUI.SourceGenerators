// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

namespace ReactiveUI.Binding.GeneratedCode.TestModels.Scenarios;

/// <summary>Scenario methods for Bind (view-first two-way compat alias) that the source generator processes at compile time.</summary>
public static class BindCompatScenarios
{
    /// <summary>View-first two-way binding for a string property using the Bind compat alias.</summary>
    /// <param name="view">The target view.</param>
    /// <param name="vm">The source view model.</param>
    /// <returns>A reactive binding representing the binding.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IReactiveBinding<TestView, BindingChange> StringProperty(
        TestView view,
        TestViewModel vm) =>
        view.Bind(vm, x => x.Name, x => x.DisplayName);

    /// <summary>View-first two-way binding through a nullable child using the Bind compat alias.</summary>
    /// <param name="view">The target view.</param>
    /// <param name="vm">The source view model.</param>
    /// <returns>A reactive binding representing the binding.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IReactiveBinding<TestView, BindingChange> ChildName(
        TestView view,
        TestViewModel vm) =>
        view.Bind(vm, x => x.Child!.Name, x => x.DisplayName);
}
