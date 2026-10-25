// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding;

namespace SharedScenarios.ToProperty.ReactiveObject;

/// <summary>Exercises ToProperty on a ReactiveUI ReactiveObject.</summary>
public static class Scenario
{
    /// <summary>Creates the view model, which backs its derived property with the observable.</summary>
    /// <param name="names">The values the derived property takes.</param>
    /// <returns>The view model.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MyViewModel Execute(IObservable<string> names) => new(names);
}
