// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators;

namespace ReactiveUI.Binding.Generator.Benchmarks.Support;

/// <summary>Builds the compilation and the generator driver the generation benchmarks run.</summary>
internal static class GeneratorHarness
{
    /// <summary>The assembly name given to the compilation the generator runs against.</summary>
    private const string CompilationAssemblyName = "Mocks";

    /// <summary>The folder beside the benchmark assembly that holds the mock consumer source.</summary>
    private const string MocksFolder = "Mocks";

    /// <summary>Matches the C# source files in the mocks folder.</summary>
    private const string SourceFilePattern = "*.cs";

    /// <summary>The feature a build lists interceptable namespaces under.</summary>
    private const string InterceptorsNamespacesFeature = "InterceptorsNamespaces";

    /// <summary>The namespace the generator emits interceptors into.</summary>
    private const string InterceptorNamespace = "ReactiveUI.Binding.Generated.Interceptors";

    /// <summary>
    /// The parse options of a build that lists the generated namespace, which is what the shipped targets set
    /// wherever the compiler can honour an interceptor emitted into it.
    /// </summary>
    /// <param name="intercept">Whether the build lists the namespace.</param>
    /// <returns>The parse options.</returns>
    internal static CSharpParseOptions ParseOptions(bool intercept)
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.Latest);

        return intercept
            ? parseOptions.WithFeatures([new KeyValuePair<string, string>(InterceptorsNamespacesFeature, InterceptorNamespace)])
            : parseOptions;
    }

    /// <summary>Builds a compilation over the mock consumer source copied beside the benchmark assembly.</summary>
    /// <param name="intercept">Whether the build lists the generated namespace for interception.</param>
    /// <returns>The compilation.</returns>
    internal static CSharpCompilation BuildCompilation(bool intercept)
    {
        var parseOptions = ParseOptions(intercept);
        var paths = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, MocksFolder), SourceFilePattern);
        Array.Sort(paths, StringComparer.Ordinal);

        var syntaxTrees = new SyntaxTree[paths.Length];
        for (var i = 0; i < paths.Length; i++)
        {
            using var reader = File.OpenText(paths[i]);
            syntaxTrees[i] = CSharpSyntaxTree.ParseText(reader.ReadToEnd(), parseOptions, paths[i]);
        }

        return BuildCompilation(CompilationAssemblyName, syntaxTrees);
    }

    /// <summary>Builds a compilation over the given source, referencing the framework and the runtime library.</summary>
    /// <param name="assemblyName">The compilation's assembly name.</param>
    /// <param name="syntaxTrees">The source.</param>
    /// <returns>The compilation.</returns>
    internal static CSharpCompilation BuildCompilation(string assemblyName, SyntaxTree[] syntaxTrees)
    {
#if NET11_0_OR_GREATER
        var references = new List<MetadataReference>(Basic.Reference.Assemblies.Net110.References.All)
#else
        var references = new List<MetadataReference>(Basic.Reference.Assemblies.Net100.References.All)
#endif
        {
            MetadataReference.CreateFromFile(typeof(ReactiveUIBindingExtensions).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ReactiveUI.Primitives.Concurrency.ISequencer).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Splat.IEnableLogger).Assembly.Location),
        };

        return CSharpCompilation.Create(
            assemblyName,
            syntaxTrees,
            references,
            new(OutputKind.DynamicallyLinkedLibrary));
    }

    /// <summary>Creates a cold generator driver, carrying no caches from a previous run.</summary>
    /// <param name="intercept">Whether the build lists the generated namespace for interception.</param>
    /// <returns>The generator driver.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static GeneratorDriver CreateDriver(bool intercept) =>
        CSharpGeneratorDriver.Create(
            [new BindingGenerator().AsSourceGenerator()],
            null,
            ParseOptions(intercept),
            null);
}
