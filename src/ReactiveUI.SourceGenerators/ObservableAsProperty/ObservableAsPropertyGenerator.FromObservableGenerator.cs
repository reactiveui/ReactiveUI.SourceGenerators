// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using ReactiveUI.SourceGenerators.Extensions;
using ReactiveUI.SourceGenerators.Helpers;
using ReactiveUI.SourceGenerators.Models;
using ReactiveUI.SourceGenerators.ObservableAsProperty.Models;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static ReactiveUI.SourceGenerators.Diagnostics.DiagnosticDescriptors;

namespace ReactiveUI.SourceGenerators;

/// <summary>
/// A source generator for generating reative properties.
/// </summary>
public sealed partial class ObservableAsPropertyGenerator
{
    private static void RunObservablePropertyAsFromObservable(in IncrementalGeneratorInitializationContext context)
    {
        // Gather info for all annotated command methods (starting from method declarations with at least one attribute)
        IncrementalValuesProvider<(HierarchyInfo Hierarchy, Result<ObservableMethodInfo> Info)> propertyInfoWithErrors =
            context.SyntaxProvider
            .ForAttributeWithMetadataName(
                AttributeDefinitions.ObservableAsPropertyAttributeType,
                static (node, _) => node is MethodDeclarationSyntax or PropertyDeclarationSyntax { Parent: ClassDeclarationSyntax or RecordDeclarationSyntax, AttributeLists.Count: > 0 },
                static (context, token) =>
                {
                    var symbol = context.TargetSymbol;
                    token.ThrowIfCancellationRequested();

                    var attributeData = context.Attributes[0];

                    // Get the can PropertyName member, if any
                    attributeData.TryGetNamedArgument("PropertyName", out string? propertyName);

                    token.ThrowIfCancellationRequested();

                    using var diagnostics = ImmutableArrayBuilder<DiagnosticInfo>.Rent();
                    var propertyInfo = default(ObservableMethodInfo?);
                    var compilation = context.SemanticModel.Compilation;
                    var hierarchy = default(HierarchyInfo);

                    if (context.TargetNode is MethodDeclarationSyntax methodSyntax)
                    {
                        var methodSymbol = (IMethodSymbol)symbol!;
                        if (methodSymbol.Parameters.Length != 0)
                        {
                            diagnostics.Add(
                                ObservableAsPropertyMethodHasParametersError,
                                methodSymbol,
                                methodSymbol.Name);
                            return default;
                        }

                        var isObservable = Execute.IsObservableReturnType(methodSymbol.ReturnType);

                        token.ThrowIfCancellationRequested();

                        Execute.GatherForwardedAttributesFromMethod(
                            methodSymbol,
                            context.SemanticModel,
                            methodSyntax,
                            token,
                            out var propertyAttributes);

                        token.ThrowIfCancellationRequested();

                        // Get the hierarchy info for the target symbol, and try to gather the property info
                        hierarchy = HierarchyInfo.From(methodSymbol.ContainingType);
                        token.ThrowIfCancellationRequested();

                        propertyInfo = new ObservableMethodInfo(
                        methodSymbol.Name,
                        methodSymbol.ReturnType,
                        methodSymbol.Parameters.FirstOrDefault()?.Type,
                        propertyName ?? (methodSymbol.Name + "Property"),
                        false,
                        propertyAttributes);
                    }

                    if (context.TargetNode is PropertyDeclarationSyntax propertySyntax)
                    {
                        var propertySymbol = (IPropertySymbol)symbol!;
                        var isObservable = Execute.IsObservableReturnType(propertySymbol.Type);

                        token.ThrowIfCancellationRequested();

                        Execute.GatherForwardedAttributesFromProperty(
                            propertySymbol,
                            context.SemanticModel,
                            propertySyntax,
                            token,
                            out var propertyAttributes);

                        token.ThrowIfCancellationRequested();

                        // Get the hierarchy info for the target symbol, and try to gather the property info
                        hierarchy = HierarchyInfo.From(propertySymbol.ContainingType);
                        token.ThrowIfCancellationRequested();

                        propertyInfo = new ObservableMethodInfo(
                        propertySymbol.Name,
                        propertySymbol.Type,
                        propertySymbol.Parameters.FirstOrDefault()?.Type,
                        propertyName ?? (propertySymbol.Name + "Property"),
                        true,
                        propertyAttributes);
                    }

                    token.ThrowIfCancellationRequested();

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
            var compilationUnit = item.Key.GetCompilationUnit(memberDeclarations)
                .WithLeadingTrivia(TriviaList(
                    Comment("using ReactiveUI;"),
                    CarriageReturn,
                    Comment("// <auto-generated/>"),
                    Trivia(PragmaWarningDirectiveTrivia(Token(SyntaxKind.DisableKeyword), true)),
                    Trivia(NullableDirectiveTrivia(Token(SyntaxKind.EnableKeyword), true)),
                    CarriageReturn))
                .NormalizeWhitespace();
            context.AddSource($"{item.Key.FilenameHint}.ObservableAsPropertyFromObservable.g.cs", compilationUnit);
        });
    }
}
