// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins;

/// <summary>Inspects native command contracts while the consumer's symbols are available.</summary>
internal interface IPlatformCommandBindingPlugin : ICommandBindingPlugin
{
    /// <summary>Offers the native route only when its required members are available.</summary>
    /// <param name="control">The concrete control type.</param>
    /// <returns>The native member data, or null when ineligible.</returns>
    NativeCommandInfo? InspectControl(INamedTypeSymbol control);
}
