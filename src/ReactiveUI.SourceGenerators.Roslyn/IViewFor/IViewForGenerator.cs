// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using ReactiveUI.SourceGenerators.Helpers;
using ReactiveUI.SourceGenerators.Models;

namespace ReactiveUI.SourceGenerators;

/// <summary>A source generator for generating reactive properties.</summary>
[Generator(LanguageNames.CSharp)]
public sealed partial class IViewForGenerator : IIncrementalGenerator
{
    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static ctx =>
            ctx.AddSource($"{AttributeDefinitions.IViewForAttributeType}.g.cs", SourceText.From(AttributeDefinitions.IViewForAttribute, Encoding.UTF8)));

        // Gather info for all annotated IViewFor Classes
        var viewForInfo =
            context.SyntaxProvider
            .ForAttributeWithMetadataNameWithGenerics(
                AttributeDefinitions.IViewForAttributeType,
                static (node, _) => node is ClassDeclarationSyntax { AttributeLists.Count: > 0 },
                static (context, token) => GetClassInfo(context, token))
            .Where(static x => x is not null)
            .Select(static (x, _) => x!)
            .Collect();

        // Generate the requested properties and methods for IViewFor
        context.RegisterSourceOutput(viewForInfo, static (context, input) =>
        {
            var groupedPropertyInfo = GroupByTarget(input);

            const string fileName = "ReactiveUI.ReactiveUISourceGeneratorsExtensions.g.cs";

            if (groupedPropertyInfo.Count == 0)
            {
                // Even if there are no views, emit an empty extension to keep API stable.
                var empty = GenerateRegistrationExtensions(ImmutableArray<IViewForInfo>.Empty);
                context.AddSource(fileName, SourceText.From(empty, Encoding.UTF8));
                return;
            }

            // Generate the IViewFor Splat Registration code for all classes in a single extension method here
            var registrationSource = GenerateRegistrationExtensions(input);
            context.AddSource(fileName, SourceText.From(registrationSource, Encoding.UTF8));

            foreach (var grouping in groupedPropertyInfo.Values)
            {
                var info = grouping[0];
                var source = GenerateSource(info, info.TargetInfo.ParentInfo);

                // Only add source if it's not empty (i.e., a supported UI framework base type was detected)
                if (!string.IsNullOrWhiteSpace(source))
                {
                    context.AddSource($"{info.TargetInfo.FileHintName}.IViewFor.g.cs", source);
                }
            }
        });
    }

    /// <summary>Groups source-generation inputs by their annotated target type.</summary>
    /// <param name="input">The discovered <c>IViewFor</c> targets.</param>
    /// <returns>The targets grouped by their generated file identity.</returns>
    private static Dictionary<(string FileHintName, string TargetName, string TargetNamespace, string TargetVisibility, string TargetType), List<IViewForInfo>> GroupByTarget(
        ImmutableArray<IViewForInfo> input)
    {
        Dictionary<(string, string, string, string, string), List<IViewForInfo>> result = [];
        foreach (var info in input)
        {
            var target = info.TargetInfo;
            var key = (target.FileHintName, target.TargetName, target.TargetNamespace, target.TargetVisibility, target.TargetType);
            if (!result.TryGetValue(key, out var values))
            {
                values = [];
                result.Add(key, values);
            }

            values.Add(info);
        }

        return result;
    }
}
