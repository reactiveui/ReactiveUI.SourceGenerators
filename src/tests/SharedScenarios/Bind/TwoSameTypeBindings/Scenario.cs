// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding;

namespace SharedScenarios.Bind.TwoSameTypeBindings;

/// <summary>
/// Exercises Bind (view-first two-way) with two bindings of the same type signature (string-to-string).
/// This ensures the generator emits "else if" for the second invocation in the dispatch method.
/// </summary>
public static class Scenario
{
    /// <summary>
    /// Creates two string-to-string two-way bindings using view-first syntax.
    /// Both bindings have the same type signature, causing them to be grouped together.
    /// </summary>
    /// <param name="view">The target view.</param>
    /// <param name="vm">The source view model.</param>
    /// <returns>A tuple of bindings.</returns>
    public static ScenarioResult Execute(MyView view, MyViewModel vm) =>
        new(view.Bind(vm, x => x.FirstName, x => x.FirstNameText), view.Bind(vm, x => x.LastName, x => x.LastNameText));
}
