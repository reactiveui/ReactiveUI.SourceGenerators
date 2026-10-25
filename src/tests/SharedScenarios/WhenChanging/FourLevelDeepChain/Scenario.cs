// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding;

namespace SharedScenarios.WhenChanging.FourLevelDeepChain;

/// <summary>Exercises WhenChanging with a 4-level deep property chain.</summary>
public static class Scenario
{
    /// <summary>Creates a WhenChanging observable for Model.Model.Model.Value.</summary>
    /// <param name="chain">The top-level object in the chain.</param>
    /// <returns>An observable of value strings from the leaf (before change).</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<string> Execute(Level1 chain) =>
        chain.WhenChanging(x => x.Model.Model.Model.Value);
}
