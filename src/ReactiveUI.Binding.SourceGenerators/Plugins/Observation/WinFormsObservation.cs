// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

/// <summary>Observes WinForms component properties through their concrete change events.</summary>
internal static class WinFormsObservation
{
    /// <summary>The mechanism's identity.</summary>
    internal const string Kind = "WinForms";

    /// <summary>Gets the plugin the registry selects this mechanism through.</summary>
    internal static NativeObservationPlugin Plugin { get; } =
        new(Kind, BindingAffinity.WinFormsEvent, Inspect, NativeEventSubscriptionEmitter.AppendSubscription);

    /// <summary>Offers a candidate for a component property that declares a <c>{Name}Changed</c> event.</summary>
    /// <param name="owner">The concrete type through which the property is accessed.</param>
    /// <param name="property">The property being observed.</param>
    /// <returns>The eligible candidate, or null.</returns>
    internal static PlatformObservationInfo? Inspect(INamedTypeSymbol owner, IPropertySymbol property)
    {
        if (!PlatformSymbols.DerivesFrom(owner, "System.ComponentModel.Component"))
        {
            return null;
        }

        var changeEvent = PlatformSymbols.FindEvent(owner, $"{property.Name}Changed");
        return changeEvent is null ? null : new(Kind, BindingAffinity.WinFormsEvent, new([changeEvent]), null, null, null);
    }
}
