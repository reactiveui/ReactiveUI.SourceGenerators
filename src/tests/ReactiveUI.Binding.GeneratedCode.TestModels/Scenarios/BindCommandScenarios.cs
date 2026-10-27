// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.GeneratedCode.TestModels.Scenarios;

/// <summary>
/// Scenario methods for BindCommand that the source generator processes at compile time.
/// Each method exercises a specific BindCommand overload with different configurations.
/// </summary>
public static class BindCommandScenarios
{
    /// <summary>Binds the Save command to the SaveButton's Click event with no parameter.</summary>
    /// <param name="vm">The source view model.</param>
    /// <param name="view">The target view.</param>
    /// <returns>A disposable representing the binding.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDisposable BasicNoParam(
        SharedScenarios.BindCommand.BasicNoParam.MyViewModel vm,
        SharedScenarios.BindCommand.BasicNoParam.MyView view) =>
        SharedScenarios.BindCommand.BasicNoParam.Scenario.Execute(vm, view);

    /// <summary>Binds the Save command with an expression-based parameter from the VM.</summary>
    /// <param name="vm">The source view model.</param>
    /// <param name="view">The target view.</param>
    /// <returns>A disposable representing the binding.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDisposable ExpressionParam(
        SharedScenarios.BindCommand.ExpressionParam.MyViewModel vm,
        SharedScenarios.BindCommand.ExpressionParam.MyView view) =>
        SharedScenarios.BindCommand.ExpressionParam.Scenario.Execute(vm, view);

    /// <summary>Binds the Save command with an observable parameter.</summary>
    /// <param name="vm">The source view model.</param>
    /// <param name="view">The target view.</param>
    /// <param name="parameter">An observable producing command parameters.</param>
    /// <returns>A disposable representing the binding.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDisposable ObservableParam(
        SharedScenarios.BindCommand.ObservableParam.MyViewModel vm,
        SharedScenarios.BindCommand.ObservableParam.MyView view,
        IObservable<string> parameter) =>
        SharedScenarios.BindCommand.ObservableParam.Scenario.Execute(vm, view, parameter);

    /// <summary>Binds the Save command to the SaveButton's MouseUp event via explicit toEvent.</summary>
    /// <param name="vm">The source view model.</param>
    /// <param name="view">The target view.</param>
    /// <returns>A disposable representing the binding.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDisposable CustomEvent(
        SharedScenarios.BindCommand.CustomEvent.MyViewModel vm,
        SharedScenarios.BindCommand.CustomEvent.MyView view) =>
        SharedScenarios.BindCommand.CustomEvent.Scenario.Execute(vm, view);

    /// <summary>Binds the Child.SaveCommand to the SaveButton's Click event via a deep command path.</summary>
    /// <param name="vm">The source view model.</param>
    /// <param name="view">The target view.</param>
    /// <returns>A disposable representing the binding.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDisposable DeepCommandPath(
        SharedScenarios.BindCommand.DeepCommandPath.MyViewModel vm,
        SharedScenarios.BindCommand.DeepCommandPath.MyView view) =>
        SharedScenarios.BindCommand.DeepCommandPath.Scenario.Execute(vm, view);

    /// <summary>Binds the Save command via Click+Enabled (no Command property, no parameter). Routes through EventEnabledBindingPlugin.</summary>
    /// <param name="vm">The source view model.</param>
    /// <param name="view">The target view.</param>
    /// <returns>A disposable representing the binding.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDisposable EventEnabled(
        SharedScenarios.BindCommand.EventEnabled.MyViewModel vm,
        SharedScenarios.BindCommand.EventEnabled.MyView view) =>
        SharedScenarios.BindCommand.EventEnabled.Scenario.Execute(vm, view);

    /// <summary>
    /// Binds the Save command via Click+Enabled with an expression parameter.
    /// Routes through EventEnabledBindingPlugin (expression parameter variant).
    /// </summary>
    /// <param name="vm">The source view model.</param>
    /// <param name="view">The target view.</param>
    /// <returns>A disposable representing the binding.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDisposable EventEnabledExprParam(
        SharedScenarios.BindCommand.EventEnabledExprParam.MyViewModel vm,
        SharedScenarios.BindCommand.EventEnabledExprParam.MyView view) =>
        SharedScenarios.BindCommand.EventEnabledExprParam.Scenario.Execute(vm, view);

    /// <summary>
    /// Binds the Save command via Click+Enabled with an observable parameter.
    /// Routes through EventEnabledBindingPlugin (observable parameter variant).
    /// </summary>
    /// <param name="vm">The source view model.</param>
    /// <param name="view">The target view.</param>
    /// <param name="parameter">An observable producing command parameters.</param>
    /// <returns>A disposable representing the binding.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDisposable EventEnabledObsParam(
        SharedScenarios.BindCommand.EventEnabledObsParam.MyViewModel vm,
        SharedScenarios.BindCommand.EventEnabledObsParam.MyView view,
        IObservable<string> parameter) =>
        SharedScenarios.BindCommand.EventEnabledObsParam.Scenario.Execute(vm, view, parameter);

    /// <summary>Binds the Save command via Command property (no event, no parameter). Routes through CommandPropertyBindingPlugin.</summary>
    /// <param name="vm">The source view model.</param>
    /// <param name="view">The target view.</param>
    /// <returns>A disposable representing the binding.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDisposable CommandProperty(
        SharedScenarios.BindCommand.CommandProperty.MyViewModel vm,
        SharedScenarios.BindCommand.CommandProperty.MyView view) =>
        SharedScenarios.BindCommand.CommandProperty.Scenario.Execute(vm, view);

    /// <summary>
    /// Binds the Save command via Command property with an expression parameter.
    /// Routes through CommandPropertyBindingPlugin (expression parameter variant).
    /// </summary>
    /// <param name="vm">The source view model.</param>
    /// <param name="view">The target view.</param>
    /// <returns>A disposable representing the binding.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDisposable CommandPropertyExprParam(
        SharedScenarios.BindCommand.CommandPropertyExprParam.MyViewModel vm,
        SharedScenarios.BindCommand.CommandPropertyExprParam.MyView view) =>
        SharedScenarios.BindCommand.CommandPropertyExprParam.Scenario.Execute(vm, view);

    /// <summary>
    /// Binds the Save command via Command property with an observable parameter.
    /// Routes through CommandPropertyBindingPlugin (observable parameter variant).
    /// </summary>
    /// <param name="vm">The source view model.</param>
    /// <param name="view">The target view.</param>
    /// <param name="parameter">An observable producing command parameters.</param>
    /// <returns>A disposable representing the binding.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDisposable CommandPropertyObsParam(
        SharedScenarios.BindCommand.CommandPropertyObsParam.MyViewModel vm,
        SharedScenarios.BindCommand.CommandPropertyObsParam.MyView view,
        IObservable<string> parameter) =>
        SharedScenarios.BindCommand.CommandPropertyObsParam.Scenario.Execute(vm, view, parameter);

    /// <summary>Binds the Save command to a control with no default event. Falls back to runtime command binding service.</summary>
    /// <param name="vm">The source view model.</param>
    /// <param name="view">The target view.</param>
    /// <returns>A disposable representing the binding.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDisposable NoEvent(
        SharedScenarios.BindCommand.NoEvent.MyViewModel vm,
        SharedScenarios.BindCommand.NoEvent.MyView view) =>
        SharedScenarios.BindCommand.NoEvent.Scenario.Execute(vm, view);
}
