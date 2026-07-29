// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Diagnostics;
using ReactiveUI.SourceGenerators.Diagnostics.Suppressions;

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>Tests the diagnostic suppression contracts exposed by the generators.</summary>
public sealed class DiagnosticSuppressorTests
{
    /// <summary>The compiler diagnostic for an invalid attribute target.</summary>
    private const string InvalidAttributeTargetDiagnosticId = "CS0657";

    /// <summary>The number of invalid target diagnostics exercised together.</summary>
    private const int InvalidTargetDiagnosticCount = 3;

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
            }
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
}
