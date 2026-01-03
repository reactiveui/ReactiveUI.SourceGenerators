// Copyright (c) 2026 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using ReactiveUI.SourceGenerators.Extensions;
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
        var symbol = context.ContainingSymbol;
        if (symbol is not IPropertySymbol propertySymbol)
        {
            return;
        }

        if (context.Node is not PropertyDeclarationSyntax propertyDeclaration)
        {
            return;
        }

        // Make sure the property is part of a class inherited from ReactiveObject.
        // In some test harnesses, ReactiveUI types might not be referenced, so the semantic
        // check can fail even when the syntax clearly indicates intended usage.
        var isTargetTypeValid = propertySymbol.IsTargetTypeValid();

        if (!isTargetTypeValid)
        {
            var containingTypeSyntax = propertyDeclaration.FirstAncestorOrSelf<TypeDeclarationSyntax>();

            var baseTypes = containingTypeSyntax?.BaseList?.Types;
            var hasReactiveBaseOrInterfaceInBaseList = baseTypes?.Any(t =>
                t.Type is IdentifierNameSyntax { Identifier.ValueText: "ReactiveObject" } ||
                t.Type is QualifiedNameSyntax { Right.Identifier.ValueText: "ReactiveObject" } ||
                t.Type is IdentifierNameSyntax { Identifier.ValueText: "IReactiveObject" } ||
                t.Type is QualifiedNameSyntax { Right.Identifier.ValueText: "IReactiveObject" }) == true;

            if (!hasReactiveBaseOrInterfaceInBaseList)
            {
                return;
            }
        }

        // Check if the property is an readonly property
        if (propertySymbol.SetMethod == null)
        {
            return;
        }

        // Check if the property is a ReactiveUI property
        if (propertySymbol.GetAttributes().Any(a => a.AttributeClass?.Name == "ReactiveAttribute" || a.AttributeClass?.Name == "ObservableAsProperty"))
        {
            return;
        }

        // If semantic attributes are not available, fall back to syntax to detect [Reactive].
        if (propertyDeclaration.AttributeLists
            .SelectMany(static a => a.Attributes)
            .Any(a => a.Name is IdentifierNameSyntax { Identifier.ValueText: "Reactive" } ||
                      a.Name is QualifiedNameSyntax { Right.Identifier.ValueText: "Reactive" }))
        {
            return;
        }

        var isAutoProperty = propertyDeclaration.ExpressionBody == null && (propertyDeclaration.AccessorList?.Accessors.All(a => a.Body == null && a.ExpressionBody == null) != false);
        var hasCorrectModifiers = propertyDeclaration.Modifiers.Any(SyntaxKind.PublicKeyword) && !propertyDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword);
        var doesNotHavePrivateSetOrInternalSet = propertyDeclaration.AccessorList?.Accessors.Any(a => a.Modifiers.Any(SyntaxKind.PrivateKeyword) || a.Modifiers.Any(SyntaxKind.InternalKeyword)) == false;
        var namesToIgnore = new List<string> { "ReactiveCommand", "ReactiveProperty", "ViewModelActivator" };
        var isNotIgnored = !namesToIgnore.Any(n => propertyDeclaration.Type.ToString().Contains(n));

        if (isAutoProperty && hasCorrectModifiers && doesNotHavePrivateSetOrInternalSet && isNotIgnored)
        {
            var diagnostic = Diagnostic.Create(PropertyToReactiveFieldRule, propertyDeclaration.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}
