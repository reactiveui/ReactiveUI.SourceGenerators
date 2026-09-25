// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Immutable;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using ReactiveUI.SourceGenerators.Extensions;
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
        var types = named.Combine(generic)
            .SelectMany(static (pair, _) => Join(pair.Left, pair.Right))
            .GroupByTarget(static info => info.TargetInfo)
            .WithTrackingName(TrackingNames.ViewForTypes);

        // View registration is not generated here: ReactiveUI.Binding's view locator registers every IViewFor<T>.
        context.RegisterSourceOutput(types, static (context, infos) =>
        {
            var info = infos[0];

            // Only a supported UI framework base type gets a source.
            if (GenerateSource(info) is not { } source)
            {
                return;
            }

            context.AddSource($"{info.TargetInfo.FileHintName}.IViewFor.g.cs", source);
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
}
