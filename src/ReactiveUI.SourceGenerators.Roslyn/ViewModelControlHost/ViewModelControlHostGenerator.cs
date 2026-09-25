// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using ReactiveUI.SourceGenerators.CodeGeneration;
using ReactiveUI.SourceGenerators.Extensions;
using ReactiveUI.SourceGenerators.Helpers;

namespace ReactiveUI.SourceGenerators.WinForms;

/// <summary>A source generator for generating reactive properties.</summary>
[Generator(LanguageNames.CSharp)]
public sealed partial class ViewModelControlHostGenerator : IIncrementalGenerator
{
    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static ctx =>
            ctx.AddSource(
                $"{AttributeDefinitions.ViewModelControlHostAttributeType}.g.cs",
                SourceText.From(AttributeDefinitions.ViewModelControlHostAttribute, SourceWriterExtensions.Utf8WithoutBom)));

        // One model per annotated class, each written by its own output so it caches on its own.
        var hosts =
            context.SyntaxProvider
            .ForAttributeWithMetadataName(
                AttributeDefinitions.ViewModelControlHostAttributeType,
                static (node, _) => node is ClassDeclarationSyntax { AttributeLists.Count: > 0 },
                static (context, token) => GetClassInfo(context, token))
            .Where(static x => x is not null)
            .Select(static (x, _) => x!)
            .WithTrackingName(TrackingNames.ViewModelControlHosts);

        context.RegisterSourceOutput(hosts.Combine(context.ReactiveUiIntegration()), static (context, input) =>
            context.AddSource($"{input.Left.FileHintName}.ViewModelControlHost.g.cs", GenerateSource(input.Left, input.Right)));
    }
}
