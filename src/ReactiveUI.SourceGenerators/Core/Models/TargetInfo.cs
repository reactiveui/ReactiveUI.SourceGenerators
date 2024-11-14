// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

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
    string TargetType)
{
    public static TargetInfo From(INamedTypeSymbol namedTypeSymbol)
    {
        var targetHintName = namedTypeSymbol.GetFullyQualifiedMetadataName().Replace("<", "_").Replace(">", "_");
        var targetName = namedTypeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        var targetNamespace = namedTypeSymbol.ContainingNamespace.ToDisplayString(SymbolHelpers.DefaultDisplay);
        var targetNameWithNamespace = namedTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var targetAccessibility = namedTypeSymbol.GetAccessibilityString();
        var targetType = namedTypeSymbol.GetTypeString();

        return new(
            targetHintName,
            targetName,
            targetNamespace,
            targetNameWithNamespace,
            targetAccessibility,
            targetType);
    }
}
