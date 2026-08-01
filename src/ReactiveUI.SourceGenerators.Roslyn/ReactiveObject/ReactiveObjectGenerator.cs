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
public sealed partial class ReactiveObjectGenerator : IIncrementalGenerator
{
    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static ctx =>
            ctx.AddSource($"{AttributeDefinitions.ReactiveObjectAttributeType}.g.cs", SourceText.From(AttributeDefinitions.ReactiveObjectAttribute, Encoding.UTF8)));

        // Gather info for all annotated IReactiveObject Classes
        var reactiveObjectInfo =
            context.SyntaxProvider
            .ForAttributeWithMetadataNameWithGenerics(
                AttributeDefinitions.ReactiveObjectAttributeType,
                static (node, _) => node is ClassDeclarationSyntax { AttributeLists.Count: > 0 },
                static (context, token) => GetClassInfo(context, token))
            .Where(static x => x is not null)
            .Select(static (x, _) => x!)
            .Collect()
            .Combine(context.ReactiveUiIntegration());

        // Generate the requested properties and methods for IReactiveObject
        context.RegisterSourceOutput(reactiveObjectInfo, static (context, input) =>
        {
            Dictionary<
                (string FileHintName, string TargetName, string TargetNamespace, string TargetVisibility, string TargetType),
                ReactiveObjectInfo> groupedPropertyInfo = [];

            foreach (var reactiveObjectInfo in input.Left)
            {
                var targetInfo = reactiveObjectInfo.TargetInfo;
                var key = (targetInfo.FileHintName, targetInfo.TargetName, targetInfo.TargetNamespace, targetInfo.TargetVisibility, targetInfo.TargetType);
                if (!groupedPropertyInfo.ContainsKey(key))
                {
                    groupedPropertyInfo.Add(key, reactiveObjectInfo);
                }
            }

            foreach (var grouping in groupedPropertyInfo)
            {
                var source = GenerateSource(grouping.Key.TargetName, grouping.Key.TargetNamespace, grouping.Key.TargetVisibility, grouping.Key.TargetType, input.Right);
                context.AddSource($"{grouping.Key.FileHintName}.IReactiveObject.g.cs", source);
            }
        });
    }
}
