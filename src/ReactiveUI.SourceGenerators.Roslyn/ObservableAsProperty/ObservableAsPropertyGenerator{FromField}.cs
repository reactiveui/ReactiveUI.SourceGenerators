// Copyright (c) 2025 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using ReactiveUI.SourceGenerators.Helpers;

namespace ReactiveUI.SourceGenerators;

/// <summary>
/// A source generator for generating reative properties.
/// </summary>
public sealed partial class ObservableAsPropertyGenerator
{
    private static void RunObservableAsPropertyFromField(in IncrementalGeneratorInitializationContext context)
    {
        var propertyInfo =
            context.SyntaxProvider
            .ForAttributeWithMetadataName(
                AttributeDefinitions.ObservableAsPropertyAttributeType,
                static (node, _) => node is VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax { Parent: FieldDeclarationSyntax { Parent: ClassDeclarationSyntax or RecordDeclarationSyntax, AttributeLists.Count: > 0 } } },
                static (context, token) => GetVariableInfo(context, token))
            .Where(x => x != null)
            .Select((x, _) => x!)
            .Collect();

        // Generate the requested properties and methods
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
                static info => (info.TargetInfo.FileHintName, info.TargetInfo.TargetName, info.TargetInfo.TargetNamespace, info.TargetInfo.TargetVisibility, info.TargetInfo.TargetType),
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
                context.AddSource($"{grouping.Key.FileHintName}.ObservableAsProperties.g.cs", source);
            }
        });
    }
}
