// Copyright (c) 2025 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace ReactiveUI.SourceGenerators.Extensions;

/// <summary>
/// Extension methods for the <see cref="INamedTypeSymbol"/> type.
/// </summary>
internal static class INamedTypeSymbolExtensions
{
    /// <summary>
    /// Gets all member symbols from a given <see cref="INamedTypeSymbol"/> instance, including inherited ones.
    /// </summary>
    /// <param name="symbol">The input <see cref="INamedTypeSymbol"/> instance.</param>
    /// <returns>A sequence of all member symbols for <paramref name="symbol"/>.</returns>
    public static IEnumerable<ISymbol> GetAllMembers(this INamedTypeSymbol symbol)
    {
        for (var currentSymbol = symbol; currentSymbol is { SpecialType: not SpecialType.System_Object }; currentSymbol = currentSymbol.BaseType)
        {
            foreach (var memberSymbol in currentSymbol.GetMembers())
            {
                yield return memberSymbol;
            }
        }
    }

    /// <summary>
    /// Gets all member symbols from a given <see cref="INamedTypeSymbol"/> instance, including inherited ones.
    /// </summary>
    /// <param name="symbol">The input <see cref="INamedTypeSymbol"/> instance.</param>
    /// <param name="name">The name of the members to look for.</param>
    /// <returns>A sequence of all member symbols for <paramref name="symbol"/>.</returns>
    public static IEnumerable<ISymbol> GetAllMembers(this INamedTypeSymbol symbol, string name)
    {
        for (var currentSymbol = symbol; currentSymbol is { SpecialType: not SpecialType.System_Object }; currentSymbol = currentSymbol.BaseType)
        {
            foreach (var memberSymbol in currentSymbol.GetMembers(name))
            {
                yield return memberSymbol;
            }
        }
    }

    /// <summary>
    /// Returns a string representation of the type, such as "class", "struct", or "interface".
    /// </summary>
    /// <param name="namedTypeSymbol">The type symbol to analyze.</param>
    /// <returns>A string representing the type kind.</returns>
    public static string GetTypeString(this INamedTypeSymbol namedTypeSymbol)
    {
        if (namedTypeSymbol.TypeKind == TypeKind.Interface)
        {
            return "interface";
        }

        if (namedTypeSymbol.TypeKind == TypeKind.Struct)
        {
            return namedTypeSymbol.IsRecord ? "record struct" : "struct";
        }

        if (namedTypeSymbol.TypeKind == TypeKind.Class)
        {
            return namedTypeSymbol.IsRecord ? "record" : "class";
        }

        throw new InvalidOperationException("Unknown type kind.");
    }
}
