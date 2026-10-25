// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

namespace ReactiveUI.Binding.GeneratedCode.TestModels.Scenarios;

/// <summary>
/// Scenario methods for BindTwoWay that the source generator processes at compile time.
/// Each method exercises a specific BindTwoWay overload with different property types.
/// </summary>
public static class BindTwoWayScenarios
{
    /// <summary>Two-way binding for a string property.</summary>
    /// <param name="source">The source view model.</param>
    /// <param name="target">The target view.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDisposable StringProperty(BigViewModel source, BigView target) =>
        source.BindTwoWay(target, x => x.Prop1, x => x.ViewProp1);

    /// <summary>Two-way binding for an int property.</summary>
    /// <param name="source">The source view model.</param>
    /// <param name="target">The target view.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDisposable IntProperty(BigViewModel source, BigView target) =>
        source.BindTwoWay(target, x => x.Prop2, x => x.ViewProp2);

    /// <summary>Two-way binding for a double property.</summary>
    /// <param name="source">The source view model.</param>
    /// <param name="target">The target view.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDisposable DoubleProperty(BigViewModel source, BigView target) =>
        source.BindTwoWay(target, x => x.Prop3, x => x.ViewProp3);

    /// <summary>Two-way binding for a bool property.</summary>
    /// <param name="source">The source view model.</param>
    /// <param name="target">The target view.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDisposable BoolProperty(BigViewModel source, BigView target) =>
        source.BindTwoWay(target, x => x.Prop4, x => x.ViewProp4);
}
