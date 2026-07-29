// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Immutable;
using HashCode = System.HashCode;

namespace Microsoft.CodeAnalysis;

/// <summary>A type containing information for a match from <see cref="SyntaxValueProviderExtensions.ForAttributeWithMetadataNameWithGenerics"/>.</summary>
internal readonly struct GenericGeneratorAttributeSyntaxContext : IEquatable<GenericGeneratorAttributeSyntaxContext>
{
    /// <summary>Initializes a new instance of the <see cref="GenericGeneratorAttributeSyntaxContext"/> struct.</summary>
    /// <param name="targetNode">The syntax node the attribute is attached to.</param>
    /// <param name="targetSymbol">The symbol that the attribute is attached to.</param>
    /// <param name="semanticModel">Semantic model for the file that <see cref="TargetNode"/> is contained within.</param>
    /// <param name="attributes">The collection of matching attributes.</param>
    internal GenericGeneratorAttributeSyntaxContext(
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
    internal SyntaxNode TargetNode { get; }

    /// <summary>
    /// Gets the symbol that the attribute is attached to. For example, with <c>[CLSCompliant] class C { }</c> this would be the <see cref="INamedTypeSymbol"/> for <c>"C"</c>.
    /// </summary>
    internal ISymbol TargetSymbol { get; }

    /// <summary>Gets semantic model for the file that <see cref="TargetNode"/> is contained within.</summary>
    internal SemanticModel SemanticModel { get; }

    /// <summary>
    /// Gets the matching attributes on <see cref="TargetSymbol"/>. This collection is always non-empty. Each attribute has an
    /// <see cref="AttributeData.AttributeClass"/> whose fully qualified metadata name matches the requested name.
    /// <para>
    /// To get the entire list of attributes, use <see cref="ISymbol.GetAttributes"/> on <see cref="TargetSymbol"/>.
    /// </para>
    /// </summary>
    internal ImmutableArray<AttributeData> Attributes { get; }

    /// <inheritdoc/>
    public bool Equals(GenericGeneratorAttributeSyntaxContext other) =>
        ReferenceEquals(TargetNode, other.TargetNode)
        && SymbolEqualityComparer.Default.Equals(TargetSymbol, other.TargetSymbol)
        && ReferenceEquals(SemanticModel, other.SemanticModel)
        && Attributes.Equals(other.Attributes);

    /// <inheritdoc/>
    public override bool Equals(object? obj) =>
        obj is GenericGeneratorAttributeSyntaxContext other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        HashCode hashCode = default;
        hashCode.Add(TargetNode);
        hashCode.Add(SymbolEqualityComparer.Default.GetHashCode(TargetSymbol));
        hashCode.Add(SemanticModel);
        hashCode.Add(Attributes);
        return hashCode.ToHashCode();
    }
}
