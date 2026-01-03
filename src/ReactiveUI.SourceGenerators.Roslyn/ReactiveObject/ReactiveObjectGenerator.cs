// Copyright (c) 2026 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using ReactiveUI.SourceGenerators.Helpers;

namespace ReactiveUI.SourceGenerators;

/// <summary>
/// A source generator for generating reactive properties.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed partial class ReactiveObjectGenerator : IIncrementalGenerator
{
    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
            ctx.AddSource(AttributeDefinitions.ReactiveObjectAttributeType + ".g.cs", SourceText.From(AttributeDefinitions.ReactiveObjectAttribute, Encoding.UTF8)));

        // Gather info for all annotated IReactiveObject Classes
        var reactiveObjectInfo =
            context.SyntaxProvider
            .ForAttributeWithMetadataNameWithGenerics(
                AttributeDefinitions.ReactiveObjectAttributeType,
                static (node, _) => node is ClassDeclarationSyntax { AttributeLists.Count: > 0 },
                static (context, token) => GetClassInfo(context, token))
            .Where(x => x != null)
            .Select((x, _) => x!)
            .Collect();

        // Generate the requested properties and methods for IReactiveObject
        context.RegisterSourceOutput(reactiveObjectInfo, static (context, input) =>
        {
            var groupedPropertyInfo = input.GroupBy(
                static info => (info.TargetInfo.FileHintName, info.TargetInfo.TargetName, info.TargetInfo.TargetNamespace, info.TargetInfo.TargetVisibility, info.TargetInfo.TargetType),
                static info => info)
                .ToImmutableArray();

            foreach (var grouping in groupedPropertyInfo)
            {
                var items = grouping.ToImmutableArray();

                if (items.Length == 0)
                {
                    continue;
                }

                var source = GenerateSource(grouping.Key.TargetName, grouping.Key.TargetNamespace, grouping.Key.TargetVisibility, grouping.Key.TargetType, grouping.FirstOrDefault());
                context.AddSource(grouping.Key.FileHintName + ".IReactiveObject.g.cs", source);
            }
        });
    }
}
