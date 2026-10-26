// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using ReactiveUI.SourceGenerators.Benchmarks.Support;
using ReactiveUI.SourceGenerators.CodeFixers;
using ReactiveUI.SourceGenerators.Diagnostics.Suppressions;

namespace ReactiveUI.SourceGenerators.Benchmarks;

/// <summary>Measures what each analyzer and suppressor the package ships costs a compilation.</summary>
/// <remarks>
/// Each measurement starts a fresh <c>CompilationWithAnalyzers</c>, which is the state a host build starts an analysis
/// with. A suppressor decides on diagnostics other analyzers report, so the suppressor benchmarks run beside a source of
/// the diagnostics they exist to silence.
/// </remarks>
public class AnalyzerBenchmarks
{
    /// <summary>How many times each mock file is repeated, each copy in its own namespace.</summary>
    private const int Copies = 20;

    /// <summary>The analyzer for <c>[Reactive]</c> on members that are not partial.</summary>
    private readonly ReactiveAttributeMisuseAnalyzer _misuseAnalyzer = new();

    /// <summary>The analyzer for <c>[ReactiveCommand]</c> methods the generator skips.</summary>
    private readonly ReactiveCommandAnalyzer _commandAnalyzer = new();

    /// <summary>Reports the diagnostics the suppressors silence.</summary>
    private readonly SuppressibleDiagnosticSource _source = new();

    /// <summary>The mock consumer compilation, built once per parameter set.</summary>
    private Compilation _compilation = null!;

    /// <summary>Gets the corpus names.</summary>
    public static IEnumerable<string> Corpora => ["Reactive", "ReactiveCommand", GeneratorHarness.AllCorpus];

    /// <summary>Gets or sets the corpus: one generator's mock file, or every mock file.</summary>
    [ParamsSource(nameof(Corpora))]
    public string Corpus { get; set; } = GeneratorHarness.AllCorpus;

    /// <summary>Builds the mock consumer compilation once per parameter set.</summary>
    [GlobalSetup]
    public void Setup() => _compilation = GeneratorHarness.BuildCompilation(Corpus, Copies);

    /// <summary>Runs the analyzer for <c>[Reactive]</c> misuse.</summary>
    /// <returns>The number of diagnostics reported.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Benchmark]
    public Task<int> AnalyzeReactiveMisuse() => CountAsync(_misuseAnalyzer);

    /// <summary>Runs the analyzer for <c>[ReactiveCommand]</c> methods the generator skips.</summary>
    /// <returns>The number of diagnostics reported.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Benchmark]
    public Task<int> AnalyzeReactiveCommand() => CountAsync(_commandAnalyzer);

    /// <summary>Runs the suppressor for CS0657 on <c>[Reactive]</c> fields.</summary>
    /// <returns>The number of diagnostics suppressed.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Benchmark]
    public Task<int> SuppressFieldAttributeTargets() => CountSuppressedAsync(new ReactiveAttributeWithFieldTargetDiagnosticSuppressor());

    /// <summary>Runs the suppressor for CS0657 on <c>[Reactive]</c> properties.</summary>
    /// <returns>The number of diagnostics suppressed.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Benchmark]
    public Task<int> SuppressPropertyAttributeTargets() => CountSuppressedAsync(new ReactiveAttributeWithPropertyTargetDiagnosticSuppressor());

    /// <summary>Runs the suppressor for CS0657 on <c>[ReactiveCommand]</c> methods.</summary>
    /// <returns>The number of diagnostics suppressed.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Benchmark]
    public Task<int> SuppressCommandAttributeTargets() => CountSuppressedAsync(new ReactiveCommandAttributeWithFieldOrPropertyTargetDiagnosticSuppressor());

    /// <summary>Runs the suppressor for static-member recommendations on <c>[ReactiveCommand]</c> methods.</summary>
    /// <returns>The number of diagnostics suppressed.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Benchmark]
    public Task<int> SuppressStaticMemberRecommendation() => CountSuppressedAsync(new ReactiveCommandMethodDoesNotNeedToBeStaticDiagnosticSuppressor());

    /// <summary>Runs the suppressor for read-only recommendations on <c>[Reactive]</c> fields.</summary>
    /// <returns>The number of diagnostics suppressed.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Benchmark]
    public Task<int> SuppressReadOnlyRecommendation() => CountSuppressedAsync(new ReactiveFieldDoesNotNeedToBeReadOnlyDiagnosticSuppressor());

    /// <summary>Runs one analyzer over the compilation and counts what it reports.</summary>
    /// <param name="analyzer">The analyzer.</param>
    /// <returns>The number of diagnostics reported.</returns>
    private async Task<int> CountAsync(DiagnosticAnalyzer analyzer)
    {
        var diagnostics = await _compilation.WithAnalyzers([analyzer]).GetAnalyzerDiagnosticsAsync().ConfigureAwait(false);
        return diagnostics.Length;
    }

    /// <summary>Runs a suppressor beside the diagnostic source and counts the diagnostics it suppressed.</summary>
    /// <param name="suppressor">The suppressor.</param>
    /// <returns>The number of diagnostics suppressed.</returns>
    private async Task<int> CountSuppressedAsync(DiagnosticSuppressor suppressor)
    {
        ImmutableArray<DiagnosticAnalyzer> analyzers = [_source, suppressor];
        var diagnostics = await _compilation.WithAnalyzers(analyzers).GetAllDiagnosticsAsync().ConfigureAwait(false);
        var suppressed = 0;
        foreach (var diagnostic in diagnostics)
        {
            if (diagnostic.IsSuppressed)
            {
                suppressed++;
            }
        }

        return suppressed;
    }
}
