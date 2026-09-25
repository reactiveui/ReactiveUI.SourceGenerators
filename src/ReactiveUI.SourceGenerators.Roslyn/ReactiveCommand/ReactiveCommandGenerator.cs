// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using ReactiveUI.SourceGenerators.CodeGeneration;
using ReactiveUI.SourceGenerators.Extensions;
using ReactiveUI.SourceGenerators.Helpers;

namespace ReactiveUI.SourceGenerators;

/// <summary>A source generator for generating reactive properties.</summary>
[Generator(LanguageNames.CSharp)]
public sealed partial class ReactiveCommandGenerator : IIncrementalGenerator
{
    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static ctx =>
            ctx.AddSource($"{AttributeDefinitions.ReactiveCommandAttributeType}.g.cs", SourceText.From(AttributeDefinitions.ReactiveCommandAttribute, SourceWriterExtensions.Utf8WithoutBom)));

        // Gather info for all annotated command methods, then group them per type: each type's file is written from
        // its own group, so an edit to one type's commands leaves every other type's file cached.
        var types =
            context.SyntaxProvider
            .ForAttributeWithMetadataName(
                AttributeDefinitions.ReactiveCommandAttributeType,
                static (node, _) => node is MethodDeclarationSyntax { Parent: ClassDeclarationSyntax or RecordDeclarationSyntax, AttributeLists.Count: > 0 },
                static (context, token) => GetMethodInfo(context, token))
            .Where(static x => x is not null)
            .Select(static (x, _) => x!)
            .WithTrackingName(TrackingNames.ReactiveCommands)
            .GroupByTarget(static command => command.TargetInfo)
            .WithTrackingName(TrackingNames.ReactiveCommandTypes);

        context.RegisterSourceOutput(types.Combine(context.ReactiveUiIntegration()), static (context, input) =>
            context.AddSource($"{input.Left[0].TargetInfo.FileHintName}.ReactiveCommands.g.cs", GenerateSource(input.Left, input.Right)));
    }
}
