// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding;

namespace SharedScenarios.Bind.MultipleBindings;

/// <summary>Exercises Bind (view-first two-way) with multiple bindings on the same view/vm pair.</summary>
public static class Scenario
{
    /// <summary>Creates multiple two-way bindings using view-first syntax.</summary>
    /// <param name="view">The target view.</param>
    /// <param name="vm">The source view model.</param>
    /// <returns>A tuple of bindings.</returns>
    public static ScenarioResult Execute(MyView view, MyViewModel vm) =>
        new(view.Bind(vm, x => x.Name, x => x.NameText), view.Bind(vm, x => x.Age, x => x.AgeText));
}
