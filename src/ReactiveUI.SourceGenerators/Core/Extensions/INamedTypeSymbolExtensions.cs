// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
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

    /// <summary>
    /// Checks if a given attribute can be applied to a specific target element type.
    /// </summary>
    /// <param name="attributeClass">The attribute class symbol.</param>
    /// <param name="target">The target element type (e.g., field, property).</param>
    /// <returns><c>true</c> if the attribute can be applied to the target; otherwise, <c>false</c>.</returns>
    public static bool AttributeCanTarget(this INamedTypeSymbol? attributeClass, AttributeTargets target)
    {
        if (attributeClass == null)
        {
            return false;
        }

        // Look for an AttributeUsage attribute to determine the valid targets.
        var usageAttribute = attributeClass.GetAttributes()
            .FirstOrDefault(attr => attr.AttributeClass?.ToDisplayString() == "System.AttributeUsageAttribute");

        if (usageAttribute == null)
        {
            // If no AttributeUsage attribute is found, assume the attribute can be applied anywhere.
            return true;
        }

        // Retrieve the valid targets from the AttributeUsage constructor arguments.
        var validTargets = (AttributeTargets)usageAttribute.ConstructorArguments[0].Value!;
        return validTargets.HasFlag(target);
    }
}
