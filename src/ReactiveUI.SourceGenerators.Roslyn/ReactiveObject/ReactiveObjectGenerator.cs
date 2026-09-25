// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using ReactiveUI.SourceGenerators.CodeGeneration;
using ReactiveUI.SourceGenerators.Extensions;
using ReactiveUI.SourceGenerators.Helpers;
using ReactiveUI.SourceGenerators.Models;

namespace ReactiveUI.SourceGenerators;

/// <summary>A source generator for generating reactive properties.</summary>
[Generator(LanguageNames.CSharp)]
public sealed partial class ReactiveObjectGenerator : IIncrementalGenerator
{
    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static ctx =>
            ctx.AddSource($"{AttributeDefinitions.ReactiveObjectAttributeType}.g.cs", SourceText.From(AttributeDefinitions.ReactiveObjectAttribute, SourceWriterExtensions.Utf8WithoutBom)));

        // Gather info for all annotated IReactiveObject Classes, one output per type.
        var types =
            context.SyntaxProvider
            .ForAttributeWithMetadataName(
                AttributeDefinitions.ReactiveObjectAttributeType,
                static (node, _) => node is ClassDeclarationSyntax { AttributeLists.Count: > 0 } declaration && declaration.Modifiers.Any(SyntaxKind.PartialKeyword),
                static (context, token) => GetClassInfo(context, token))
            .Where(static x => x is not null)
            .Select(static (x, _) => x!)
            .GroupByTarget(static info => info.TargetInfo)
            .WithTrackingName(TrackingNames.ReactiveObjectTypes);

        context.RegisterSourceOutput(types.Combine(context.ReactiveUiIntegration()), static (context, input) =>
        {
            var target = input.Left[0].TargetInfo;
            context.AddSource($"{target.FileHintName}.IReactiveObject.g.cs", GenerateSource(target, input.Right));
        });
    }
}
