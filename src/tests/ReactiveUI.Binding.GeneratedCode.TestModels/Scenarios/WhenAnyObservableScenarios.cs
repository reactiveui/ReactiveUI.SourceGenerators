// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

namespace ReactiveUI.Binding.GeneratedCode.TestModels.Scenarios;

/// <summary>
/// Scenario methods for WhenAnyObservable (Switch/Merge) that the source generator processes at compile time.
/// Each method exercises a specific WhenAnyObservable overload.
/// </summary>
public static class WhenAnyObservableScenarios
{
    /// <summary>Single observable property observation using Switch pattern.</summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>An observable that switches to the latest MyCommand observable.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<string> SingleObservable_Switch(ObservablePropertyViewModel vm) =>
        vm.WhenAnyObservable(x => x.MyCommand);

    /// <summary>Two observable properties of the same type using Merge pattern.</summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>An observable that merges both command observables.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<string> TwoObservables_Merge(ObservablePropertyViewModel vm) =>
        vm.WhenAnyObservable(x => x.MyCommand, x => x.OtherCommand);
}
