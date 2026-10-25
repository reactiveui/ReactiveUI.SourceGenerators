// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding;

namespace SharedScenarios.WhenAnyObservable.DeepObservableMerge;

/// <summary>Exercises WhenAnyObservable with deep property chains in a merge pattern.</summary>
public static class Scenario
{
    /// <summary>Creates a WhenAnyObservable that merges two deep observable properties.</summary>
    /// <param name="vm">The parent view model to observe.</param>
    /// <returns>An observable that merges values from both deep observable properties.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<string> Execute(ParentViewModel vm) =>
        vm.WhenAnyObservable(x => x.Child.Command1, x => x.Child.Command2);
}
