// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ReactiveUI.Binding;

namespace SharedScenarios.Bind.SinglePropertyStringToString;

/// <summary>Exercises Bind (view-first two-way) with string-to-string property binding.</summary>
public static class Scenario
{
    /// <summary>Creates a two-way binding between ViewModel.Name and View.NameText using view-first syntax.</summary>
    /// <param name="view">The target view.</param>
    /// <param name="vm">The source view model.</param>
    /// <returns>A reactive binding representing the binding.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IReactiveBinding<MyView, BindingChange> Execute(MyView view, MyViewModel vm) =>
        view.Bind(vm, x => x.Name, x => x.NameText);
}
