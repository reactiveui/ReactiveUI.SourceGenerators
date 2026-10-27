// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding;

namespace SharedScenarios.WhenAnyObservable.TwoObservablesWithSelector;

/// <summary>Exercises WhenAnyObservable on two observable properties with different types and a selector (CombineLatest pattern).</summary>
public static class Scenario
{
    /// <summary>Creates a WhenAnyObservable that combines the two observable properties using a selector.</summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>An observable that combines values from both observable properties.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<string> Execute(MyViewModel vm) =>
        vm.WhenAnyObservable(x => x.Count, x => x.Message, (count, message) => $"{message}: {count}");
}
