// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using ReactiveUI.SourceGenerators.Extensions;
using ReactiveUI.SourceGenerators.Helpers;

namespace ReactiveUI.SourceGenerators.Models;

/// <summary>Describes a target type for generated source output.</summary>
/// <param name="FileHintName">The generated file hint name.</param>
/// <param name="TargetName">The target type name.</param>
/// <param name="TargetNamespace">The target namespace.</param>
/// <param name="TargetNamespaceWithNamespace">The fully qualified target type name.</param>
/// <param name="TargetVisibility">The target type visibility.</param>
/// <param name="TargetType">The target type keyword.</param>
/// <param name="ParentInfo">The containing type, if the target is nested.</param>
internal sealed record TargetInfo(
    string FileHintName,
    string TargetName,
    string TargetNamespace,
    string TargetNamespaceWithNamespace,
    string TargetVisibility,
    string TargetType,
    TargetInfo? ParentInfo)
{
    /// <summary>Creates target information from a named type symbol.</summary>
    /// <param name="namedTypeSymbol">The target type symbol.</param>
    /// <returns>The generated target information.</returns>
    internal static TargetInfo From(INamedTypeSymbol namedTypeSymbol)
    {
        var targetHintName = namedTypeSymbol.GetFullyQualifiedMetadataName().Replace("<", "_").Replace(">", "_");
        var targetName = namedTypeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        var targetNamespace = namedTypeSymbol.ContainingNamespace.ToDisplayString(SymbolHelpers.DefaultDisplay);
        var targetNameWithNamespace = namedTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var targetAccessibility = namedTypeSymbol.GetAccessibilityString();
        var targetType = namedTypeSymbol.GetTypeString();

        var parentInfo = namedTypeSymbol.ContainingType is not null
            ? From(namedTypeSymbol.ContainingType)
            : null;

        return new(
            targetHintName,
            targetName,
            targetNamespace,
            targetNameWithNamespace,
            targetAccessibility,
            targetType,
            parentInfo);
    }

    /// <summary>Generates the containing type declarations and corresponding closing braces.</summary>
    /// <param name="targetInfos">The target types whose parents should be generated.</param>
    /// <returns>The declarations and closing braces.</returns>
    internal static (string Declarations, string ClosingBrackets) GenerateParentClassDeclarations(TargetInfo?[] targetInfos)
    {
        var parentClassDeclarations = new List<string>();
        foreach (var targetInfo in targetInfos)
        {
            GetParentClasses(parentClassDeclarations, targetInfo);
        }

        var parentClassDeclarationsString = GenerateParentClassDeclarations(parentClassDeclarations);
        var closingBrackets = GenerateClosingBrackets(parentClassDeclarations.Count);
        return (parentClassDeclarationsString, closingBrackets);
    }

    /// <summary>Adds all containing type declarations for a target type.</summary>
    /// <param name="parentClassDeclarations">The declarations to populate.</param>
    /// <param name="targetInfo">The target whose parents are processed.</param>
    private static void GetParentClasses(List<string> parentClassDeclarations, TargetInfo? targetInfo)
    {
        if (targetInfo is null)
        {
            return;
        }

        var parentClassDeclaration = $"{targetInfo.TargetVisibility} partial {targetInfo.TargetType} {targetInfo.TargetName}";

        // Add the parent class declaration if it does not exist in the list
        if (!parentClassDeclarations.Contains(parentClassDeclaration))
        {
            parentClassDeclarations.Add(parentClassDeclaration);
        }

        if (targetInfo.ParentInfo is null)
        {
            return;
        }

        // Recursively get the parent classes
        GetParentClasses(parentClassDeclarations, targetInfo.ParentInfo);
    }

    /// <summary>Generates the text for the supplied containing type declarations.</summary>
    /// <param name="parentClassDeclarations">The containing type declarations.</param>
    /// <returns>The generated declaration text.</returns>
    private static string GenerateParentClassDeclarations(List<string> parentClassDeclarations)
    {
        // Reverse the list to get the parent classes in the correct order
        parentClassDeclarations.Reverse();

        // Generate the parent class declarations
        var parentClassDeclarationsString = string.Join("\n{\n", parentClassDeclarations);
        if (!string.IsNullOrWhiteSpace(parentClassDeclarationsString))
        {
            parentClassDeclarationsString += "\n{\n";
        }

        return parentClassDeclarationsString;
    }

    /// <summary>Generates closing braces for a nesting depth.</summary>
    /// <param name="numberOfBrackets">The nesting depth.</param>
    /// <returns>The generated closing brace text.</returns>
    private static string GenerateClosingBrackets(int numberOfBrackets)
    {
        var closingBrackets = new string('}', numberOfBrackets);
        closingBrackets = closingBrackets.Replace("}", "}\n");
        if (!string.IsNullOrWhiteSpace(closingBrackets))
        {
            closingBrackets = $"\n{closingBrackets}";
        }

        return closingBrackets;
    }
}
