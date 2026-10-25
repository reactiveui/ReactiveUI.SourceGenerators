// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding;

namespace SharedScenarios.BindCommand.CommandPropertyExprParam;

/// <summary>
/// Exercises BindCommand with a Command property control and expression parameter.
/// This should route through CommandPropertyBindingPlugin (expression parameter variant).
/// </summary>
public static class Scenario
{
    /// <summary>Binds the Save command with an expression parameter.</summary>
    /// <param name="vm">The source view model.</param>
    /// <param name="view">The target view.</param>
    /// <returns>A disposable representing the binding.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDisposable Execute(MyViewModel vm, MyView view) =>
        view.BindCommand(vm, x => x.Save, x => x.SaveButton, x => x.CurrentItem);
}
