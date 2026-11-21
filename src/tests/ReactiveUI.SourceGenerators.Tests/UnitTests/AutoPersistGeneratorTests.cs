// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>Tests compile-time persistence metadata generation for <c>[AutoPersist]</c>.</summary>
public sealed class AutoPersistGeneratorTests
{
    /// <summary>Verifies <c>AutoPersistMetadata</c> captures <c>[DataContract]</c> and <c>[DataMember]</c> properties without reflection.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task GeneratesAutoPersistMetadataForDataContractClass()
    {
        const string source = """
            using System.Runtime.Serialization;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            [DataContract]
            [AutoPersist]
            public partial class AppState : ReactiveObject
            {
                [DataMember]
                public string Title { get; set; } = string.Empty;

                [DataMember]
                public int Count { get; set; }

                public string Ephemeral { get; set; } = string.Empty;
            }
            """;

        var (generatedSource, errors) = RunGenerator(source);

        await Assert.That(generatedSource).IsNotNull();
        await Assert.That(errors).IsEmpty();
        await Assert.That(generatedSource!).Contains("public static global::ReactiveUI.AutoPersistHelperMixins.AutoPersistMetadata AutoPersistMetadata { get; } =");
        await Assert.That(generatedSource!).Contains(
            "new global::ReactiveUI.AutoPersistHelperMixins.AutoPersistMetadata("
            + "true, new global::System.Collections.Generic.HashSet<string>(global::System.StringComparer.Ordinal)");
        await Assert.That(generatedSource!).Contains("\"Title\",");
        await Assert.That(generatedSource!).Contains("\"Count\",");
        await Assert.That(generatedSource!).DoesNotContain("\"Ephemeral\"");
    }

    /// <summary>Verifies non-partial classes marked with <c>[AutoPersist]</c> are ignored.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task IgnoresNonPartialClass()
    {
        const string source = """
            using System.Runtime.Serialization;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            [DataContract]
            [AutoPersist]
            public sealed class NonPartialState
            {
                [DataMember]
                public string Title { get; set; } = string.Empty;
            }
            """;

        var (generatedSource, errors) = RunGenerator(source);

        await Assert.That(generatedSource).IsNull();
        await Assert.That(errors).IsEmpty();
    }

    /// <summary>Runs <see cref="AutoPersistGenerator"/> against the given source.</summary>
    /// <param name="source">The C# source text.</param>
    /// <returns>The generated source text (or null) and any error diagnostics.</returns>
    private static (string? GeneratedSource, ImmutableArray<Diagnostic> Errors) RunGenerator(string source)
    {
        var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp13);
        var compilation = CSharpCompilation.Create(
            "AutoPersistTest",
            [CSharpSyntaxTree.ParseText(source, parseOptions)],
            TestCompilationReferences.CreatePortableDefault(),
            new(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [new AutoPersistGenerator().AsSourceGenerator()],
            parseOptions: parseOptions);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);

        var runResult = driver.GetRunResult();
        var generated = !runResult.GeneratedTrees.IsEmpty ? runResult.GeneratedTrees[0].ToString() : null;
        var errors = outputCompilation.GetDiagnostics()
            .Where(static d => d.Severity == DiagnosticSeverity.Error)
            .ToImmutableArray();

        return (generated, errors);
    }
}
