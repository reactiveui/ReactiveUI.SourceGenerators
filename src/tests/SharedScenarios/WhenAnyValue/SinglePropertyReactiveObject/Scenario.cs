// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding;

namespace SharedScenarios.WhenAnyValue.SinglePropertyReactiveObject;

/// <summary>Exercises WhenAnyValue on a ReactiveObject property.</summary>
public static class Scenario
{
    /// <summary>Creates a WhenAnyValue observable for the Name property.</summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>An observable of name values.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<string> Execute(MyViewModel vm) =>
        vm.WhenAnyValue(x => x.Name);
}
