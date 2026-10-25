// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Helpers;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Conversion;

/// <summary>Emits native Foundation date operators with their null-input contract.</summary>
internal sealed class AppleConversionPlugin : IConversionPlugin
{
    /// <summary>The Foundation date converter's score.</summary>
    private const int Affinity = 8;

    /// <inheritdoc/>
    public ConversionInfo? Select(ITypeSymbol source, ITypeSymbol target, Compilation compilation)
    {
        var from = UnwrapNullable(source);
        var to = UnwrapNullable(target);
        var fromNative = NativeTypeIdentity.Matches(from, "Foundation.NSDate");
        var toNative = NativeTypeIdentity.Matches(to, "Foundation.NSDate");
        if (fromNative == toNative)
        {
            return null;
        }

        return fromNative ? FromNative(from, target, compilation) : ToNative(source, to, compilation);
    }

    /// <summary>Emits the managed-date to native-date direction, including nullable rejection.</summary>
    /// <param name="source">The managed input type.</param>
    /// <param name="target">The native output type.</param>
    /// <param name="compilation">The consumer compilation.</param>
    /// <returns>The direct conversion, or null when the native operator is unavailable.</returns>
    internal static ConversionInfo? ToNative(ITypeSymbol source, ITypeSymbol target, Compilation compilation)
    {
        var managed = UnwrapNullable(source);
        if (!IsManagedDate(managed) || !HasOperator(compilation.GetSpecialType(SpecialType.System_DateTime), target, compilation))
        {
            return null;
        }

        var nullable = !SymbolEqualityComparer.Default.Equals(managed, source);
        var value = nullable ? "__value.Value" : "__value";
        var member = managed.Name == "DateTimeOffset" ? ".DateTime" : string.Empty;
        return new($"(global::Foundation.NSDate){value}{member}", nullable ? "__value.HasValue" : "true", Affinity);
    }

    /// <summary>Emits the native-date to managed-date direction with native-null rejection.</summary>
    /// <param name="source">The native input type.</param>
    /// <param name="target">The managed output type.</param>
    /// <param name="compilation">The consumer compilation.</param>
    /// <returns>The direct conversion, or null when the native operator is unavailable.</returns>
    internal static ConversionInfo? FromNative(ITypeSymbol source, ITypeSymbol target, Compilation compilation)
    {
        var managed = UnwrapNullable(target);
        if (!IsManagedDate(managed) || !HasOperator(source, compilation.GetSpecialType(SpecialType.System_DateTime), compilation))
        {
            return null;
        }

        var expression = managed.Name == "DateTimeOffset"
            ? "new global::System.DateTimeOffset((global::System.DateTime)__value)"
            : "(global::System.DateTime)__value";
        return new($"({target.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)})({expression})", "__value != null", Affinity);
    }

    /// <summary>Unwraps nullable value types while retaining the native reference type.</summary>
    /// <param name="type">The source or destination type.</param>
    /// <returns>The value type used to select the native operator.</returns>
    internal static ITypeSymbol UnwrapNullable(ITypeSymbol type) =>
        type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable
            ? nullable.TypeArguments[0]
            : type;

    /// <summary>Identifies the managed date types covered by the Foundation converters.</summary>
    /// <param name="type">The unwrapped value type.</param>
    /// <returns>True for a supported managed date.</returns>
    private static bool IsManagedDate(ITypeSymbol type) =>
        type.SpecialType == SpecialType.System_DateTime || NativeTypeIdentity.Matches(type, "System.DateTimeOffset");

    /// <summary>Verifies the concrete native cast against the consumer's symbols.</summary>
    /// <param name="source">The operator input.</param>
    /// <param name="target">The operator output.</param>
    /// <param name="compilation">The consumer compilation.</param>
    /// <returns>True when the native operator exists.</returns>
    /// <remarks>A user-defined conversion always exists; a missing or ambiguous one classifies as no conversion.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool HasOperator(ITypeSymbol source, ITypeSymbol target, Compilation compilation) =>
        ((CSharpCompilation)compilation).ClassifyConversion(source, target).IsUserDefined;
}
