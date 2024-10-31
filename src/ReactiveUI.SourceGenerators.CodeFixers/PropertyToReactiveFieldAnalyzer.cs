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
using static ReactiveUI.SourceGenerators.CodeFixers.Diagnostics.DiagnosticDescriptors;

namespace ReactiveUI.SourceGenerators.CodeFixers;

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
        ImmutableArray.Create(PropertyToReactiveFieldRule);

    /// <summary>
    /// Initializes the specified context.
    /// </summary>
    /// <param name="context">The context.</param>
    public override void Initialize(AnalysisContext context)
    {
        if (context is null)
        {
            throw new System.ArgumentNullException(nameof(context));
        }

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.PropertyDeclaration);
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not PropertyDeclarationSyntax propertyDeclaration)
        {
            return;
        }

        var isAutoProperty = propertyDeclaration.ExpressionBody == null && (propertyDeclaration.AccessorList?.Accessors.All(a => a.Body == null && a.ExpressionBody == null) != false);
        var hasCorrectModifiers = propertyDeclaration.Modifiers.Any(SyntaxKind.PublicKeyword) && !propertyDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword);
        var doesNotHavePrivateSetOrInternalSet = propertyDeclaration.AccessorList?.Accessors.Any(a => a.Modifiers.Any(SyntaxKind.PrivateKeyword) || a.Modifiers.Any(SyntaxKind.InternalKeyword)) == false;
        var isNotReactiveCommand = !propertyDeclaration.Type.ToString().Contains("ReactiveCommand");
        var isNotReactiveProperty = !propertyDeclaration.Type.ToString().Contains("ReactiveProperty");

        if (isAutoProperty && hasCorrectModifiers && doesNotHavePrivateSetOrInternalSet && isNotReactiveCommand && isNotReactiveProperty)
        {
            var diagnostic = Diagnostic.Create(PropertyToReactiveFieldRule, propertyDeclaration.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}
