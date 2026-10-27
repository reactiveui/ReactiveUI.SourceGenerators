// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding;

namespace SharedScenarios.WhenChanged.IntProperty;

/// <summary>Exercises WhenChanged with an integer property type.</summary>
public static class Scenario
{
    /// <summary>Creates a WhenChanged observable for the Count property.</summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>An observable of count values.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<int> Execute(MyViewModel vm) =>
        vm.WhenChanged(x => x.Count);
}
