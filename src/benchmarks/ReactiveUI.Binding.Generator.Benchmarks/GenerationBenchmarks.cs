// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Attributes;
using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.Generator.Benchmarks.Support;

namespace ReactiveUI.Binding.Generator.Benchmarks;

/// <summary>Measures a full generation pass over the mock consumer source.</summary>
/// <remarks>
/// The driver is rebuilt per iteration so each measurement is a cold generation, which is what a consumer's
/// build actually pays. Reusing a primed driver would measure the incremental cache instead, and hoisting the
/// driver into setup would let one iteration's caches serve the next.
/// </remarks>
public class GenerationBenchmarks
{
    /// <summary>The mock consumer compilation, built once per parameter set.</summary>
    private Compilation _compilation = null!;

    /// <summary>
    /// Gets or sets a value indicating whether the build lists the generated namespace, which is what decides
    /// between claiming each call site outright and offering an overload that competes for them all.
    /// </summary>
    [Params(false, true)]
    public bool Intercept { get; set; }

    /// <summary>
    /// Builds the mock consumer compilation once per parameter set. Loading a framework's worth of metadata
    /// references costs far more than a generation pass and is work the host build does once, so measuring it
    /// per iteration would bury what this benchmark is for.
    /// </summary>
    [GlobalSetup]
    public void Setup() => _compilation = GeneratorHarness.BuildCompilation(Intercept);

    /// <summary>Runs a whole cold generation: syntax scan, extraction, and emission.</summary>
    /// <returns>The number of generated characters, returned so the work cannot be optimized away.</returns>
    /// <exception cref="InvalidOperationException">The mock consumer source generated nothing, so there is no result to report.</exception>
    [Benchmark]
    public int Generate()
    {
        // A fresh driver per iteration: a reused one would serve the next iteration from its caches and
        // measure the incremental path rather than the cold generation a consumer's build pays for.
        var driver = GeneratorHarness.CreateDriver(Intercept);
        var result = driver.RunGenerators(_compilation).GetRunResult();

        var characters = 0;
        foreach (var generated in result.Results[0].GeneratedSources)
        {
            characters += generated.SourceText.Length;
        }

        // Mock source that stopped matching the APIs would generate nothing and quietly turn this into a
        // measurement of driver overhead, so refuse to report a number for it.
        return characters == 0
            ? throw new InvalidOperationException("The mock consumer source generated nothing; the benchmark is measuring nothing.")
            : characters;
    }
}
