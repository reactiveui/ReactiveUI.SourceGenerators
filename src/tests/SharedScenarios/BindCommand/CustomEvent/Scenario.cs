// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding;

namespace SharedScenarios.BindCommand.CustomEvent;

/// <summary>Exercises BindCommand with an explicit toEvent parameter.</summary>
public static class Scenario
{
    /// <summary>Binds the Save command to the SaveButton's MouseUp event via explicit toEvent.</summary>
    /// <param name="vm">The source view model.</param>
    /// <param name="view">The target view.</param>
    /// <returns>A disposable representing the binding.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDisposable Execute(MyViewModel vm, MyView view) =>
        view.BindCommand(vm, x => x.Save, x => x.SaveButton, "MouseUp");
}
