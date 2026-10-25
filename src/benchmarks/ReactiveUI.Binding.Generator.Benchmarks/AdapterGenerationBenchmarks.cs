// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.Binding.Generator.Benchmarks.Support;

namespace ReactiveUI.Binding.Generator.Benchmarks;

/// <summary>Measures generation for native observations, commands, collection setters and typed conversions.</summary>
public class AdapterGenerationBenchmarks
{
    /// <summary>The number of native command bindings in the command corpus.</summary>
    private const int CommandWorkers = 3;

    /// <summary>The number of typed bindings in the conversion corpus.</summary>
    private const int ConversionWorkers = 4;

    /// <summary>The number of binding workers in each other corpus.</summary>
    private const int PairedWorkers = 2;

    /// <summary>The prepared consumer compilation.</summary>
    private Compilation _compilation = null!;

    /// <summary>The dispatch file required by the selected corpus.</summary>
    private string _dispatchHint = string.Empty;

    /// <summary>The validated dispatch size for the fixed consumer input.</summary>
    private int _dispatchCharacters;

    /// <summary>Gets or sets the adapter family to measure.</summary>
    [Params("Observation", "Commands", "Collections", "Conversions")]
    public string Family { get; set; } = "Observation";

    /// <summary>Prepares the consumer and verifies that every generated file compiles.</summary>
    /// <exception cref="InvalidOperationException">The corpus fails compilation.</exception>
    [GlobalSetup]
    public void Setup()
    {
        _compilation = CreateCompilation();
        var driver = GeneratorHarness.CreateDriver(false).RunGeneratorsAndUpdateCompilation(_compilation, out var output, out _);
        ValidateCompilation(output);
        _dispatchHint = Family switch
        {
            "Observation" => "WhenChangedDispatch.g.cs",
            "Commands" => "BindCommandDispatch.g.cs",
            _ => "BindToDispatch.g.cs",
        };
        ValidateWorkers(driver.GetRunResult());
        _dispatchCharacters = CountDispatchCharacters(driver.GetRunResult());
    }

    /// <summary>Runs selection and emission with a fresh generator driver.</summary>
    /// <returns>The dispatch size, to keep the generated work observable.</returns>
    /// <exception cref="InvalidOperationException">The output differs from the validated corpus.</exception>
    [Benchmark]
    public int Generate()
    {
        var characters = CountDispatchCharacters(GeneratorHarness.CreateDriver(false).RunGenerators(_compilation).GetRunResult());
        return characters == _dispatchCharacters
            ? characters
            : throw new InvalidOperationException("A generation pass changed the validated dispatch output.");
    }

    /// <summary>Rejects output that cannot compile before collecting timings.</summary>
    /// <param name="output">The generated consumer compilation.</param>
    /// <exception cref="InvalidOperationException">The output contains compiler errors.</exception>
    private static void ValidateCompilation(Compilation output)
    {
        var errors = new List<string>();
        foreach (var diagnostic in output.GetDiagnostics())
        {
            if (diagnostic.Severity == DiagnosticSeverity.Error)
            {
                errors.Add(diagnostic.ToString());
            }
        }

        if (errors.Count != 0)
        {
            throw new InvalidOperationException(string.Join(Environment.NewLine, errors));
        }
    }

    /// <summary>Builds the consumer with the same referenced runtime assemblies its output needs.</summary>
    /// <returns>The compilation passed to the generator.</returns>
    private CSharpCompilation CreateCompilation()
    {
        var compilation = GeneratorHarness.BuildCompilation(false);
        var references = new List<MetadataReference>(compilation.References);
        var paths = new HashSet<string?>(StringComparer.Ordinal);
        foreach (var reference in references)
        {
            _ = paths.Add(reference.Display);
        }

        foreach (var assembly in new[]
        {
            typeof(ReactiveUI.Primitives.LinqExtensions).Assembly,
            typeof(ReactiveUI.Primitives.SubscribeExtensions).Assembly,
            typeof(ReactiveUI.Primitives.Disposables.EmptyDisposable).Assembly,
            typeof(ReactiveUI.Primitives.Advanced.ImmediateReturnSignal<int>).Assembly,
            typeof(Splat.AppLocator).Assembly,
        })
        {
            if (paths.Add(assembly.Location))
            {
                references.Add(MetadataReference.CreateFromFile(assembly.Location));
            }
        }

        return compilation.RemoveAllSyntaxTrees().WithReferences(references)
            .AddSyntaxTrees(CSharpSyntaxTree.ParseText(AdapterBenchmarkCorpus.Source(Family), GeneratorHarness.ParseOptions(false)));
    }

    /// <summary>Counts API workers so unrelated view dispatch cannot satisfy a missing binding.</summary>
    /// <param name="result">The generated consumer.</param>
    /// <exception cref="InvalidOperationException">The selected corpus did not produce every binding worker.</exception>
    private void ValidateWorkers(GeneratorDriverRunResult result)
    {
        var expected = Family switch { "Commands" => CommandWorkers, "Conversions" => ConversionWorkers, _ => PairedWorkers };
        var prefix = Family switch { "Observation" => "__WhenChanged_", "Commands" => "__BindCommand_", _ => "__BindTo_" };
        var workers = new HashSet<string>(StringComparer.Ordinal);
        foreach (var generated in result.Results[0].GeneratedSources)
        {
            if (generated.HintName != _dispatchHint)
            {
                continue;
            }

            ValidateMechanisms(generated.SourceText.ToString());
            var root = CSharpSyntaxTree.ParseText(generated.SourceText).GetRoot();
            foreach (var node in root.DescendantNodes())
            {
                if (node is MethodDeclarationSyntax method && method.Identifier.ValueText.StartsWith(prefix, StringComparison.Ordinal))
                {
                    _ = workers.Add(method.Identifier.ValueText);
                }
            }
        }

        if (workers.Count != expected)
        {
            throw new InvalidOperationException($"{Family}: expected {expected} binding workers, found {workers.Count}.");
        }
    }

    /// <summary>Requires the native operations the corpus is intended to measure.</summary>
    /// <param name="source">The validated API dispatch.</param>
    /// <exception cref="InvalidOperationException">A native mechanism was replaced by a fallback.</exception>
    private void ValidateMechanisms(string source)
    {
        string[] required = Family switch
        {
            "Observation" => ["__UIKitValueObservable", "__UIKitObservable", "AddObserver"],
            "Commands" => ["AddTarget", "__AppKitCommandTarget", ".Click +="],
            "Collections" => ["SuspendLayout", "AddRange", ".Owner", ".Container"],
            _ => [".ToString(", ".Visible", "__value.HasValue", "global::Foundation.NSDate"],
        };
        foreach (var operation in required)
        {
            if (source.IndexOf(operation, StringComparison.Ordinal) < 0)
            {
                throw new InvalidOperationException($"{Family}: missing generated operation {operation}.");
            }
        }
    }

    /// <summary>Rejects a benchmark corpus that fails to select any binding call sites.</summary>
    /// <param name="result">The generator output.</param>
    /// <returns>The number of emitted dispatch characters.</returns>
    /// <exception cref="InvalidOperationException">No dispatch was emitted.</exception>
    private int CountDispatchCharacters(GeneratorDriverRunResult result)
    {
        if (!result.Diagnostics.IsEmpty || result.Results[0].Exception is not null)
        {
            throw new InvalidOperationException("The generator reported diagnostics or an exception.");
        }

        var count = 0;
        foreach (var generated in result.Results[0].GeneratedSources)
        {
            if (generated.HintName == _dispatchHint)
            {
                count += generated.SourceText.Length;
            }
        }

        return count != 0 ? count : throw new InvalidOperationException("The adapter corpus produced no binding dispatch.");
    }
}
