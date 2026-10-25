// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Conversion;

/// <summary>Builds typed formatting and parsing expressions with nullable-value semantics.</summary>
internal static class StringConversionExpressions
{
    /// <summary>Builds the common scalar-to-string and string-to-scalar directions.</summary>
    /// <param name="source">The declared input.</param>
    /// <param name="target">The declared output.</param>
    /// <param name="sourceSupported">Whether this mechanism supports the input scalar.</param>
    /// <param name="targetSupported">Whether this mechanism supports the output scalar.</param>
    /// <param name="formatArguments">The arguments to the scalar's ToString method.</param>
    /// <returns>The direct conversion, or null.</returns>
    internal static ConversionInfo? SelectScalar(ITypeSymbol source, ITypeSymbol target, bool sourceSupported, bool targetSupported, string formatArguments)
    {
        if (target.SpecialType == SpecialType.System_String && sourceSupported)
        {
            var value = ConversionSymbols.IsNullable(source) ? "__value.Value" : "__value";
            return Format($"{value}.ToString({formatArguments})", ConversionSymbols.IsNullable(source));
        }

        return source.SpecialType == SpecialType.System_String && targetSupported ? Parse(target) : null;
    }

    /// <summary>Preserves a successful null result when formatting a nullable value.</summary>
    /// <param name="expression">The non-null formatting operation.</param>
    /// <param name="nullable">Whether the input is nullable.</param>
    /// <returns>The formatting candidate.</returns>
    internal static ConversionInfo Format(string expression, bool nullable) =>
        new(nullable ? $"__value.HasValue ? ({expression}) : null" : expression, "true", BindingAffinity.DefaultInternalTypeConverter);

    /// <summary>Parses directly into the concrete value type, treating an empty nullable input as a successful null.</summary>
    /// <param name="target">The declared output type.</param>
    /// <returns>The parsing candidate.</returns>
    internal static ConversionInfo Parse(ITypeSymbol target)
    {
        var underlying = ConversionSymbols.Unwrap(target).ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var targetName = target.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var nullable = ConversionSymbols.IsNullable(target);
        var parse = $"{underlying}.TryParse(__value, out __parsed)";
        var condition = nullable ? $"global::System.String.IsNullOrEmpty(__value) || {parse}" : parse;
        var expression = nullable ? $"global::System.String.IsNullOrEmpty(__value) ? default({targetName}) : ({targetName})__parsed" : "__parsed";
        return new(expression, condition, BindingAffinity.DefaultInternalTypeConverter) { Preparation = $"{underlying} __parsed = default({underlying});", };
    }
}
