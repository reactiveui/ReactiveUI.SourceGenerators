// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding;

namespace SharedScenarios.BindOneWay.SinglePropertyWithConverter;

/// <summary>Exercises BindOneWay with a conversion function from int to string.</summary>
public static class Scenario
{
    /// <summary>Creates a one-way binding from ViewModel.Count to View.CountText with int-to-string conversion.</summary>
    /// <param name="vm">The source view model.</param>
    /// <param name="view">The target view.</param>
    /// <returns>A disposable representing the binding.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDisposable Execute(MyViewModel vm, MyView view) =>
        vm.BindOneWay(view, x => x.Count, x => x.CountText, count => count.ToString());
}
