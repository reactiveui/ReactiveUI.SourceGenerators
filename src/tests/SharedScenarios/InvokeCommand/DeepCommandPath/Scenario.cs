// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding;

namespace SharedScenarios.InvokeCommand.DeepCommandPath;

/// <summary>Exercises InvokeCommand reaching the command through a chain the path follows.</summary>
public static class Scenario
{
    /// <summary>Offers each value the stream produces to the command the child currently holds.</summary>
    /// <param name="values">The values driving the executions.</param>
    /// <param name="viewModel">The view model whose child holds the command.</param>
    /// <returns>A disposable that stops executing the command.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDisposable Execute(IObservable<int> values, MyViewModel viewModel) =>
        values.InvokeCommand(viewModel, x => x.Child.Save);
}
