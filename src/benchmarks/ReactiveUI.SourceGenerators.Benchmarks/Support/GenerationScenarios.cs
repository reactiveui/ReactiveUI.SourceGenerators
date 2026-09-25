// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.SourceGenerators.Benchmarks.Shared;

namespace ReactiveUI.SourceGenerators.Benchmarks.Support;

/// <summary>The generation scenarios the EventPipe allocation harness measures.</summary>
internal static class GenerationScenarios
{
    /// <summary>How many times each mock file is repeated, each copy in its own namespace.</summary>
    private const int Copies = 20;

    /// <summary>The operations a cold-generation session runs; enough for hundreds of megabytes of allocation.</summary>
    private const int ColdOperations = 60;

    /// <summary>The operations an incremental session runs.</summary>
    private const int IncrementalOperations = 400;

    /// <summary>The text an unrelated edit changes: a member of a noise class no generator reads.</summary>
    private const string UnrelatedEditTarget = "public override string ToString() => Customer ?? string.Empty;";

    /// <summary>The text a related edit changes: a member of a class with <c>[Reactive]</c> fields that is not itself generated from.</summary>
    private const string RelatedEditTarget = "public string FullName => FirstName + \" \" + LastName;";

    /// <summary>The text a model edit changes: a <c>[Reactive]</c> field's name, which changes that one type's model.</summary>
    private const string ModelEditTarget = "private int _age;";

    /// <summary>Creates every scenario: a cold generation per corpus, and warm reruns after an edit.</summary>
    /// <returns>The scenarios.</returns>
    internal static List<AllocationScenario> Create()
    {
        var scenarios = new List<AllocationScenario>();
        foreach (var corpus in GeneratorCatalog.Corpora)
        {
            Compilation compilation = null!;
            scenarios.Add(new(
                $"Cold-{corpus}",
                ColdOperations,
                () => compilation = GeneratorHarness.BuildCompilation(corpus, Copies),
                () => CountGenerated(GeneratorHarness.CreateDriver(corpus).RunGenerators(compilation))));
        }

        scenarios.Add(CreateIncremental("Incremental-UnrelatedEdit", "Noise0.cs", UnrelatedEditTarget, "public override string ToString() => Customer ?? \"none\";"));
        scenarios.Add(CreateIncremental("Incremental-ReactiveClassEdit", "Reactive0.cs", RelatedEditTarget, "public string FullName => LastName + \", \" + FirstName;"));
        scenarios.Add(CreateIncremental("Incremental-ReactiveModelEdit", "Reactive0.cs", ModelEditTarget, "private int _ageInYears;"));
        return scenarios;
    }

    /// <summary>Creates a scenario that reruns a warm driver over a compilation with one tree edited.</summary>
    /// <param name="name">The scenario name.</param>
    /// <param name="treeName">The file name of the tree to edit.</param>
    /// <param name="find">The text to replace in that tree.</param>
    /// <param name="replace">The replacement.</param>
    /// <returns>The scenario.</returns>
    /// <remarks>
    /// The warm driver is never replaced, so every operation reruns from the same cached state against the same edit,
    /// which is what one keystroke costs in the IDE.
    /// </remarks>
    private static AllocationScenario CreateIncremental(string name, string treeName, string find, string replace)
    {
        GeneratorDriver warm = null!;
        Compilation edited = null!;
        return new(
            name,
            IncrementalOperations,
            () =>
            {
                var compilation = GeneratorHarness.BuildCompilation(GeneratorHarness.AllCorpus, Copies);
                warm = GeneratorHarness.CreateDriver(GeneratorHarness.AllCorpus).RunGenerators(compilation);
                var tree = compilation.SyntaxTrees.Single(t => Path.GetFileName(t.FilePath) == treeName);
                var text = tree.GetText().ToString();
                if (!text.Contains(find, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException($"{treeName} no longer contains the text the {name} scenario edits.");
                }

                var newTree = tree.WithChangedText(Microsoft.CodeAnalysis.Text.SourceText.From(text.Replace(find, replace, StringComparison.Ordinal)));
                edited = compilation.ReplaceSyntaxTree(tree, newTree);
            },
            () => CountGenerated(warm.RunGenerators(edited)));
    }

    /// <summary>Counts the characters a driver's last run generated.</summary>
    /// <param name="driver">The driver.</param>
    /// <returns>The number of generated characters.</returns>
    private static long CountGenerated(GeneratorDriver driver)
    {
        var characters = 0L;
        foreach (var result in driver.GetRunResult().Results)
        {
            foreach (var generated in result.GeneratedSources)
            {
                characters += generated.SourceText.Length;
            }
        }

        return characters;
    }
}
