// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using ReactiveUI.SourceGenerators.Extensions;
using ReactiveUI.SourceGenerators.Helpers;
using ReactiveUI.SourceGenerators.Input.Models;
using ReactiveUI.SourceGenerators.Models;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

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
            ctx.AddSource($"{RxCmdAttribute}.g.cs", SourceText.From(AttributeDefinitions.ReactiveCommandAttribute, Encoding.UTF8)));

        // Gather info for all annotated command methods (starting from method declarations with at least one attribute)
        IncrementalValuesProvider<(HierarchyInfo Hierarchy, Result<CommandInfo> Info)> commandInfoWithErrors =
            context.SyntaxProvider
            .ForAttributeWithMetadataName(
                RxCmdAttribute,
                static (node, _) => node is MethodDeclarationSyntax { Parent: ClassDeclarationSyntax or RecordDeclarationSyntax, AttributeLists.Count: > 0 },
                static (context, token) =>
                {
                    CommandInfo? commandExtensionInfos = default;
                    HierarchyInfo? hierarchy = default;
                    using var diagnostics = ImmutableArrayBuilder<DiagnosticInfo>.Rent();

                    var methodSyntax = (MethodDeclarationSyntax)context.TargetNode;
                    var symbol = ModelExtensions.GetDeclaredSymbol(context.SemanticModel, methodSyntax, token)!;
                    token.ThrowIfCancellationRequested();

                    // Skip symbols without the target attribute
                    if (!symbol.TryGetAttributeWithFullyQualifiedMetadataName(RxCmdAttribute, out var attributeData))
                    {
                        return default;
                    }

                    token.ThrowIfCancellationRequested();
                    if (attributeData != null)
                    {
                        var compilation = context.SemanticModel.Compilation;
                        var methodSymbol = (IMethodSymbol)symbol!;
                        var isTask = Execute.IsTaskReturnType(methodSymbol.ReturnType);
                        var isObservable = Execute.IsObservableReturnType(methodSymbol.ReturnType);
                        var realReturnType = isTask || isObservable ? Execute.GetTaskReturnType(compilation, methodSymbol.ReturnType) : methodSymbol.ReturnType;
                        var isReturnTypeVoid = SymbolEqualityComparer.Default.Equals(realReturnType, compilation.GetSpecialType(SpecialType.System_Void));
                        var hasCancellationToken = isTask && methodSymbol.Parameters.Any(x => x.Type.ToDisplayString() == "System.Threading.CancellationToken");
                        var methodParameters = new List<IParameterSymbol>();
                        if (hasCancellationToken && methodSymbol.Parameters.Length == 2)
                        {
                            methodParameters.Add(methodSymbol.Parameters[0]);
                        }
                        else if (!hasCancellationToken)
                        {
                            methodParameters.AddRange(methodSymbol.Parameters);
                        }

                        if (methodParameters.Count > 1)
                        {
                            return default; // Too many parameters, continue
                        }

                        token.ThrowIfCancellationRequested();

                        // Get the hierarchy info for the target symbol, and try to gather the command info
                        hierarchy = HierarchyInfo.From(methodSymbol.ContainingType);

                        // Get the CanExecute expression type, if any
                        Execute.TryGetCanExecuteExpressionType(
                            methodSymbol,
                            attributeData,
                            out var canExecuteMemberName,
                            out var canExecuteTypeInfo);

                        token.ThrowIfCancellationRequested();

                        Execute.GatherForwardedAttributes(
                            methodSymbol,
                            context.SemanticModel,
                            methodSyntax,
                            token,
                            out var propertyAttributes);

                        token.ThrowIfCancellationRequested();

                        commandExtensionInfos = new(
                            methodSymbol.Name,
                            realReturnType,
                            methodParameters.SingleOrDefault()?.Type,
                            isTask,
                            isReturnTypeVoid,
                            isObservable,
                            canExecuteMemberName,
                            canExecuteTypeInfo,
                            propertyAttributes);
                    }

                    token.ThrowIfCancellationRequested();
                    return (Hierarchy: hierarchy, new Result<CommandInfo?>(commandExtensionInfos, diagnostics.ToImmutable()));
                })
            .Where(static item => item.Hierarchy is not null)!;

        ////// Output the diagnostics
        ////context.ReportDiagnostics(propertyInfoWithErrors.Select(static (item, _) => item.Info.Errors));

        // Get the filtered sequence to enable caching
        var propertyInfo =
            commandInfoWithErrors
            .Where(static item => item.Info.Value is not null)!;

        // Split and group by containing type
        var groupedPropertyInfo =
            propertyInfo
            .GroupBy(static item => item.Left, static item => item.Right.Value);

        // Generate the requested properties and methods
        context.RegisterSourceOutput(groupedPropertyInfo, static (context, item) =>
        {
            var commandInfos = item.Right.ToArray();

            // Generate all member declarations for the current type
            var propertyDeclarations =
                commandInfos
                .Select(Execute.GetCommandProperty)
                .ToList();

            var c = Execute.GetCommandInitiliser(commandInfos);
            propertyDeclarations.Add(c);
            var memberDeclarations = propertyDeclarations.ToImmutableArray();

            // Insert all members into the same partial type declaration
            var compilationUnit = item.Key.GetCompilationUnit(memberDeclarations)
                .WithLeadingTrivia(TriviaList(
                    Comment("// <auto-generated/>"),
                    Trivia(PragmaWarningDirectiveTrivia(Token(SyntaxKind.DisableKeyword), true)),
                    Trivia(NullableDirectiveTrivia(Token(SyntaxKind.EnableKeyword), true)),
                    CarriageReturn))
                .NormalizeWhitespace();
            context.AddSource($"{item.Key.FilenameHint}.ReactiveCommands.g.cs", compilationUnit);
        });
    }
}
