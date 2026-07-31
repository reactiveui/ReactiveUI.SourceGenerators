// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Text;
using ReactiveUI.SourceGenerators.Helpers;
using ReactiveUI.SourceGenerators.Models;

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>Tests serializable diagnostic metadata paths.</summary>
public sealed class DiagnosticInfoTests
{
    /// <summary>Diagnostics support source and source-free locations plus null argument text.</summary>
    /// <returns>A task representing the asynchronous assertion work.</returns>
    [Test]
    public async Task ToDiagnosticCoversSourceFreeAndNullArgumentRepresentations()
    {
        var descriptor = new DiagnosticDescriptor(
            "TEST0001",
            "Test diagnostic",
            "Value '{0}'",
            "Tests",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);
        var syntaxTree = CSharpSyntaxTree.ParseText("namespace Coverage; public class Target { }");
        var compilation = CSharpCompilation.Create(
            "DiagnosticInfoConsumer",
            [syntaxTree],
            TestCompilationReferences.CreateDefault(),
            new(OutputKind.DynamicallyLinkedLibrary));
        var symbol = compilation.GetTypeByMetadataName("Coverage.Target")
            ?? throw new InvalidOperationException("The diagnostic target was not found.");
        var sourceDiagnostic = DiagnosticInfo.Create(descriptor, symbol, "source").ToDiagnostic();
        var sourceFreeDiagnostic = new DiagnosticInfo(
            descriptor,
            null,
            default(TextSpan),
            ImmutableArray<string>.Empty.AsEquatableArray()).ToDiagnostic();

        await Assert.That(sourceDiagnostic.Location.IsInSource).IsTrue();
        await Assert.That(sourceDiagnostic.GetMessage()).IsEqualTo("Value 'source'");
        await Assert.That(sourceFreeDiagnostic.Location).IsEqualTo(Location.None);
    }
}
