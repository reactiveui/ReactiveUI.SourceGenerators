// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

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
        var results =
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
            .WithTrackingName(TrackingNames.ReactiveFields);

        RegisterOutputs(context, results, ".Properties.g.cs", TrackingNames.ReactiveFieldTypes);
    }

#if ROSYLN_412 || ROSYLN_500
    /// <summary>Registers generation for partial properties annotated with <c>ReactiveAttribute</c>.</summary>
    /// <param name="context">The incremental generator initialization context.</param>
    private static void RunReactiveFromProperty(in IncrementalGeneratorInitializationContext context)
    {
        // Gather info for all annotated variable with at least one attribute.
        var results =
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
            .WithTrackingName(TrackingNames.ReactivePartialProperties);

        RegisterOutputs(context, results, ".PartialProperties.g.cs", TrackingNames.ReactivePartialPropertyTypes);
    }
#endif

    /// <summary>Reports the extraction diagnostics and writes one file per type, each cached on its own.</summary>
    /// <param name="context">The incremental generator initialization context.</param>
    /// <param name="results">The extraction results.</param>
    /// <param name="hintSuffix">The suffix of each type's file name.</param>
    /// <param name="trackingName">The tracking name of the per-type step.</param>
    private static void RegisterOutputs(
        in IncrementalGeneratorInitializationContext context,
        IncrementalValuesProvider<Result<PropertyInfo?>> results,
        string hintSuffix,
        string trackingName)
    {
        context.RegisterDiagnostics(results);

        var types = results
            .Where(static result => result.Value is not null)
            .Select(static (result, _) => result.Value!)
            .GroupByTarget(static property => property.TargetInfo)
            .WithTrackingName(trackingName);

        context.RegisterSourceOutput(types.Combine(context.ReactiveUiIntegration()), (context, input) =>
        {
            var properties = input.Left;
            context.AddSource(properties[0].TargetInfo.FileHintName + hintSuffix, GenerateSource(properties, input.Right));
        });
    }
}
