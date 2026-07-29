// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using ReactiveUI.SourceGenerators.Extensions;
using ReactiveUI.SourceGenerators.Helpers;
using ReactiveUI.SourceGenerators.Models;

namespace ReactiveUI.SourceGenerators;

/// <summary>A source generator for generating reactive properties.</summary>
public sealed partial class ObservableAsPropertyGenerator
{
    /// <summary>Registers generation of observable-backed properties declared from fields.</summary>
    /// <param name="context">The incremental generator initialization context.</param>
    private static void RunObservableAsPropertyFromField(in IncrementalGeneratorInitializationContext context)
    {
        var propertyInfo =
            context.SyntaxProvider
            .ForAttributeWithMetadataName(
                AttributeDefinitions.ObservableAsPropertyAttributeType,
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
                List<ObservableFieldInfo>> groupedPropertyInfo = [];

            foreach (var result in input.Left)
            {
                foreach (var diagnostic in result.Errors.AsImmutableArray())
                {
                    context.ReportDiagnostic(diagnostic.ToDiagnostic());
                }

                if (result.Value is not ObservableFieldInfo propertyInfo)
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
                context.AddSource($"{grouping.Key.FileHintName}.ObservableAsProperties.g.cs", source);
            }
        });
    }
}
