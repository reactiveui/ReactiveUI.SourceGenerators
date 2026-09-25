// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.SourceGenerators.Benchmarks.Shared;

namespace ReactiveUI.SourceGenerators.Benchmarks.Support;

/// <summary>The attribute-discovery scenarios: each strategy cold, and rerun after an unrelated edit.</summary>
internal static class DiscoveryScenarios
{
    /// <summary>How many times each mock file is repeated.</summary>
    private const int Copies = 20;

    /// <summary>The operations a cold session runs.</summary>
    private const int ColdOperations = 100;

    /// <summary>The operations an incremental session runs.</summary>
    private const int IncrementalOperations = 400;

    /// <summary>The text the unrelated edit changes.</summary>
    private const string UnrelatedEditTarget = "public override string ToString() => Customer ?? string.Empty;";

    /// <summary>Creates a cold and an incremental scenario per discovery strategy.</summary>
    /// <returns>The scenarios.</returns>
    internal static List<AllocationScenario> Create()
    {
        // Every generator's output joins the corpus first, so the attributes resolve and the generated files add the
        // kind of code a real build carries alongside the consumer's.
        Compilation compilation = null!;
        Compilation edited = null!;
        void Prepare()
        {
            if (compilation is not null)
            {
                return;
            }

            _ = GeneratorHarness.CreateDriver(GeneratorHarness.AllCorpus)
                .RunGeneratorsAndUpdateCompilation(GeneratorHarness.BuildCompilation(GeneratorHarness.AllCorpus, Copies), out compilation, out _);
            var tree = FindTree(compilation, "Noise0.cs");
            var text = tree.GetText().ToString().Replace(UnrelatedEditTarget, "public override string ToString() => Customer ?? \"none\";", StringComparison.Ordinal);
            edited = compilation.ReplaceSyntaxTree(tree, tree.WithChangedText(Microsoft.CodeAnalysis.Text.SourceText.From(text)));
        }

        var scenarios = new List<AllocationScenario>();
        foreach (var (name, generator) in DiscoveryGenerators.Strategies)
        {
            scenarios.Add(new($"Discovery-Cold-{name}", ColdOperations, Prepare, () => Run(CreateDriver(generator), compilation)));

            GeneratorDriver warm = null!;
            scenarios.Add(new(
                $"Discovery-Incremental-{name}",
                IncrementalOperations,
                () =>
                {
                    Prepare();
                    warm = CreateDriver(generator).RunGenerators(compilation);
                },
                () => Run(warm, edited)));
        }

        return scenarios;
    }

    /// <summary>Runs each strategy once and reports what it found, to show they agree.</summary>
    /// <returns>One line per strategy.</returns>
    internal static IEnumerable<string> DescribeFindings()
    {
        _ = GeneratorHarness.CreateDriver(GeneratorHarness.AllCorpus)
            .RunGeneratorsAndUpdateCompilation(GeneratorHarness.BuildCompilation(GeneratorHarness.AllCorpus, Copies), out var compilation, out _);
        foreach (var (name, generator) in DiscoveryGenerators.Strategies)
        {
            var found = CreateDriver(generator).RunGenerators(compilation).GetRunResult().Results[0].GeneratedSources[0].SourceText.ToString();
            yield return $"{name}: {found}";
        }
    }

    /// <summary>Creates a cold driver for one discovery generator.</summary>
    /// <param name="generator">The generator.</param>
    /// <returns>The driver.</returns>
    private static CSharpGeneratorDriver CreateDriver(IIncrementalGenerator generator) =>
        CSharpGeneratorDriver.Create([generator.AsSourceGenerator()], parseOptions: new(LanguageVersion.CSharp13));

    /// <summary>Finds a tree by file name.</summary>
    /// <param name="compilation">The compilation.</param>
    /// <param name="fileName">The file name.</param>
    /// <returns>The tree.</returns>
    private static SyntaxTree FindTree(Compilation compilation, string fileName)
    {
        foreach (var tree in compilation.SyntaxTrees)
        {
            if (Path.GetFileName(tree.FilePath) == fileName)
            {
                return tree;
            }
        }

        throw new InvalidOperationException($"The corpus has no {fileName}.");
    }

    /// <summary>Runs a driver and returns the length of what it wrote, so the work is kept.</summary>
    /// <param name="driver">The driver.</param>
    /// <param name="compilation">The compilation.</param>
    /// <returns>The generated length.</returns>
    private static long Run(GeneratorDriver driver, Compilation compilation) =>
        driver.RunGenerators(compilation).GetRunResult().Results[0].GeneratedSources[0].SourceText.Length;
}
