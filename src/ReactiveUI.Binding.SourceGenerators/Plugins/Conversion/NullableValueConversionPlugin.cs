// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Conversion;

/// <summary>Emits numeric nullable wrapping and unwrapping without boxing.</summary>
internal sealed class NullableValueConversionPlugin : IConversionPlugin
{
    /// <inheritdoc/>
    public ConversionInfo? Select(ITypeSymbol source, ITypeSymbol target, Compilation compilation)
    {
        var underlying = ConversionSymbols.Unwrap(source);
        var sourceNullable = ConversionSymbols.IsNullable(source);
        if (!ConversionSymbols.IsNumeric(underlying)
            || sourceNullable == ConversionSymbols.IsNullable(target)
            || !SymbolEqualityComparer.Default.Equals(underlying, ConversionSymbols.Unwrap(target)))
        {
            return null;
        }

        return new(
            sourceNullable ? "__value.Value" : $"({target.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)})__value",
            sourceNullable ? "__value.HasValue" : "true",
            BindingAffinity.DefaultInternalTypeConverter);
    }
}
