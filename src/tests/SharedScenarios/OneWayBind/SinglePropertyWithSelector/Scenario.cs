// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ReactiveUI.Binding;

namespace SharedScenarios.OneWayBind.SinglePropertyWithSelector;

/// <summary>Exercises OneWayBind (view-first) with a conversion function from int to string.</summary>
public static class Scenario
{
    /// <summary>Creates a one-way binding from ViewModel.Count to View.CountText with int-to-string conversion.</summary>
    /// <param name="view">The target view.</param>
    /// <param name="vm">The source view model.</param>
    /// <returns>A reactive binding representing the binding.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IReactiveBinding<MyView, string> Execute(MyView view, MyViewModel vm) =>
        view.OneWayBind(vm, x => x.Count, x => x.CountText, count => count.ToString());
}
