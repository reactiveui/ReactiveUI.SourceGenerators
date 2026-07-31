// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TUnit.Assertions;
using TUnit.Core;

namespace ReactiveUI.SourceGenerators.V23.Tests;

/// <summary>Verifies source generation against the real pinned ReactiveUI 23 package.</summary>
public sealed class ReactiveUiV23CompatibilityTests
{
    /// <summary>The ReactiveUI package version intentionally covered by this project.</summary>
    private const string ExpectedReactiveUiVersion = "23.2.28";

    /// <summary>ReactiveUI 23 produces compilable legacy command output.</summary>
    /// <returns>A task representing the asynchronous assertion work.</returns>
    [Test]
    public async Task RealReactiveUiPackageUsesLegacyGeneratorProfile()
    {
        var reactiveUiAssembly = typeof(ReactiveObject).Assembly;
        var informationalVersion = reactiveUiAssembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion.Split('+')[0]
            ?? throw new InvalidOperationException("ReactiveUI did not expose an informational version.");
        var compilation = CreateCompilation(reactiveUiAssembly);
        GeneratorDriver driver = CSharpGeneratorDriver
            .Create([new ReactiveCommandGenerator(), new ReactiveGenerator()])
            .WithUpdatedParseOptions((CSharpParseOptions)compilation.SyntaxTrees.Single().Options);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var generatorDiagnostics);
        var generatedSource = GetGeneratedCommandSource(driver);
        var errors = GetErrors(outputCompilation, generatorDiagnostics);

        await Assert.That(informationalVersion).IsEqualTo(ExpectedReactiveUiVersion);
        await Assert.That(generatedSource.Contains("global::ReactiveUI.ReactiveCommand", StringComparison.Ordinal)).IsTrue();
        await Assert.That(generatedSource.Contains("global::System.Reactive.Unit", StringComparison.Ordinal)).IsTrue();
        await Assert.That(generatedSource.Contains("ReactiveUI.Primitives", StringComparison.Ordinal)).IsFalse();
        await Assert.That(errors).IsEmpty();
    }

    /// <summary>Creates a consumer compilation rooted in the real ReactiveUI 23 assembly.</summary>
    /// <param name="reactiveUiAssembly">The pinned ReactiveUI assembly.</param>
    /// <returns>The consumer compilation.</returns>
    private static CSharpCompilation CreateCompilation(Assembly reactiveUiAssembly)
    {
        const string source = """
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace Compatibility;

            public partial class ViewModel : ReactiveObject
            {
                [Reactive]
                private string? _name;

                [ReactiveCommand]
                private void Save()
                {
                }
            }
            """;
        var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp13);
        return CSharpCompilation.Create(
            "ReactiveUiV23Consumer",
            [CSharpSyntaxTree.ParseText(SourceText.From(source), parseOptions)],
            CreateReferences(reactiveUiAssembly),
            new(OutputKind.DynamicallyLinkedLibrary));
    }

    /// <summary>Creates framework and package references for the compatibility compilation.</summary>
    /// <param name="reactiveUiAssembly">The pinned ReactiveUI assembly.</param>
    /// <returns>The metadata references.</returns>
    private static ImmutableArray<MetadataReference> CreateReferences(Assembly reactiveUiAssembly)
    {
        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var references = ImmutableArray.CreateBuilder<MetadataReference>();
        var trustedPlatformAssemblies = (string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") ?? string.Empty;
        foreach (var path in trustedPlatformAssemblies.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            if (paths.Add(path))
            {
                references.Add(MetadataReference.CreateFromFile(path));
            }
        }

        AddReference(reactiveUiAssembly.Location, paths, references);
        AddReference(typeof(System.Reactive.Unit).Assembly.Location, paths, references);
        return references.ToImmutable();
    }

    /// <summary>Adds a metadata reference when its path has not already been included.</summary>
    /// <param name="path">The assembly path.</param>
    /// <param name="paths">The known paths.</param>
    /// <param name="references">The destination references.</param>
    private static void AddReference(
        string path,
        HashSet<string> paths,
        ImmutableArray<MetadataReference>.Builder references)
    {
        if (!paths.Add(path))
        {
            return;
        }

        references.Add(MetadataReference.CreateFromFile(path));
    }

    /// <summary>Gets the generated command source from a completed driver run.</summary>
    /// <param name="driver">The completed generator driver.</param>
    /// <returns>The generated command source.</returns>
    private static string GetGeneratedCommandSource(GeneratorDriver driver)
    {
        foreach (var result in driver.GetRunResult().Results)
        {
            foreach (var source in result.GeneratedSources)
            {
                if (source.HintName.EndsWith(".ReactiveCommands.g.cs", StringComparison.Ordinal))
                {
                    return source.SourceText.ToString();
                }
            }
        }

        throw new InvalidOperationException("ReactiveCommandGenerator did not produce command source.");
    }

    /// <summary>Formats generator and compilation errors.</summary>
    /// <param name="compilation">The generated output compilation.</param>
    /// <param name="generatorDiagnostics">The generator diagnostics.</param>
    /// <returns>A newline-delimited error string.</returns>
    private static string GetErrors(Compilation compilation, ImmutableArray<Diagnostic> generatorDiagnostics)
    {
        var errors = new List<string>();
        foreach (var diagnostic in generatorDiagnostics)
        {
            if (diagnostic.Severity == DiagnosticSeverity.Error)
            {
                errors.Add(diagnostic.ToString());
            }
        }

        foreach (var diagnostic in compilation.GetDiagnostics())
        {
            if (diagnostic.Severity == DiagnosticSeverity.Error)
            {
                errors.Add(diagnostic.ToString());
            }
        }

        return string.Join(Environment.NewLine, errors);
    }
}
