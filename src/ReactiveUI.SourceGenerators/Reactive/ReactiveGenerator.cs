// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using ReactiveUI.SourceGenerators.Extensions;
using ReactiveUI.SourceGenerators.Helpers;

namespace ReactiveUI.SourceGenerators;

/// <summary>
/// A source generator for generating reative properties.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed partial class ReactiveGenerator : IIncrementalGenerator
{
    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
        {
            // Add the AccessModifier enum to the compilation
            ctx.AddSource($"{AttributeDefinitions.AccessModifierType}.g.cs", SourceText.From(AttributeDefinitions.GetAccessModifierEnum(), Encoding.UTF8));

            // Add the ReactiveAttribute to the compilation
            ctx.AddSource($"{AttributeDefinitions.ReactiveAttributeType}.g.cs", SourceText.From(AttributeDefinitions.ReactiveAttribute, Encoding.UTF8));
        });

        // Gather info for all annotated variable with at least one attribute.
        var propertyInfo =
            context.SyntaxProvider
            .ForAttributeWithMetadataName(
                AttributeDefinitions.ReactiveAttributeType,
                static (node, _) => node is VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax { Parent: FieldDeclarationSyntax { Parent: ClassDeclarationSyntax or RecordDeclarationSyntax, AttributeLists.Count: > 0 } } },
                static (context, token) => GetVariableInfo(context, token))
            .Where(x => x != null)
            .Select((x, _) => x!)
            .Collect();

        // Generate the requested properties
        context.RegisterSourceOutput(propertyInfo, static (context, input) =>
        {
            foreach (var diagnostic in input.SelectMany(static x => x.Errors))
            {
                // Output the diagnostics
                context.ReportDiagnostic(diagnostic.ToDiagnostic());
            }

            // Gather all the properties that are valid and group them by the target information.
            var groupedPropertyInfo = input
                .Where(static x => x.Value != null)
                .Select(static x => x.Value!).GroupBy(
                static info => (info.FileHintName, info.TargetName, info.TargetNamespace, info.TargetVisibility, info.TargetType),
                static info => info)
                .ToImmutableArray();

            if (groupedPropertyInfo.Length == 0)
            {
                return;
            }

            foreach (var grouping in groupedPropertyInfo)
            {
                var items = grouping.ToImmutableArray();

                if (items.Length == 0)
                {
                    continue;
                }

                var source = GenerateSource(grouping.Key.TargetName, grouping.Key.TargetNamespace, grouping.Key.TargetVisibility, grouping.Key.TargetType, [.. grouping]);
                context.AddSource($"{grouping.Key.FileHintName}.Properties.g.cs", source);
            }
        });
    }
}
