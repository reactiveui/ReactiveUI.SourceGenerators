// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>Tests view-for source generation.</summary>
public class IViewForGeneratorTests : TestBase<IViewForGenerator>
{
    /// <summary>The generated source count for four supported platform targets.</summary>
    private const int SupportedPlatformGeneratedSourceCount = 6;

    /// <summary>The generated source count when no target can produce a view implementation.</summary>
    private const int NoViewImplementationGeneratedSourceCount = 2;

    /// <summary>The stable hint name of the generated registration extensions.</summary>
    private const string RegistrationExtensionsHintName = "ReactiveUI.ReactiveUISourceGeneratorsExtensions.g.cs";

    /// <summary>Tests that the source generator correctly generates reactive properties.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public Task Basic()
    {
        // Arrange: Setup the source code that matches the generator input expectations.
        const string sourceCode = """
                using System.Collections.ObjectModel;
                using System.Windows;
                using ReactiveUI;
                using ReactiveUI.SourceGenerators;

                namespace TestNs;

                [IViewFor<TestViewModel>]
                public partial class TestViewWpf : Window
                {
                    /// <summary>
                    /// Initializes a new instance of the <see cref="TestViewWpf"/> class.
                    /// </summary>
                    public TestViewWpf() => ViewModel = TestViewModel.Instance;
                }

                public partial class TestViewModel : ReactiveObject
                {
                    /// <summary>
                    /// Gets the instance of the test view model.
                    /// </summary>
                    public static TestViewModel Instance { get; } = new();

                    /// <summary>
                    /// Gets or sets the test property.
                    /// </summary>
                    public int TestProperty { get; set; }
                }
            """;

        // Act: Initialize the helper and run the generator. Assert: Verify the generated code.
        return TestHelper.TestPass(sourceCode);
    }

    /// <summary>Verifies the platform-specific IViewFor source shapes without creating snapshots.</summary>
    /// <returns>A task representing the asynchronous assertion work.</returns>
    [Test]
    public async Task GeneratesPlatformSpecificSourcesForSupportedViewBases()
    {
        var generatedSources = RunIViewForGenerator(
            """
            using ReactiveUI.SourceGenerators;

            namespace System.Windows { public class Window { } }
            namespace System.Windows.Forms { public class UserControl { } }
            namespace Avalonia.Controls { public class UserControl { } }
            namespace Microsoft.Maui.Controls { public class ContentPage { } }

            namespace TestNs
            {
                public sealed class WpfViewModel { }
                public sealed class WinFormsViewModel { }
                public sealed class AvaloniaViewModel { }
                public sealed class MauiViewModel { }

                [IViewFor<WpfViewModel>]
                public partial class WpfView : System.Windows.Window { }

                [IViewFor<WinFormsViewModel>]
                public partial class WinFormsView : System.Windows.Forms.UserControl { }

                [IViewFor<AvaloniaViewModel>]
                public partial class AvaloniaView : Avalonia.Controls.UserControl { }

                [IViewFor<MauiViewModel>]
                public partial class MauiView : Microsoft.Maui.Controls.ContentPage { }
            }
            """);

        var wpfSource = GetGeneratedSource(generatedSources, "TestNs.WpfView.IViewFor.g.cs");
        var winFormsSource = GetGeneratedSource(generatedSources, "TestNs.WinFormsView.IViewFor.g.cs");
        var avaloniaSource = GetGeneratedSource(generatedSources, "TestNs.AvaloniaView.IViewFor.g.cs");
        var mauiSource = GetGeneratedSource(generatedSources, "TestNs.MauiView.IViewFor.g.cs");

        await Assert.That(generatedSources.Count).IsEqualTo(SupportedPlatformGeneratedSourceCount);
        await Assert.That(wpfSource.Contains("using System.Windows;", StringComparison.Ordinal)).IsTrue();
        await Assert.That(winFormsSource.Contains(
            "[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]",
            StringComparison.Ordinal)).IsTrue();
        await Assert.That(avaloniaSource.Contains(
            "AvaloniaProperty.Register<AvaloniaView, TestNs.AvaloniaViewModel>",
            StringComparison.Ordinal)).IsTrue();
        await Assert.That(mauiSource.Contains(
            "BindableProperty.Create(nameof(ViewModel), typeof(TestNs.MauiViewModel)",
            StringComparison.Ordinal)).IsTrue();
    }

    /// <summary>Verifies WinUI, Uno, string view models, and every registration mapping.</summary>
    /// <returns>A task representing the asynchronous assertion work.</returns>
    [Test]
    public async Task GeneratesWinUiUnoAndRegistrationVariants()
    {
        var generatedSources = RunIViewForGenerator(
            """
            using ReactiveUI.SourceGenerators;

            namespace Microsoft.UI.Xaml.Controls { public class Page { } }
            namespace Windows.UI.Xaml.Controls { public class Page { } }
            namespace ReactiveUI.SourceGenerators
            {
                internal enum SplatRegistrationType { None, LazySingleton, Constant, PerRequest }
            }

            namespace TestNs
            {
                public sealed class ViewModel { }

                [IViewFor<ViewModel>(RegistrationType = SplatRegistrationType.LazySingleton, ViewModelRegistrationType = SplatRegistrationType.Constant)]
                public partial class WinUiView : Microsoft.UI.Xaml.Controls.Page { }

                [IViewFor("TestNs.ViewModel", RegistrationType = SplatRegistrationType.PerRequest, ViewModelRegistrationType = SplatRegistrationType.PerRequest)]
                public partial class UnoView : Windows.UI.Xaml.Controls.Page { }

                [IViewFor("")]
                public partial class MissingViewModelView : Microsoft.UI.Xaml.Controls.Page { }
            }
            """);
        var winUiSource = GetGeneratedSource(generatedSources, "TestNs.WinUiView.IViewFor.g.cs");
        var unoSource = GetGeneratedSource(generatedSources, "TestNs.UnoView.IViewFor.g.cs");
        var registrationSource = GetGeneratedSource(generatedSources, RegistrationExtensionsHintName);

        await Assert.That(winUiSource.Contains("using Microsoft.UI.Xaml;", StringComparison.Ordinal)).IsTrue();
        await Assert.That(unoSource.Contains("using Windows.UI.Xaml;", StringComparison.Ordinal)).IsTrue();
        await Assert.That(registrationSource.Contains("RegisterLazySingleton", StringComparison.Ordinal)).IsTrue();
        await Assert.That(registrationSource.Contains("RegisterConstant", StringComparison.Ordinal)).IsTrue();
        await Assert.That(registrationSource.Contains("Register", StringComparison.Ordinal)).IsTrue();
        await Assert.That(registrationSource.Contains("MissingViewModelView", StringComparison.Ordinal)).IsFalse();
    }

    /// <summary>Verifies unsupported and non-partial IViewFor targets do not emit view implementations.</summary>
    /// <returns>A task representing the asynchronous assertion work.</returns>
    [Test]
    public async Task DoesNotGenerateViewSourceForUnsupportedOrNonPartialTargets()
    {
        var generatedSources = RunIViewForGenerator(
            """
            using ReactiveUI.SourceGenerators;

            namespace TestNs
            {
                public sealed class ViewModel { }

                [IViewFor<ViewModel>]
                public partial class UnsupportedView { }

                [IViewFor<ViewModel>]
                public class NonPartialView { }
            }
            """);

        await Assert.That(generatedSources.Count).IsEqualTo(NoViewImplementationGeneratedSourceCount);
        await Assert.That(generatedSources.Any(static source => source.HintName.EndsWith(".IViewFor.g.cs", StringComparison.Ordinal))).IsFalse();
        await Assert.That(GetGeneratedSource(generatedSources, RegistrationExtensionsHintName).Contains("UnsupportedView", StringComparison.Ordinal)).IsFalse();
        await Assert.That(GetGeneratedSource(generatedSources, RegistrationExtensionsHintName).Contains("NonPartialView", StringComparison.Ordinal)).IsFalse();
    }

    /// <summary>Runs the IViewFor generator and returns its emitted sources for focused assertions.</summary>
    /// <param name="source">The consumer source text.</param>
    /// <returns>The generated source results.</returns>
    private static ImmutableArray<GeneratedSourceResult> RunIViewForGenerator(string source)
    {
        var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);
        var compilation = CSharpCompilation.Create(
            "IViewForConsumer",
            [CSharpSyntaxTree.ParseText(SourceText.From(source, Encoding.UTF8), parseOptions)],
            TestCompilationReferences.CreatePortableDefault(),
            new(OutputKind.DynamicallyLinkedLibrary));
        GeneratorDriver driver = CSharpGeneratorDriver.Create([new IViewForGenerator()]).WithUpdatedParseOptions(parseOptions);
        driver = driver.RunGenerators(compilation);

        return driver.GetRunResult().Results.Single().GeneratedSources;
    }

    /// <summary>Gets one generated source by its stable hint name.</summary>
    /// <param name="generatedSources">The sources emitted by the generator.</param>
    /// <param name="hintName">The source hint name to find.</param>
    /// <returns>The generated source text.</returns>
    private static string GetGeneratedSource(ImmutableArray<GeneratedSourceResult> generatedSources, string hintName)
    {
        foreach (var source in generatedSources)
        {
            if (source.HintName == hintName)
            {
                return source.SourceText.ToString();
            }
        }

        throw new InvalidOperationException($"The IViewFor generator did not produce '{hintName}'.");
    }
}
