// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding;

namespace SharedScenarios.BindTwoWay.SinglePropertyStringToString;

/// <summary>Exercises BindTwoWay with string-to-string property binding.</summary>
public static class Scenario
{
    /// <summary>Creates a two-way binding between ViewModel.Name and View.NameText.</summary>
    /// <param name="vm">The source view model.</param>
    /// <param name="view">The target view.</param>
    /// <returns>A disposable representing the binding.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDisposable Execute(MyViewModel vm, MyView view) =>
        vm.BindTwoWay(view, x => x.Name, x => x.NameText);
}
