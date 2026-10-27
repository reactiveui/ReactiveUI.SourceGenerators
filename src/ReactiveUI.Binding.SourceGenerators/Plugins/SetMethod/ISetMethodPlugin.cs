// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.SetMethod;

/// <summary>Offers a typed native setter for a source and destination pair.</summary>
internal interface ISetMethodPlugin
{
    /// <summary>Inspects the consumer's collection and value contracts.</summary>
    /// <param name="source">The incoming value type.</param>
    /// <param name="target">The property's collection type.</param>
    /// <returns>The scored mechanism, or null when ineligible.</returns>
    SetMethodInfo? Select(ITypeSymbol source, ITypeSymbol target);
}
