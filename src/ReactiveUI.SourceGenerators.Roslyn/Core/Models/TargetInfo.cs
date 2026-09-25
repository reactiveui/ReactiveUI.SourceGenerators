// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
    /// <summary>The target information already built for a type symbol in the current compilation.</summary>
    /// <remarks>
    /// Every attributed member of a type needs its type's information, and each member is extracted separately, so without
    /// this the same display strings were built once per member. The table holds its keys weakly, so it never keeps an
    /// old compilation's symbols alive, and the values hold no symbols.
    /// </remarks>
    private static readonly ConditionalWeakTable<INamedTypeSymbol, TargetInfo> Cache = new();

    /// <summary>Gets the target information for a named type symbol, building it once per symbol.</summary>
    /// <param name="namedTypeSymbol">The target type symbol.</param>
    /// <returns>The generated target information.</returns>
    internal static TargetInfo From(INamedTypeSymbol namedTypeSymbol) => Cache.GetValue(namedTypeSymbol, Create);

    /// <summary>Creates target information from a named type symbol.</summary>
    /// <param name="namedTypeSymbol">The target type symbol.</param>
    /// <returns>The generated target information.</returns>
    private static TargetInfo Create(INamedTypeSymbol namedTypeSymbol)
    {
        var targetHintName = ToHintName(namedTypeSymbol.GetFullyQualifiedMetadataName());

        // A plain type's display name is its name; only a generic or a keyword-named type needs the display format.
        var targetName = namedTypeSymbol.IsGenericType || SyntaxFacts.GetKeywordKind(namedTypeSymbol.Name) != SyntaxKind.None
            ? namedTypeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)
            : namedTypeSymbol.Name;
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

    /// <summary>Makes a metadata name safe for a hint name, replacing angle brackets only when there are any.</summary>
    /// <param name="metadataName">The fully qualified metadata name.</param>
    /// <returns>The hint name.</returns>
    private static string ToHintName(string metadataName) =>
        metadataName.IndexOf('<') < 0 && metadataName.IndexOf('>') < 0
            ? metadataName
            : metadataName.Replace('<', '_').Replace('>', '_');
}
