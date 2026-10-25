// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

extern alias analyzer;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

/// <summary>Runs the analyzers that report a binding call over what the generator produced, as a build does.</summary>
internal static class CallSiteDiagnostics
{
    /// <summary>The method every scenario runs through.</summary>
    private const string ScenarioMethodName = "Run";

    /// <summary>Runs every analyzer that reports a binding call, and keeps the errors.</summary>
    /// <param name="result">The generator result.</param>
    /// <returns>The ids of the errors reported.</returns>
    internal static async Task<ImmutableArray<string>> ErrorsAsync(GeneratorTestResult result)
    {
        var diagnostics = await result.OutputCompilation
            .WithAnalyzers(
            [
                new analyzer::ReactiveUI.Binding.Analyzer.Analyzers.BindingInvocationAnalyzer(),
                new analyzer::ReactiveUI.Binding.Analyzer.Analyzers.UnreachableTypeAnalyzer(),
                new analyzer::ReactiveUI.Binding.Analyzer.Analyzers.NoGeneratedBindingAnalyzer(),
                new analyzer::ReactiveUI.Binding.Analyzer.Analyzers.ToPropertyAnalyzer(),
            ])
            .GetAnalyzerDiagnosticsAsync();

        return [.. diagnostics.Where(static d => d.Severity == DiagnosticSeverity.Error).Select(static d => d.Id)];
    }

    /// <summary>Runs a scenario's <c>Run</c> method and returns what it recorded.</summary>
    /// <param name="result">The generator result, which has to compile.</param>
    /// <returns>The values the scenario returned.</returns>
    internal static int[] RunScenario(GeneratorTestResult result)
    {
        var (assembly, context) = TestHelper.EmitAndLoad(result);
        try
        {
            return (int[])assembly.GetType(UnnameableTypeScenarios.ScenarioTypeName)!.GetMethod(ScenarioMethodName)!.Invoke(null, null)!;
        }
        finally
        {
            context.Unload();
        }
    }
}
