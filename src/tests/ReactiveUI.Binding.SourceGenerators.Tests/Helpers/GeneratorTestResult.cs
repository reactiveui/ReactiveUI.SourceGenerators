// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

/// <summary>Wraps the results of a source generator execution for assertion and verification.</summary>
public sealed class GeneratorTestResult
{
    /// <summary>Initializes a new instance of the <see cref="GeneratorTestResult"/> class.</summary>
    /// <param name="driver">The generator driver after execution.</param>
    /// <param name="outputCompilation">The compilation after generator execution.</param>
    /// <param name="generatorDiagnostics">Diagnostics produced by the generator itself.</param>
    public GeneratorTestResult(
        GeneratorDriver driver,
        Compilation outputCompilation,
        ImmutableArray<Diagnostic> generatorDiagnostics)
    {
        ArgumentNullException.ThrowIfNull(driver);
        ArgumentNullException.ThrowIfNull(outputCompilation);

        Driver = driver;
        OutputCompilation = outputCompilation;
        GeneratorDiagnostics = generatorDiagnostics;

        var compilationDiagnostics = outputCompilation.GetDiagnostics();
        CompilationErrors =
        [
            ..compilationDiagnostics
                .Where(static d => d.Severity == DiagnosticSeverity.Error)
        ];
        CompilationWarnings =
        [
            ..compilationDiagnostics
                .Where(static d => d.Severity == DiagnosticSeverity.Warning)
        ];

        var generatedSources = new Dictionary<string, string>();
        var runResult = driver.GetRunResult();
        foreach (var result in runResult.Results)
        {
            foreach (var source in result.GeneratedSources)
            {
                generatedSources[source.HintName] = source.SourceText.ToString();
            }
        }

        GeneratedSources = generatedSources;
    }

    /// <summary>Gets the generator driver after execution, for snapshot comparison.</summary>
    public GeneratorDriver Driver { get; }

    /// <summary>Gets the compilation after generator execution.</summary>
    public Compilation OutputCompilation { get; }

    /// <summary>Gets the diagnostics produced by the generator itself.</summary>
    public ImmutableArray<Diagnostic> GeneratorDiagnostics { get; }

    /// <summary>Gets the compilation diagnostics with severity Error.</summary>
    public ImmutableArray<Diagnostic> CompilationErrors { get; }

    /// <summary>Gets the compilation diagnostics with severity Warning.</summary>
    public ImmutableArray<Diagnostic> CompilationWarnings { get; }

    /// <summary>Gets the generated source files as a dictionary of hint name to source text.</summary>
    public IReadOnlyDictionary<string, string> GeneratedSources { get; }

    /// <summary>Gets a value indicating whether the output compilation has no errors.</summary>
    public bool CompilesWithoutErrors => CompilationErrors.IsEmpty;
}
