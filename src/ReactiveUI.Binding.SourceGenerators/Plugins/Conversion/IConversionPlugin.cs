// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Conversion;

/// <summary>Adapts a native conversion into typed generated code.</summary>
internal interface IConversionPlugin
{
    /// <summary>Offers a conversion only when the consumer's symbols support its emitted expression.</summary>
    /// <param name="source">The declared input type.</param>
    /// <param name="target">The declared output type.</param>
    /// <param name="compilation">The consumer compilation.</param>
    /// <returns>The candidate's typed expression and affinity, or null when ineligible.</returns>
    ConversionInfo? Select(ITypeSymbol source, ITypeSymbol target, Compilation compilation);
}
