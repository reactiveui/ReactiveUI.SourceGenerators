// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using ReactiveUI.Binding;

namespace SharedScenarios.WhenAnyObservable.MultipleInvocationsSameType;

/// <summary>Exercises multiple WhenAnyObservable invocations with the same type signature to test else-if dispatch.</summary>
public static class Scenario
{
    /// <summary>Creates two WhenAnyObservable calls on the same VM with the same type signature.</summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>A tuple of observables for each command.</returns>
    public static ScenarioResult Execute(MyViewModel vm) =>
        new(vm.WhenAnyObservable(x => x.Command1), vm.WhenAnyObservable(x => x.Command2));
}
