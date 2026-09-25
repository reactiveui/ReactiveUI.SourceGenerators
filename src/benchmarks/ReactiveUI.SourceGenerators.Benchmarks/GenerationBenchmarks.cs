// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Attributes;
using Microsoft.CodeAnalysis;
using ReactiveUI.SourceGenerators.Benchmarks.Support;

namespace ReactiveUI.SourceGenerators.Benchmarks;

/// <summary>Measures a cold generation pass over a mock consumer corpus.</summary>
/// <remarks>
/// The driver is rebuilt per iteration so each measurement is a cold generation, which is what a consumer's build
/// pays. A reused driver would serve the next iteration from its incremental caches.
/// </remarks>
public class GenerationBenchmarks
{
    /// <summary>How many times each mock file is repeated, each copy in its own namespace.</summary>
    private const int Copies = 20;

    /// <summary>The mock consumer compilation, built once per parameter set.</summary>
    private Compilation _compilation = null!;

    /// <summary>Gets the corpus names.</summary>
    public static IEnumerable<string> Corpora => GeneratorCatalog.Corpora;

    /// <summary>Gets or sets the corpus: one generator's mock file, or every mock file.</summary>
    [ParamsSource(nameof(Corpora))]
    public string Corpus { get; set; } = GeneratorHarness.AllCorpus;

    /// <summary>Builds the mock consumer compilation once per parameter set.</summary>
    [GlobalSetup]
    public void Setup() => _compilation = GeneratorHarness.BuildCompilation(Corpus, Copies);

    /// <summary>Runs a whole cold generation: syntax scan, extraction and emission.</summary>
    /// <returns>The number of generated characters, so the work cannot be optimized away.</returns>
    /// <exception cref="InvalidOperationException">The corpus generated nothing beyond the injected attributes.</exception>
    [Benchmark]
    public int Generate()
    {
        var result = GeneratorHarness.CreateDriver(Corpus).RunGenerators(_compilation).GetRunResult();

        var characters = 0;
        var files = 0;
        foreach (var generator in result.Results)
        {
            foreach (var generated in generator.GeneratedSources)
            {
                characters += generated.SourceText.Length;
                files++;
            }
        }

        // A corpus that stopped matching the attributes would quietly measure driver overhead instead.
        return files <= Copies
            ? throw new InvalidOperationException($"The {Corpus} corpus generated {files} files; the benchmark is measuring nothing.")
            : characters;
    }
}
