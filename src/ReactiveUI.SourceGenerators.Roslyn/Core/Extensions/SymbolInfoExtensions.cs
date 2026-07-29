// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace ReactiveUI.SourceGenerators.Extensions;

/// <summary>Extension methods for the <see cref="SymbolInfo"/> type.</summary>
internal static class SymbolInfoExtensions
{
    /// <summary>Provides extension members for symbol information.</summary>
    /// <param name="symbolInfo">The symbol information to extend.</param>
    extension(SymbolInfo symbolInfo)
    {
        /// <summary>Tries to get the resolved attribute type symbol from this value.</summary>
        /// <param name="typeSymbol">The resulting attribute type symbol, if correctly resolved.</param>
        /// <returns>Whether this value is resolved to a symbol.</returns>
        internal bool TryGetAttributeTypeSymbol([NotNullWhen(true)] out INamedTypeSymbol? typeSymbol)
        {
        var attributeSymbol = symbolInfo.Symbol;

        // If no symbol is selected and there is a single candidate symbol, use that
        if (attributeSymbol is null && symbolInfo.CandidateSymbols is [ISymbol candidateSymbol])
        {
            attributeSymbol = candidateSymbol;
        }

        // Extract the symbol from either the current one or the containing type
        if ((attributeSymbol as INamedTypeSymbol ?? attributeSymbol?.ContainingType) is not INamedTypeSymbol resultingSymbol)
        {
            typeSymbol = null;

            return false;
        }

        typeSymbol = resultingSymbol;

        return true;
        }
    }
}
