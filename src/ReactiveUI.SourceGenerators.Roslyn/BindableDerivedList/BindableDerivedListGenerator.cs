// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using ReactiveUI.SourceGenerators.BindableDerivedList.Models;
using ReactiveUI.SourceGenerators.Helpers;

namespace ReactiveUI.SourceGenerators;

/// <summary>A source generator for generating BindableDerivedList properties.</summary>
[Generator(LanguageNames.CSharp)]
public sealed partial class BindableDerivedListGenerator : IIncrementalGenerator
{
    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static ctx =>
        {
            // Add the BindableDerivedListAttribute to the compilation
            ctx.AddSource($"{AttributeDefinitions.BindableDerivedListAttributeType}.g.cs", SourceText.From(AttributeDefinitions.BindableDerivedListAttribute, Encoding.UTF8));
        });

        // Gather info for all annotated variable with at least one attribute.
        var bindableDerivedListInfo =
            context.SyntaxProvider
            .ForAttributeWithMetadataName(
                AttributeDefinitions.BindableDerivedListAttributeType,
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
            .Collect();

        // Generate the requested properties
        context.RegisterSourceOutput(bindableDerivedListInfo, static (context, input) =>
        {
            Dictionary<
                (string FileHintName, string TargetName, string TargetNamespace, string TargetVisibility, string TargetType),
                List<BindableDerivedListInfo>> groupedPropertyInfo = [];

            foreach (var result in input)
            {
                foreach (var diagnostic in result.Errors.AsImmutableArray())
                {
                    context.ReportDiagnostic(diagnostic.ToDiagnostic());
                }

                if (result.Value is not BindableDerivedListInfo propertyInfo)
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
                var source = GenerateSource(grouping.Key.TargetName, grouping.Key.TargetNamespace, grouping.Key.TargetVisibility, grouping.Key.TargetType, grouping.Value.ToArray());
                context.AddSource($"{grouping.Key.FileHintName}.BindableDerivedList.g.cs", source);
            }
        });
    }
}
