// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>Extended unit tests for <see cref="ReactiveAttributeMisuseAnalyzer" />.</summary>
public sealed class AttrMisuseExtTests
{
    /// <summary>Identifies the diagnostic validated by this test class.</summary>
    private const string ReactivePartialDiagnosticId = "RXUISG0020";

    /// <summary>Defines the expected number of diagnostics for the multi-diagnostic test.</summary>
    private const int ExpectedDiagnosticCount = 3;

    /// <summary>Verifies a non-partial property with [Reactive] in a partial type produces a warning.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task WhenReactiveOnNonPartialPropertyInPartialTypeThenWarn()
    {
        const string source = """
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [Reactive]
                public string Name { get; set; } = string.Empty;
            }
            """;

        var diagnostics = await GetDiagnostics(source);

        await AssertContainsDiagnostic(diagnostics, ReactivePartialDiagnosticId);
    }

    /// <summary>Verifies a partial property with [Reactive] in a non-partial type produces a warning.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task WhenReactiveOnPartialPropertyInNonPartialTypeThenWarn()
    {
        const string source = """
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public class TestVM : ReactiveObject
            {
                [Reactive]
                public partial string Name { get; set; }
            }
            """;

        var diagnostics = await GetDiagnostics(source);

        await AssertContainsDiagnostic(diagnostics, ReactivePartialDiagnosticId);
    }

    /// <summary>Verifies no warning when both property and type are partial.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task WhenReactiveOnPartialPropertyInPartialTypeThenNoWarn()
    {
        const string source = """
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [Reactive]
                public partial string Name { get; set; }
            }
            """;

        var diagnostics = await GetDiagnostics(source);

        await AssertDoesNotContainDiagnostic(diagnostics, ReactivePartialDiagnosticId);
    }

    /// <summary>Verifies warning in nested non-partial class.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task WhenReactiveInNestedNonPartialClassThenWarn()
    {
        const string source = """
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class OuterVM : ReactiveObject
            {
                public class InnerVM : ReactiveObject
                {
                    [Reactive]
                    public partial string Name { get; set; }
                }
            }
            """;

        var diagnostics = await GetDiagnostics(source);

        await AssertContainsDiagnostic(diagnostics, ReactivePartialDiagnosticId);
    }

    /// <summary>Verifies no warning in nested partial class with partial property.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task WhenReactiveInNestedPartialClassWithPartialPropertyThenNoWarn()
    {
        const string source = """
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class OuterVM : ReactiveObject
            {
                public partial class InnerVM : ReactiveObject
                {
                    [Reactive]
                    public partial string Name { get; set; }
                }
            }
            """;

        var diagnostics = await GetDiagnostics(source);

        await AssertDoesNotContainDiagnostic(diagnostics, ReactivePartialDiagnosticId);
    }

    /// <summary>Verifies warning for non-partial record.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task WhenReactiveInNonPartialRecordThenWarn()
    {
        const string source = """
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public record TestVMRecord : ReactiveObject
            {
                [Reactive]
                public partial string Name { get; set; }
            }
            """;

        var diagnostics = await GetDiagnostics(source);

        await AssertContainsDiagnostic(diagnostics, ReactivePartialDiagnosticId);
    }

    /// <summary>Verifies no warning for partial record with partial property.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task WhenReactiveInPartialRecordWithPartialPropertyThenNoWarn()
    {
        const string source = """
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial record TestVMRecord : ReactiveObject
            {
                [Reactive]
                public partial string Name { get; set; }
            }
            """;

        var diagnostics = await GetDiagnostics(source);

        await AssertDoesNotContainDiagnostic(diagnostics, ReactivePartialDiagnosticId);
    }

    /// <summary>Verifies warning for multiple non-partial properties.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task WhenMultipleNonPartialPropertiesWithReactiveThenWarnForEach()
    {
        const string source = """
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [Reactive]
                public string Name { get; set; } = string.Empty;

                [Reactive]
                public int Age { get; set; }

                [Reactive]
                public bool IsActive { get; set; }
            }
            """;

        var diagnostics = await GetDiagnostics(source);

        await AssertDiagnosticCount(diagnostics, ReactivePartialDiagnosticId, ExpectedDiagnosticCount);
    }

    /// <summary>Verifies no warning for field-based [Reactive] attribute.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task WhenReactiveOnFieldThenNoWarn()
    {
        const string source = """
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [Reactive]
                private string _name = string.Empty;
            }
            """;

        var diagnostics = await GetDiagnostics(source);

        await AssertDoesNotContainDiagnostic(diagnostics, ReactivePartialDiagnosticId);
    }

    /// <summary>Verifies warning for deeply nested non-partial types.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task WhenReactiveInDeeplyNestedNonPartialTypeThenWarn()
    {
        const string source = """
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class Level1 : ReactiveObject
            {
                public partial class Level2 : ReactiveObject
                {
                    public class Level3 : ReactiveObject
                    {
                        [Reactive]
                        public partial string Name { get; set; }
                    }
                }
            }
            """;

        var diagnostics = await GetDiagnostics(source);

        await AssertContainsDiagnostic(diagnostics, ReactivePartialDiagnosticId);
    }

    /// <summary>Verifies no warning for deeply nested partial types with partial properties.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task WhenReactiveInDeeplyNestedPartialTypesWithPartialPropertyThenNoWarn()
    {
        const string source = """
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class Level1 : ReactiveObject
            {
                public partial class Level2 : ReactiveObject
                {
                    public partial class Level3 : ReactiveObject
                    {
                        [Reactive]
                        public partial string Name { get; set; }
                    }
                }
            }
            """;

        var diagnostics = await GetDiagnostics(source);

        await AssertDoesNotContainDiagnostic(diagnostics, ReactivePartialDiagnosticId);
    }

    /// <summary>Verifies warning when using fully qualified ReactiveAttribute.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task WhenFullyQualifiedReactiveAttributeOnNonPartialPropertyThenWarn()
    {
        const string source = """
            using ReactiveUI;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [ReactiveUI.SourceGenerators.Reactive]
                public string Name { get; set; } = string.Empty;
            }
            """;

        var diagnostics = await GetDiagnostics(source);

        await AssertContainsDiagnostic(diagnostics, ReactivePartialDiagnosticId);
    }

    /// <summary>Verifies no warning for properties without [Reactive] attribute.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task WhenNoReactiveAttributeThenNoWarn()
    {
        const string source = """
            using ReactiveUI;

            namespace TestNs;

            public class TestVM : ReactiveObject
            {
                public string Name { get; set; } = string.Empty;
            }
            """;

        var diagnostics = await GetDiagnostics(source);

        await AssertDoesNotContainDiagnostic(diagnostics, ReactivePartialDiagnosticId);
    }

    /// <summary>Verifies warning for generic partial class with non-partial property.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task WhenReactiveOnNonPartialPropertyInGenericPartialClassThenWarn()
    {
        const string source = """
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class GenericVM<T> : ReactiveObject where T : class
            {
                [Reactive]
                public T Item { get; set; } = default!;
            }
            """;

        var diagnostics = await GetDiagnostics(source);

        await AssertContainsDiagnostic(diagnostics, ReactivePartialDiagnosticId);
    }

    /// <summary>Verifies no warning for generic partial class with partial property.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task WhenReactiveOnPartialPropertyInGenericPartialClassThenNoWarn()
    {
        const string source = """
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class GenericVM<T> : ReactiveObject where T : class
            {
                [Reactive]
                public partial T? Item { get; set; }
            }
            """;

        var diagnostics = await GetDiagnostics(source);

        await AssertDoesNotContainDiagnostic(diagnostics, ReactivePartialDiagnosticId);
    }

    /// <summary>Verifies warning for record struct (non-partial).</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task WhenReactiveOnPropertyInNonPartialRecordStructThenWarn()
    {
        const string source = """
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public record struct TestVMStruct
            {
                [Reactive]
                public partial string Name { get; set; }
            }
            """;

        var diagnostics = await GetDiagnostics(source);

        await AssertContainsDiagnostic(diagnostics, ReactivePartialDiagnosticId);
    }

    /// <summary>Verifies warning for readonly property with [Reactive] that is not partial.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task NonPartialRead()
    {
        const string source = """
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [Reactive]
                public string Name { get; }
            }
            """;

        var diagnostics = await GetDiagnostics(source);

        // [Reactive] on non-partial property should produce RXUISG0020
        // because the attribute requires the property to be partial
        await AssertContainsDiagnostic(diagnostics, ReactivePartialDiagnosticId);
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
            references: [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
            ],
            options: new(OutputKind.DynamicallyLinkedLibrary));

        var analyzer = new ReactiveAttributeMisuseAnalyzer();

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
