// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using ReactiveUI.SourceGenerators.Extensions;
using ReactiveUI.SourceGenerators.Helpers;

namespace ReactiveUI.SourceGenerators;

/// <summary>A source generator for generating reactive properties.</summary>
[Generator(LanguageNames.CSharp)]
public sealed partial class ReactiveCollectionGenerator : IIncrementalGenerator
{
    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static ctx =>
        {
            // Add the ReactiveAttribute to the compilation
            ctx.AddSource($"{AttributeDefinitions.ReactiveCollectionAttributeType}.g.cs", SourceText.From(AttributeDefinitions.ReactiveCollectionAttribute, Encoding.UTF8));
        });

        RunReactiveCollectionFromField(in context);
    }

    /// <summary>Registers the pipeline that generates properties from reactive collection fields.</summary>
    /// <param name="context">The incremental generator initialization context.</param>
    private static void RunReactiveCollectionFromField(in IncrementalGeneratorInitializationContext context)
    {
        var results =
            context.SyntaxProvider
            .ForAttributeWithMetadataName(
                AttributeDefinitions.ReactiveCollectionAttributeType,
                static (node, _) => node is VariableDeclaratorSyntax
                {
                    Parent.Parent: FieldDeclarationSyntax
                    {
                        Parent: ClassDeclarationSyntax or RecordDeclarationSyntax,
                        AttributeLists.Count: > 0,
                    },
                },
                static (context, token) => GetVariableInfo(context, token))
            .Where(static x => x is not null)
            .Select(static (x, _) => x!)
            .WithTrackingName(TrackingNames.ReactiveCollections);

        context.RegisterDiagnostics(results);

        var types = results
            .Where(static result => result.Value is not null)
            .Select(static (result, _) => result.Value!)
            .GroupByTarget(static property => property.TargetInfo)
            .WithTrackingName(TrackingNames.ReactiveCollectionTypes);

        context.RegisterSourceOutput(types.Combine(context.ReactiveUiIntegration()), static (context, input) =>
            context.AddSource($"{input.Left[0].TargetInfo.FileHintName}.ReactiveCollections.g.cs", GenerateSource(input.Left, input.Right)));
    }
}
