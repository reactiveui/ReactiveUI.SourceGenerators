// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding;

namespace SharedScenarios.WhenChanged.NullForgivingDeepChain;

/// <summary>
/// Exercises WhenChanged with the null-forgiving operator (x => x.Child!.Name).
/// This is a common pattern when the developer knows Child is set before observation starts.
/// </summary>
public static class Scenario
{
    /// <summary>Creates a WhenChanged observable for Child!.Name using the null-forgiving operator.</summary>
    /// <param name="vm">The parent view model to observe.</param>
    /// <returns>An observable of name values from the child.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<string> Execute(ParentViewModel vm) =>
        vm.WhenChanged(x => x.Child!.Name);
}
