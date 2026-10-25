// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ReactiveUI.Binding;

namespace SharedScenarios.BindInteraction.DeepPropertyPath;

/// <summary>Exercises BindInteraction with a deep property path (Child.Confirm).</summary>
public static class Scenario
{
    /// <summary>Binds a task handler to the ViewModel's Child.Confirm interaction.</summary>
    /// <param name="vm">The source view model.</param>
    /// <param name="view">The target view.</param>
    /// <returns>A disposable representing the binding.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDisposable Execute(MyViewModel vm, MyView view) =>
        view.BindInteraction(vm, x => x.Child!.Confirm, ctx =>
        {
            ctx.SetOutput(true);
            return Task.CompletedTask;
        });
}
