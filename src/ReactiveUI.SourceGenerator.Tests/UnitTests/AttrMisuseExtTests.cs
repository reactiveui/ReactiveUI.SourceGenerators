// Copyright (c) 2026 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>
/// Extended unit tests for <see cref="ReactiveAttributeMisuseAnalyzer" />.
/// </summary>
[TestFixture]
public sealed class AttrMisuseExtTests
{
    /// <summary>
    /// Verifies a non-partial property with [Reactive] in a partial type produces a warning.
    /// </summary>
    [Test]
    public void WhenReactiveOnNonPartialPropertyInPartialTypeThenWarn()
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

        var diagnostics = GetDiagnostics(source);

        Assert.That(diagnostics.Any(d => d.Id == "RXUISG0020"), Is.True);
    }

    /// <summary>
    /// Verifies a partial property with [Reactive] in a non-partial type produces a warning.
    /// </summary>
    [Test]
    public void WhenReactiveOnPartialPropertyInNonPartialTypeThenWarn()
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

        var diagnostics = GetDiagnostics(source);

        Assert.That(diagnostics.Any(d => d.Id == "RXUISG0020"), Is.True);
    }

    /// <summary>
    /// Verifies no warning when both property and type are partial.
    /// </summary>
    [Test]
    public void WhenReactiveOnPartialPropertyInPartialTypeThenNoWarn()
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

        var diagnostics = GetDiagnostics(source);

        Assert.That(diagnostics.Any(d => d.Id == "RXUISG0020"), Is.False);
    }

    /// <summary>
    /// Verifies warning in nested non-partial class.
    /// </summary>
    [Test]
    public void WhenReactiveInNestedNonPartialClassThenWarn()
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

        var diagnostics = GetDiagnostics(source);

        Assert.That(diagnostics.Any(d => d.Id == "RXUISG0020"), Is.True);
    }

    /// <summary>
    /// Verifies no warning in nested partial class with partial property.
    /// </summary>
    [Test]
    public void WhenReactiveInNestedPartialClassWithPartialPropertyThenNoWarn()
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

        var diagnostics = GetDiagnostics(source);

        Assert.That(diagnostics.Any(d => d.Id == "RXUISG0020"), Is.False);
    }

    /// <summary>
    /// Verifies warning for non-partial record.
    /// </summary>
    [Test]
    public void WhenReactiveInNonPartialRecordThenWarn()
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

        var diagnostics = GetDiagnostics(source);

        Assert.That(diagnostics.Any(d => d.Id == "RXUISG0020"), Is.True);
    }

    /// <summary>
    /// Verifies no warning for partial record with partial property.
    /// </summary>
    [Test]
    public void WhenReactiveInPartialRecordWithPartialPropertyThenNoWarn()
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

        var diagnostics = GetDiagnostics(source);

        Assert.That(diagnostics.Any(d => d.Id == "RXUISG0020"), Is.False);
    }

    /// <summary>
    /// Verifies warning for multiple non-partial properties.
    /// </summary>
    [Test]
    public void WhenMultipleNonPartialPropertiesWithReactiveThenWarnForEach()
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

        var diagnostics = GetDiagnostics(source);

        Assert.That(diagnostics.Count(d => d.Id == "RXUISG0020"), Is.EqualTo(3));
    }

    /// <summary>
    /// Verifies no warning for field-based [Reactive] attribute.
    /// </summary>
    [Test]
    public void WhenReactiveOnFieldThenNoWarn()
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

        var diagnostics = GetDiagnostics(source);

        Assert.That(diagnostics.Any(d => d.Id == "RXUISG0020"), Is.False);
    }

    /// <summary>
    /// Verifies warning for deeply nested non-partial types.
    /// </summary>
    [Test]
    public void WhenReactiveInDeeplyNestedNonPartialTypeThenWarn()
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

        var diagnostics = GetDiagnostics(source);

        Assert.That(diagnostics.Any(d => d.Id == "RXUISG0020"), Is.True);
    }

    /// <summary>
    /// Verifies no warning for deeply nested partial types with partial properties.
    /// </summary>
    [Test]
    public void WhenReactiveInDeeplyNestedPartialTypesWithPartialPropertyThenNoWarn()
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

        var diagnostics = GetDiagnostics(source);

        Assert.That(diagnostics.Any(d => d.Id == "RXUISG0020"), Is.False);
    }

    /// <summary>
    /// Verifies warning when using fully qualified ReactiveAttribute.
    /// </summary>
    [Test]
    public void WhenFullyQualifiedReactiveAttributeOnNonPartialPropertyThenWarn()
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

        var diagnostics = GetDiagnostics(source);

        Assert.That(diagnostics.Any(d => d.Id == "RXUISG0020"), Is.True);
    }

    /// <summary>
    /// Verifies no warning for properties without [Reactive] attribute.
    /// </summary>
    [Test]
    public void WhenNoReactiveAttributeThenNoWarn()
    {
        const string source = """
            using ReactiveUI;

            namespace TestNs;

            public class TestVM : ReactiveObject
            {
                public string Name { get; set; } = string.Empty;
            }
            """;

        var diagnostics = GetDiagnostics(source);

        Assert.That(diagnostics.Any(d => d.Id == "RXUISG0020"), Is.False);
    }

    /// <summary>
    /// Verifies warning for generic partial class with non-partial property.
    /// </summary>
    [Test]
    public void WhenReactiveOnNonPartialPropertyInGenericPartialClassThenWarn()
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

        var diagnostics = GetDiagnostics(source);

        Assert.That(diagnostics.Any(d => d.Id == "RXUISG0020"), Is.True);
    }

    /// <summary>
    /// Verifies no warning for generic partial class with partial property.
    /// </summary>
    [Test]
    public void WhenReactiveOnPartialPropertyInGenericPartialClassThenNoWarn()
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

        var diagnostics = GetDiagnostics(source);

        Assert.That(diagnostics.Any(d => d.Id == "RXUISG0020"), Is.False);
    }

    /// <summary>
    /// Verifies warning for record struct (non-partial).
    /// </summary>
    [Test]
    public void WhenReactiveOnPropertyInNonPartialRecordStructThenWarn()
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

        var diagnostics = GetDiagnostics(source);

        Assert.That(diagnostics.Any(d => d.Id == "RXUISG0020"), Is.True);
    }

    /// <summary>
    /// Verifies warning for readonly property with [Reactive] that is not partial.
    /// </summary>
    [Test]
    public void NonPartialRead()
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

        var diagnostics = GetDiagnostics(source);

        // [Reactive] on non-partial property should produce RXUISG0020
        // because the attribute requires the property to be partial
        Assert.That(diagnostics.Any(d => d.Id == "RXUISG0020"), Is.True);
    }

    private static Diagnostic[] GetDiagnostics(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp13));

        var compilation = CSharpCompilation.Create(
            assemblyName: "AnalyzerTests",
            syntaxTrees: [syntaxTree],
            references: [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
            ],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var analyzer = new ReactiveAttributeMisuseAnalyzer();

        var compilationWithAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(analyzer));
        return compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult().ToArray();
    }
}
