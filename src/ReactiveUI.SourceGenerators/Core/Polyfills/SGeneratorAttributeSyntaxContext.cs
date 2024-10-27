// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis;

/// <summary>
/// A type containing information for a match from <see cref="SyntaxValueProviderExtensions.ForAttributeWithMetadataNameInternal"/>.
/// </summary>
internal readonly struct SGeneratorAttributeSyntaxContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SGeneratorAttributeSyntaxContext"/> struct.
    /// Creates a new <see cref="SGeneratorAttributeSyntaxContext"/> instance with the specified parameters.
    /// </summary>
    /// <param name="targetNode">The syntax node the attribute is attached to.</param>
    /// <param name="targetSymbol">The symbol that the attribute is attached to.</param>
    /// <param name="semanticModel">Semantic model for the file that <see cref="TargetNode"/> is contained within.</param>
    /// <param name="attributes">The collection of matching attributes.</param>
    internal SGeneratorAttributeSyntaxContext(
        SyntaxNode targetNode,
        ISymbol targetSymbol,
        SemanticModel semanticModel,
        ImmutableArray<AttributeData> attributes)
    {
        TargetNode = targetNode;
        TargetSymbol = targetSymbol;
        SemanticModel = semanticModel;
        Attributes = attributes;
    }

    /// <summary>
    /// Gets the syntax node the attribute is attached to. For example, with <c>[CLSCompliant] class C { }</c> this would the class declaration node.
    /// </summary>
    public SyntaxNode TargetNode { get; }

    /// <summary>
    /// Gets the symbol that the attribute is attached to. For example, with <c>[CLSCompliant] class C { }</c> this would be the <see cref="INamedTypeSymbol"/> for <c>"C"</c>.
    /// </summary>
    public ISymbol TargetSymbol { get; }

    /// <summary>
    /// Gets semantic model for the file that <see cref="TargetNode"/> is contained within.
    /// </summary>
    public SemanticModel SemanticModel { get; }

    /// <summary>
    /// <see cref="AttributeData"/>Gets s for any matching attributes on <see cref="TargetSymbol"/>. Always non-empty. All
    /// these attributes will have an <see cref="AttributeData.AttributeClass"/> whose fully qualified name metadata
    /// name matches the name requested in <see cref="SyntaxValueProviderExtensions.ForAttributeWithMetadataNameInternal"/>.
    /// <para>
    /// To get the entire list of attributes, use <see cref="ISymbol.GetAttributes"/> on <see cref="TargetSymbol"/>.
    /// </para>
    /// </summary>
    public ImmutableArray<AttributeData> Attributes { get; }
}
