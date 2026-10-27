// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ReactiveUI.Binding;

namespace SharedScenarios.OneWayBind.SinglePropertyIntToInt;

/// <summary>Exercises OneWayBind (view-first) with int-to-int property binding.</summary>
public static class Scenario
{
    /// <summary>Creates a one-way binding from ViewModel.Count to View.CountValue.</summary>
    /// <param name="view">The target view.</param>
    /// <param name="vm">The source view model.</param>
    /// <returns>A reactive binding representing the binding.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IReactiveBinding<MyView, int> Execute(MyView view, MyViewModel vm) =>
        view.OneWayBind(vm, x => x.Count, x => x.CountValue);
}
