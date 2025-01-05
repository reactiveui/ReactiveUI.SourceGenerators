// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
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

    public static void GetParentClasses(List<string> parentClassDeclarations, TargetInfo? targetInfo)
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

    public static string GenerateParentClassDeclarations(List<string> parentClassDeclarations)
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

    public static string GenerateClosingBrackets(int numberOfBrackets)
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
