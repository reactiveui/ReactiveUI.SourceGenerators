// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding;

namespace SharedScenarios.WhenChanged.MultiPropertyThreeProperties;

/// <summary>Exercises WhenChanged with three properties returning a tuple.</summary>
public static class Scenario
{
    /// <summary>Creates a WhenChanged observable for Name, Age, and Score.</summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>An observable of (name, age, score) tuples.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<PropertyValues<string, int, double>> Execute(MyViewModel vm) =>
        vm.WhenChanged(x => x.Name, x => x.Age, x => x.Score);
}
