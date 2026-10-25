// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using ReactiveUI.Binding;

namespace SharedScenarios.BindTwoWay.MultipleBindings;

/// <summary>Exercises BindTwoWay with multiple bindings on the same source/target pair.</summary>
public static class Scenario
{
    /// <summary>Creates two-way bindings for Name and Age properties.</summary>
    /// <param name="vm">The source view model.</param>
    /// <param name="view">The target view.</param>
    /// <returns>A tuple of disposables representing the bindings.</returns>
    public static ScenarioResult Execute(MyViewModel vm, MyView view) =>
        new(vm.BindTwoWay(view, x => x.Name, x => x.NameText), vm.BindTwoWay(view, x => x.Age, x => x.AgeDisplay));
}
