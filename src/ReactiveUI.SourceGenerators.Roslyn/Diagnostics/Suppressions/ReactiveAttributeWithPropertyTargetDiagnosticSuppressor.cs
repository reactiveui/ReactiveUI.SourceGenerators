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

namespace ReactiveUI.SourceGenerators.Diagnostics.Suppressions
{
    /// <summary>
    /// ReactiveCommand Attribute With Field Or Property Target Diagnostic Suppressor.
    /// </summary>
    /// <seealso cref="DiagnosticSuppressor" />
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ReactiveAttributeWithPropertyTargetDiagnosticSuppressor : DiagnosticSuppressor
    {
        /// <inheritdoc/>
        public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions => ImmutableArray.Create(FieldOrPropertyAttributeListForReactiveProperty);

        /// <inheritdoc/>
        public override void ReportSuppressions(SuppressionAnalysisContext context)
        {
            foreach (var diagnostic in context.ReportedDiagnostics)
            {
                var syntaxNode = diagnostic.Location.SourceTree?.GetRoot(context.CancellationToken).FindNode(diagnostic.Location.SourceSpan);

                // Check that the target is effectively [property:] over a method declaration, which is the case we're looking for
                if (syntaxNode is AttributeTargetSpecifierSyntax { Parent.Parent: FieldDeclarationSyntax, Identifier: SyntaxToken(SyntaxKind.PropertyKeyword) })
                {
                    var semanticModel = context.GetSemanticModel(syntaxNode.SyntaxTree);

                    // Check if the method is using [Reactive], in which case we should suppress the warning
                    if (semanticModel.Compilation.GetTypeByMetadataName(AttributeDefinitions.ReactiveAttributeType) is not null)
                    {
                        context.ReportSuppression(Suppression.Create(FieldOrPropertyAttributeListForReactiveProperty, diagnostic));
                    }
                }
            }
        }
    }
}
