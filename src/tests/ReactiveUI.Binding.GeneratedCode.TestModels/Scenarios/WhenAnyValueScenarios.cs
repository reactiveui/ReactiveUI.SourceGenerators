// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

namespace ReactiveUI.Binding.GeneratedCode.TestModels.Scenarios;

/// <summary>
/// Scenario methods for WhenAnyValue that the source generator processes at compile time.
/// Each method exercises a specific WhenAnyValue overload.
/// </summary>
public static class WhenAnyValueScenarios
{
    /// <summary>Single property observation using WhenAnyValue.</summary>
    /// <param name="fixture">The fixture to observe.</param>
    /// <returns>An observable of the Value1 property value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<string> SingleProperty(WhenAnyTestFixture fixture) =>
        fixture.WhenAnyValue(x => x.Value1);

    /// <summary>Two-property observation using WhenAnyValue returning a tuple.</summary>
    /// <param name="fixture">The fixture to observe.</param>
    /// <returns>An observable of the property value tuple.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<PropertyValues<string, string>> TwoProperties(WhenAnyTestFixture fixture) =>
        fixture.WhenAnyValue(x => x.Value1, x => x.Value2);

    /// <summary>Three-property observation using WhenAnyValue returning a tuple.</summary>
    /// <param name="fixture">The fixture to observe.</param>
    /// <returns>An observable of the property value tuple.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<PropertyValues<string, string, string>> ThreeProperties(
        WhenAnyTestFixture fixture) =>
        fixture.WhenAnyValue(x => x.Value1, x => x.Value2, x => x.Value3);

    /// <summary>Two-property observation with a selector using WhenAnyValue.</summary>
    /// <param name="fixture">The fixture to observe.</param>
    /// <returns>An observable of the combined string value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<string> WithSelector_TwoProperties(WhenAnyTestFixture fixture) =>
        fixture.WhenAnyValue(x => x.Value1, x => x.Value2, static (v1, v2) => $"{v1}_{v2}");

    /// <summary>Deep property chain observation using WhenAnyValue on BigViewModel.Address.City.</summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>An observable of the nested City property value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<string> DeepChain_AddressCity(BigViewModel vm) =>
        vm.WhenAnyValue(x => x.Address.City);

    /// <summary>
    /// Deep property chain observation on HostTestFixture.Child!.Name.
    /// Uses the null-forgiving operator to test generator support for nullable intermediates.
    /// </summary>
    /// <param name="host">The host fixture to observe.</param>
    /// <returns>An observable of the nested Name property value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<string> DeepChain_ChildName(HostTestFixture host) =>
        host.WhenAnyValue(x => x.Child!.Name);

    /// <summary>Three-link property chain observation on HostTestFixture.Child!.Child!.Name.</summary>
    /// <param name="host">The host fixture to observe.</param>
    /// <returns>An observable of the nested grandchild Name property value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<string> DeepChain_GrandchildName(HostTestFixture host) =>
        host.WhenAnyValue(x => x.Child!.Child!.Name);

    /// <summary>Deep property chain observation on HostTestFixture.Child!.Age.</summary>
    /// <param name="host">The host fixture to observe.</param>
    /// <returns>An observable of the nested Age property value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<int> DeepChain_ChildAge(HostTestFixture host) =>
        host.WhenAnyValue(x => x.Child!.Age);
}
