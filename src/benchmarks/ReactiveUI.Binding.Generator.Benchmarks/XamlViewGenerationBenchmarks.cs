// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using BenchmarkDotNet.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using ReactiveUI.Binding.Generator.Benchmarks.Support;

namespace ReactiveUI.Binding.Generator.Benchmarks;

/// <summary>Measures a generation pass over Avalonia-style views that bind every named control in one lambda.</summary>
/// <remarks>
/// With <see cref="Xaml"/> on, the controls are named in XAML pages and a view model has a <c>[Reactive]</c> field, so
/// the call sites are read from the generator's private copy of the compilation. With it off, the views declare their
/// controls and the call sites are read from the consumer's compilation. Growing <see cref="Bindings"/> shows how the
/// cost of one view grows with the call sites in its lambda.
/// </remarks>
public class XamlViewGenerationBenchmarks
{
    /// <summary>The number of views in the corpus.</summary>
    private const int Views = 4;

    /// <summary>The consumer compilation.</summary>
    private Compilation _compilation = null!;

    /// <summary>The consumer compilation after a one-line edit to one view.</summary>
    private Compilation _edited = null!;

    /// <summary>A driver that has already run over the unedited compilation.</summary>
    private GeneratorDriver _primed = null!;

    /// <summary>The XAML pages the driver sees.</summary>
    private ImmutableArray<AdditionalText> _pages;

    /// <summary>The validated size of the generated output.</summary>
    private int _characters;

    /// <summary>Gets or sets the number of controls each view names and binds.</summary>
    [Params(10, 50, 100)]
    public int Bindings { get; set; }

    /// <summary>Gets or sets a value indicating whether the controls come from XAML pages.</summary>
    [Params(false, true)]
    public bool Xaml { get; set; }

    /// <summary>Builds the corpus and checks the generated output compiles and covers every call site.</summary>
    /// <exception cref="InvalidOperationException">The output does not compile.</exception>
    [GlobalSetup]
    public void Setup()
    {
        _compilation = XamlViewCorpus.Build(Views, Bindings, Xaml, out _pages);
        var driver = XamlViewCorpus.CreateDriver(_pages).RunGeneratorsAndUpdateCompilation(_compilation, out var output, out _);

        // The other generators do not run here, so their members are added the way a full build would add them.
        var full = Xaml ? output.AddSyntaxTrees(XamlViewCorpus.OtherGeneratorsOutput(Views, Bindings)) : output;
        foreach (var diagnostic in full.GetDiagnostics())
        {
            if (diagnostic.Severity == DiagnosticSeverity.Error)
            {
                throw new InvalidOperationException($"The generated output does not compile: {diagnostic}");
            }
        }

        _characters = Count(driver.GetRunResult());

        // A one-line edit to one view, as a developer makes between two builds.
        foreach (var tree in _compilation.SyntaxTrees)
        {
            if (!tree.FilePath.EndsWith(".axaml.cs", StringComparison.Ordinal))
            {
                continue;
            }

            var text = tree.GetText();
            _edited = _compilation.ReplaceSyntaxTree(tree, tree.WithChangedText(text.WithChanges(new TextChange(new(text.Length, 0), "\n// edited"))));
            break;
        }
    }

    /// <summary>Primes a driver with a run over the unedited views, so the next run is incremental.</summary>
    [IterationSetup(Target = nameof(Edit))]
    public void PrimeDriver() => _primed = XamlViewCorpus.CreateDriver(_pages).RunGenerators(_compilation);

    /// <summary>Runs the generation that follows a one-line edit to one view, reusing the previous run's caches.</summary>
    /// <returns>The number of generated characters.</returns>
    /// <exception cref="InvalidOperationException">The pass generated something other than the validated output.</exception>
    [Benchmark]
    public int Edit()
    {
        // The primed driver's caches serve every step whose input the edit did not change.
        var result = _primed.RunGenerators(_edited).GetRunResult();
        var characters = Count(result);
        return characters == _characters
            ? characters
            : throw new InvalidOperationException("An edit pass changed the validated output.");
    }

    /// <summary>Runs a whole cold generation over the views.</summary>
    /// <returns>The number of generated characters.</returns>
    /// <exception cref="InvalidOperationException">The pass generated something other than the validated output.</exception>
    [Benchmark]
    public int Generate()
    {
        var characters = Count(XamlViewCorpus.CreateDriver(_pages).RunGenerators(_compilation).GetRunResult());
        return characters == _characters
            ? characters
            : throw new InvalidOperationException("A generation pass changed the validated output.");
    }

    /// <summary>Counts the generated characters.</summary>
    /// <param name="result">The run result.</param>
    /// <returns>The number of characters.</returns>
    private static int Count(GeneratorDriverRunResult result)
    {
        var characters = 0;
        foreach (var generated in result.Results[0].GeneratedSources)
        {
            characters += generated.SourceText.Length;
        }

        return characters;
    }
}
