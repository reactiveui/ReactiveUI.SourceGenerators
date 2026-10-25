// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins;

/// <summary>Owns symbol eligibility and direct emission for one platform notification mechanism.</summary>
internal interface IPlatformObservationPlugin : IObservationPlugin
{
    /// <summary>Offers a candidate only when the consumer exposes the required native members.</summary>
    /// <param name="owner">The concrete type through which the property is accessed.</param>
    /// <param name="property">The property being observed.</param>
    /// <returns>Value-equatable emission data, or null when this plugin is ineligible.</returns>
    PlatformObservationInfo? InspectProperty(INamedTypeSymbol owner, IPropertySymbol property);
}
