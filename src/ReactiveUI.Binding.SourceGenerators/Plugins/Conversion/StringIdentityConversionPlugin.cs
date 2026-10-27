// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Conversion;

/// <summary>Offers the standard string converter's identity operation.</summary>
internal sealed class StringIdentityConversionPlugin : IConversionPlugin
{
    /// <inheritdoc/>
    public ConversionInfo? Select(ITypeSymbol source, ITypeSymbol target, Compilation compilation) =>
        source.SpecialType == SpecialType.System_String && target.SpecialType == SpecialType.System_String
            ? new("__value", "__value != null", BindingAffinity.DefaultInternalTypeConverter)
            : null;
}
