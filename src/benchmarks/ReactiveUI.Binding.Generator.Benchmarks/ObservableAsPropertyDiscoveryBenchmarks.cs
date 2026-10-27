// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;
using BenchmarkDotNet.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using ReactiveUI.Binding.Generator.Benchmarks.Support;

namespace ReactiveUI.Binding.Generator.Benchmarks;

/// <summary>
/// Measures the two ways a generator can find <c>[ObservableAsProperty]</c> properties, so the choice of
/// <c>ForAttributeWithMetadataName</c> is backed by numbers: the compiler's attribute index, and a syntax predicate
/// over every attributed property followed by a semantic check of its attributes.
/// </summary>
/// <remarks>
/// Each approach is measured on a fresh driver, and again after one unrelated file changes, which is the run an
/// editor repeats on every keystroke. Both find the same properties; only the discovery differs.
/// </remarks>
public class ObservableAsPropertyDiscoveryBenchmarks
{
    /// <summary>The number of files in the corpus.</summary>
    private const int FileCount = 50;

    /// <summary>The number of properties in each file, one of which carries the attribute.</summary>
    private const int PropertiesPerFile = 20;

    /// <summary>The attribute's metadata name.</summary>
    private const string AttributeMetadataName = "ReactiveUI.Binding.ObservableAsPropertyAttribute";

    /// <summary>The corpus compilation.</summary>
    private Compilation _compilation = null!;

    /// <summary>The corpus with one unrelated file edited.</summary>
    private Compilation _edited = null!;

    /// <summary>A driver that has already run the attribute-index generator over the corpus.</summary>
    private GeneratorDriver _warmIndex = null!;

    /// <summary>A driver that has already run the syntax-scan generator over the corpus.</summary>
    private GeneratorDriver _warmScan = null!;

    /// <summary>Builds the corpus and warms one driver per approach.</summary>
    [GlobalSetup]
    public void Setup()
    {
        var parseOptions = GeneratorHarness.ParseOptions(false);
        var trees = new SyntaxTree[FileCount];
        for (var f = 0; f < FileCount; f++)
        {
            var source = new StringBuilder()
                .AppendLine("using ReactiveUI.Binding;")
                .AppendLine("namespace Corpus;")
                .Append("public partial class Vm").Append(f).AppendLine()
                .AppendLine("{")
                .AppendLine("    [ObservableAsProperty]")
                .AppendLine("    public partial string Backed { get; }");
            for (var p = 1; p < PropertiesPerFile; p++)
            {
                _ = source.Append("    [System.ComponentModel.Description(\"p\")] public int P").Append(p).AppendLine(" { get; set; }");
            }

            _ = source.AppendLine("}");
            trees[f] = CSharpSyntaxTree.ParseText(source.ToString(), parseOptions, $"Vm{f}.cs");
        }

        _compilation = GeneratorHarness.BuildCompilation(false).AddSyntaxTrees(trees);
        var last = trees[FileCount - 1];
        _edited = _compilation.ReplaceSyntaxTree(last, last.WithChangedText(last.GetText().WithChanges(new TextChange(new TextSpan(0, 0), "// edited\n"))));
        _warmIndex = CreateDriver(new AttributeIndexGenerator(), parseOptions).RunGenerators(_compilation);
        _warmScan = CreateDriver(new SyntaxScanGenerator(), parseOptions).RunGenerators(_compilation);
    }

    /// <summary>Finds the properties through the compiler's attribute index, on a fresh driver.</summary>
    /// <returns>The run result's generated source count, read so the run is not elided.</returns>
    [Benchmark(Baseline = true, Description = "Cold: ForAttributeWithMetadataName")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ColdIndex() =>
        CreateDriver(new AttributeIndexGenerator(), GeneratorHarness.ParseOptions(false)).RunGenerators(_compilation).GetRunResult().GeneratedTrees.Length;

    /// <summary>Finds the properties by scanning attributed properties and checking each one semantically, on a fresh driver.</summary>
    /// <returns>The run result's generated source count, read so the run is not elided.</returns>
    [Benchmark(Description = "Cold: syntax scan + semantic check")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ColdScan() =>
        CreateDriver(new SyntaxScanGenerator(), GeneratorHarness.ParseOptions(false)).RunGenerators(_compilation).GetRunResult().GeneratedTrees.Length;

    /// <summary>Reruns the attribute-index generator after one unrelated file changed.</summary>
    /// <returns>The run result's generated source count, read so the run is not elided.</returns>
    [Benchmark(Description = "Edit: ForAttributeWithMetadataName")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int EditIndex() => _warmIndex.RunGenerators(_edited).GetRunResult().GeneratedTrees.Length;

    /// <summary>Reruns the syntax-scan generator after one unrelated file changed.</summary>
    /// <returns>The run result's generated source count, read so the run is not elided.</returns>
    [Benchmark(Description = "Edit: syntax scan + semantic check")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int EditScan() => _warmScan.RunGenerators(_edited).GetRunResult().GeneratedTrees.Length;

    /// <summary>Creates a driver that tracks incremental steps, as the IDE's driver does.</summary>
    /// <param name="generator">The generator.</param>
    /// <param name="parseOptions">The parse options.</param>
    /// <returns>The driver.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static CSharpGeneratorDriver CreateDriver(IIncrementalGenerator generator, CSharpParseOptions parseOptions) =>
        CSharpGeneratorDriver.Create([generator.AsSourceGenerator()], parseOptions: parseOptions);

    /// <summary>Emits one file listing the property names it was given.</summary>
    /// <param name="context">The output context.</param>
    /// <param name="names">The property names.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Emit(SourceProductionContext context, ImmutableArray<string> names) =>
        context.AddSource("Found.g.cs", $"// {names.Length}");

    /// <summary>Finds the attribute through <c>ForAttributeWithMetadataName</c>, as the shipped generator does.</summary>
    private sealed class AttributeIndexGenerator : IIncrementalGenerator
    {
        /// <inheritdoc/>
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var names = context.SyntaxProvider.ForAttributeWithMetadataName(
                AttributeMetadataName,
                static (node, _) => node is PropertyDeclarationSyntax,
                static (attributeContext, _) => attributeContext.TargetSymbol.Name);
            context.RegisterSourceOutput(names.Collect(), Emit);
        }
    }

    /// <summary>Finds the attribute by filtering attributed properties in syntax, then checking each one's attributes.</summary>
    private sealed class SyntaxScanGenerator : IIncrementalGenerator
    {
        /// <inheritdoc/>
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var names = context.SyntaxProvider.CreateSyntaxProvider(
                    static (node, _) => node is PropertyDeclarationSyntax { AttributeLists.Count: > 0 },
                    ReadName)
                .Where(static name => name is not null)
                .Select(static (name, _) => name!);
            context.RegisterSourceOutput(names.Collect(), Emit);
        }

        /// <summary>Returns the property's name when it carries the attribute.</summary>
        /// <param name="context">The syntax context.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>The name, or null when the property does not carry the attribute.</returns>
        private static string? ReadName(GeneratorSyntaxContext context, CancellationToken ct)
        {
            if (context.SemanticModel.GetDeclaredSymbol(context.Node, ct) is not IPropertySymbol property)
            {
                return null;
            }

            foreach (var attribute in property.GetAttributes())
            {
                if (attribute.AttributeClass?.ToDisplayString() == AttributeMetadataName)
                {
                    return property.Name;
                }
            }

            return null;
        }
    }
}
