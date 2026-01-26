// Copyright (c) 2026 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>
/// Extended unit tests for <see cref="PropertyToReactiveFieldAnalyzer" />.
/// </summary>
[TestFixture]
public sealed class PropAnalyzerExtTests
{
    /// <summary>
    /// Validates a static property does not trigger the diagnostic.
    /// </summary>
    [Test]
    public void StaticNoDiag()
    {
        const string source = """
            using ReactiveUI;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                public static string StaticProperty { get; set; } = string.Empty;
            }
            """;

        var diagnostics = GetDiagnostics(source);

        Assert.That(diagnostics.Any(d => d.Id == "RXUISG0016"), Is.False);
    }

    /// <summary>
    /// Validates a property with private setter does not trigger the diagnostic.
    /// </summary>
    [Test]
    public void WhenPrivateSetterThenDoesNotReportDiagnostic()
    {
        const string source = """
            using ReactiveUI;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                public string Name { get; private set; } = string.Empty;
            }
            """;

        var diagnostics = GetDiagnostics(source);

        Assert.That(diagnostics.Any(d => d.Id == "RXUISG0016"), Is.False);
    }

    /// <summary>
    /// Validates a property with internal setter does not trigger the diagnostic.
    /// </summary>
    [Test]
    public void WhenInternalSetterThenDoesNotReportDiagnostic()
    {
        const string source = """
            using ReactiveUI;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                public string Name { get; internal set; } = string.Empty;
            }
            """;

        var diagnostics = GetDiagnostics(source);

        Assert.That(diagnostics.Any(d => d.Id == "RXUISG0016"), Is.False);
    }

    /// <summary>
    /// Validates a read-only property does not trigger the diagnostic.
    /// </summary>
    [Test]
    public void WhenReadOnlyPropertyThenDoesNotReportDiagnostic()
    {
        const string source = """
            using ReactiveUI;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                public string Name { get; } = string.Empty;
            }
            """;

        var diagnostics = GetDiagnostics(source);

        Assert.That(diagnostics.Any(d => d.Id == "RXUISG0016"), Is.False);
    }

    /// <summary>
    /// Validates a computed property does not trigger the diagnostic.
    /// </summary>
    [Test]
    public void WhenComputedPropertyThenDoesNotReportDiagnostic()
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

        var diagnostics = GetDiagnostics(source);

        Assert.That(diagnostics.Any(d => d.Id == "RXUISG0016"), Is.False);
    }

    /// <summary>
    /// Validates a property with getter body does not trigger the diagnostic.
    /// </summary>
    [Test]
    public void WhenPropertyWithGetterBodyThenDoesNotReportDiagnostic()
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

        var diagnostics = GetDiagnostics(source);

        Assert.That(diagnostics.Any(d => d.Id == "RXUISG0016"), Is.False);
    }

    /// <summary>
    /// Validates a ReactiveCommand property type is ignored.
    /// </summary>
    [Test]
    public void WhenReactiveCommandPropertyThenDoesNotReportDiagnostic()
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

        var diagnostics = GetDiagnostics(source);

        Assert.That(diagnostics.Any(d => d.Id == "RXUISG0016"), Is.False);
    }

    /// <summary>
    /// Validates a ViewModelActivator property type is ignored.
    /// </summary>
    [Test]
    public void WhenViewModelActivatorPropertyThenDoesNotReportDiagnostic()
    {
        const string source = """
            using ReactiveUI;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                public ViewModelActivator Activator { get; set; }
            }
            """;

        var diagnostics = GetDiagnostics(source);

        Assert.That(diagnostics.Any(d => d.Id == "RXUISG0016"), Is.False);
    }

    /// <summary>
    /// Validates a property already annotated with ObservableAsProperty is ignored.
    /// </summary>
    [Test]
    public void WhenObservableAsPropertyPresentThenDoesNotReportDiagnostic()
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

        var diagnostics = GetDiagnostics(source);

        Assert.That(diagnostics.Any(d => d.Id == "RXUISG0016"), Is.False);
    }

    /// <summary>
    /// Validates multiple public auto-properties trigger multiple diagnostics.
    /// </summary>
    [Test]
    public void WhenMultiplePublicAutoPropertiesThenReportsMultipleDiagnostics()
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

        var diagnostics = GetDiagnostics(source);

        Assert.That(diagnostics.Count(d => d.Id == "RXUISG0016"), Is.EqualTo(3));
    }

    /// <summary>
    /// Validates non-ReactiveObject class does not trigger the diagnostic.
    /// </summary>
    [Test]
    public void WhenNotReactiveObjectThenDoesNotReportDiagnostic()
    {
        const string source = """
            namespace TestNs;

            public class TestVM
            {
                public string Name { get; set; } = string.Empty;
            }
            """;

        var diagnostics = GetDiagnostics(source);

        Assert.That(diagnostics.Any(d => d.Id == "RXUISG0016"), Is.False);
    }

    /// <summary>
    /// Validates IReactiveObject implementation triggers the diagnostic.
    /// </summary>
    [Test]
    public void WhenIReactiveObjectThenReportsDiagnostic()
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

        var diagnostics = GetDiagnostics(source);

        Assert.That(diagnostics.Any(d => d.Id == "RXUISG0016"), Is.True);
    }

    /// <summary>
    /// Validates nested class inheriting ReactiveObject triggers the diagnostic.
    /// </summary>
    [Test]
    public void WhenNestedReactiveObjectThenReportsDiagnostic()
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

        var diagnostics = GetDiagnostics(source);

        Assert.That(diagnostics.Any(d => d.Id == "RXUISG0016"), Is.True);
    }

    /// <summary>
    /// Validates protected property does not trigger the diagnostic.
    /// </summary>
    [Test]
    public void WhenProtectedPropertyThenDoesNotReportDiagnostic()
    {
        const string source = """
            using ReactiveUI;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                protected string Name { get; set; } = string.Empty;
            }
            """;

        var diagnostics = GetDiagnostics(source);

        Assert.That(diagnostics.Any(d => d.Id == "RXUISG0016"), Is.False);
    }

    /// <summary>
    /// Validates internal property does not trigger the diagnostic.
    /// </summary>
    [Test]
    public void WhenInternalPropertyThenDoesNotReportDiagnostic()
    {
        const string source = """
            using ReactiveUI;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                internal string Name { get; set; } = string.Empty;
            }
            """;

        var diagnostics = GetDiagnostics(source);

        Assert.That(diagnostics.Any(d => d.Id == "RXUISG0016"), Is.False);
    }

    /// <summary>
    /// Validates private property does not trigger the diagnostic.
    /// </summary>
    [Test]
    public void WhenPrivatePropertyThenDoesNotReportDiagnostic()
    {
        const string source = """
            using ReactiveUI;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                private string Name { get; set; } = string.Empty;
            }
            """;

        var diagnostics = GetDiagnostics(source);

        Assert.That(diagnostics.Any(d => d.Id == "RXUISG0016"), Is.False);
    }

    /// <summary>
    /// Validates property in class directly inheriting ReactiveObject triggers the diagnostic.
    /// </summary>
    [Test]
    public void DirectInherit()
    {
        const string source = """
            using ReactiveUI;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                public string TestProperty { get; set; } = string.Empty;
            }
            """;

        var diagnostics = GetDiagnostics(source);

        Assert.That(diagnostics.Any(d => d.Id == "RXUISG0016"), Is.True);
    }

    /// <summary>
    /// Validates init-only property triggers the diagnostic (has init setter).
    /// </summary>
    [Test]
    public void InitOnlyDiag()
    {
        const string source = """
            using ReactiveUI;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                public string Name { get; init; } = string.Empty;
            }
            """;

        var diagnostics = GetDiagnostics(source);

        // Init-only properties have a setter (init), so the analyzer reports them
        Assert.That(diagnostics.Any(d => d.Id == "RXUISG0016"), Is.True);
    }

    /// <summary>
    /// Validates required property triggers the diagnostic.
    /// </summary>
    [Test]
    public void RequiredDiag()
    {
        const string source = """
            using ReactiveUI;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                public required string Name { get; set; }
            }
            """;

        var diagnostics = GetDiagnostics(source);

        Assert.That(diagnostics.Any(d => d.Id == "RXUISG0016"), Is.True);
    }

    private static Diagnostic[] GetDiagnostics(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp13));

        var compilation = CSharpCompilation.Create(
            assemblyName: "AnalyzerTests",
            syntaxTrees: [syntaxTree],
            references: TestCompilationReferences.CreateDefault(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var analyzer = new PropertyToReactiveFieldAnalyzer();

        var compilationWithAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(analyzer));
        return compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult().ToArray();
    }
}
