// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ReactiveUI.SourceGenerators.Benchmarks.Support;

/// <summary>
/// Reports the diagnostics the package's suppressors exist to silence, on every field and method, so a benchmark has
/// something for them to decide on without referencing a third-party analyzer.
/// </summary>
public sealed class SuppressibleDiagnosticSource : DiagnosticAnalyzer
{
    /// <summary>The readonly-field recommendation the field suppressor silences on <c>[Reactive]</c> fields.</summary>
    private static readonly DiagnosticDescriptor ReadOnlyFieldRecommendation = new(
        "RCS1169",
        "Make field read-only",
        "Make field read-only",
        "Roslynator",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    /// <summary>The static-member recommendation the command suppressor silences on <c>[ReactiveCommand]</c> methods.</summary>
    private static readonly DiagnosticDescriptor StaticMemberRecommendation = new(
        "CA1822",
        "Mark members as static",
        "Mark members as static",
        "Performance",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [ReadOnlyFieldRecommendation, StaticMemberRecommendation];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSymbolAction(static symbolContext => Report(in symbolContext), SymbolKind.Field, SymbolKind.Method);
    }

    /// <summary>Reports the recommendation that fits a field or a method.</summary>
    /// <param name="context">The symbol analysis context.</param>
    private static void Report(in SymbolAnalysisContext context)
    {
        var descriptor = context.Symbol.Kind == SymbolKind.Field ? ReadOnlyFieldRecommendation : StaticMemberRecommendation;
        foreach (var location in context.Symbol.Locations)
        {
            if (location.IsInSource)
            {
                context.ReportDiagnostic(Diagnostic.Create(descriptor, location));
            }
        }
    }
}
