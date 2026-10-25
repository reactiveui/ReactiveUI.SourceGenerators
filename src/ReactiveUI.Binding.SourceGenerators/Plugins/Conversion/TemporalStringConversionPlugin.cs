// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Conversion;

/// <summary>Emits formatting and parsing for the standard date and time values.</summary>
internal sealed class TemporalStringConversionPlugin : IConversionPlugin
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ConversionInfo? Select(ITypeSymbol source, ITypeSymbol target, Compilation compilation) =>
        StringConversionExpressions.SelectScalar(source, target, Supports(ConversionSymbols.Unwrap(source)), Supports(ConversionSymbols.Unwrap(target)), string.Empty);

    /// <summary>Identifies the date and time types covered by the standard converters.</summary>
    /// <param name="type">The unwrapped value type.</param>
    /// <returns>True for a supported temporal value.</returns>
    internal static bool Supports(ITypeSymbol type) =>
        type.ContainingNamespace is { Name: "System", ContainingNamespace.IsGlobalNamespace: true }
        && type.Name is "DateTime" or "DateTimeOffset" or "TimeSpan" or "DateOnly" or "TimeOnly";
}
