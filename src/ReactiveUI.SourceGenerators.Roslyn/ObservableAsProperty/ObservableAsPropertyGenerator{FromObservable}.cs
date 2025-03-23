// Copyright (c) 2025 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
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
    private static void RunObservableAsPropertyFromObservable(in IncrementalGeneratorInitializationContext context)
    {
        // Gather info for all annotated command methods (starting from method declarations with at least one attribute)
        var methodInfo =
            context.SyntaxProvider
            .ForAttributeWithMetadataName(
                AttributeDefinitions.ObservableAsPropertyAttributeType,
                static (node, _) => node is MethodDeclarationSyntax or PropertyDeclarationSyntax { Parent: ClassDeclarationSyntax or RecordDeclarationSyntax, AttributeLists.Count: > 0 },
                static (context, token) => GetObservableInfo(context, token))
            .Where(x => x != null)
            .Select((x, _) => x!)
            .Collect();

        // Generate the requested properties and methods
        context.RegisterSourceOutput(methodInfo, static (context, input) =>
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

                var source = GenerateObservableSource(grouping.Key.TargetName, grouping.Key.TargetNamespace, grouping.Key.TargetVisibility, grouping.Key.TargetType, [.. grouping]);
                context.AddSource($"{grouping.Key.FileHintName}.ObservableAsPropertyFromObservable.g.cs", source);
            }
        });
    }
}
