// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
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

/// <summary>Reactive Attribute ReadOnly Field Target Diagnostic Suppressor.</summary>
/// <seealso cref="DiagnosticSuppressor" />
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ReactiveFieldDoesNotNeedToBeReadOnlyDiagnosticSuppressor : DiagnosticSuppressor
{
        /// <inheritdoc/>
        public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions => ImmutableArray<SuppressionDescriptor>.Empty.Add(ReactiveFieldsShouldNotBeReadOnly);

        /// <inheritdoc/>
        public override void ReportSuppressions(SuppressionAnalysisContext context)
        {
            foreach (var diagnostic in context.ReportedDiagnostics)
            {
                var syntaxNode = diagnostic.Location.SourceTree?.GetRoot(context.CancellationToken).FindNode(diagnostic.Location.SourceSpan);

                // RCS1169 can report either the complete declaration or a nested variable span.
                var fieldDeclaration = syntaxNode as FieldDeclarationSyntax
                    ?? syntaxNode?.FirstAncestorOrSelf<FieldDeclarationSyntax>();
                if (fieldDeclaration is not null)
                {
                    var semanticModel = context.GetSemanticModel(fieldDeclaration.SyntaxTree);
                    var reactiveSymbol = semanticModel.Compilation.GetTypeByMetadataName(AttributeDefinitions.ReactiveAttributeType);
                    if (reactiveSymbol is null)
                    {
                        continue;
                    }

                    foreach (var variable in fieldDeclaration.Declaration.Variables)
                    {
                        if (semanticModel.GetDeclaredSymbol(variable, context.CancellationToken) is IFieldSymbol fieldSymbol
                            && fieldSymbol.HasAttributeWithType(reactiveSymbol))
                        {
                            context.ReportSuppression(Suppression.Create(ReactiveFieldsShouldNotBeReadOnly, diagnostic));
                            break;
                        }
                    }
                }
            }
        }
}
