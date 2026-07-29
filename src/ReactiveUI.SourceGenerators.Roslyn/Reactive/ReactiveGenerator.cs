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
public sealed partial class ReactiveGenerator : IIncrementalGenerator
{
    /// <summary>The number of accessors required for a partial property.</summary>
    private const int RequiredAccessorCount = 2;

    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static ctx =>
        {
            // Add the AccessModifier enum to the compilation
            ctx.AddSource($"{AttributeDefinitions.AccessModifierType}.g.cs", SourceText.From(AttributeDefinitions.GetAccessModifierEnum(), Encoding.UTF8));

            // Add the ReactiveAttribute to the compilation
            ctx.AddSource($"{AttributeDefinitions.ReactiveAttributeType}.g.cs", SourceText.From(AttributeDefinitions.ReactiveAttribute, Encoding.UTF8));
        });

        RunReactiveFromField(context);
#if ROSYLN_412 || ROSYLN_500
        RunReactiveFromProperty(context);
#endif
    }

    /// <summary>Registers generation for fields annotated with <c>ReactiveAttribute</c>.</summary>
    /// <param name="context">The incremental generator initialization context.</param>
    private static void RunReactiveFromField(in IncrementalGeneratorInitializationContext context)
    {
        // Gather info for all annotated variable with at least one attribute.
        var propertyInfo =
            context.SyntaxProvider
            .ForAttributeWithMetadataName(
                AttributeDefinitions.ReactiveAttributeType,
                static (node, _) => node is VariableDeclaratorSyntax
                {
                    Parent.Parent: FieldDeclarationSyntax { AttributeLists.Count: > 0 },
                    Parent.Parent.Parent: ClassDeclarationSyntax or RecordDeclarationSyntax,
                },
                static (context, token) => GetVariableInfo(context, token))
            .Where(static x => x is not null)
            .Select(static (x, _) => x!)
            .Collect()
            .Combine(context.ReactiveUiIntegration());

        // Generate the requested properties
        context.RegisterSourceOutput(propertyInfo, static (context, input) =>
        {
            Dictionary<
                (string FileHintName, string TargetName, string TargetNamespace, string TargetVisibility, string TargetType),
                List<PropertyInfo>> groupedPropertyInfo = [];

            foreach (var result in input.Left)
            {
                foreach (var diagnostic in result.Errors.AsImmutableArray())
                {
                    context.ReportDiagnostic(diagnostic.ToDiagnostic());
                }

                if (result.Value is not PropertyInfo propertyInfo)
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
                context.AddSource($"{grouping.Key.FileHintName}.Properties.g.cs", source);
            }
        });
    }

#if ROSYLN_412 || ROSYLN_500
    /// <summary>Registers generation for partial properties annotated with <c>ReactiveAttribute</c>.</summary>
    /// <param name="context">The incremental generator initialization context.</param>
    private static void RunReactiveFromProperty(in IncrementalGeneratorInitializationContext context)
    {
        // Gather info for all annotated variable with at least one attribute.
        var propertyInfo =
            context.SyntaxProvider
            .ForAttributeWithMetadataName(
                AttributeDefinitions.ReactiveAttributeType,
                static (node, _) => node is PropertyDeclarationSyntax
                {
                    AccessorList.Accessors.Count: RequiredAccessorCount,
                    AttributeLists.Count: > 0,
                },
                static (context, token) => GetPropertyInfo(context, token))
            .Where(static x => x is not null)
            .Select(static (x, _) => x!)
            .Collect()
            .Combine(context.ReactiveUiIntegration());

        // Generate the requested properties
        context.RegisterSourceOutput(propertyInfo, static (context, input) =>
        {
            Dictionary<
                (string FileHintName, string TargetName, string TargetNamespace, string TargetVisibility, string TargetType),
                List<PropertyInfo>> groupedPropertyInfo = [];

            foreach (var result in input.Left)
            {
                foreach (var diagnostic in result.Errors.AsImmutableArray())
                {
                    context.ReportDiagnostic(diagnostic.ToDiagnostic());
                }

                if (result.Value is not PropertyInfo propertyInfo)
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
                context.AddSource($"{grouping.Key.FileHintName}.PartialProperties.g.cs", source);
            }
        });
    }
#endif
}
