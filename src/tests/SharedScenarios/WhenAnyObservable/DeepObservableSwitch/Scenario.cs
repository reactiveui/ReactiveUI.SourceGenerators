// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding;

namespace SharedScenarios.WhenAnyObservable.DeepObservableSwitch;

/// <summary>Exercises WhenAnyObservable with a deep property chain (x => x.Child.MyCommand).</summary>
public static class Scenario
{
    /// <summary>Creates a WhenAnyObservable that switches to the latest value of Child.MyCommand.</summary>
    /// <param name="vm">The parent view model to observe.</param>
    /// <returns>An observable that switches to the latest Child.MyCommand observable.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<string> Execute(ParentViewModel vm) =>
        vm.WhenAnyObservable(x => x.Child.MyCommand);
}
