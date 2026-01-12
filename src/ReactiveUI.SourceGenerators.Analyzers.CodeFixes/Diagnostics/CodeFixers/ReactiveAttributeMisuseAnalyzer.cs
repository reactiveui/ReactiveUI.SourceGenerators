// Copyright (c) 2026 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using static ReactiveUI.SourceGenerators.CodeFixers.Diagnostics.DiagnosticDescriptors;

namespace ReactiveUI.SourceGenerators.CodeFixers;

/// <summary>
/// ReactiveAttributeMisuseAnalyzer.
/// </summary>
/// <seealso cref="Microsoft.CodeAnalysis.Diagnostics.DiagnosticAnalyzer" />
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ReactiveAttributeMisuseAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// Gets a set of descriptors for the diagnostics that this analyzer is capable of producing.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(ReactiveAttributeRequiresPartialRule);

    /// <summary>
    /// Called once at session start to register actions in the analysis context.
    /// </summary>
    /// <param name="context">The analysis context.</param>
    /// <exception cref="ArgumentNullException">context.</exception>
    public override void Initialize(AnalysisContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeProperty, SyntaxKind.PropertyDeclaration);
    }

    private static void AnalyzeProperty(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not PropertyDeclarationSyntax property)
        {
            return;
        }

        // Only care about scenarios where the user explicitly used `[Reactive]`.
        if (!HasReactiveAttribute(property.AttributeLists))
        {
            return;
        }

        // Generation requires: property is `partial` and containing type is `partial`.
        var propertyIsPartial = property.Modifiers.Any(SyntaxKind.PartialKeyword);

        var containingType = property.FirstAncestorOrSelf<TypeDeclarationSyntax>();
        var containingTypeIsPartial = containingType?.Modifiers.Any(SyntaxKind.PartialKeyword) == true;

        if (propertyIsPartial && containingTypeIsPartial)
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(ReactiveAttributeRequiresPartialRule, property.Identifier.GetLocation()));
    }

    private static bool HasReactiveAttribute(SyntaxList<AttributeListSyntax> attributeLists)
    {
        foreach (var list in attributeLists)
        {
            foreach (var attr in list.Attributes)
            {
                var name = attr.Name.ToString();
                if (name.EndsWith("Reactive", StringComparison.Ordinal) || name.EndsWith("ReactiveAttribute", StringComparison.Ordinal))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
