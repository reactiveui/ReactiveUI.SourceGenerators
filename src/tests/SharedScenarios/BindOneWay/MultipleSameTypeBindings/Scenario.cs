// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using ReactiveUI.Binding;

namespace SharedScenarios.BindOneWay.MultipleSameTypeBindings;

/// <summary>Exercises BindOneWay with multiple bindings sharing the same type signature (string to string).</summary>
public static class Scenario
{
    /// <summary>Creates two one-way bindings with the same type signature to test else-if dispatch.</summary>
    /// <param name="vm">The source view model.</param>
    /// <param name="view">The target view.</param>
    /// <returns>A tuple of disposables representing the bindings.</returns>
    public static ScenarioResult Execute(MyViewModel vm, MyView view) =>
        new(vm.BindOneWay(view, x => x.FirstName, x => x.FirstNameText), vm.BindOneWay(view, x => x.LastName, x => x.LastNameText));
}
