// Copyright (c) 2025 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Immutable;
using System.Threading;
using ReactiveUI.SourceGenerators.Extensions;

namespace Microsoft.CodeAnalysis;

/// <summary>
/// Extension methods for the <see cref="SyntaxValueProvider"/> type.
/// </summary>
internal static class SyntaxValueProviderExtensions
{
    /// <summary>
    /// Creates an <see cref="IncrementalValuesProvider{T}"/> that can provide a transform over all <see
    /// cref="SyntaxNode"/>s if that node has an attribute on it that binds to a <see cref="INamedTypeSymbol"/> with the
    /// same fully-qualified metadata as the provided <paramref name="fullyQualifiedMetadataName"/>. <paramref
    /// name="fullyQualifiedMetadataName"/> should be the fully-qualified, metadata name of the attribute, including the
    /// <c>Attribute</c> suffix.  For example <c>"System.CLSCompliantAttribute</c> for <see cref="CLSCompliantAttribute"/>.
    /// </summary>
    /// <param name="syntaxValueProvider">The source <see cref="SyntaxValueProvider"/> instance to use.</param>
    /// <param name="fullyQualifiedMetadataName">The fully qualified metadata name of the attribute to look for.</param>
    /// <param name="predicate">A function that determines if the given <see cref="SyntaxNode"/> attribute target (<see
    /// cref="GenericGeneratorAttributeSyntaxContext.TargetNode"/>) should be transformed.  Nodes that do not pass this
    /// predicate will not have their attributes looked at all.</param>
    /// <param name="transform">A function that performs the transform. This will only be passed nodes that return <see
    /// langword="true"/> for <paramref name="predicate"/> and which have a matching <see cref="AttributeData"/> whose
    /// <see cref="AttributeData.AttributeClass"/> has the same fully qualified, metadata name as <paramref
    /// name="fullyQualifiedMetadataName"/>.</param>
    public static IncrementalValuesProvider<T> ForAttributeWithMetadataNameWithGenerics<T>(
        this in SyntaxValueProvider syntaxValueProvider,
        string fullyQualifiedMetadataName,
        Func<SyntaxNode, CancellationToken, bool> predicate,
        Func<GenericGeneratorAttributeSyntaxContext, CancellationToken, T> transform) => syntaxValueProvider
            .CreateSyntaxProvider(
                predicate,
                (context, token) =>
                {
                    var symbol = context.SemanticModel.GetDeclaredSymbol(context.Node, token);

                    // If the syntax node doesn't have a declared symbol, just skip this node. This would be
                    // the case for eg. lambda attributes, but those are not supported by the MVVM Toolkit.
                    if (symbol is null)
                    {
                        return null;
                    }

                    // Skip symbols without the target attribute
                    if (!symbol.TryGetAttributeWithFullyQualifiedMetadataName(fullyQualifiedMetadataName, out var attributeData))
                    {
                        return null;
                    }

                    // Edge case: if the symbol is a partial method, skip the implementation part and only process the partial method
                    // definition. This is needed because attributes will be reported as available on both the definition and the
                    // implementation part. To avoid generating duplicate files, we only give priority to the definition part.
                    // On Roslyn 4.3+, ForAttributeWithMetadataName will already only return the symbol the attribute was located on.
                    if (symbol is IMethodSymbol { IsPartialDefinition: false, PartialDefinitionPart: not null })
                    {
                        return null;
                    }

                    // Create the GeneratorAttributeSyntaxContext value to pass to the input transform. The attributes array
                    // will only ever have a single value, but that's fine with the attributes the various generators look for.
                    GenericGeneratorAttributeSyntaxContext syntaxContext = new(
                        targetNode: context.Node,
                        targetSymbol: symbol,
                        semanticModel: context.SemanticModel,
                        attributes: ImmutableArray.Create(attributeData));

                    return new Option<T>(transform(syntaxContext, token));
                })
            .Where(static item => item is not null)
            .Select(static (item, _) => item!.Value)!;

    /// <summary>
    /// A simple record to wrap a value that might be missing.
    /// </summary>
    /// <typeparam name="T">The type of values to wrap.</typeparam>
    /// <param name="Value">The wrapped value, if it exists.</param>
    private sealed record Option<T>(T? Value);
}
