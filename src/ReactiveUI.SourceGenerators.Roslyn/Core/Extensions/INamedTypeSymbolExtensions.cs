// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace ReactiveUI.SourceGenerators.Extensions;

/// <summary>Extension methods for the <see cref="INamedTypeSymbol"/> type.</summary>
internal static class INamedTypeSymbolExtensions
{
    /// <summary>Provides extension members for a named type symbol.</summary>
    /// <param name="symbol">The named type symbol to extend.</param>
    extension(INamedTypeSymbol symbol)
    {
        /// <summary>Gets all member symbols from this instance, including inherited ones.</summary>
        /// <returns>A sequence of all member symbols.</returns>
        internal IEnumerable<ISymbol> GetAllMembers()
        {
        for (var currentSymbol = symbol; currentSymbol is { SpecialType: not SpecialType.System_Object }; currentSymbol = currentSymbol.BaseType)
        {
            foreach (var memberSymbol in currentSymbol.GetMembers())
            {
                yield return memberSymbol;
            }
        }
        }

        /// <summary>Gets all member symbols with a given name from this instance, including inherited ones.</summary>
        /// <param name="name">The name of the members to look for.</param>
        /// <returns>A sequence of all matching member symbols.</returns>
        internal IEnumerable<ISymbol> GetAllMembers(string name)
        {
        for (var currentSymbol = symbol; currentSymbol is { SpecialType: not SpecialType.System_Object }; currentSymbol = currentSymbol.BaseType)
        {
            foreach (var memberSymbol in currentSymbol.GetMembers(name))
            {
                yield return memberSymbol;
            }
        }
        }

        /// <summary>Returns a string representation of this type, such as "class", "struct", or "interface".</summary>
        /// <returns>A string representing the type kind.</returns>
        internal string GetTypeString()
        {
        if (symbol.TypeKind == TypeKind.Interface)
        {
            return "interface";
        }

        if (symbol.TypeKind == TypeKind.Struct)
        {
            return symbol.IsRecord ? "record struct" : "struct";
        }

        if (symbol.TypeKind == TypeKind.Class)
        {
            return symbol.IsRecord ? "record" : "class";
        }

        throw new InvalidOperationException("Unknown type kind.");
        }
    }
}
