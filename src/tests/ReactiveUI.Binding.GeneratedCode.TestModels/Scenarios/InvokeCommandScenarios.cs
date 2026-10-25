// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.GeneratedCode.TestModels.Scenarios;

/// <summary>Scenario methods for InvokeCommand that the source generator processes at compile time.</summary>
public static class InvokeCommandScenarios
{
    /// <summary>Offers each value the stream produces to the command a view model property holds.</summary>
    /// <param name="values">The values driving the executions.</param>
    /// <param name="viewModel">The view model holding the command.</param>
    /// <returns>A disposable that stops executing the command.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDisposable CommandProperty(
        IObservable<string> values,
        SharedScenarios.InvokeCommand.CommandProperty.MyViewModel viewModel) =>
        SharedScenarios.InvokeCommand.CommandProperty.Scenario.Execute(values, viewModel);

    /// <summary>Offers each value the stream produces to the command reached through a chain.</summary>
    /// <param name="values">The values driving the executions.</param>
    /// <param name="viewModel">The view model whose child holds the command.</param>
    /// <returns>A disposable that stops executing the command.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDisposable DeepCommandPath(
        IObservable<int> values,
        SharedScenarios.InvokeCommand.DeepCommandPath.MyViewModel viewModel) =>
        SharedScenarios.InvokeCommand.DeepCommandPath.Scenario.Execute(values, viewModel);
}
