// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.SourceGenerators.Extensions;
using ReactiveUI.SourceGenerators.Helpers;

namespace ReactiveUI.SourceGenerators;

/// <summary>A source generator for generating BindableDerivedList properties.</summary>
[Generator(LanguageNames.CSharp)]
public sealed partial class BindableDerivedListGenerator : IIncrementalGenerator
{
    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Gather info for all annotated variable with at least one attribute.
        var results =
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
            .WithTrackingName(TrackingNames.BindableDerivedLists);

        context.RegisterDiagnostics(results);

        var types = results
            .Where(static result => result.Value is not null)
            .Select(static (result, _) => result.Value!)
            .GroupByTarget(static property => property.TargetInfo)
            .WithTrackingName(TrackingNames.BindableDerivedListTypes);

        context.RegisterSourceOutput(types, static (context, properties) =>
            context.AddSource($"{properties[0].TargetInfo.FileHintName}.BindableDerivedList.g.cs", GenerateSource(properties)));
    }
}
