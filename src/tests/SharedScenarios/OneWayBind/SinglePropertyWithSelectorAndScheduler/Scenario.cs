// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ReactiveUI.Binding;
using ReactiveUI.Primitives.Concurrency;

namespace SharedScenarios.OneWayBind.SinglePropertyWithSelectorAndScheduler;

/// <summary>Exercises OneWayBind (view-first) with a conversion function and scheduler.</summary>
public static class Scenario
{
    /// <summary>Creates a one-way binding from ViewModel.Count to View.CountText with int-to-string conversion and scheduler.</summary>
    /// <param name="view">The target view.</param>
    /// <param name="vm">The source view model.</param>
    /// <param name="scheduler">The scheduler to observe on.</param>
    /// <returns>A reactive binding representing the binding.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IReactiveBinding<MyView, string> Execute(MyView view, MyViewModel vm, ISequencer scheduler) =>
        view.OneWayBind(vm, x => x.Count, x => x.CountText, count => count.ToString(), scheduler);
}
