// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.GeneratedCode.TestModels.Scenarios;

/// <summary>
/// Scenario methods for BindInteraction that the source generator processes at compile time.
/// Each method exercises a specific BindInteraction overload with different handler types.
/// </summary>
public static class BindInteractionScenarios
{
    /// <summary>Binds a task-based handler that always sets output to <see langword="true"/>.</summary>
    /// <param name="vm">The source view model.</param>
    /// <param name="view">The target view.</param>
    /// <returns>A disposable representing the binding.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDisposable TaskHandler(
        SharedScenarios.BindInteraction.TaskHandler.MyViewModel vm,
        SharedScenarios.BindInteraction.TaskHandler.MyView view) =>
        SharedScenarios.BindInteraction.TaskHandler.Scenario.Execute(vm, view);

    /// <summary>Binds an observable-based handler that always sets output to <see langword="true"/>.</summary>
    /// <param name="vm">The source view model.</param>
    /// <param name="view">The target view.</param>
    /// <returns>A disposable representing the binding.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDisposable ObservableHandler(
        SharedScenarios.BindInteraction.ObservableHandler.MyViewModel vm,
        SharedScenarios.BindInteraction.ObservableHandler.MyView view) =>
        SharedScenarios.BindInteraction.ObservableHandler.Scenario.Execute(vm, view);

    /// <summary>Binds a task-based handler to a deep property path (nested interaction).</summary>
    /// <param name="vm">The source view model.</param>
    /// <param name="view">The target view.</param>
    /// <returns>A disposable representing the binding.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDisposable DeepPropertyPath(
        SharedScenarios.BindInteraction.DeepPropertyPath.MyViewModel vm,
        SharedScenarios.BindInteraction.DeepPropertyPath.MyView view) =>
        SharedScenarios.BindInteraction.DeepPropertyPath.Scenario.Execute(vm, view);
}
