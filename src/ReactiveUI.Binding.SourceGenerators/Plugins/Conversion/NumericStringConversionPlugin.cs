// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Conversion;

/// <summary>Emits the standard numeric formatting and parsing operations.</summary>
internal sealed class NumericStringConversionPlugin : IConversionPlugin
{
    /// <inheritdoc/>
    public ConversionInfo? Select(ITypeSymbol source, ITypeSymbol target, Compilation compilation)
    {
        var input = ConversionSymbols.Unwrap(source);
        if (target.SpecialType == SpecialType.System_String && ConversionSymbols.IsNumeric(input))
        {
            return StringConversionExpressions.Format(FormattingExpression(input, ConversionSymbols.IsNullable(source)), ConversionSymbols.IsNullable(source));
        }

        return source.SpecialType == SpecialType.System_String && ConversionSymbols.IsNumeric(ConversionSymbols.Unwrap(target))
            ? StringConversionExpressions.Parse(target)
            : null;
    }

    /// <summary>Preserves width, precision and custom format using the current culture.</summary>
    /// <param name="type">The unwrapped numeric type.</param>
    /// <param name="nullable">Whether the input has a nullable wrapper.</param>
    /// <returns>The concrete ToString operation.</returns>
    internal static string FormattingExpression(ITypeSymbol type, bool nullable)
    {
        var value = nullable ? "__value.Value" : "__value";
        var prefix = type.SpecialType is SpecialType.System_Single or SpecialType.System_Double or SpecialType.System_Decimal ? "F" : "D";
        return $"__hint is int __precision ? {value}.ToString(\"{prefix}\" + __precision.ToString())"
            + $" : __hint is string __format ? {value}.ToString(__format) : {value}.ToString()";
    }
}
