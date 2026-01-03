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
public sealed partial class IViewForGenerator : IIncrementalGenerator
{
    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
            ctx.AddSource(AttributeDefinitions.IViewForAttributeType + ".g.cs", SourceText.From(AttributeDefinitions.IViewForAttribute, Encoding.UTF8)));

        // Gather info for all annotated IViewFor Classes
        var iViewForInfo =
            context.SyntaxProvider
            .ForAttributeWithMetadataNameWithGenerics(
                AttributeDefinitions.IViewForAttributeType,
                static (node, _) => node is ClassDeclarationSyntax { AttributeLists.Count: > 0 },
                static (context, token) => GetClassInfo(context, token))
            .Where(x => x != null)
            .Select((x, _) => x!)
            .Collect();

        // Generate the requested properties and methods for IViewFor
        context.RegisterSourceOutput(iViewForInfo, static (context, input) =>
        {
            var groupedPropertyInfo = input.GroupBy(
                static info => (info.TargetInfo.FileHintName, info.TargetInfo.TargetName, info.TargetInfo.TargetNamespace, info.TargetInfo.TargetVisibility, info.TargetInfo.TargetType),
                static info => info)
                .ToImmutableArray();

            const string fileName = "ReactiveUI.ReactiveUISourceGeneratorsExtensions.g.cs";

            if (groupedPropertyInfo.Length == 0)
            {
                // Even if there are no views, emit an empty extension to keep API stable.
                var empty = GenerateRegistrationExtensions(ImmutableArray.Create<Models.IViewForInfo>());
                context.AddSource(fileName, SourceText.From(empty, Encoding.UTF8));
                return;
            }

            // Generate the IViewFor Splat Registration code for all classes in a single extension method here
            var registrationSource = GenerateRegistrationExtensions(input);
            context.AddSource(fileName, SourceText.From(registrationSource, Encoding.UTF8));

            foreach (var grouping in groupedPropertyInfo)
            {
                var items = grouping.ToImmutableArray();

                if (items.Length == 0)
                {
                    continue;
                }

                var source = GenerateSource(grouping.Key.TargetName, grouping.Key.TargetNamespace, grouping.Key.TargetVisibility, grouping.Key.TargetType, grouping.FirstOrDefault());

                // Only add source if it's not empty (i.e., a supported UI framework base type was detected)
                if (!string.IsNullOrWhiteSpace(source))
                {
                    context.AddSource(grouping.Key.FileHintName + ".IViewFor.g.cs", source);
                }
            }
        });
    }
}
