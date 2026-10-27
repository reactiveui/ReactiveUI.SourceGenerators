// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding;

namespace SharedScenarios.WhenAny.DeepPropertyChain;

/// <summary>Exercises WhenAny with a deep property chain (x => x.Child.Name).</summary>
public static class Scenario
{
    /// <summary>Creates a WhenAny observable for Child.Name via IObservedChange.</summary>
    /// <param name="vm">The parent view model to observe.</param>
    /// <returns>An observable of name values from the child.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<string> Execute(ParentViewModel vm) =>
        vm.WhenAny(x => x.Child.Name, c => c.Value);
}
