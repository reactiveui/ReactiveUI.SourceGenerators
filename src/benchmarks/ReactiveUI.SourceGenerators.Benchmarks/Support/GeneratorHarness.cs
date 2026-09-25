// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ReactiveUI.SourceGenerators.Benchmarks.Support;

/// <summary>Builds the compilation and the generator driver the generation benchmarks run.</summary>
internal static class GeneratorHarness
{
    /// <summary>The corpus name that selects every mock file.</summary>
    internal const string AllCorpus = "All";

    /// <summary>The folder beside the benchmark assembly that holds the mock consumer source.</summary>
    private const string MocksFolder = "Mocks";

    /// <summary>The token in a mock file that each copy replaces with its own index.</summary>
    private const string IndexToken = "__N__";

    /// <summary>The parse options every benchmark compilation uses.</summary>
    private static readonly CSharpParseOptions ParseOptions = new(LanguageVersion.CSharp13);

    /// <summary>The metadata references for the mock consumer, resolved once per process.</summary>
    private static readonly Lazy<List<MetadataReference>> References = new(CreateReferences);

    /// <summary>Builds a compilation over the mock consumer source copied beside the benchmark assembly.</summary>
    /// <param name="corpus">The mock file name without extension, or <see cref="AllCorpus"/> for every mock file.</param>
    /// <param name="copies">How many times each mock file is repeated, each copy in its own namespace.</param>
    /// <returns>The compilation.</returns>
    internal static CSharpCompilation BuildCompilation(string corpus, int copies)
    {
        var paths = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, MocksFolder), "*.cs");
        Array.Sort(paths, StringComparer.Ordinal);

        var syntaxTrees = new List<SyntaxTree>();
        foreach (var path in paths)
        {
            var name = Path.GetFileNameWithoutExtension(path);
            if (corpus != AllCorpus && !string.Equals(name, corpus, StringComparison.Ordinal) && !string.Equals(name, "Stubs", StringComparison.Ordinal))
            {
                continue;
            }

            var template = File.ReadAllText(path);
            if (!template.Contains(IndexToken, StringComparison.Ordinal))
            {
                syntaxTrees.Add(CSharpSyntaxTree.ParseText(template, ParseOptions, path));
                continue;
            }

            for (var i = 0; i < copies; i++)
            {
                var text = template.Replace(IndexToken, i.ToString(System.Globalization.CultureInfo.InvariantCulture), StringComparison.Ordinal);
                syntaxTrees.Add(CSharpSyntaxTree.ParseText(text, ParseOptions, $"{name}{i}.cs"));
            }
        }

        return CSharpCompilation.Create(
            "Mocks",
            syntaxTrees,
            References.Value,
            new(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
    }

    /// <summary>Creates a cold generator driver for a corpus, carrying no caches from a previous run.</summary>
    /// <param name="corpus">The corpus whose generators the driver runs.</param>
    /// <returns>The generator driver.</returns>
    internal static GeneratorDriver CreateDriver(string corpus) =>
        CSharpGeneratorDriver.Create(
            GeneratorCatalog.Create(corpus),
            parseOptions: ParseOptions);

    /// <summary>Generates over a corpus and counts the errors in the resulting compilation, printing each one.</summary>
    /// <param name="corpus">The corpus name.</param>
    /// <returns>The number of errors.</returns>
    internal static int CountErrors(string corpus)
    {
        _ = CreateDriver(corpus).RunGeneratorsAndUpdateCompilation(BuildCompilation(corpus, 1), out var output, out _);
        var errors = 0;
        foreach (var diagnostic in output.GetDiagnostics())
        {
            if (diagnostic.Severity == DiagnosticSeverity.Error)
            {
                errors++;
                Console.WriteLine($"  {diagnostic}");
            }
        }

        return errors;
    }

    /// <summary>Creates the metadata references for the mock consumer: the runtime plus ReactiveUI and its dependencies.</summary>
    /// <returns>The references.</returns>
    private static List<MetadataReference> CreateReferences()
    {
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var references = new List<MetadataReference>();
        Assembly[] seeds =
        [
            typeof(object).Assembly,
            typeof(Enumerable).Assembly,
            typeof(System.ComponentModel.INotifyPropertyChanged).Assembly,
            typeof(ReactiveUI.Reactive.ReactiveObject).Assembly,
            typeof(System.Reactive.Unit).Assembly,
            typeof(DynamicData.SourceList<>).Assembly,
            typeof(Splat.Locator).Assembly,
        ];

        foreach (var seed in seeds)
        {
            AddTransitive(seed, visited, references);
        }

        return references;
    }

    /// <summary>Adds an assembly and its loadable references to the reference list.</summary>
    /// <param name="assembly">The assembly.</param>
    /// <param name="visited">The assembly locations already added.</param>
    /// <param name="references">The references to add to.</param>
    private static void AddTransitive(Assembly assembly, HashSet<string> visited, List<MetadataReference> references)
    {
        if (assembly.IsDynamic || string.IsNullOrEmpty(assembly.Location) || !visited.Add(assembly.Location))
        {
            return;
        }

        references.Add(MetadataReference.CreateFromFile(assembly.Location));
        foreach (var name in assembly.GetReferencedAssemblies())
        {
            try
            {
                AddTransitive(Assembly.Load(name), visited, references);
            }
            catch (FileNotFoundException)
            {
                // A facade the runtime does not ship is not needed by the mocks.
            }
        }
    }
}
