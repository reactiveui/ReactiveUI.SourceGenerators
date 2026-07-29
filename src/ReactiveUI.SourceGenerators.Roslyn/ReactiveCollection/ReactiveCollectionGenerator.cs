// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using ReactiveUI.SourceGenerators.Extensions;
using ReactiveUI.SourceGenerators.Helpers;
using ReactiveUI.SourceGenerators.Models;

namespace ReactiveUI.SourceGenerators;

/// <summary>A source generator for generating reactive properties.</summary>
[Generator(LanguageNames.CSharp)]
public sealed partial class ReactiveCollectionGenerator : IIncrementalGenerator
{
    /// <summary>Gets the generator type name used in generated-code metadata.</summary>
    internal static readonly string GeneratorName = typeof(ReactiveCollectionGenerator).FullName!;

    /// <summary>Gets the generator assembly version used in generated-code metadata.</summary>
    internal static readonly string GeneratorVersion = typeof(ReactiveCollectionGenerator).Assembly.GetName().Version.ToString();

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
        var propertyInfo =
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
            .Collect()
            .Combine(context.ReactiveUiIntegration());

        // Generate the requested properties and methods
        context.RegisterSourceOutput(propertyInfo, static (context, input) =>
        {
            Dictionary<
                (string FileHintName, string TargetName, string TargetNamespace, string TargetVisibility, string TargetType),
                List<ReactiveCollectionFieldInfo>> groupedPropertyInfo = [];

            foreach (var result in input.Left)
            {
                foreach (var diagnostic in result.Errors.AsImmutableArray())
                {
                    context.ReportDiagnostic(diagnostic.ToDiagnostic());
                }

                if (result.Value is not ReactiveCollectionFieldInfo propertyInfo)
                {
                    continue;
                }

                var targetInfo = propertyInfo.TargetInfo;
                var key = (targetInfo.FileHintName, targetInfo.TargetName, targetInfo.TargetNamespace, targetInfo.TargetVisibility, targetInfo.TargetType);
                if (!groupedPropertyInfo.TryGetValue(key, out var properties))
                {
                    properties = [];
                    groupedPropertyInfo.Add(key, properties);
                }

                properties.Add(propertyInfo);
            }

            foreach (var grouping in groupedPropertyInfo)
            {
                var source = GenerateSource(grouping.Key.TargetName, grouping.Key.TargetNamespace, grouping.Key.TargetVisibility, grouping.Key.TargetType, grouping.Value.ToArray(), input.Right);
                context.AddSource($"{grouping.Key.FileHintName}.ReactiveCollections.g.cs", source);
            }
        });
    }
}
