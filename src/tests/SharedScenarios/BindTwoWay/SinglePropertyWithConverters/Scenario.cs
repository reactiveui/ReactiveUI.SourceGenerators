// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding;

namespace SharedScenarios.BindTwoWay.SinglePropertyWithConverters;

/// <summary>Exercises BindTwoWay with conversion functions between int and string.</summary>
public static class Scenario
{
    /// <summary>Creates a two-way binding between ViewModel.Count and View.CountText with int-string converters.</summary>
    /// <param name="vm">The source view model.</param>
    /// <param name="view">The target view.</param>
    /// <returns>A disposable representing the binding.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDisposable Execute(MyViewModel vm, MyView view) =>
        vm.BindTwoWay(
            view,
            x => x.Count,
            x => x.CountText,
            count => count.ToString(),
            int.Parse);
}
