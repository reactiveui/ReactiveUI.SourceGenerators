// Copyright (c) 2025 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using ReactiveUI.SourceGenerators.Helpers;

namespace ReactiveUI.SourceGenerators;

/// <summary>
/// A source generator for generating reative properties.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed partial class ReactiveCollectionGenerator : IIncrementalGenerator
{
    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
        {
            // Add the ReactiveAttribute to the compilation
            ctx.AddSource($"{AttributeDefinitions.ReactiveCollectionAttributeType}.g.cs", SourceText.From(AttributeDefinitions.ReactiveCollectionAttribute, Encoding.UTF8));
        });

        RunReactiveCollectionFromField(context);
    }

    private static void RunReactiveCollectionFromField(IncrementalGeneratorInitializationContext context)
    {
        ////// Gather info for all annotated variable with at least one attribute.
        ////var propertyInfo =
        ////    context.SyntaxProvider
        ////    .ForAttributeWithMetadataName(
        ////        AttributeDefinitions.ReactiveAttributeType,
        ////        static (node, _) => node is VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax { Parent: FieldDeclarationSyntax { Parent: ClassDeclarationSyntax or RecordDeclarationSyntax, AttributeLists.Count: > 0 } } },
        ////        static (context, token) => GetVariableInfo(context, token))
        ////    .Where(x => x != null)
        ////    .Select((x, _) => x!)
        ////    .Collect();

        ////// Generate the requested properties
        ////context.RegisterSourceOutput(propertyInfo, static (context, input) =>
        ////{
        ////    foreach (var diagnostic in input.SelectMany(static x => x.Errors))
        ////    {
        ////        // Output the diagnostics
        ////        context.ReportDiagnostic(diagnostic.ToDiagnostic());
        ////    }

        ////    // Gather all the properties that are valid and group them by the target information.
        ////    var groupedPropertyInfo = input
        ////        .Where(static x => x.Value != null)
        ////        .Select(static x => x.Value!).GroupBy(
        ////        static info => (info.TargetInfo.FileHintName, info.TargetInfo.TargetName, info.TargetInfo.TargetNamespace, info.TargetInfo.TargetVisibility, info.TargetInfo.TargetType),
        ////        static info => info)
        ////        .ToImmutableArray();

        ////    if (groupedPropertyInfo.Length == 0)
        ////    {
        ////        return;
        ////    }

        ////    foreach (var grouping in groupedPropertyInfo)
        ////    {
        ////        var items = grouping.ToImmutableArray();

        ////        if (items.Length == 0)
        ////        {
        ////            continue;
        ////        }

        ////        var source = GenerateSource(grouping.Key.TargetName, grouping.Key.TargetNamespace, grouping.Key.TargetVisibility, grouping.Key.TargetType, [.. grouping]);
        ////        context.AddSource($"{grouping.Key.FileHintName}.Properties.g.cs", source);
        ////    }
        ////});
    }
}
