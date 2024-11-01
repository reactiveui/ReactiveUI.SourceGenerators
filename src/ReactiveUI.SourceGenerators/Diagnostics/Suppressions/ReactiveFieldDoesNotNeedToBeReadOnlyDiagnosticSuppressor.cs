// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using ReactiveUI.SourceGenerators.Extensions;
using ReactiveUI.SourceGenerators.Helpers;
using static ReactiveUI.SourceGenerators.Diagnostics.SuppressionDescriptors;

namespace ReactiveUI.SourceGenerators.Diagnostics.Suppressions
{
    /// <summary>
    /// Reactive Attribute ReadOnly Field Target Diagnostic Suppressor.
    /// </summary>
    /// <seealso cref="DiagnosticSuppressor" />
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ReactiveFieldDoesNotNeedToBeReadOnlyDiagnosticSuppressor : DiagnosticSuppressor
    {
        /// <inheritdoc/>
        public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions => ImmutableArray.Create(ReactiveFieldsShouldNotBeReadOnly);

        /// <inheritdoc/>
        public override void ReportSuppressions(SuppressionAnalysisContext context)
        {
            foreach (var diagnostic in context.ReportedDiagnostics)
            {
                var syntaxNode = diagnostic.Location.SourceTree?.GetRoot(context.CancellationToken).FindNode(diagnostic.Location.SourceSpan);

                // Check that the target is a method declaration, which is the case we're looking for
                if (syntaxNode is FieldDeclarationSyntax fieldDeclaration)
                {
                    var semanticModel = context.GetSemanticModel(syntaxNode.SyntaxTree);

                    // Get the method symbol from the first variable declaration
                    var declaredSymbol = semanticModel.GetDeclaredSymbol(fieldDeclaration, context.CancellationToken);

                    // Check if the method is using [Reactive], in which case we should suppress the warning
                    if (declaredSymbol is IFieldSymbol fieldSymbol &&
                        semanticModel.Compilation.GetTypeByMetadataName("ReactiveUI.SourceGenerators.ReactiveAttribute") is INamedTypeSymbol reactiveSymbol &&
                        fieldSymbol.HasAttributeWithType(reactiveSymbol))
                    {
                        context.ReportSuppression(Suppression.Create(ReactiveFieldsShouldNotBeReadOnly, diagnostic));
                    }
                }
            }
        }
    }
}
