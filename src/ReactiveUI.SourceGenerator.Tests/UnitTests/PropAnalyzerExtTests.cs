// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>Extended unit tests for <see cref="PropertyToReactiveFieldAnalyzer" />.</summary>
public sealed class PropAnalyzerExtTests
{
    /// <summary>Identifies the diagnostic validated by this test class.</summary>
    private const string ReactiveFieldDiagnosticId = "RXUISG0016";

    /// <summary>Defines the expected number of diagnostics for the multi-diagnostic test.</summary>
    private const int ExpectedDiagnosticCount = 3;

    /// <summary>Validates a static property does not trigger the diagnostic.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task StaticNoDiag()
    {
        const string source = """
            using ReactiveUI;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                public static string StaticProperty { get; set; } = string.Empty;
            }
            """;

        var diagnostics = await GetDiagnostics(source);

        await AssertDoesNotContainDiagnostic(diagnostics, ReactiveFieldDiagnosticId);
    }

    /// <summary>Validates a property with private setter does not trigger the diagnostic.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task WhenPrivateSetterThenDoesNotReportDiagnostic()
    {
        const string source = """
            using ReactiveUI;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                public string Name { get; private set; } = string.Empty;
            }
            """;

        var diagnostics = await GetDiagnostics(source);

        await AssertDoesNotContainDiagnostic(diagnostics, ReactiveFieldDiagnosticId);
    }

    /// <summary>Validates a property with internal setter does not trigger the diagnostic.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task WhenInternalSetterThenDoesNotReportDiagnostic()
    {
        const string source = """
            using ReactiveUI;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                public string Name { get; internal set; } = string.Empty;
            }
            """;

        var diagnostics = await GetDiagnostics(source);

        await AssertDoesNotContainDiagnostic(diagnostics, ReactiveFieldDiagnosticId);
    }

    /// <summary>Validates a read-only property does not trigger the diagnostic.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task WhenReadOnlyPropertyThenDoesNotReportDiagnostic()
    {
        const string source = """
            using ReactiveUI;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                public string Name { get; } = string.Empty;
            }
            """;

        var diagnostics = await GetDiagnostics(source);

        await AssertDoesNotContainDiagnostic(diagnostics, ReactiveFieldDiagnosticId);
    }

    /// <summary>Validates a computed property does not trigger the diagnostic.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task WhenComputedPropertyThenDoesNotReportDiagnostic()
    {
        const string source = """
            using ReactiveUI;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                private string _name = string.Empty;
                public string Name => _name;
            }
            """;

        var diagnostics = await GetDiagnostics(source);

        await AssertDoesNotContainDiagnostic(diagnostics, ReactiveFieldDiagnosticId);
    }

    /// <summary>Validates a property with getter body does not trigger the diagnostic.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task WhenPropertyWithGetterBodyThenDoesNotReportDiagnostic()
    {
        const string source = """
            using ReactiveUI;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                private string _name = string.Empty;
                public string Name
                {
                    get { return _name; }
                    set { _name = value; }
                }
            }
            """;

        var diagnostics = await GetDiagnostics(source);

        await AssertDoesNotContainDiagnostic(diagnostics, ReactiveFieldDiagnosticId);
    }

    /// <summary>Validates a ReactiveCommand property type is ignored.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task WhenReactiveCommandPropertyThenDoesNotReportDiagnostic()
    {
        const string source = """
            using ReactiveUI;
            using System.Reactive;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                public ReactiveCommand<Unit, Unit> MyCommand { get; set; }
            }
            """;

        var diagnostics = await GetDiagnostics(source);

        await AssertDoesNotContainDiagnostic(diagnostics, ReactiveFieldDiagnosticId);
    }

    /// <summary>Validates a ViewModelActivator property type is ignored.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task WhenViewModelActivatorPropertyThenDoesNotReportDiagnostic()
    {
        const string source = """
            using ReactiveUI;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                public ViewModelActivator Activator { get; set; }
            }
            """;

        var diagnostics = await GetDiagnostics(source);

        await AssertDoesNotContainDiagnostic(diagnostics, ReactiveFieldDiagnosticId);
    }

    /// <summary>Validates a property already annotated with ObservableAsProperty is ignored.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task WhenObservableAsPropertyPresentThenDoesNotReportDiagnostic()
    {
        const string source = """
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [ObservableAsProperty]
                public bool IsLoading { get; set; }
            }
            """;

        var diagnostics = await GetDiagnostics(source);

        await AssertDoesNotContainDiagnostic(diagnostics, ReactiveFieldDiagnosticId);
    }

    /// <summary>Validates multiple public auto-properties trigger multiple diagnostics.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task WhenMultiplePublicAutoPropertiesThenReportsMultipleDiagnostics()
    {
        const string source = """
            using ReactiveUI;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                public string Name { get; set; } = string.Empty;
                public int Age { get; set; }
                public bool IsActive { get; set; }
            }
            """;

        var diagnostics = await GetDiagnostics(source);

        await AssertDiagnosticCount(diagnostics, ReactiveFieldDiagnosticId, ExpectedDiagnosticCount);
    }

    /// <summary>Validates non-ReactiveObject class does not trigger the diagnostic.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task WhenNotReactiveObjectThenDoesNotReportDiagnostic()
    {
        const string source = """
            namespace TestNs;

            public class TestVM
            {
                public string Name { get; set; } = string.Empty;
            }
            """;

        var diagnostics = await GetDiagnostics(source);

        await AssertDoesNotContainDiagnostic(diagnostics, ReactiveFieldDiagnosticId);
    }

    /// <summary>Validates IReactiveObject implementation triggers the diagnostic.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task WhenIReactiveObjectThenReportsDiagnostic()
    {
        const string source = """
            using ReactiveUI;

            namespace TestNs;

            public partial class TestVM : IReactiveObject
            {
                public string Name { get; set; } = string.Empty;

                public void RaisePropertyChanging(PropertyChangingEventArgs args) { }
                public void RaisePropertyChanged(PropertyChangedEventArgs args) { }
            }
            """;

        var diagnostics = await GetDiagnostics(source);

        await AssertContainsDiagnostic(diagnostics, ReactiveFieldDiagnosticId);
    }

    /// <summary>Validates nested class inheriting ReactiveObject triggers the diagnostic.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task WhenNestedReactiveObjectThenReportsDiagnostic()
    {
        const string source = """
            using ReactiveUI;

            namespace TestNs;

            public partial class OuterVM : ReactiveObject
            {
                public partial class InnerVM : ReactiveObject
                {
                    public string InnerName { get; set; } = string.Empty;
                }
            }
            """;

        var diagnostics = await GetDiagnostics(source);

        await AssertContainsDiagnostic(diagnostics, ReactiveFieldDiagnosticId);
    }

    /// <summary>Validates protected property does not trigger the diagnostic.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task WhenProtectedPropertyThenDoesNotReportDiagnostic()
    {
        const string source = """
            using ReactiveUI;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                protected string Name { get; set; } = string.Empty;
            }
            """;

        var diagnostics = await GetDiagnostics(source);

        await AssertDoesNotContainDiagnostic(diagnostics, ReactiveFieldDiagnosticId);
    }

    /// <summary>Validates internal property does not trigger the diagnostic.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task WhenInternalPropertyThenDoesNotReportDiagnostic()
    {
        const string source = """
            using ReactiveUI;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                internal string Name { get; set; } = string.Empty;
            }
            """;

        var diagnostics = await GetDiagnostics(source);

        await AssertDoesNotContainDiagnostic(diagnostics, ReactiveFieldDiagnosticId);
    }

    /// <summary>Validates private property does not trigger the diagnostic.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task WhenPrivatePropertyThenDoesNotReportDiagnostic()
    {
        const string source = """
            using ReactiveUI;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                private string Name { get; set; } = string.Empty;
            }
            """;

        var diagnostics = await GetDiagnostics(source);

        await AssertDoesNotContainDiagnostic(diagnostics, ReactiveFieldDiagnosticId);
    }

    /// <summary>Validates property in class directly inheriting ReactiveObject triggers the diagnostic.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task DirectInherit()
    {
        const string source = """
            using ReactiveUI;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                public string TestProperty { get; set; } = string.Empty;
            }
            """;

        var diagnostics = await GetDiagnostics(source);

        await AssertContainsDiagnostic(diagnostics, ReactiveFieldDiagnosticId);
    }

    /// <summary>Validates init-only property triggers the diagnostic (has init setter).</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task InitOnlyDiag()
    {
        const string source = """
            using ReactiveUI;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                public string Name { get; init; } = string.Empty;
            }
            """;

        var diagnostics = await GetDiagnostics(source);

        // Init-only properties have a setter (init), so the analyzer reports them
        await AssertContainsDiagnostic(diagnostics, ReactiveFieldDiagnosticId);
    }

    /// <summary>Validates required property triggers the diagnostic.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task RequiredDiag()
    {
        const string source = """
            using ReactiveUI;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                public required string Name { get; set; }
            }
            """;

        var diagnostics = await GetDiagnostics(source);

        await AssertContainsDiagnostic(diagnostics, ReactiveFieldDiagnosticId);
    }

    /// <summary>Gets diagnostics produced by the analyzer for the supplied source.</summary>
    /// <param name="source">The source to analyze.</param>
    /// <returns>A task that resolves to the diagnostics.</returns>
    private static async Task<Diagnostic[]> GetDiagnostics(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp13));

        var compilation = CSharpCompilation.Create(
            assemblyName: "AnalyzerTests",
            syntaxTrees: [syntaxTree],
            references: TestCompilationReferences.CreateDefault(),
            options: new(OutputKind.DynamicallyLinkedLibrary));

        var analyzer = new PropertyToReactiveFieldAnalyzer();

        var compilationWithAnalyzers = compilation.WithAnalyzers([analyzer]);
        return (await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync()).ToArray();
    }

    /// <summary>Asserts that the diagnostics contain the specified ID.</summary>
    /// <param name="diagnostics">The diagnostics to inspect.</param>
    /// <param name="diagnosticId">The expected diagnostic ID.</param>
    /// <returns>A task that represents the assertion.</returns>
    private static async Task AssertContainsDiagnostic(IEnumerable<Diagnostic> diagnostics, string diagnosticId)
    {
        var found = false;
        foreach (var diagnostic in diagnostics)
        {
            if (diagnostic.Id == diagnosticId)
            {
                found = true;
                break;
            }
        }

        await Assert.That(found).IsTrue();
    }

    /// <summary>Asserts that the diagnostics do not contain the specified ID.</summary>
    /// <param name="diagnostics">The diagnostics to inspect.</param>
    /// <param name="diagnosticId">The unexpected diagnostic ID.</param>
    /// <returns>A task that represents the assertion.</returns>
    private static async Task AssertDoesNotContainDiagnostic(IEnumerable<Diagnostic> diagnostics, string diagnosticId)
    {
        var found = false;
        foreach (var diagnostic in diagnostics)
        {
            if (diagnostic.Id == diagnosticId)
            {
                found = true;
                break;
            }
        }

        await Assert.That(found).IsFalse();
    }

    /// <summary>Asserts that the diagnostics contain the expected number of occurrences of an ID.</summary>
    /// <param name="diagnostics">The diagnostics to inspect.</param>
    /// <param name="diagnosticId">The diagnostic ID to count.</param>
    /// <param name="expectedCount">The expected number of occurrences.</param>
    /// <returns>A task that represents the assertion.</returns>
    private static async Task AssertDiagnosticCount(IEnumerable<Diagnostic> diagnostics, string diagnosticId, int expectedCount)
    {
        var actualCount = 0;
        foreach (var diagnostic in diagnostics)
        {
            if (diagnostic.Id == diagnosticId)
            {
                actualCount++;
            }
        }

        await Assert.That(actualCount).IsEqualTo(expectedCount);
    }
}
