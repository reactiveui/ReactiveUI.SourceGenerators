// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Conversion;

/// <summary>Provides type predicates shared by the fixed conversion mechanisms.</summary>
internal static class ConversionSymbols
{
    /// <summary>Records the typed assignment available when a registered converter declines a value.</summary>
    /// <param name="conversion">The selected mechanism.</param>
    /// <param name="source">The input type.</param>
    /// <param name="target">The output type.</param>
    /// <param name="compilation">The consumer compilation.</param>
    /// <returns>The mechanism with its assignment contract.</returns>
    internal static ConversionInfo WithAssignment(ConversionInfo conversion, ITypeSymbol source, ITypeSymbol target, Compilation compilation)
    {
        var assignment = ((CSharpCompilation)compilation).ClassifyConversion(source, target);
        var nullableLift = !IsNullable(source) && IsNullable(target) && SymbolEqualityComparer.Default.Equals(source, Unwrap(target));
        var assignable = assignment.IsIdentity || nullableLift || (assignment.IsImplicit && (assignment.IsReference || assignment.IsBoxing));
        return conversion with
        {
            AssignmentFallback = assignable ? $"({target.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)})__value" : null,
            IsIdentity = assignment.IsIdentity,
        };
    }

    /// <summary>Unwraps nullable value types for mechanism selection.</summary>
    /// <param name="type">The declared value type.</param>
    /// <returns>The underlying value type.</returns>
    internal static ITypeSymbol Unwrap(ITypeSymbol type) =>
        type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable
            ? nullable.TypeArguments[0]
            : type;

    /// <summary>Determines whether a value has a nullable wrapper.</summary>
    /// <param name="type">The declared value type.</param>
    /// <returns>True for a nullable value type.</returns>
    internal static bool IsNullable(ITypeSymbol type) =>
        type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T };

    /// <summary>Recognizes the numeric types covered by the standard converters.</summary>
    /// <param name="type">The unwrapped value type.</param>
    /// <returns>True for a supported numeric type.</returns>
    internal static bool IsNumeric(ITypeSymbol type) => type.SpecialType is
        SpecialType.System_Byte or SpecialType.System_Int16 or SpecialType.System_Int32 or SpecialType.System_Int64
        or SpecialType.System_Single or SpecialType.System_Double or SpecialType.System_Decimal;

    /// <summary>Recognizes a named framework type without treating nullable annotations as another type.</summary>
    /// <param name="type">The value type.</param>
    /// <param name="name">The framework type name.</param>
    /// <returns>True for the named type in the System namespace.</returns>
    internal static bool IsSystemType(ITypeSymbol type, string name) =>
        type.Name == name && type.ContainingNamespace is { Name: "System", ContainingNamespace.IsGlobalNamespace: true };
}
