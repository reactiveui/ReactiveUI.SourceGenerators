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
public sealed partial class ReactiveCommandGenerator : IIncrementalGenerator
{
    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static ctx =>
            ctx.AddSource($"{AttributeDefinitions.ReactiveCommandAttributeType}.g.cs", SourceText.From(AttributeDefinitions.ReactiveCommandAttribute, Encoding.UTF8)));

        // Gather info for all annotated command methods (starting from method declarations with at least one attribute)
        var commandInfo =
            context.SyntaxProvider
            .ForAttributeWithMetadataName(
                AttributeDefinitions.ReactiveCommandAttributeType,
                static (node, _) => node is MethodDeclarationSyntax { Parent: ClassDeclarationSyntax or RecordDeclarationSyntax, AttributeLists.Count: > 0 },
                static (context, token) => GetMethodInfo(context, token))
            .Where(static x => x is not null)
            .Select(static (x, _) => x!)
            .Collect()
            .Combine(context.ReactiveUiIntegration());

        // Generate the requested properties and methods
        context.RegisterSourceOutput(commandInfo, static (context, input) =>
        {
            Dictionary<
                (string FileHintName, string TargetName, string TargetNamespace, string TargetVisibility, string TargetType),
                List<CommandInfo>> groupedCommandInfo = [];

            foreach (var command in input.Left)
            {
                var targetInfo = command.TargetInfo;
                var key = (targetInfo.FileHintName, targetInfo.TargetName, targetInfo.TargetNamespace, targetInfo.TargetVisibility, targetInfo.TargetType);
                if (!groupedCommandInfo.TryGetValue(key, out var commands))
                {
                    commands = [];
                    groupedCommandInfo.Add(key, commands);
                }

                commands.Add(command);
            }

            foreach (var grouping in groupedCommandInfo)
            {
                var source = GenerateSource(
                    grouping.Key.TargetName,
                    grouping.Key.TargetNamespace,
                    grouping.Key.TargetVisibility,
                    grouping.Key.TargetType,
                    grouping.Value.ToArray(),
                    input.Right);
                context.AddSource($"{grouping.Key.FileHintName}.ReactiveCommands.g.cs", source);
            }
        });
    }
}
