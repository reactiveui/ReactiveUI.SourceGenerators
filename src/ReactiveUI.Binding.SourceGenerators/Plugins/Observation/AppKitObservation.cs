// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

/// <summary>Observes AppKit control values through sender-scoped text-change notifications.</summary>
internal static class AppKitObservation
{
    /// <summary>The mechanism's identity.</summary>
    internal const string Kind = "AppKit";

    /// <summary>The native NSControl property observation score.</summary>
    private const int ControlAffinity = 20;

    /// <summary>Gets the plugin the registry selects this mechanism through.</summary>
    internal static NativeObservationPlugin Plugin { get; } =
        new(Kind, ControlAffinity, Inspect, AppleNotificationEmitter.AppendSubscription);

    /// <summary>Offers a candidate for an NSControl value whose type resolves the text-change notification.</summary>
    /// <param name="owner">The concrete type through which the property is accessed.</param>
    /// <param name="property">The property being observed.</param>
    /// <returns>The eligible candidate, or null.</returns>
    internal static PlatformObservationInfo? Inspect(INamedTypeSymbol owner, IPropertySymbol property)
    {
        if (!Reports(property.Name) || !PlatformSymbols.DerivesFrom(owner, "AppKit.NSControl"))
        {
            return null;
        }

        var notification = AppleNotificationEmitter.ResolveNotification(owner, "TextDidChangeNotification");
        return notification is null ? null : new(Kind, ControlAffinity, default, notification, null, null);
    }

    /// <summary>Identifies the NSControl values covered by its native text notification.</summary>
    /// <param name="name">The property name.</param>
    /// <returns>True for a supported native value.</returns>
    internal static bool Reports(string name) => name is
        "AlphaValue" or "DoubleValue" or "FloatValue" or "IntValue" or "NintValue" or "ObjectValue" or "StringValue" or "AttributedStringValue";
}
