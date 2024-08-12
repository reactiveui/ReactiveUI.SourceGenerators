﻿// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using ReactiveUI.SourceGenerators.Extensions;
using static ReactiveUI.SourceGenerators.Diagnostics.SuppressionDescriptors;

namespace ReactiveUI.SourceGenerators.Diagnostics.Suppressions
{
    /// <summary>
    /// ObservableAsProperty Attribute With Field Never Read Diagnostic Suppressor.
    /// </summary>
    /// <seealso cref="DiagnosticSuppressor" />
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ObservableAsPropertyAttributeWithFieldNeverReadDiagnosticSuppressor : DiagnosticSuppressor
    {
        /// <inheritdoc/>
        public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions => ImmutableArray.Create(FieldIsUsedToGenerateAObservableAsPropertyHelper);

        /// <inheritdoc/>
        public override void ReportSuppressions(SuppressionAnalysisContext context)
        {
            foreach (var diagnostic in context.ReportedDiagnostics)
            {
                var syntaxNode = diagnostic.Location.SourceTree?.GetRoot(context.CancellationToken).FindNode(diagnostic.Location.SourceSpan);

                // Check that the target is effectively [field:] or [property:] over a method declaration, which is the case we're looking for
                if (syntaxNode is AttributeTargetSpecifierSyntax { Parent.Parent: MethodDeclarationSyntax methodDeclaration, Identifier: SyntaxToken(SyntaxKind.FieldKeyword or SyntaxKind.PropertyKeyword) })
                {
                    var semanticModel = context.GetSemanticModel(syntaxNode.SyntaxTree);

                    // Get the method symbol from the first variable declaration
                    ISymbol? declaredSymbol = semanticModel.GetDeclaredSymbol(methodDeclaration, context.CancellationToken);

                    // Check if the method is using [ObservableAsProperty], in which case we should suppress the warning
                    if (declaredSymbol is IMethodSymbol methodSymbol &&
                        semanticModel.Compilation.GetTypeByMetadataName("ReactiveUI.SourceGenerators.ObservableAsPropertyAttribute") is INamedTypeSymbol reactiveCommandSymbol &&
                        methodSymbol.HasAttributeWithType(reactiveCommandSymbol))
                    {
                        context.ReportSuppression(Suppression.Create(FieldIsUsedToGenerateAObservableAsPropertyHelper, diagnostic));
                    }
                }
            }
        }
    }
}
