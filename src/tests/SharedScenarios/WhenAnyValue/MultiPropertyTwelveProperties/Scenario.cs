// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding;

namespace SharedScenarios.WhenAnyValue.MultiPropertyTwelveProperties;

/// <summary>Exercises WhenAnyValue with twelve properties (maximum standard overload).</summary>
public static class Scenario
{
    /// <summary>Creates a WhenAnyValue observable for twelve properties.</summary>
    /// <param name="fixture">The fixture to observe.</param>
    /// <returns>An observable of twelve-property tuples.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static
        IObservable<PropertyValues<string, string, string, string, string, string, string, string, string, string, string, string>> Execute(WhenAnyFixture fixture) =>
        fixture.WhenAnyValue(
            x => x.Value1,
            x => x.Value2,
            x => x.Value3,
            x => x.Value4,
            x => x.Value5,
            x => x.Value6,
            x => x.Value7,
            x => x.Value8,
            x => x.Value9,
            x => x.Value10,
            x => x.Value11,
            x => x.Value12);
}
