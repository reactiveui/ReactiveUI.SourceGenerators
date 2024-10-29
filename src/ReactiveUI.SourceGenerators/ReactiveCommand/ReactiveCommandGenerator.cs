// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using ReactiveUI.SourceGenerators.Extensions;
using ReactiveUI.SourceGenerators.Helpers;

namespace ReactiveUI.SourceGenerators;

/// <summary>
/// A source generator for generating reative properties.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed partial class ReactiveCommandGenerator : IIncrementalGenerator
{
    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
            ctx.AddSource($"{AttributeDefinitions.ReactiveCommandAttributeType}.g.cs", SourceText.From(AttributeDefinitions.ReactiveCommandAttribute, Encoding.UTF8)));

        // Gather info for all annotated command methods (starting from method declarations with at least one attribute)
        var commandInfo =
            context.SyntaxProvider
            .ForAttributeWithMetadataName(
                AttributeDefinitions.ReactiveCommandAttributeType,
                static (node, _) => node is MethodDeclarationSyntax { Parent: ClassDeclarationSyntax or RecordDeclarationSyntax, AttributeLists.Count: > 0 },
                static (context, token) => GetMethodInfo(context, token))
            .Where(x => x != null)
            .Select((x, _) => x!)
            .Collect();

        // Generate the requested properties and methods
        context.RegisterSourceOutput(commandInfo, static (context, input) =>
        {
            var groupedcommandInfo = input.GroupBy(
                static info => (info.FileHintName, info.TargetName, info.TargetNamespace, info.TargetVisibility, info.TargetType),
                static info => info)
                .ToImmutableArray();

            if (groupedcommandInfo.Length == 0)
            {
                return;
            }

            foreach (var grouping in groupedcommandInfo)
            {
                var items = grouping.ToImmutableArray();

                if (items.Length == 0)
                {
                    continue;
                }

                var (fileHintName, targetName, targetNamespace, targetVisibility, targetType) = grouping.Key;

                var source = GenerateSource(targetName, targetNamespace, targetVisibility, targetType, [.. grouping]);

                context.AddSource($"{fileHintName}.ReactiveCommands.g.cs", source);
            }
        });
    }
}
