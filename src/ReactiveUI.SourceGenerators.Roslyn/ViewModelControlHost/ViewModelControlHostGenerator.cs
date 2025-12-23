// Copyright (c) 2025 ReactiveUI and contributors. All rights reserved.
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

namespace ReactiveUI.SourceGenerators.WinForms;

/// <summary>
/// A source generator for generating reactive properties.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed partial class ViewModelControlHostGenerator : IIncrementalGenerator
{
    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
            ctx.AddSource($"{AttributeDefinitions.ViewModelControlHostAttributeType}.g.cs", SourceText.From(AttributeDefinitions.ViewModelControlHostAttribute, Encoding.UTF8)));

        // Gather info for all annotated IViewFor Classes
        var vmcInfo =
            context.SyntaxProvider
            .ForAttributeWithMetadataName(
                AttributeDefinitions.ViewModelControlHostAttributeType,
                static (node, _) => node is ClassDeclarationSyntax { AttributeLists.Count: > 0 },
                static (context, token) => GetClassInfo(context, token))
            .Where(x => x != null)
            .Select((x, _) => x!)
            .Collect();

        // Generate the requested properties and methods for IViewFor
        context.RegisterSourceOutput(vmcInfo, static (context, input) =>
        {
            var groupedPropertyInfo = input.GroupBy(
                static info => (info.FileHintName, info.TargetName, info.TargetNamespace, info.TargetVisibility, info.TargetType),
                static info => info)
                .ToImmutableArray();

            if (groupedPropertyInfo.Length == 0)
            {
                return;
            }

            foreach (var grouping in groupedPropertyInfo)
            {
                var items = grouping.ToImmutableArray();

                if (items.Length == 0)
                {
                    continue;
                }

                var source = GetViewModelControlHost(grouping.Key.TargetName, grouping.Key.TargetNamespace, grouping.Key.TargetVisibility, grouping.Key.TargetType, grouping.FirstOrDefault());
                context.AddSource($"{grouping.Key.FileHintName}.ViewModelControlHost.g.cs", source);
            }
        });
    }
}
