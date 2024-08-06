// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using ReactiveUI.SourceGenerators.Diagnostics;

namespace ReactiveUI.SourceGenerators.CodeAnalyzers
{
    /// <summary>
    /// PropertyToFieldAnalyzer.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class PropertyToReactiveFieldAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// Gets the supported diagnostics.
        /// </summary>
        /// <value>
        /// The supported diagnostics.
        /// </value>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.PropertyToReactiveFieldRule);

        /// <summary>
        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public override void Initialize(AnalysisContext context)
        {
            context?.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context?.EnableConcurrentExecution();
            context?.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.PropertyDeclaration);
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
            {
            var propertyDeclaration = (PropertyDeclarationSyntax)context.Node;
            var isAutoProperty = propertyDeclaration.ExpressionBody == null && (propertyDeclaration.AccessorList?.Accessors.All(a => a.Body == null && a.ExpressionBody == null) != false);

            if (isAutoProperty && propertyDeclaration.Modifiers.Any(SyntaxKind.PublicKeyword) && !propertyDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword))
            {
                var diagnostic = Diagnostic.Create(DiagnosticDescriptors.PropertyToReactiveFieldRule, propertyDeclaration.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
