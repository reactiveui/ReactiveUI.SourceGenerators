// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Text;
using Microsoft.CodeAnalysis.Text;
using ReactiveUI.SourceGenerators.Extensions;
using ReactiveUI.SourceGenerators.Models;
using ReactiveUI.SourceGenerators.WinForms;

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>
/// Tests that the Windows Forms host generators name <c>IViewFor</c> and the view locator from the assembly the compilation uses:
/// ReactiveUI itself, ReactiveUI.Binding, or ReactiveUI.Binding.Reactive.
/// </summary>
public sealed class ViewApiIntegrationTests
{
    /// <summary>The lean ReactiveUI.Binding namespace.</summary>
    private const string BindingNamespace = "ReactiveUI.Binding";

    /// <summary>The System.Reactive-based ReactiveUI.Binding namespace.</summary>
    private const string BindingReactiveNamespace = "ReactiveUI.Binding.Reactive";

    /// <summary>Declares the <c>IViewFor</c> interfaces a ReactiveUI release not built on ReactiveUI.Binding ships.</summary>
    private const string LegacyViewForSource = """
        namespace ReactiveUI
        {
            public interface IViewFor { object? ViewModel { get; set; } }
            public interface IViewFor<T> : IViewFor where T : class { new T? ViewModel { get; set; } }
        }
        """;

    /// <summary>Declares the command type only ReactiveUI.Reactive ships.</summary>
    private const string ReactiveUIReactiveSource = """
        namespace ReactiveUI.Reactive
        {
            public static class ReactiveCommand { }
        }
        """;

    /// <summary>A Windows Forms routed host and view-model host.</summary>
    private const string HostsSource = """
        using ReactiveUI.SourceGenerators.WinForms;

        namespace Hosts
        {
            [RoutedControlHost("System.Windows.Forms.UserControl")]
            public partial class RoutedHost { }

            [ViewModelControlHost("System.Windows.Forms.UserControl")]
            public partial class ViewModelHost { }
        }
        """;

    /// <summary>Verifies which assembly's view API is picked from each combination of references.</summary>
    /// <returns>A task representing the asynchronous assertion work.</returns>
    [Test]
    public async Task ViewApiFollowsTheReferencedAssemblies()
    {
        await Assert.That(Detect()).IsEqualTo(ReactiveUiViewApi.ReactiveUI);
        await Assert.That(Detect(LegacyViewForSource, BindingStub(BindingNamespace))).IsEqualTo(ReactiveUiViewApi.ReactiveUI);
        await Assert.That(Detect(BindingStub(BindingNamespace))).IsEqualTo(ReactiveUiViewApi.Binding);
        await Assert.That(Detect(BindingStub(BindingReactiveNamespace))).IsEqualTo(ReactiveUiViewApi.BindingReactive);
        await Assert.That(Detect(BindingStub(BindingNamespace), BindingStub(BindingReactiveNamespace))).IsEqualTo(ReactiveUiViewApi.Binding);
        await Assert.That(Detect(BindingStub(BindingNamespace), BindingStub(BindingReactiveNamespace), ReactiveUIReactiveSource))
            .IsEqualTo(ReactiveUiViewApi.BindingReactive);
    }

    /// <summary>Verifies the qualified names each view API writes.</summary>
    /// <returns>A task representing the asynchronous assertion work.</returns>
    [Test]
    public async Task ViewApiNamesTheInterfaceAndLocatorInFull()
    {
        var legacy = new ReactiveUiIntegration(ReactiveUiApi.Legacy, true);
        var binding = legacy with { ViewApi = ReactiveUiViewApi.Binding };
        var bindingReactive = legacy with { ViewApi = ReactiveUiViewApi.BindingReactive };

        await Assert.That(legacy.ViewNamespace).IsEqualTo("global::ReactiveUI");
        await Assert.That(legacy.CurrentViewLocator).IsEqualTo("global::ReactiveUI.ViewLocator.Current");
        await Assert.That(binding.ViewNamespace).IsEqualTo("global::ReactiveUI.Binding");
        await Assert.That(binding.CurrentViewLocator).IsEqualTo("global::ReactiveUI.Binding.ViewLocator.GetCurrent()");
        await Assert.That(bindingReactive.ViewNamespace).IsEqualTo("global::ReactiveUI.Binding.Reactive");
        await Assert.That(bindingReactive.CurrentViewLocator).IsEqualTo("global::ReactiveUI.Binding.Reactive.ViewLocator.GetCurrent()");
    }

    /// <summary>Verifies the Windows Forms hosts resolve views through ReactiveUI.Binding's view locator, in each flavour.</summary>
    /// <param name="bindingNamespace">The ReactiveUI.Binding flavour's namespace.</param>
    /// <returns>A task representing the asynchronous assertion work.</returns>
    [Test]
    [Arguments(BindingNamespace)]
    [Arguments(BindingReactiveNamespace)]
    public async Task HostsUseTheBindingViewLocator(string bindingNamespace)
    {
        var (_, routed) = Run<RoutedControlHostGenerator>(HostsSource, ".RoutedControlHost.g.cs", BindingStub(bindingNamespace));
        var (_, viewModel) = Run<ViewModelControlHostGenerator>(HostsSource, ".ViewModelControlHost.g.cs", BindingStub(bindingNamespace));

        foreach (var generated in (string[])[routed, viewModel])
        {
            await Assert.That(generated.Contains($"public global::{bindingNamespace}.IViewLocator? ViewLocator", StringComparison.Ordinal)).IsTrue();
            await Assert.That(generated.Contains($"ViewLocator ?? global::{bindingNamespace}.ViewLocator.GetCurrent();", StringComparison.Ordinal)).IsTrue();
            await Assert.That(generated.Contains("ReactiveUI.ViewLocator.Current", StringComparison.Ordinal)).IsFalse();
        }

        await Assert.That(viewModel.Contains($", IReactiveObject, global::{bindingNamespace}.IViewFor", StringComparison.Ordinal)).IsTrue();
        await Assert.That(viewModel.Contains($"_content as global::{bindingNamespace}.IViewFor;", StringComparison.Ordinal)).IsTrue();
    }

    /// <summary>Declares a ReactiveUI.Binding flavour's view interfaces and view locator.</summary>
    /// <param name="bindingNamespace">The flavour's namespace.</param>
    /// <returns>The stub source.</returns>
    private static string BindingStub(string bindingNamespace) => $$"""
        namespace ReactiveUI
        {
            internal static class NamespaceMarker { }
        }

        namespace {{bindingNamespace}}
        {
            public interface IViewFor { object? ViewModel { get; set; } }
            public interface IViewFor<T> : IViewFor where T : class { new T? ViewModel { get; set; } }
            public interface IViewLocator { IViewFor? ResolveView(object? viewModel, string? contract = null); }
            public static class ViewLocator { public static IViewLocator GetCurrent() => null!; }
        }
        """;

    /// <summary>Detects the view API of a compilation made of the given sources.</summary>
    /// <param name="sources">The sources declaring the referenced API surface.</param>
    /// <returns>The detected view API.</returns>
    private static ReactiveUiViewApi Detect(params string[] sources) =>
        CreateCompilation(sources).GetReactiveUiIntegration().ViewApi;

    /// <summary>Runs one generator over a consumer source, the desktop stubs and a view API stub.</summary>
    /// <typeparam name="TGenerator">The generator to run.</typeparam>
    /// <param name="source">The consumer source.</param>
    /// <param name="hintSuffix">The suffix of the generated files to collect.</param>
    /// <param name="stub">The view API stub.</param>
    /// <returns>The output compilation and the collected generated text.</returns>
    private static (Compilation Compilation, string Generated) Run<TGenerator>(string source, string hintSuffix, string stub)
        where TGenerator : IIncrementalGenerator, new()
    {
        var compilation = CreateCompilation(source, TestCompilationReferences.WindowsDesktopStubs, stub);
        GeneratorDriver driver = CSharpGeneratorDriver.Create([new TGenerator()])
            .WithUpdatedParseOptions((CSharpParseOptions)compilation.SyntaxTrees[0].Options);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out _);

        var generated = new StringBuilder();
        foreach (var result in driver.GetRunResult().Results)
        {
            foreach (var file in result.GeneratedSources)
            {
                if (file.HintName.EndsWith(hintSuffix, StringComparison.Ordinal))
                {
                    _ = generated.AppendLine(file.SourceText.ToString());
                }
            }
        }

        return (output, generated.ToString());
    }

    /// <summary>Creates a compilation of the given sources against the core and component-model assemblies only.</summary>
    /// <param name="sources">The sources.</param>
    /// <returns>The compilation.</returns>
    private static CSharpCompilation CreateCompilation(params string[] sources)
    {
        var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp13);
        var trees = new SyntaxTree[sources.Length];
        for (var i = 0; i < sources.Length; i++)
        {
            trees[i] = CSharpSyntaxTree.ParseText(SourceText.From(sources[i], Encoding.UTF8), parseOptions, path: $"Source{i}.cs");
        }

        return CSharpCompilation.Create(
            "ViewApiConsumer",
            trees,
            TestCompilationReferences.CreateForAssemblies(
                typeof(object).Assembly,
                typeof(Component).Assembly,
                typeof(System.ComponentModel.CategoryAttribute).Assembly,
                typeof(BindableAttribute).Assembly,
                typeof(ViewModelControlHostAttribute).Assembly),
            new(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
    }
}
