// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>Tests compile-time view registration generation for <c>[RegisterView]</c>.</summary>
public sealed class RegisterViewGeneratorTests
{
    /// <summary>Verifies generic, inferred, contract, and singleton view registrations generate clean compilable code.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task GeneratesRegisterGeneratedViewsForAnnotatedViews()
    {
        const string source = """
            using ReactiveUI;
            using ReactiveUI.Binding;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public sealed class FirstViewModel : ReactiveObject { }
            public sealed class SecondViewModel : ReactiveObject { }

            [RegisterView]
            public sealed class FirstView : IViewFor<FirstViewModel>
            {
                public FirstViewModel? ViewModel { get; set; }
                object? IViewFor.ViewModel { get => ViewModel; set => ViewModel = (FirstViewModel?)value; }
            }

            [RegisterView<SecondViewModel>(Contract = "Dialog", IsSingleton = true)]
            public sealed class SecondView : IViewFor<SecondViewModel>
            {
                public SecondViewModel? ViewModel { get; set; }
                object? IViewFor.ViewModel { get => ViewModel; set => ViewModel = (SecondViewModel?)value; }
            }

            [RegisterView]
            public abstract class AbstractIgnoredView : IViewFor<FirstViewModel>
            {
                public FirstViewModel? ViewModel { get; set; }
                object? IViewFor.ViewModel { get => ViewModel; set => ViewModel = (FirstViewModel?)value; }
            }
            """;

        var (generatedSource, diagnostics) = RunGenerator(source);

        await Assert.That(generatedSource).IsNotNull();
        await Assert.That(diagnostics).IsEmpty();
        await Assert.That(generatedSource!).Contains("public static class GeneratedViewRegistrations");
        await Assert.That(generatedSource!).Contains("builder.RegisterView<global::TestNs.FirstView, global::TestNs.FirstViewModel>();");
        await Assert.That(generatedSource!).Contains(
            "builder.WithRegistration(static resolver => resolver.RegisterLazySingleton<"
            + "global::ReactiveUI.Binding.IViewFor<global::TestNs.SecondViewModel>>("
            + "static () => new global::TestNs.SecondView(), \"Dialog\"));");
        await Assert.That(generatedSource!).DoesNotContain("AbstractIgnoredView");
    }

    /// <summary>Verifies no source is emitted when no classes carry <c>[RegisterView]</c>.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task EmitsNothingWhenNoViewsMarked()
    {
        const string source = """
            namespace TestNs;
            public sealed class PlainClass { }
            """;

        var (generatedSource, diagnostics) = RunGenerator(source);

        await Assert.That(generatedSource).IsNull();
        await Assert.That(diagnostics).IsEmpty();
    }

    /// <summary>Runs <see cref="RegisterViewGenerator"/> against the given source and returns the generated file and compile errors.</summary>
    /// <param name="source">The C# source text.</param>
    /// <returns>The generated source text (or null) and any error diagnostics.</returns>
    private static (string? GeneratedSource, ImmutableArray<Diagnostic> Errors) RunGenerator(string source)
    {
        var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp13);
        var compilation = CSharpCompilation.Create(
            "RegisterViewTest",
            [CSharpSyntaxTree.ParseText(source, parseOptions)],
            TestCompilationReferences.CreatePortableDefault(),
            new(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [new RegisterViewGenerator().AsSourceGenerator()],
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
