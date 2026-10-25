// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.Helpers;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.CommandBinding;

/// <summary>Provides native command member checks shared by the platform mechanisms.</summary>
internal static class NativeCommandSymbols
{
    /// <summary>Reads a native event route with a writable enabled flag.</summary>
    /// <param name="control">The concrete control.</param>
    /// <param name="kind">The native route.</param>
    /// <param name="eventName">The required event.</param>
    /// <returns>The verified route, or null.</returns>
    internal static NativeCommandInfo? Event(INamedTypeSymbol control, NativeCommandKind kind, string eventName)
    {
        var changeEvent = PlatformSymbols.FindEvent(control, eventName);
        var enabled = HasWritableProperty(control, "Enabled", "bool");
        return changeEvent is null || !enabled
            ? null
            : new(kind, eventName, EventHelpers.FindEventArgsType(control, eventName), true, false);
    }

    /// <summary>Checks that a native property has a public setter and the required type.</summary>
    /// <param name="control">The concrete owner.</param>
    /// <param name="name">The property name.</param>
    /// <param name="typeName">The required CLR type.</param>
    /// <returns>True when the property can be assigned directly.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool HasWritableProperty(INamedTypeSymbol control, string name, string typeName) =>
        NativeCommandMembers.HasWritableProperty(control, name, typeName);
}
