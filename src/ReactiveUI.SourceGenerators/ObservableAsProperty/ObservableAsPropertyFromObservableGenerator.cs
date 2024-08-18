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
using ReactiveUI.SourceGenerators.Input.Models;
using ReactiveUI.SourceGenerators.Models;
using ReactiveUI.SourceGenerators.ObservableAsProperty.Models;

namespace ReactiveUI.SourceGenerators;

/// <summary>
/// A source generator for generating reative properties.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed partial class ObservableAsPropertyFromObservableGenerator : IIncrementalGenerator
{
    private const string ObservableAsPropertyAttribute = "ReactiveUI.SourceGenerators.ObservableAsPropertyAttribute";

    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Gather info for all annotated command methods (starting from method declarations with at least one attribute)
        IncrementalValuesProvider<(HierarchyInfo Hierarchy, Result<ObservableMethodInfo> Info)> propertyInfoWithErrors =
            context.SyntaxProvider
            .ForAttributeWithMetadataName(
                ObservableAsPropertyAttribute,
                static (node, _) => node is MethodDeclarationSyntax { Parent: ClassDeclarationSyntax or RecordDeclarationSyntax, AttributeLists.Count: > 0 },
                static (context, token) =>
                {
                    using var diagnostics = ImmutableArrayBuilder<DiagnosticInfo>.Rent();
                    var propertyInfo = default(ObservableMethodInfo?);
                    var methodSyntax = (MethodDeclarationSyntax)context.TargetNode;
                    var symbol = ModelExtensions.GetDeclaredSymbol(context.SemanticModel, methodSyntax, token)!;
                    token.ThrowIfCancellationRequested();

                    // Skip symbols without the target attribute
                    if (!symbol.TryGetAttributeWithFullyQualifiedMetadataName(ObservableAsPropertyAttribute, out var attributeData))
                    {
                        return default;
                    }

                    token.ThrowIfCancellationRequested();

                    var compilation = context.SemanticModel.Compilation;
                    var methodSymbol = (IMethodSymbol)symbol!;
                    var isObservable = Execute.IsObservableReturnType(methodSymbol.ReturnType);

                    token.ThrowIfCancellationRequested();

                    Execute.GatherForwardedAttributes(
                        methodSymbol,
                        context.SemanticModel,
                        methodSyntax,
                        token,
                        out var propertyAttributes);

                    token.ThrowIfCancellationRequested();

                    // Get the can execute member, if any
                    if (!attributeData.TryGetNamedArgument("PropertyName", out string? propertyName))
                    {
                        return default;
                    }

                    if (string.IsNullOrWhiteSpace(propertyName))
                    {
                        return default;
                    }

                    token.ThrowIfCancellationRequested();

                    // Get the hierarchy info for the target symbol, and try to gather the property info
                    var hierarchy = HierarchyInfo.From(methodSymbol.ContainingType);

                    token.ThrowIfCancellationRequested();
                    propertyInfo = new ObservableMethodInfo(
                        methodSymbol.Name,
                        methodSymbol.ReturnType,
                        methodSymbol.Parameters.FirstOrDefault()?.Type,
                        propertyName!,
                        propertyAttributes);

                    return (Hierarchy: hierarchy, new Result<ObservableMethodInfo?>(propertyInfo, diagnostics.ToImmutable()));
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
            var propertyInfos = item.Right.ToArray();

            // Generate all member declarations for the current type
            var propertyDeclarations =
                propertyInfos
                .Select(Execute.GetPropertySyntax)
                .SelectMany(x => x)
                .ToList();

            var c = Execute.GetPropertyInitiliser(propertyInfos);
            propertyDeclarations.Add(c);
            var memberDeclarations = propertyDeclarations.ToImmutableArray();

            // Insert all members into the same partial type declaration
            var compilationUnit = item.Key.GetCompilationUnit(memberDeclarations);
            context.AddSource($"{item.Key.FilenameHint}.ObservableAsPropertyFromObservable.g.cs", compilationUnit);
        });
    }
}
