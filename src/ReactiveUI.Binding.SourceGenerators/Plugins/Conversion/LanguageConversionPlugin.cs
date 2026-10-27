// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Conversion;

/// <summary>Offers the compiler's implicit conversion without adding a boxing boundary.</summary>
internal sealed class LanguageConversionPlugin : IConversionPlugin
{
    /// <inheritdoc/>
    public ConversionInfo? Select(ITypeSymbol source, ITypeSymbol target, Compilation compilation)
    {
        var conversion = ((CSharpCompilation)compilation).ClassifyConversion(source, target);
        return conversion.IsImplicit && !conversion.IsBoxing
            ? new($"({target.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)})__value", "true", 1)
            : null;
    }
}
