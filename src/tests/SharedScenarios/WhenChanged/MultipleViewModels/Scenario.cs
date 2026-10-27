// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using ReactiveUI.Binding;

namespace SharedScenarios.WhenChanged.MultipleViewModels;

/// <summary>Exercises WhenChanged with multiple ViewModels in the same compilation.</summary>
public static class Scenario
{
    /// <summary>Creates WhenChanged observables for two different ViewModels.</summary>
    /// <param name="vm1">The first view model.</param>
    /// <param name="vm2">The second view model.</param>
    /// <returns>A tuple of observables for name and count.</returns>
    public static ScenarioResult Execute(ViewModel1 vm1, ViewModel2 vm2) =>
        new(vm1.WhenChanged(x => x.Name), vm2.WhenChanged(x => x.Count));
}
