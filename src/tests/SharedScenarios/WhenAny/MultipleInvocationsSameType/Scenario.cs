// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using ReactiveUI.Binding;

namespace SharedScenarios.WhenAny.MultipleInvocationsSameType;

/// <summary>Exercises multiple WhenAny invocations with the same type signature to test else-if dispatch.</summary>
public static class Scenario
{
    /// <summary>Creates two WhenAny observables with the same type signature (MyViewModel, string, string).</summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>A tuple of observables for first name and last name.</returns>
    public static ScenarioResult Execute(MyViewModel vm) =>
        new(vm.WhenAny(x => x.FirstName, c => c.Value), vm.WhenAny(x => x.LastName, c => c.Value));
}
