// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

/// <summary>Observes the value exposed by UIKit controls through their native value-change event.</summary>
internal static class UIKitValueObservation
{
    /// <summary>The mechanism's identity.</summary>
    internal const string Kind = "UIKitValue";

    /// <summary>The native UIControl value observation score.</summary>
    private const int ValueAffinity = 20;

    /// <summary>Gets the plugin the registry selects this mechanism through.</summary>
    internal static NativeObservationPlugin Plugin { get; } =
        new(Kind, ValueAffinity, Inspect, NativeEventSubscriptionEmitter.AppendSubscription);

    /// <summary>Offers a candidate for a UIControl's <c>Value</c> when the control declares <c>ValueChanged</c>.</summary>
    /// <param name="owner">The concrete type through which the property is accessed.</param>
    /// <param name="property">The property being observed.</param>
    /// <returns>The eligible candidate, or null.</returns>
    internal static PlatformObservationInfo? Inspect(INamedTypeSymbol owner, IPropertySymbol property)
    {
        if (property.Name != "Value" || !PlatformSymbols.DerivesFrom(owner, "UIKit.UIControl"))
        {
            return null;
        }

        var changeEvent = PlatformSymbols.FindEvent(owner, "ValueChanged");
        return changeEvent is null ? null : new(Kind, ValueAffinity, new([changeEvent]), null, null, null);
    }
}
