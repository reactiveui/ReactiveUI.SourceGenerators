// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding;

namespace SharedScenarios.WhenChanged.MultiPropertyWithSelector;

/// <summary>Exercises WhenChanged with a selector function combining two properties.</summary>
public static class Scenario
{
    /// <summary>Creates a WhenChanged observable combining FirstName and LastName.</summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>An observable of combined name strings.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<string> Execute(MyViewModel vm) =>
        vm.WhenChanged(x => x.FirstName, x => x.LastName, (f, l) => $"{f} {l}");
}
