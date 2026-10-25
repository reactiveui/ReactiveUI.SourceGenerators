// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Loggers;
using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.Analyzer.Analyzers;
using ReactiveUI.Binding.Analyzer.Benchmarks.Support;

namespace ReactiveUI.Binding.Analyzer.Benchmarks;

/// <summary>Measures what each analyzer costs a compilation, on the three paths that separate fixed cost from per-node cost.</summary>
/// <remarks>
/// <c>Startup</c> is the cost every compilation pays before an analyzer has anything to decide. <c>Clean</c>
/// minus <c>Startup</c> is the per-node cost of deciding there is nothing to report, and the
/// <c>CleanNullForgiving</c> pair isolates what reading a path through the null-forgiving operator adds.
/// <c>Violating</c> adds the cost of reporting. <c>UnsafeTargets</c> reaches the type check through the
/// overloads whose first type argument is not the observed object. Each measurement starts a fresh
/// <c>CompilationWithAnalyzers</c>, which is the state a host build starts an analysis with.
/// </remarks>
public class AnalyzerBenchmarks
{
    /// <summary>The analyzer for path checks, private members and the observed-path notification check.</summary>
    private readonly BindingInvocationAnalyzer _invocationAnalyzer = new();

    /// <summary>The analyzer for the type-level notification check.</summary>
    private readonly TypeAnalyzer _typeAnalyzer = new();

    /// <summary>The compilation the invocation analyzer is measured over, built once per corpus.</summary>
    private Compilation _invocationCompilation = null!;

    /// <summary>The compilation the type analyzer is measured over, built once per corpus.</summary>
    private Compilation _typeCompilation = null!;

    /// <summary>Gets or sets the corpus the analyzers run over.</summary>
    [Params(
        AnalyzerCorpus.Startup,
        AnalyzerCorpus.Clean,
        AnalyzerCorpus.CleanNullForgiving,
        AnalyzerCorpus.Violating,
        AnalyzerCorpus.ViolatingNullForgiving,
        AnalyzerCorpus.UnsafeTargets)]
    public string Corpus { get; set; } = AnalyzerCorpus.Startup;

    /// <summary>
    /// Builds the corpus compilations once per parameter set and checks the corpus does what its name says.
    /// Loading a framework's worth of metadata references costs far more than an analysis, so measuring it per
    /// iteration would bury what this benchmark is for.
    /// </summary>
    /// <returns>A task representing the asynchronous setup.</returns>
    /// <exception cref="InvalidOperationException">The corpus does not bind, or a clean corpus reports, or a violating one does not.</exception>
    [GlobalSetup]
    public async Task Setup()
    {
        var sources = AnalyzerCorpus.Build(Corpus);

        // The measured compilations stay untouched by validation, which would otherwise leave its bound
        // trees behind for the first measurement to find.
        await ValidateAsync(AnalyzerHarness.BuildCompilation(sources, [_invocationAnalyzer, _typeAnalyzer]), sources.Length).ConfigureAwait(false);

        _invocationCompilation = AnalyzerHarness.BuildCompilation(sources, [_invocationAnalyzer]);
        _typeCompilation = AnalyzerHarness.BuildCompilation(sources, [_typeAnalyzer]);
    }

    /// <summary>Runs the invocation analyzer: private members, unsupported segments and silent path links.</summary>
    /// <returns>The number of diagnostics reported.</returns>
    [Benchmark]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<int> AnalyzeInvocations() => AnalyzerHarness.AnalyzeAsync(_invocationCompilation, _invocationAnalyzer);

    /// <summary>Runs the type analyzer, which decides which type argument of each call names the observed object.</summary>
    /// <returns>The number of diagnostics reported.</returns>
    [Benchmark]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<int> AnalyzeTypes() => AnalyzerHarness.AnalyzeAsync(_typeCompilation, _typeAnalyzer);

    /// <summary>Checks the corpus binds, and that a clean one reports nothing and a violating one reports something.</summary>
    /// <param name="compilation">A compilation over the corpus that is not measured.</param>
    /// <param name="fileCount">How many files the corpus holds.</param>
    /// <returns>A task representing the asynchronous validation.</returns>
    /// <exception cref="InvalidOperationException">The corpus does not do what its name says.</exception>
    private async Task ValidateAsync(Compilation compilation, int fileCount)
    {
        var errors = AnalyzerHarness.CountCompilerErrors(compilation);
        if (errors != 0)
        {
            throw new InvalidOperationException($"The '{Corpus}' corpus has {errors} compiler errors, so the analyzers measure calls that did not bind.");
        }

        var invocations = await AnalyzerHarness.AnalyzeAsync(compilation, _invocationAnalyzer).ConfigureAwait(false);
        var types = await AnalyzerHarness.AnalyzeAsync(compilation, _typeAnalyzer).ConfigureAwait(false);
        ConsoleLogger.Default.WriteLine(
            LogKind.Info,
            $"Corpus {Corpus} ({fileCount} files): {invocations} invocation diagnostics, {types} type diagnostics.");

        // The null-forgiving and unsafe corpora report different amounts before and after a change to how
        // paths are read, so only the plain ones are held to an exact answer.
        var expectsNothing = Corpus is AnalyzerCorpus.Startup or AnalyzerCorpus.Clean or AnalyzerCorpus.CleanNullForgiving;
        if (expectsNothing && (invocations != 0 || types != 0))
        {
            throw new InvalidOperationException($"The '{Corpus}' corpus is clean but reports {invocations} invocation and {types} type diagnostics.");
        }

        if (Corpus == AnalyzerCorpus.Violating && (invocations == 0 || types == 0))
        {
            throw new InvalidOperationException($"The '{Corpus}' corpus violates the rules but reports {invocations} invocation and {types} type diagnostics.");
        }
    }
}
