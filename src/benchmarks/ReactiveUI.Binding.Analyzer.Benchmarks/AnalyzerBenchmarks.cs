// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Loggers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using ReactiveUI.Binding.Analyzer.Analyzers;
using ReactiveUI.Binding.Analyzer.Benchmarks.Support;

namespace ReactiveUI.Binding.Analyzer.Benchmarks;

/// <summary>Measures what each shipped analyzer costs a compilation, on the paths that separate fixed cost from per-node cost.</summary>
/// <remarks>
/// <c>Startup</c> is the cost every compilation pays before an analyzer has anything to decide. <c>Clean</c>
/// minus <c>Startup</c> is the per-node cost of deciding there is nothing to report, and the
/// <c>CleanNullForgiving</c> pair isolates what reading a path through the null-forgiving operator adds.
/// <c>Violating</c> adds the cost of reporting. <c>UnsafeTargets</c> reaches the checks through the
/// overloads whose first type argument is not the observed object. Each measurement starts a fresh
/// <c>CompilationWithAnalyzers</c>, which is the state a host build starts an analysis with. Every analyzer the
/// package ships has its own benchmark, so a change to one shows up against its own baseline.
/// </remarks>
public class AnalyzerBenchmarks
{
    /// <summary>The position of the invocation analyzer in the analyzer and compilation arrays.</summary>
    private const int InvocationIndex = 0;

    /// <summary>The position of the dispatch reach analyzer.</summary>
    private const int DispatchReachIndex = 1;

    /// <summary>The position of the mixin shadow analyzer.</summary>
    private const int MixinShadowIndex = 2;

    /// <summary>The position of the analyzer for calls nothing was generated for.</summary>
    private const int NoGeneratedBindingIndex = 3;

    /// <summary>The position of the <c>[ObservableAsProperty]</c> analyzer.</summary>
    private const int ObservableAsPropertyIndex = 4;

    /// <summary>The position of the <c>ToProperty</c> analyzer.</summary>
    private const int ToPropertyIndex = 5;

    /// <summary>The position of the <c>ToProperty</c> initial value analyzer.</summary>
    private const int ToPropertyInitialValueIndex = 6;

    /// <summary>The position of the unreachable type analyzer.</summary>
    private const int UnreachableTypeIndex = 7;

    /// <summary>The position of the view thread invoker analyzer.</summary>
    private const int ViewThreadInvokerIndex = 8;

    /// <summary>The analyzer for path checks, private members and the observed-path notification check.</summary>
    private readonly BindingInvocationAnalyzer _invocationAnalyzer = new();

    /// <summary>The analyzer for dispatch overloads out of reach of their file.</summary>
    private readonly DispatchReachAnalyzer _dispatchReachAnalyzer = new();

    /// <summary>The analyzer for calls that bind to ReactiveUI's own mixin.</summary>
    private readonly MixinShadowAnalyzer _mixinShadowAnalyzer = new();

    /// <summary>The analyzer for calls nothing was generated for.</summary>
    private readonly NoGeneratedBindingAnalyzer _noGeneratedBindingAnalyzer = new();

    /// <summary>The analyzer for <c>[ObservableAsProperty]</c> targets.</summary>
    private readonly ObservableAsPropertyAnalyzer _observableAsPropertyAnalyzer = new();

    /// <summary>The analyzer for <c>ToProperty</c> sources and names.</summary>
    private readonly ToPropertyAnalyzer _toPropertyAnalyzer = new();

    /// <summary>The analyzer for <c>ToProperty</c> initial values below C# 13.</summary>
    private readonly ToPropertyInitialValueAnalyzer _toPropertyInitialValueAnalyzer = new();

    /// <summary>The analyzer for calls that name a type generated code cannot reach.</summary>
    private readonly UnreachableTypeAnalyzer _unreachableTypeAnalyzer = new();

    /// <summary>The analyzer for bindings that write to a UI object without its platform package.</summary>
    private readonly ViewThreadInvokerAnalyzer _viewThreadInvokerAnalyzer = new();

    /// <summary>Every analyzer, in the order the benchmarks below run them.</summary>
    private DiagnosticAnalyzer[] _all = null!;

    /// <summary>One compilation per analyzer, in the same order as <see cref="_all"/>.</summary>
    private Compilation[] _compilations = null!;

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
        _all =
        [
            _invocationAnalyzer,
            _dispatchReachAnalyzer,
            _mixinShadowAnalyzer,
            _noGeneratedBindingAnalyzer,
            _observableAsPropertyAnalyzer,
            _toPropertyAnalyzer,
            _toPropertyInitialValueAnalyzer,
            _unreachableTypeAnalyzer,
            _viewThreadInvokerAnalyzer,
        ];

        // The measured compilations stay untouched by validation, which would otherwise leave its bound
        // trees behind for the first measurement to find.
        await ValidateAsync(AnalyzerHarness.BuildCompilation(sources, _all), sources.Length).ConfigureAwait(false);

        _compilations = new Compilation[_all.Length];
        for (var i = 0; i < _all.Length; i++)
        {
            _compilations[i] = AnalyzerHarness.BuildCompilation(sources, [_all[i]]);
        }
    }

    /// <summary>Runs the invocation analyzer: private members, unsupported segments and silent path links.</summary>
    /// <returns>The number of diagnostics reported.</returns>
    [Benchmark]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<int> AnalyzeInvocations() => Run(InvocationIndex);

    /// <summary>Runs the dispatch reach analyzer.</summary>
    /// <returns>The number of diagnostics reported.</returns>
    [Benchmark]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<int> AnalyzeDispatchReach() => Run(DispatchReachIndex);

    /// <summary>Runs the mixin shadow analyzer.</summary>
    /// <returns>The number of diagnostics reported.</returns>
    [Benchmark]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<int> AnalyzeMixinShadow() => Run(MixinShadowIndex);

    /// <summary>Runs the analyzer that reports calls nothing was generated for.</summary>
    /// <returns>The number of diagnostics reported.</returns>
    [Benchmark]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<int> AnalyzeNoGeneratedBinding() => Run(NoGeneratedBindingIndex);

    /// <summary>Runs the <c>[ObservableAsProperty]</c> analyzer.</summary>
    /// <returns>The number of diagnostics reported.</returns>
    [Benchmark]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<int> AnalyzeObservableAsProperty() => Run(ObservableAsPropertyIndex);

    /// <summary>Runs the <c>ToProperty</c> analyzer.</summary>
    /// <returns>The number of diagnostics reported.</returns>
    [Benchmark]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<int> AnalyzeToProperty() => Run(ToPropertyIndex);

    /// <summary>Runs the <c>ToProperty</c> initial value analyzer.</summary>
    /// <returns>The number of diagnostics reported.</returns>
    [Benchmark]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<int> AnalyzeToPropertyInitialValue() => Run(ToPropertyInitialValueIndex);

    /// <summary>Runs the analyzer for calls that name a type generated code cannot reach.</summary>
    /// <returns>The number of diagnostics reported.</returns>
    [Benchmark]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<int> AnalyzeUnreachableTypes() => Run(UnreachableTypeIndex);

    /// <summary>Runs the analyzer for bindings that write to a UI object without its platform package.</summary>
    /// <returns>The number of diagnostics reported.</returns>
    [Benchmark]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<int> AnalyzeViewThreadInvokers() => Run(ViewThreadInvokerIndex);

    /// <summary>Runs the analyzer at an index over its own compilation.</summary>
    /// <param name="index">The index into the analyzer and compilation arrays.</param>
    /// <returns>The number of diagnostics reported.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Task<int> Run(int index) => AnalyzerHarness.AnalyzeAsync(_compilations[index], _all[index]);

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

        var total = 0;
        for (var i = 0; i < _all.Length; i++)
        {
            total += await AnalyzerHarness.AnalyzeAsync(compilation, _all[i]).ConfigureAwait(false);
        }

        ConsoleLogger.Default.WriteLine(
            LogKind.Info,
            $"Corpus {Corpus} ({fileCount} files): {total} diagnostics across {_all.Length} analyzers.");

        // The null-forgiving and unsafe corpora report different amounts before and after a change to how
        // paths are read, so only the plain ones are held to an exact answer.
        var expectsNothing = Corpus is AnalyzerCorpus.Startup or AnalyzerCorpus.Clean or AnalyzerCorpus.CleanNullForgiving;
        if (expectsNothing && total != 0)
        {
            throw new InvalidOperationException($"The '{Corpus}' corpus is clean but reports {total} diagnostics.");
        }

        if (Corpus == AnalyzerCorpus.Violating && total == 0)
        {
            throw new InvalidOperationException($"The '{Corpus}' corpus violates the rules but reports nothing.");
        }
    }
}
