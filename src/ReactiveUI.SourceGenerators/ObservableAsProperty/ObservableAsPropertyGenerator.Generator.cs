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
using ReactiveUI.SourceGenerators.Models;

namespace ReactiveUI.SourceGenerators;

/// <summary>
/// A source generator for generating reative properties.
/// </summary>
public sealed partial class ObservableAsPropertyGenerator
{
    private static void RunGenerator(in IncrementalGeneratorInitializationContext context)
    {
        // Gather info for all annotated command methods (starting from method declarations with at least one attribute)
        IncrementalValuesProvider<(HierarchyInfo Hierarchy, Result<PropertyInfo> Info)> propertyInfoWithErrors =
            context.SyntaxProvider
            .ForAttributeWithMetadataName(
                AttributeDefinitions.ObservableAsPropertyAttributeType,
                static (node, _) => node is VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax { Parent: FieldDeclarationSyntax { Parent: ClassDeclarationSyntax or RecordDeclarationSyntax, AttributeLists.Count: > 0 } } },
                static (context, token) =>
                {
                    var symbol = ModelExtensions.GetDeclaredSymbol(context.SemanticModel, context.TargetNode, token)!;
                    token.ThrowIfCancellationRequested();

                    // Skip symbols without the target attribute
                    if (!symbol.TryGetAttributeWithFullyQualifiedMetadataName(AttributeDefinitions.ObservableAsPropertyAttributeType, out var attributeData))
                    {
                        return default;
                    }

                    // Get the can PropertyName member, if any
                    attributeData.TryGetNamedArgument("ReadOnly", out bool? isReadonly);

                    var fieldDeclaration = (FieldDeclarationSyntax)context.TargetNode.Parent!.Parent!;
                    var fieldSymbol = (IFieldSymbol)context.TargetSymbol;

                    // Get the hierarchy info for the target symbol, and try to gather the property info
                    var hierarchy = HierarchyInfo.From(fieldSymbol.ContainingType);

                    token.ThrowIfCancellationRequested();

                    Execute.GetFieldInfoFromClass(fieldDeclaration, fieldSymbol, context.SemanticModel, isReadonly, token, out var propertyInfo, out var diagnostics);

                    token.ThrowIfCancellationRequested();
                    return (Hierarchy: hierarchy, new Result<PropertyInfo?>(propertyInfo, diagnostics));
                })
            .Where(static item => item.Hierarchy is not null)!;

        // Output the diagnostics
        context.ReportDiagnostics(propertyInfoWithErrors.Select(static (item, _) => item.Info.Errors));

        // Get the filtered sequence to enable caching
        var propertyInfo =
            propertyInfoWithErrors
            .Where(static item => item.Info.Value is not null)!;

        // Split and group by containing type
        var groupedPropertyInfo =
            propertyInfo
            .GroupBy(static item => item.Left, static item => item.Right.Value);

        // Generate the requested properties and methods
        context.RegisterSourceOutput(groupedPropertyInfo, static (context, item) =>
        {
            // Generate all member declarations for the current type
            var memberDeclarations =
                item.Right
                .Select(Execute.GetPropertySyntax)
                .SelectMany(static m => m)
                .ToImmutableArray();

            // Insert all members into the same partial type declaration
            var compilationUnit = item.Key.GetCompilationUnit(memberDeclarations);
            context.AddSource($"{item.Key.FilenameHint}.ObservableAsProperties.g.cs", compilationUnit);
        });
    }
}
