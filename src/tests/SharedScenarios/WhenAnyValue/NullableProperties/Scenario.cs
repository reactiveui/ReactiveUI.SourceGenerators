// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using ReactiveUI.Binding;

namespace SharedScenarios.WhenAnyValue.NullableProperties;

/// <summary>Exercises WhenAnyValue with nullable property types.</summary>
public static class Scenario
{
    /// <summary>Creates WhenAnyValue observables for nullable properties.</summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>A tuple of observables for nullable name and age.</returns>
    public static ScenarioResult Execute(MyViewModel vm) =>
        new(vm.WhenAnyValue(x => x.NullableName!), vm.WhenAnyValue(x => x.NullableAge!));
}
