// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ReactiveUI.Binding.Analyzer.Benchmarks.Support;

/// <summary>Builds the compilation an analyzer runs over and runs it the way a host build does.</summary>
internal static class AnalyzerHarness
{
    /// <summary>The assembly name given to the compilation the analyzers run against.</summary>
    private const string CompilationAssemblyName = "Corpus";

    /// <summary>Builds a compilation over the corpus, with every diagnostic the analyzers support switched on.</summary>
    /// <param name="sources">The source text of each file.</param>
    /// <param name="analyzers">The analyzers whose diagnostics are forced on, so an opt-in descriptor cannot hide its cost.</param>
    /// <returns>The compilation.</returns>
    internal static CSharpCompilation BuildCompilation(string[] sources, DiagnosticAnalyzer[] analyzers)
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.Latest);
        var syntaxTrees = new SyntaxTree[sources.Length];
        for (var i = 0; i < sources.Length; i++)
        {
            syntaxTrees[i] = CSharpSyntaxTree.ParseText(sources[i], parseOptions);
        }

#if NET11_0_OR_GREATER
        var references = new List<MetadataReference>(Basic.Reference.Assemblies.Net110.References.All)
#else
        var references = new List<MetadataReference>(Basic.Reference.Assemblies.Net100.References.All)
#endif
        {
            MetadataReference.CreateFromFile(typeof(ReactiveUIBindingExtensions).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ReactiveUI.Primitives.Concurrency.ISequencer).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Splat.IEnableLogger).Assembly.Location),
        };

        var severities = ImmutableDictionary.CreateBuilder<string, ReportDiagnostic>();
        for (var i = 0; i < analyzers.Length; i++)
        {
            var supported = analyzers[i].SupportedDiagnostics;
            for (var j = 0; j < supported.Length; j++)
            {
                severities[supported[j].Id] = ReportDiagnostic.Warn;
            }
        }

        var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            .WithNullableContextOptions(NullableContextOptions.Enable)
            .WithSpecificDiagnosticOptions(severities.ToImmutable());

        return CSharpCompilation.Create(CompilationAssemblyName, syntaxTrees, references, options);
    }

    /// <summary>Counts the compiler errors in a compilation, which a corpus that does not bind would hide behind an empty result.</summary>
    /// <param name="compilation">The compilation to check.</param>
    /// <returns>The number of errors.</returns>
    internal static int CountCompilerErrors(Compilation compilation)
    {
        var count = 0;
        var diagnostics = compilation.GetDiagnostics();
        for (var i = 0; i < diagnostics.Length; i++)
        {
            if (diagnostics[i].Severity == DiagnosticSeverity.Error)
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>Runs one analyzer over a compilation with a fresh analysis state.</summary>
    /// <param name="compilation">The compilation to analyze.</param>
    /// <param name="analyzer">The analyzer to run.</param>
    /// <returns>The number of diagnostics the analyzer reported, returned so the work cannot be optimized away.</returns>
    internal static async Task<int> AnalyzeAsync(Compilation compilation, DiagnosticAnalyzer analyzer)
    {
        var analyzed = compilation.WithAnalyzers([analyzer]);
        var diagnostics = await analyzed.GetAnalyzerDiagnosticsAsync().ConfigureAwait(false);

        return diagnostics.Length;
    }
}
