// Copyright (c) 2025 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using ReactiveUI.SourceGenerators.Extensions;
using ReactiveUI.SourceGenerators.Helpers;
using static ReactiveUI.SourceGenerators.Diagnostics.SuppressionDescriptors;

namespace ReactiveUI.SourceGenerators.Diagnostics.Suppressions;

/// <summary>
/// ReactiveCommand Attribute With Field Or Property Target Diagnostic Suppressor.
/// </summary>
/// <seealso cref="DiagnosticSuppressor" />
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class OAPHMethodDoesNotNeedToBeStaticDiagnosticSuppressor : DiagnosticSuppressor
{
    /// <inheritdoc/>
    public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions => ImmutableArray.Create(ReactiveCommandDoesNotAccessInstanceData);

    /// <inheritdoc/>
    public override void ReportSuppressions(SuppressionAnalysisContext context)
    {
        foreach (var diagnostic in context.ReportedDiagnostics)
        {
            var syntaxNode = diagnostic.Location.SourceTree?.GetRoot(context.CancellationToken).FindNode(diagnostic.Location.SourceSpan);

            // Check that the target is a method declaration, which is the case we're looking for
            if (syntaxNode is MethodDeclarationSyntax methodDeclaration)
            {
                var semanticModel = context.GetSemanticModel(syntaxNode.SyntaxTree);

                // Get the method symbol from the first variable declaration
                ISymbol? declaredSymbol = semanticModel.GetDeclaredSymbol(methodDeclaration, context.CancellationToken);

                // Check if the method is using [ObservableAsProperty], in which case we should suppress the warning
                if (declaredSymbol is IMethodSymbol methodSymbol &&
                    semanticModel.Compilation.GetTypeByMetadataName(AttributeDefinitions.ObservableAsPropertyAttributeType) is INamedTypeSymbol oaphSymbol &&
                    methodSymbol.HasAttributeWithType(oaphSymbol))
                {
                    context.ReportSuppression(Suppression.Create(ReactiveCommandDoesNotAccessInstanceData, diagnostic));
                }
            }

            // Check that the target is a property declaration, which is the case we're looking for
            if (syntaxNode is PropertyDeclarationSyntax propertyDeclaration)
            {
                var semanticModel = context.GetSemanticModel(syntaxNode.SyntaxTree);

                // Get the method symbol from the first variable declaration
                ISymbol? declaredSymbol = semanticModel.GetDeclaredSymbol(propertyDeclaration, context.CancellationToken);

                // Check if the method is using [ObservableAsProperty], in which case we should suppress the warning
                if (declaredSymbol is IPropertySymbol propertySymbol &&
                    semanticModel.Compilation.GetTypeByMetadataName(AttributeDefinitions.ObservableAsPropertyAttributeType) is INamedTypeSymbol oaphSymbol &&
                    propertySymbol.HasAttributeWithType(oaphSymbol))
                {
                    context.ReportSuppression(Suppression.Create(ReactiveCommandDoesNotAccessInstanceData, diagnostic));
                }
            }
        }
    }
}
