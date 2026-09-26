// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using ReactiveUI.SourceGenerators.Helpers;
using static ReactiveUI.SourceGenerators.CodeFixers.Diagnostics.DiagnosticDescriptors;

namespace ReactiveUI.SourceGenerators.CodeFixers;

/// <summary>Reports WinForms hosts generated without ReactiveUI.Binding's <c>ObservedProperty</c>.</summary>
/// <remarks>The rule is the host generators' own, from the <see cref="ViewApiRules"/> both compile.</remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ControlHostAnalyzer : DiagnosticAnalyzer
{
    /// <summary>Gets the diagnostics this analyzer reports.</summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(ControlHostWithoutObservedPropertyRule);

    /// <summary>Registers the host analysis for compilations without <c>ObservedProperty</c>.</summary>
    /// <param name="context">The analysis context.</param>
    public override void Initialize(AnalysisContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(static startContext =>
        {
            if (ViewApiRules.HasObservedProperty(startContext.Compilation))
            {
                return;
            }

            startContext.RegisterSymbolAction(static symbolContext => AnalyzeType(in symbolContext), SymbolKind.NamedType);
        });
    }

    /// <summary>Reports a type that is a WinForms host.</summary>
    /// <param name="context">The symbol analysis context.</param>
    private static void AnalyzeType(in SymbolAnalysisContext context)
    {
        foreach (var attribute in context.Symbol.GetAttributes())
        {
            if (attribute.AttributeClass?.ToDisplayString() is "ReactiveUI.SourceGenerators.WinForms.RoutedControlHostAttribute"
                or "ReactiveUI.SourceGenerators.WinForms.ViewModelControlHostAttribute")
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    ControlHostWithoutObservedPropertyRule,
                    context.Symbol.Locations[0],
                    context.Symbol.Name,
                    ViewApiRules.ObservedPropertyMinimumBindingVersion));
                return;
            }
        }
    }
}
