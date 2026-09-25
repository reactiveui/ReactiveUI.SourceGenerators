// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
        // The generic IViewFor<T> and the IViewFor(string) forms are different metadata names, so each has its own
        // ForAttributeWithMetadataName; both share one predicate and one transform. Only a partial class can take the
        // generated members, so anything else is rejected from syntax before any binding.
        Func<SyntaxNode, CancellationToken, bool> predicate = static (node, _) =>
            node is ClassDeclarationSyntax { AttributeLists.Count: > 0 } declaration && declaration.Modifiers.Any(SyntaxKind.PartialKeyword);
        Func<GeneratorAttributeSyntaxContext, CancellationToken, IViewForInfo?> transform = static (context, token) => GetClassInfo(context, token);
        var named = context.SyntaxProvider.ForAttributeWithMetadataName(AttributeDefinitions.IViewForAttributeType, predicate, transform).Collect();
        var generic = context.SyntaxProvider.ForAttributeWithMetadataName(AttributeDefinitions.IViewForGenericAttributeType, predicate, transform).Collect();
        var viewForInfo = named.Combine(generic).Select(static (pair, _) => Join(pair.Left, pair.Right));

        // Generate the requested properties and methods for IViewFor
        context.RegisterSourceOutput(viewForInfo, static (context, input) =>
        {
            var groupedPropertyInfo = GroupByTarget(input);

            // View registration is not generated here: ReactiveUI.Binding's view locator registers every IViewFor<T>.
            foreach (var grouping in groupedPropertyInfo.Values)
            {
                var info = grouping[0];
                var source = GenerateSource(info);

                // Only add source when a supported UI framework base type was detected
                if (source is not null)
                {
                    context.AddSource($"{info.TargetInfo.FileHintName}.IViewFor.g.cs", source);
                }
            }
        });
    }

    /// <summary>Joins the views found through each attribute form, dropping the targets that were declined.</summary>
    /// <param name="named">The views marked <c>[IViewFor("...")]</c>.</param>
    /// <param name="generic">The views marked <c>[IViewFor&lt;T&gt;]</c>.</param>
    /// <returns>The views.</returns>
    private static ImmutableArray<IViewForInfo> Join(ImmutableArray<IViewForInfo?> named, ImmutableArray<IViewForInfo?> generic)
    {
        var builder = ImmutableArray.CreateBuilder<IViewForInfo>(named.Length + generic.Length);
        foreach (var info in named)
        {
            if (info is not null)
            {
                builder.Add(info);
            }
        }

        foreach (var info in generic)
        {
            if (info is not null)
            {
                builder.Add(info);
            }
        }

        return builder.ToImmutable();
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
