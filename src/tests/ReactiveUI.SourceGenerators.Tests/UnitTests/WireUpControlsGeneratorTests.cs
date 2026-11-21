// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.SourceGenerators.Android;

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>Tests compile-time Android control wire-up generation for <c>[WireUpControls]</c>.</summary>
public sealed class WireUpControlsGeneratorTests
{
    /// <summary>Minimal Android stubs for cross-platform generator testing.</summary>
    private const string AndroidStubs = """
        namespace Android.Views
        {
            public class View
            {
                public T? FindViewById<T>(int id) where T : View => default;
            }
        }

        namespace Android.App
        {
            public class Activity
            {
                public T? FindViewById<T>(int id) where T : global::Android.Views.View => default;
            }
        }

        namespace Android.Widget
        {
            public class TextView : global::Android.Views.View { }
            public class Button : global::Android.Views.View { }
        }

        namespace TestNs
        {
            public static class Resource
            {
                public static class Id
                {
                    public const int TitleText = 1;
                    public const int submit_btn = 2;
                }
            }
        }
        """;

    /// <summary>Verifies <c>WireUpControls()</c> is generated for an Activity with resource overrides and ignored properties.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task GeneratesParameterlessWireUpControlsForActivity()
    {
        const string source = """
            using Android.App;
            using Android.Widget;
            using ReactiveUI.SourceGenerators.Android;

            namespace TestNs;

            [WireUpControls]
            public partial class MainActivity : Activity
            {
                public TextView? TitleText { get; private set; }

                [WireUpResource("submit_btn")]
                public Button? SubmitButton { get; private set; }

                [IgnoreResource]
                public TextView? IgnoredLabel { get; private set; }

                public string NonViewProperty { get; set; } = string.Empty;

                public void Init() => WireUpControls();
            }
            """;

        var (generatedSource, errors) = RunGenerator(source);

        await Assert.That(generatedSource).IsNotNull();
        await Assert.That(errors).IsEmpty();
        await Assert.That(generatedSource!).Contains("private void WireUpControls()");
        await Assert.That(generatedSource!).Contains("this.TitleText = this.FindViewById<global::Android.Widget.TextView>(Resource.Id.TitleText)!;");
        await Assert.That(generatedSource!).Contains("this.SubmitButton = this.FindViewById<global::Android.Widget.Button>(Resource.Id.submit_btn)!;");
        await Assert.That(generatedSource!).DoesNotContain("IgnoredLabel");
        await Assert.That(generatedSource!).DoesNotContain("NonViewProperty");
    }

    /// <summary>Verifies <c>WireUpControls(View rootView)</c> and <c>ExplicitOptIn = true</c> work for Fragment-like classes.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task GeneratesRootViewOverloadAndRespectsExplicitOptIn()
    {
        const string source = """
            using Android.Views;
            using Android.Widget;
            using ReactiveUI.SourceGenerators.Android;

            namespace TestNs;

            [WireUpControls(ExplicitOptIn = true)]
            public partial class ProfileFragment
            {
                [WireUpResource]
                public TextView? TitleText { get; private set; }

                public Button? NotOptedIn { get; private set; }

                public void Init(View root) => WireUpControls(root);
            }
            """;

        var (generatedSource, errors) = RunGenerator(source);

        await Assert.That(generatedSource).IsNotNull();
        await Assert.That(errors).IsEmpty();
        await Assert.That(generatedSource!).Contains("private void WireUpControls(global::Android.Views.View rootView)");
        await Assert.That(generatedSource!).Contains("this.TitleText = rootView.FindViewById<global::Android.Widget.TextView>(Resource.Id.TitleText)!;");
        await Assert.That(generatedSource!).DoesNotContain("NotOptedIn");
    }

    /// <summary>Runs <see cref="WireUpControlsGenerator"/> against the given source with Android stubs.</summary>
    /// <param name="source">The C# source text.</param>
    /// <returns>The generated source text (or null) and any error diagnostics.</returns>
    private static (string? GeneratedSource, ImmutableArray<Diagnostic> Errors) RunGenerator(string source)
    {
        var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp13);
        var compilation = CSharpCompilation.Create(
            "WireUpControlsTest",
            [
                CSharpSyntaxTree.ParseText(AndroidStubs, parseOptions, path: "AndroidStubs.cs"),
                CSharpSyntaxTree.ParseText(source, parseOptions, path: "Activity.cs"),
            ],
            TestCompilationReferences.CreatePortableDefault(),
            new(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [new WireUpControlsGenerator().AsSourceGenerator()],
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
