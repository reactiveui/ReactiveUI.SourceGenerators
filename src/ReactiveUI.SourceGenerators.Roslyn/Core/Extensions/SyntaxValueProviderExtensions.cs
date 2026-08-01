// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Threading;
using ReactiveUI.SourceGenerators.Extensions;

namespace Microsoft.CodeAnalysis;

/// <summary>Extension methods for the <see cref="SyntaxValueProvider"/> type.</summary>
internal static class SyntaxValueProviderExtensions
{
    /// <summary>Provides extension members for syntax value providers.</summary>
    /// <param name="syntaxValueProvider">The syntax value provider to extend.</param>
    extension(in SyntaxValueProvider syntaxValueProvider)
    {
        /// <summary>Creates a provider for nodes with an attribute matching the supplied metadata name.</summary>
        /// <typeparam name="T">The generated value type.</typeparam>
        /// <param name="fullyQualifiedMetadataName">The fully qualified metadata name of the attribute to look for.</param>
        /// <param name="predicate">Determines whether a syntax node should be inspected.</param>
        /// <param name="transform">Transforms a matching attribute context into a result.</param>
        /// <returns>A provider that produces the transformed matching attribute contexts.</returns>
        internal IncrementalValuesProvider<T> ForAttributeWithMetadataNameWithGenerics<T>(
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
#if ROSYLN_412 || ROSYLN_500
                    System.Collections.Immutable.ImmutableArray<AttributeData> attributes = [attributeData];
#else
                    var attributes = System.Collections.Immutable.ImmutableArray.Create(attributeData);
#endif
                    GenericGeneratorAttributeSyntaxContext syntaxContext = new(
                        targetNode: context.Node,
                        targetSymbol: symbol,
                        semanticModel: context.SemanticModel,
                        attributes);

                    return new Option<T>(transform(syntaxContext, token));
                })
            .Where(static item => item is not null)
            .Select(static (item, _) => item!.Value)!;
    }

    /// <summary>A simple record to wrap a value that might be missing.</summary>
    /// <typeparam name="T">The type of values to wrap.</typeparam>
    /// <param name="Value">The wrapped value, if it exists.</param>
    private sealed record Option<T>(T? Value);
}
