// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding;

namespace SharedScenarios.WhenAnyObservable.TwoObservablesMerge;

/// <summary>Exercises WhenAnyObservable on two observable properties of the same type (merge pattern).</summary>
public static class Scenario
{
    /// <summary>Creates a WhenAnyObservable that merges the two command observables.</summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>An observable that merges values from both command observables.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<string> Execute(MyViewModel vm) =>
        vm.WhenAnyObservable(x => x.Command1, x => x.Command2);
}
