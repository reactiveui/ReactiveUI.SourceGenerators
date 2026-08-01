// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>End-to-end coverage for generated-output validation and option branches.</summary>
public sealed class GeneratedOutputCoverageTests
{
    /// <summary>The diagnostic emitted when a field name collides with its generated property.</summary>
    private const string NameCollisionDiagnosticId = "RXUISG0009";

    /// <summary>ReactiveObject ignores attributed classes that cannot receive a partial implementation.</summary>
    /// <returns>A task representing the asynchronous assertion work.</returns>
    [Test]
    public async Task ReactiveObjectSkipsNonPartialTargets()
    {
        const string source = """
            using ReactiveUI.SourceGenerators;
            namespace Coverage;

            [ReactiveObject]
            public class NonPartialViewModel
            {
            }
            """;

        var (generatedSource, diagnostics) = RunGenerator(source, new ReactiveObjectGenerator());

        await Assert.That(generatedSource.Contains("PropertyChangedEventHandler? PropertyChanged", StringComparison.Ordinal)).IsFalse();
        await Assert.That(diagnostics).IsEmpty();
    }

    /// <summary>ReactiveCollection reports invalid targets and collisions while forwarding valid attributes.</summary>
    /// <returns>A task representing the asynchronous assertion work.</returns>
    [Test]
    public async Task ReactiveCollectionCoversInvalidCollisionAndForwardedOutput()
    {
        const string source = """
            using System.Collections.ObjectModel;
            using System.Runtime.Serialization;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace Coverage;

            public partial class InvalidCollectionTarget
            {
                [ReactiveCollection]
                private ObservableCollection<int> _invalid = new();
            }

            public partial class CollectionViewModel : ReactiveObject
            {
                [ReactiveCollection]
                private ObservableCollection<int> Items = new();

                [DataMember, ReactiveCollection]
                private ObservableCollection<int> _forwarded = new();
            }
            """;

        var (generatedSource, diagnostics) = RunGenerator(source, new ReactiveCollectionGenerator());
        var diagnosticIds = GetDiagnosticIds(diagnostics);

        await Assert.That(diagnosticIds).Contains("RXUISG0018");
        await Assert.That(diagnosticIds).Contains(NameCollisionDiagnosticId);
        await Assert.That(generatedSource.Contains("DataMemberAttribute", StringComparison.Ordinal)).IsTrue();
    }

    /// <summary>BindableDerivedList reports invalid types and collisions while forwarding valid attributes.</summary>
    /// <returns>A task representing the asynchronous assertion work.</returns>
    [Test]
    public async Task BindableDerivedListCoversInvalidCollisionAndForwardedOutput()
    {
        const string source = """
            using System.Collections.ObjectModel;
            using System.Runtime.Serialization;
            using DynamicData;
            using ReactiveUI.SourceGenerators;

            namespace Coverage;

            public partial class DerivedListViewModel
            {
                [BindableDerivedList]
                private int _invalid;

                [BindableDerivedList]
                private ReadOnlyObservableCollection<int> Items = null!;

                [DataMember, BindableDerivedList]
                private ReadOnlyObservableCollection<int> _forwarded = null!;
            }
            """;

        var (generatedSource, diagnostics) = RunGenerator(source, new BindableDerivedListGenerator());
        var diagnosticIds = GetDiagnosticIds(diagnostics);

        await Assert.That(diagnosticIds).Contains("RXUISG0019");
        await Assert.That(diagnosticIds).Contains(NameCollisionDiagnosticId);
        await Assert.That(generatedSource.Contains("DataMemberAttribute", StringComparison.Ordinal)).IsTrue();
    }

    /// <summary>ObservableAsProperty fields cover invalid targets, collisions, and inheritance modifiers.</summary>
    /// <returns>A task representing the asynchronous assertion work.</returns>
    [Test]
    public async Task ObservableAsPropertyFieldsCoverValidationAndInheritanceOutput()
    {
        const string source = """
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace Coverage;

            public partial class InvalidObservableTarget
            {
                [ObservableAsProperty]
                private int _invalid;
            }

            public class ObservableBase : ReactiveObject
            {
                public virtual int Override { get; set; }
            }

            public partial class ObservableViewModel : ObservableBase
            {
                [ObservableAsProperty]
                private int Collision;

                [ObservableAsProperty(Inheritance = InheritanceModifier.Virtual)]
                private int _virtual;

                [ObservableAsProperty(Inheritance = InheritanceModifier.Override)]
                private int _override;

                [ObservableAsProperty(Inheritance = InheritanceModifier.New)]
                private int _new;
            }
            """;

        var (generatedSource, diagnostics) = RunGenerator(
            source,
            new ObservableAsPropertyGenerator(),
            new ReactiveGenerator());
        var diagnosticIds = GetDiagnosticIds(diagnostics);

        await Assert.That(diagnosticIds).Contains("RXUISG0018");
        await Assert.That(diagnosticIds).Contains(NameCollisionDiagnosticId);
        await Assert.That(generatedSource).Contains(" virtual int Virtual");
        await Assert.That(generatedSource).Contains(" override int Override");
        await Assert.That(generatedSource).Contains(" new int New");
    }

    /// <summary>Runs one generator and returns its product source and diagnostics.</summary>
    /// <param name="source">The consumer source.</param>
    /// <param name="generators">The generators to execute together.</param>
    /// <returns>The concatenated generated source and generator diagnostics.</returns>
    private static (string GeneratedSource, ImmutableArray<Diagnostic> Diagnostics) RunGenerator(
        string source,
        params IIncrementalGenerator[] generators)
    {
        var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);
        var compilation = CSharpCompilation.Create(
            "GeneratedOutputCoverageConsumer",
            [CSharpSyntaxTree.ParseText(SourceText.From(source, Encoding.UTF8), parseOptions)],
            TestCompilationReferences.CreateDefault(),
            new(OutputKind.DynamicallyLinkedLibrary));
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generators).WithUpdatedParseOptions(parseOptions);
        driver = driver.RunGenerators(compilation);
        var generatedSource = new StringBuilder();
        foreach (var result in driver.GetRunResult().Results)
        {
            foreach (var generated in result.GeneratedSources)
            {
                _ = generatedSource.AppendLine(generated.SourceText.ToString());
            }
        }

        return (generatedSource.ToString(), driver.GetRunResult().Diagnostics);
    }

    /// <summary>Copies diagnostic identifiers without a LINQ iterator chain.</summary>
    /// <param name="diagnostics">The diagnostics to inspect.</param>
    /// <returns>The diagnostic identifiers.</returns>
    private static string[] GetDiagnosticIds(ImmutableArray<Diagnostic> diagnostics)
    {
        var ids = new string[diagnostics.Length];
        for (var index = 0; index < diagnostics.Length; index++)
        {
            ids[index] = diagnostics[index].Id;
        }

        return ids;
    }
}
