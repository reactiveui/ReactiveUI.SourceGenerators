// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using ReactiveUI.SourceGenerators.Extensions;
using ReactiveUI.SourceGenerators.Helpers;

namespace ReactiveUI.SourceGenerators.WinForms;

/// <summary>A source generator for generating reactive properties.</summary>
[Generator(LanguageNames.CSharp)]
public sealed partial class RoutedControlHostGenerator : IIncrementalGenerator
{
    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static ctx =>
            ctx.AddSource($"{AttributeDefinitions.RoutedControlHostAttributeType}.g.cs", SourceText.From(AttributeDefinitions.GetRoutedControlHostAttribute(), Encoding.UTF8)));

        var reactiveUiIntegrationProvider = context.ReactiveUiIntegration();

        // Gather info for all annotated IViewFor Classes
        var rchInfo =
            context.SyntaxProvider
            .ForAttributeWithMetadataName(
                AttributeDefinitions.RoutedControlHostAttributeType,
                static (node, _) => node is ClassDeclarationSyntax { AttributeLists.Count: > 0 },
                static (context, token) => GetClassInfo(context, token))
            .Where(static x => x is not null)
            .Select(static (x, _) => x!)
            .Collect()
            .Combine(reactiveUiIntegrationProvider);

        // Generate the requested properties and methods for IViewFor
        context.RegisterSourceOutput(rchInfo, static (context, input) =>
        {
            foreach (var info in input.Left)
            {
                var source = GetRoutedControlHost(
                    info.TargetName,
                    info.TargetNamespace,
                    info.TargetVisibility,
                    info.TargetType,
                    info,
                    input.Right);
                context.AddSource($"{info.FileHintName}.RoutedControlHost.g.cs", source);
            }
        });
    }
}
