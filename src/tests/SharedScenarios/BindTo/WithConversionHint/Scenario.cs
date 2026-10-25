// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding;

namespace SharedScenarios.BindTo.WithConversionHint;

/// <summary>Exercises BindTo with a conversion hint forwarded to the resolved converter.</summary>
public static class Scenario
{
    /// <summary>Binds the int observable stream to the view's string caption using a conversion hint.</summary>
    /// <param name="source">The source observable stream.</param>
    /// <param name="view">The target view.</param>
    /// <returns>A disposable representing the binding.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDisposable Execute(IObservable<int> source, MyView view) =>
        source.BindTo(view, x => x.Caption, (object)"D2");
}
