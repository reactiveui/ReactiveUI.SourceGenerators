﻿// Copyright (c) 2025 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using ReactiveUI.SourceGenerators.Extensions;
using ReactiveUI.SourceGenerators.Helpers;

namespace ReactiveUI.SourceGenerators.Models;

internal sealed partial record TargetInfo(
    string FileHintName,
    string TargetName,
    string TargetNamespace,
    string TargetNamespaceWithNamespace,
    string TargetVisibility,
    string TargetType,
    TargetInfo? ParentInfo)
{
    public static TargetInfo From(INamedTypeSymbol namedTypeSymbol)
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

    public static (string Declarations, string ClosingBrackets) GenerateParentClassDeclarations(TargetInfo?[] targetInfos)
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

    private static void GetParentClasses(List<string> parentClassDeclarations, TargetInfo? targetInfo)
    {
        if (targetInfo is not null)
        {
            var parentClassDeclaration = $"{targetInfo.TargetVisibility} partial {targetInfo.TargetType} {targetInfo.TargetName}";

            // Add the parent class declaration if it does not exist in the list
            if (!parentClassDeclarations.Contains(parentClassDeclaration))
            {
                parentClassDeclarations.Add(parentClassDeclaration);
            }

            if (targetInfo.ParentInfo is not null)
            {
                // Recursively get the parent classes
                GetParentClasses(parentClassDeclarations, targetInfo.ParentInfo);
            }
        }
    }

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

    private static string GenerateClosingBrackets(int numberOfBrackets)
    {
        var closingBrackets = new string('}', numberOfBrackets);
        closingBrackets = closingBrackets.Replace("}", "}\n");
        if (!string.IsNullOrWhiteSpace(closingBrackets))
        {
            closingBrackets = "\n" + closingBrackets;
        }

        return closingBrackets;
    }
}
