// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

namespace ReactiveUI.Binding.GeneratedCode.TestModels.Scenarios;

/// <summary>
/// Scenario methods for WhenChanging that the source generator processes at compile time.
/// Each method exercises a specific WhenChanging overload for before-change observation.
/// </summary>
public static class WhenChangingScenarios
{
    /// <summary>Single property before-change observation on TestViewModel.Name.</summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>An observable of the Name property value before changes.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<string> SingleProperty_Name(TestViewModel vm) =>
        vm.WhenChanging(x => x.Name);

    /// <summary>Single property before-change observation on BigViewModel.Prop1.</summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>An observable of the Prop1 property value before changes.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<string> SingleProperty_Prop1(BigViewModel vm) =>
        vm.WhenChanging(x => x.Prop1);

    /// <summary>Two-property before-change observation returning a tuple.</summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>An observable of the property value tuple before changes.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<PropertyValues<string, int>> TwoProperties(BigViewModel vm) =>
        vm.WhenChanging(x => x.Prop1, x => x.Prop2);

    /// <summary>Three-property before-change observation returning a tuple.</summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>An observable of the property value tuple before changes.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<PropertyValues<string, int, double>> ThreeProperties(BigViewModel vm) =>
        vm.WhenChanging(x => x.Prop1, x => x.Prop2, x => x.Prop3);

    /// <summary>Four-property before-change observation returning a tuple.</summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>An observable of the property value tuple before changes.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<PropertyValues<string, int, double, bool>> FourProperties(
        BigViewModel vm) =>
        vm.WhenChanging(x => x.Prop1, x => x.Prop2, x => x.Prop3, x => x.Prop4);

    /// <summary>Deep property chain before-change observation on BigViewModel.Address.City.</summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>An observable of the nested City property value before changes.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<string> DeepChain_AddressCity(BigViewModel vm) =>
        vm.WhenChanging(x => x.Address.City);
}
