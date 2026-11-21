// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>Tests compile-time DataAnnotations validation wiring for <c>[ReactiveValidation]</c>.</summary>
public sealed class ReactiveValidationGeneratorTests
{
    /// <summary>Minimal stubs for <c>ReactiveProperty&lt;T&gt;</c> and <c>ReactivePropertyMixins.AddValidation</c> so the generated output compiles against the AOT-safe overload.</summary>
    private const string ValidationStubs = """
        namespace ReactiveUI
        {
            public sealed class ReactiveProperty<T> { }

            public static class ReactivePropertyMixins
            {
                public static ReactiveProperty<T> AddValidation<T>(
                    this ReactiveProperty<T> self,
                    string? displayName,
                    params global::System.ComponentModel.DataAnnotations.ValidationAttribute[] attributes) => self;
            }
        }
        """;

    /// <summary>Verifies <c>ConfigureValidation()</c> wires all validation attributes and <c>[Display(Name = ...)]</c> without reflection.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task GeneratesConfigureValidationForAnnotatedReactiveProperties()
    {
        const string source = """
            using System.ComponentModel.DataAnnotations;
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            [ReactiveValidation]
            public partial class RegistrationViewModel
            {
                [Display(Name = "User Name")]
                [Required(ErrorMessage = "Name is required")]
                [StringLength(50, MinimumLength = 3)]
                public ReactiveProperty<string> Name { get; } = new();

                [Range(18, 120)]
                public ReactiveProperty<int> Age { get; } = new();

                public ReactiveProperty<string> Unvalidated { get; } = new();

                public RegistrationViewModel() => ConfigureValidation();
            }
            """;

        var (generatedSource, errors) = RunGenerator(source);

        await Assert.That(generatedSource).IsNotNull();
        await Assert.That(errors).IsEmpty();
        await Assert.That(generatedSource!).Contains("private void ConfigureValidation()");
        await Assert.That(generatedSource!).Contains("\"User Name\"");
        await Assert.That(generatedSource!).Contains("new global::System.ComponentModel.DataAnnotations.RequiredAttribute() { ErrorMessage = \"Name is required\" }");
        await Assert.That(generatedSource!).Contains("new global::System.ComponentModel.DataAnnotations.StringLengthAttribute(50) { MinimumLength = 3 }");
        await Assert.That(generatedSource!).Contains("new global::System.ComponentModel.DataAnnotations.RangeAttribute(18, 120)");
        await Assert.That(generatedSource!).DoesNotContain("this.Unvalidated");
    }

    /// <summary>Runs <see cref="ReactiveValidationGenerator"/> against the given source with AOT-safe <c>ReactivePropertyMixins</c> stubs.</summary>
    /// <param name="source">The C# source text.</param>
    /// <returns>The generated source text (or null) and any error diagnostics.</returns>
    private static (string? GeneratedSource, ImmutableArray<Diagnostic> Errors) RunGenerator(string source)
    {
        var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp13);
        var references = TestCompilationReferences.CreatePortableDefault()
            .Where(static r => r.Display?.EndsWith("ReactiveUI.dll", StringComparison.OrdinalIgnoreCase) != true)
            .Append(MetadataReference.CreateFromFile(typeof(System.ComponentModel.DataAnnotations.RequiredAttribute).Assembly.Location))
            .ToImmutableArray();

        var compilation = CSharpCompilation.Create(
            "ReactiveValidationTest",
            [
                CSharpSyntaxTree.ParseText(ValidationStubs, parseOptions, path: "Stubs.cs"),
                CSharpSyntaxTree.ParseText(source, parseOptions, path: "ViewModel.cs"),
            ],
            references,
            new(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [new ReactiveValidationGenerator().AsSourceGenerator()],
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
