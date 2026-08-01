// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using ReactiveUI.SourceGenerators.Extensions;
using ReactiveUI.SourceGenerators.Helpers;
using static ReactiveUI.SourceGenerators.Diagnostics.SuppressionDescriptors;

namespace ReactiveUI.SourceGenerators.Diagnostics.Suppressions;

/// <summary>ReactiveCommand Attribute With Field Or Property Target Diagnostic Suppressor.</summary>
/// <seealso cref="DiagnosticSuppressor" />
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class OAPHMethodDoesNotNeedToBeStaticDiagnosticSuppressor : DiagnosticSuppressor
{
    /// <inheritdoc/>
    public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions => ImmutableArray<SuppressionDescriptor>.Empty.Add(ReactiveCommandDoesNotAccessInstanceData);

    /// <inheritdoc/>
    public override void ReportSuppressions(SuppressionAnalysisContext context)
    {
        foreach (var diagnostic in context.ReportedDiagnostics)
        {
            var syntaxNode = diagnostic.Location.SourceTree?.GetRoot(context.CancellationToken).FindNode(diagnostic.Location.SourceSpan);
            if (syntaxNode is not (MethodDeclarationSyntax or PropertyDeclarationSyntax))
            {
                continue;
            }

            var semanticModel = context.GetSemanticModel(syntaxNode.SyntaxTree);
            var declaredSymbol = semanticModel.GetDeclaredSymbol(syntaxNode, context.CancellationToken);
            var oaphSymbol = semanticModel.Compilation.GetTypeByMetadataName(AttributeDefinitions.ObservableAsPropertyAttributeType);
            if (oaphSymbol is not null && declaredSymbol?.HasAttributeWithType(oaphSymbol) == true)
            {
                context.ReportSuppression(Suppression.Create(ReactiveCommandDoesNotAccessInstanceData, diagnostic));
            }
        }
    }
}
