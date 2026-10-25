// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding;

namespace SharedScenarios.InvokeCommand.CommandProperty;

/// <summary>Exercises InvokeCommand executing the command a view model property holds.</summary>
public static class Scenario
{
    /// <summary>Offers each value the stream produces to the view model's command.</summary>
    /// <param name="values">The values driving the executions.</param>
    /// <param name="viewModel">The view model holding the command.</param>
    /// <returns>A disposable that stops executing the command.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDisposable Execute(IObservable<string> values, MyViewModel viewModel) =>
        values.InvokeCommand(viewModel, x => x.Save);
}
