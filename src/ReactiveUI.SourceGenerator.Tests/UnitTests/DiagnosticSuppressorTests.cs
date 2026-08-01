// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Diagnostics;
using ReactiveUI.SourceGenerators.Diagnostics.Suppressions;
using AssemblyLoadContext = System.Runtime.Loader.AssemblyLoadContext;

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>Tests the diagnostic suppression contracts exposed by the generators.</summary>
public sealed class DiagnosticSuppressorTests
{
    /// <summary>The compiler diagnostic for an invalid attribute target.</summary>
    private const string InvalidAttributeTargetDiagnosticId = "CS0657";

    /// <summary>The number of invalid target diagnostics exercised together.</summary>
    private const int InvalidTargetDiagnosticCount = 4;

    /// <summary>The number of synthetic diagnostics expected to be suppressed.</summary>
    private const int SyntheticSuppressedDiagnosticCount = 8;

    /// <summary>The number of synthetic diagnostics intentionally left unsuppressed.</summary>
    private const int SyntheticUnsuppressedDiagnosticCount = 7;

    /// <summary>Source containing each invalid generated-member attribute target.</summary>
    private const string AttributeTargetSource = """
        using System;

        namespace ReactiveUI.SourceGenerators
        {
            [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
            public sealed class ReactiveAttribute : Attribute;

            [AttributeUsage(AttributeTargets.Method)]
            public sealed class ReactiveCommandAttribute : Attribute;
        }

        namespace TestNs
        {
            public partial class ViewModel
            {
                [ReactiveUI.SourceGenerators.Reactive]
                [field: Obsolete]
                public partial string Name { get; set; }

                [ReactiveUI.SourceGenerators.Reactive]
                [property: Obsolete]
                private string _name = string.Empty;

                [ReactiveUI.SourceGenerators.ReactiveCommand]
                [field: Obsolete]
                partial void Save();

                [ReactiveUI.SourceGenerators.ReactiveCommand]
                [property: Obsolete]
                partial void SavePropertyTarget();

                [field: Obsolete]
                partial void PlainCommandTarget();

                [field: Obsolete]
                public string PlainProperty { get; set; } = string.Empty;
            }
        }
        """;

    /// <summary>Source covering member and attribute-target suppression decisions.</summary>
    private const string MemberSuppressionSource = """
        using System;

        namespace ReactiveUI.SourceGenerators
        {
            [AttributeUsage(AttributeTargets.Field)]
            public sealed class ReactiveAttribute : Attribute;

            [AttributeUsage(AttributeTargets.Method)]
            public sealed class ReactiveCommandAttribute : Attribute;

            [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
            public sealed class ObservableAsPropertyAttribute : Attribute;
        }

        namespace TestNs
        {
            public partial class ViewModel
            {
                [ReactiveUI.SourceGenerators.Reactive]
                private string _reactive = string.Empty;

                private string _ordinary = string.Empty;

                [ReactiveUI.SourceGenerators.ReactiveCommand]
                private void Save()
                {
                }

                private void Plain()
                {
                }

                [ReactiveUI.SourceGenerators.ObservableAsProperty]
                [field: Obsolete]
                private object Observe() => new();

                [field: Obsolete]
                private object NotObservable() => new();

                [ReactiveUI.SourceGenerators.ObservableAsProperty]
                private object ObservableValue => new();

                [ReactiveUI.SourceGenerators.ObservableAsProperty]
                [property: Obsolete]
                private object ObservePropertyTarget() => new();

                [method: Obsolete]
                private object MethodTarget() => new();

                [ReactiveUI.SourceGenerators.ObservableAsProperty]
                [field: Obsolete]
                private object ObservablePropertyTarget => new();
            }
        }
        """;

    /// <summary>Analyzer source that reports deterministic diagnostics for production suppressors.</summary>
    private const string MemberCoverageAnalyzerSource = """
        using System.Collections.Immutable;
        using Microsoft.CodeAnalysis;
        using Microsoft.CodeAnalysis.CSharp;
        using Microsoft.CodeAnalysis.CSharp.Syntax;
        using Microsoft.CodeAnalysis.Diagnostics;

        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public sealed class MemberCoverageDiagnosticAnalyzer : DiagnosticAnalyzer
        {
            private static readonly DiagnosticDescriptor DoesNotAccessInstanceData =
                new("CA1822", "Member can be static", "Member can be static", "Tests", DiagnosticSeverity.Warning, true);

            private static readonly DiagnosticDescriptor FieldNeverRead =
                new("IDE0052", "Field is never read", "Field is never read", "Tests", DiagnosticSeverity.Warning, true);

            private static readonly DiagnosticDescriptor FieldCanBeReadOnly =
                new("RCS1169", "Field can be read-only", "Field can be read-only", "Tests", DiagnosticSeverity.Warning, true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
                [DoesNotAccessInstanceData, FieldNeverRead, FieldCanBeReadOnly];

            public override void Initialize(AnalysisContext context)
            {
                context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
                context.EnableConcurrentExecution();
                context.RegisterSyntaxNodeAction(AnalyzeMember, SyntaxKind.MethodDeclaration, SyntaxKind.PropertyDeclaration, SyntaxKind.FieldDeclaration);
                context.RegisterSyntaxNodeAction(AnalyzeAttributeTarget, SyntaxKind.AttributeTargetSpecifier);
            }

            private static void AnalyzeMember(SyntaxNodeAnalysisContext context)
            {
                var descriptor = context.Node is FieldDeclarationSyntax
                    ? FieldCanBeReadOnly
                    : DoesNotAccessInstanceData;
                context.ReportDiagnostic(Diagnostic.Create(descriptor, context.Node.GetLocation()));
            }

            private static void AnalyzeAttributeTarget(SyntaxNodeAnalysisContext context) =>
                context.ReportDiagnostic(Diagnostic.Create(FieldNeverRead, context.Node.GetLocation()));
        }
        """;

    /// <summary>Each suppressor exposes the intended diagnostic-to-suppression mapping.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task SupportedSuppressions_ExposeExpectedDescriptor()
    {
        await AssertDescriptor(
            new ReactiveCommandAttributeWithFieldOrPropertyTargetDiagnosticSuppressor(),
            "RXUISPR0001",
            InvalidAttributeTargetDiagnosticId);
        await AssertDescriptor(
            new ObservableAsPropertyAttributeWithFieldNeverReadDiagnosticSuppressor(),
            "RXUISPR0002",
            "IDE0052");
        await AssertDescriptor(
            new ReactiveCommandMethodDoesNotNeedToBeStaticDiagnosticSuppressor(),
            "RXUISPR0003",
            "CA1822");
        await AssertDescriptor(
            new OAPHMethodDoesNotNeedToBeStaticDiagnosticSuppressor(),
            "RXUISPR0003",
            "CA1822");
        await AssertDescriptor(
            new ReactiveFieldDoesNotNeedToBeReadOnlyDiagnosticSuppressor(),
            "RXUISPR0004",
            "RCS1169");
        await AssertDescriptor(
            new ReactiveAttributeWithFieldTargetDiagnosticSuppressor(),
            "RXUISPR0005",
            InvalidAttributeTargetDiagnosticId);
        await AssertDescriptor(
            new ReactiveAttributeWithPropertyTargetDiagnosticSuppressor(),
            "RXUISPR0005",
            InvalidAttributeTargetDiagnosticId);
    }

    /// <summary>Reactive field, property, and command targets suppress matching compiler diagnostics.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task AttributeTargetSuppressors_SuppressMatchingCompilerDiagnostics()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(
            AttributeTargetSource,
            CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview));
        var compilation = CSharpCompilation.Create(
            nameof(AttributeTargetSuppressors_SuppressMatchingCompilerDiagnostics),
            [syntaxTree],
            TestCompilationReferences.CreateDefault(),
            new(OutputKind.DynamicallyLinkedLibrary));
        ImmutableArray<DiagnosticAnalyzer> suppressors =
        [
            new ReactiveAttributeWithFieldTargetDiagnosticSuppressor(),
            new ReactiveAttributeWithPropertyTargetDiagnosticSuppressor(),
            new ReactiveCommandAttributeWithFieldOrPropertyTargetDiagnosticSuppressor(),
        ];
        var options = new CompilationWithAnalyzersOptions(
            new AnalyzerOptions(ImmutableArray<AdditionalText>.Empty),
            onAnalyzerException: null,
            concurrentAnalysis: true,
            logAnalyzerExecutionTime: false,
            reportSuppressedDiagnostics: true);

        var diagnostics = await compilation
            .WithAnalyzers(suppressors, options)
            .GetAllDiagnosticsAsync();
        var suppressedDiagnosticCount = 0;

        foreach (var diagnostic in diagnostics)
        {
            if (diagnostic.Id == InvalidAttributeTargetDiagnosticId && diagnostic.IsSuppressed)
            {
                suppressedDiagnosticCount++;
            }
        }

        await Assert.That(suppressedDiagnosticCount).IsEqualTo(InvalidTargetDiagnosticCount);
    }

    /// <summary>Member suppressors accept only matching attributes and syntax shapes.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task MemberSuppressors_SuppressOnlyMatchingSyntheticDiagnostics()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(
            MemberSuppressionSource,
            CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview));
        var compilation = CSharpCompilation.Create(
            nameof(MemberSuppressors_SuppressOnlyMatchingSyntheticDiagnostics),
            [syntaxTree],
            TestCompilationReferences.CreateDefault(),
            new(OutputKind.DynamicallyLinkedLibrary));
        ImmutableArray<DiagnosticAnalyzer> analyzers =
        [
            CreateMemberCoverageDiagnosticAnalyzer(),
            new OAPHMethodDoesNotNeedToBeStaticDiagnosticSuppressor(),
            new ObservableAsPropertyAttributeWithFieldNeverReadDiagnosticSuppressor(),
            new ReactiveCommandMethodDoesNotNeedToBeStaticDiagnosticSuppressor(),
            new ReactiveFieldDoesNotNeedToBeReadOnlyDiagnosticSuppressor(),
        ];
        var options = new CompilationWithAnalyzersOptions(
            new AnalyzerOptions(ImmutableArray<AdditionalText>.Empty),
            onAnalyzerException: null,
            concurrentAnalysis: true,
            logAnalyzerExecutionTime: false,
            reportSuppressedDiagnostics: true);
        var diagnostics = await compilation.WithAnalyzers(analyzers, options).GetAnalyzerDiagnosticsAsync();
        var suppressedCount = 0;
        var unsuppressedCount = 0;
        foreach (var diagnostic in diagnostics)
        {
            if (TestContext.Current is { } testContext)
            {
                await testContext.OutputWriter.WriteLineAsync($"{diagnostic.Id}: suppressed={diagnostic.IsSuppressed}; {diagnostic.Location}");
            }

            if (diagnostic.IsSuppressed)
            {
                suppressedCount++;
            }
            else
            {
                unsuppressedCount++;
            }
        }

        await Assert.That(suppressedCount).IsEqualTo(SyntheticSuppressedDiagnosticCount);
        await Assert.That(unsuppressedCount).IsEqualTo(SyntheticUnsuppressedDiagnosticCount);
    }

    /// <summary>Asserts a suppressor's single supported descriptor.</summary>
    /// <param name="suppressor">The suppressor under test.</param>
    /// <param name="suppressionId">The expected suppression identifier.</param>
    /// <param name="diagnosticId">The expected suppressed diagnostic identifier.</param>
    /// <returns>A task to monitor the async.</returns>
    private static async Task AssertDescriptor(
        DiagnosticSuppressor suppressor,
        string suppressionId,
        string diagnosticId)
    {
        var descriptor = suppressor.SupportedSuppressions.Single();

        await Assert.That(descriptor.Id).IsEqualTo(suppressionId);
        await Assert.That(descriptor.SuppressedDiagnosticId).IsEqualTo(diagnosticId);
        await Assert.That(descriptor.Justification.ToString()).IsNotEmpty();
    }

    /// <summary>Compiles and creates the deterministic analyzer used by the suppression test.</summary>
    /// <returns>The synthetic analyzer.</returns>
    private static DiagnosticAnalyzer CreateMemberCoverageDiagnosticAnalyzer()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(
            MemberCoverageAnalyzerSource,
            CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview));
        var compilation = CSharpCompilation.Create(
            "MemberCoverageDiagnosticAnalyzerAssembly",
            [syntaxTree],
            TestCompilationReferences.CreateForAssemblies(
                typeof(object).Assembly,
                typeof(DiagnosticAnalyzer).Assembly,
                typeof(CSharpCompilation).Assembly),
            new(OutputKind.DynamicallyLinkedLibrary));
        var analyzerPath = Path.Combine(AppContext.BaseDirectory, "MemberCoverageDiagnosticAnalyzer.dll");
        var emitResult = compilation.Emit(analyzerPath);
        if (!emitResult.Success)
        {
            throw new InvalidOperationException(string.Join(Environment.NewLine, emitResult.Diagnostics));
        }

        var analyzerType = AssemblyLoadContext.Default.LoadFromAssemblyPath(analyzerPath).GetType("MemberCoverageDiagnosticAnalyzer")
            ?? throw new InvalidOperationException("Could not load the synthetic member diagnostic analyzer.");
        return (DiagnosticAnalyzer?)Activator.CreateInstance(analyzerType)
            ?? throw new InvalidOperationException("Could not create the synthetic member diagnostic analyzer.");
    }
}
