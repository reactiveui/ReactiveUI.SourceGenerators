// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Conversion;

/// <summary>Compares an object-valued source with the caller's comparison hint.</summary>
internal sealed class EqualityConversionPlugin : IConversionPlugin
{
    /// <inheritdoc/>
    public ConversionInfo? Select(ITypeSymbol source, ITypeSymbol target, Compilation compilation) =>
        source.SpecialType == SpecialType.System_Object && target.SpecialType == SpecialType.System_Boolean
            ? new("global::System.Object.Equals(__value, __hint)", "true", BindingAffinity.Fallback)
            : null;
}
