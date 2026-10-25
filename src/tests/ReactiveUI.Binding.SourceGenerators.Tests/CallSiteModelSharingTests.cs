// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using ReactiveUI.Binding.SourceGenerators.Helpers;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>
/// Covers reading call sites from the compilation that declares the members other generators write. Every call site in
/// one file shares one semantic model, so a file with many call sites in one method binds that method once.
/// </summary>
public class CallSiteModelSharingTests
{
    /// <summary>The number of controls the view binds.</summary>
    private const int Controls = 9;

    /// <summary>The number of reads each processor makes at once.</summary>
    private const int ReadsPerProcessor = 4;

    /// <summary>The control types the view rotates through: two text boxes, then a button.</summary>
    private const int ControlKinds = 3;

    /// <summary>A file with one class and no call sites.</summary>
    private const string OneClass = "class A { }";

    /// <summary>The path of the view's code-behind, which both compilations share.</summary>
    private const string CodeBehindPath = "/App/LoginView.axaml.cs";

    /// <summary>The controls and the view model.</summary>
    private const string Support = """
        #nullable enable
        using System.ComponentModel;
        using System.Windows.Input;

        namespace App
        {
            public class TextBox : INotifyPropertyChanged
            {
                public event PropertyChangedEventHandler? PropertyChanged;
                public string? Text { get; set; }
            }

            public class Button
            {
                public event System.EventHandler? Click;
                public ICommand? Command { get; set; }
            }

            public class LoginViewModel : INotifyPropertyChanged
            {
                public event PropertyChangedEventHandler? PropertyChanged;
                public string? P0 { get; set; }
                public string? P1 { get; set; }
                public ICommand? P2 { get; set; }
                public string? P3 { get; set; }
                public string? P4 { get; set; }
                public ICommand? P5 { get; set; }
                public string? P6 { get; set; }
                public string? P7 { get; set; }
                public ICommand? P8 { get; set; }
            }

            public static class Activation
            {
                public static void WhenActivated(this object view, System.Action<System.Collections.Generic.List<System.IDisposable>> block) =>
                    block(new System.Collections.Generic.List<System.IDisposable>());
            }
        }
        """;

    /// <summary>A view that binds every control in one activation lambda.</summary>
    private const string CodeBehind = """
        #nullable enable
        using ReactiveUI.Binding;

        namespace App
        {
            public partial class LoginView : IViewFor<LoginViewModel>
            {
                public LoginView()
                {
                    this.WhenActivated(d =>
                    {
                        d.Add(this.Bind(ViewModel, vm => vm.P0, v => v.C0.Text));
                        d.Add(this.OneWayBind(ViewModel, vm => vm.P1, v => v.C1.Text));
                        d.Add(this.BindCommand(ViewModel, vm => vm.P2, v => v.C2));
                        d.Add(this.Bind(ViewModel, vm => vm.P3, v => v.C3.Text));
                        d.Add(this.OneWayBind(ViewModel, vm => vm.P4, v => v.C4.Text));
                        d.Add(this.BindCommand(ViewModel, vm => vm.P5, v => v.C5));
                        d.Add(this.Bind(ViewModel, vm => vm.P6, v => v.C6.Text));
                        d.Add(this.OneWayBind(ViewModel, vm => vm.P7, v => v.C7.Text));
                        d.Add(this.BindCommand(ViewModel, vm => vm.P8, v => v.C8));
                    });
                }

                public LoginViewModel? ViewModel { get; set; }

                object? IViewFor.ViewModel
                {
                    get => ViewModel;
                    set => ViewModel = (LoginViewModel?)value;
                }
            }
        }
        """;

    /// <summary>Asking twice for a file's model gives the same model, so its bound members are reused.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SameFile_SharesOneModel()
    {
        var compilation = TestHelper.CreateCompilation(OneClass, LanguageVersion.CSharp10);
        var tree = compilation.SyntaxTrees.First();

        var first = CallSiteContext.ModelFor(compilation, tree);
        var second = CallSiteContext.ModelFor(compilation, tree);

        await Assert.That(second).IsSameReferenceAs(first);
        await Assert.That(first.SyntaxTree).IsSameReferenceAs(tree);
        await Assert.That(first.Compilation).IsSameReferenceAs(compilation);
    }

    /// <summary>Each file, and each compilation, gets a model of its own.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OtherFileOrCompilation_GetsItsOwnModel()
    {
        var compilation = TestHelper.CreateCompilation(OneClass, LanguageVersion.CSharp10);
        var tree = compilation.SyntaxTrees.First();
        var other = CSharpSyntaxTree.ParseText("class B { }", TestHelper.ParseOptionsFor(LanguageVersion.CSharp10));
        var withOther = compilation.AddSyntaxTrees(other);

        var model = CallSiteContext.ModelFor(withOther, tree);

        await Assert.That(CallSiteContext.ModelFor(withOther, other)).IsNotSameReferenceAs(model);
        await Assert.That(CallSiteContext.ModelFor(compilation, tree)).IsNotSameReferenceAs(model);
    }

    /// <summary>Call sites read in parallel, as the generator's transforms are, all receive the one model.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ParallelReads_ShareOneModel()
    {
        var compilation = TestHelper.CreateCompilation(OneClass, LanguageVersion.CSharp10);
        var tree = compilation.SyntaxTrees.First();
        var models = new ConcurrentBag<SemanticModel>();

        _ = Parallel.For(0, Environment.ProcessorCount * ReadsPerProcessor, _ => models.Add(CallSiteContext.ModelFor(compilation, tree)));

        await Assert.That(models.Distinct().Count()).IsEqualTo(1);
    }

    /// <summary>
    /// A view whose controls an Avalonia page names is read from the compilation copy, and generates the same code as
    /// the same view with its controls declared in source.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ControlsNamedInXaml_GenerateTheSameCodeAsDeclaredControls()
    {
        var parseOptions = TestHelper.ParseOptionsFor(LanguageVersion.CSharp10);
        var baseline = TestHelper.CreateCompilation(Support, LanguageVersion.CSharp10)
            .AddSyntaxTrees(CSharpSyntaxTree.ParseText(CodeBehind, parseOptions, CodeBehindPath));

        var declared = Run(baseline.AddSyntaxTrees(CSharpSyntaxTree.ParseText(Fields(), parseOptions, "/App/LoginView.Fields.cs")), parseOptions, []);
        var named = Run(baseline, parseOptions, [new Page(Xaml())]);

        await Assert.That(declared.CompilationErrors).IsEmpty();
        var namedOutput = named.OutputCompilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(Fields(), parseOptions, "/App/LoginView.Fields.cs"));
        await Assert.That(namedOutput.GetDiagnostics().Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)).IsEmpty();
        await Assert.That(named.GeneratorDiagnostics).IsEmpty();
        await Assert.That(declared.GeneratedSources.Count).IsGreaterThan(1);
        await Assert.That(named.GeneratedSources.Keys).IsEquivalentTo(declared.GeneratedSources.Keys);
        foreach (var (hint, text) in declared.GeneratedSources)
        {
            await Assert.That(named.GeneratedSources[hint]).IsEqualTo(text);
        }

        await Assert.That(declared.GeneratedSources["BindDispatch.g.cs"]).Contains("C6");
        await Assert.That(declared.GeneratedSources["OneWayBindDispatch.g.cs"]).Contains("C7");
        await Assert.That(declared.GeneratedSources["BindCommandDispatch.g.cs"]).Contains("C8");
    }

    /// <summary>Runs the generator with the given XAML pages, which Avalonia marks as its own.</summary>
    /// <param name="compilation">The compilation.</param>
    /// <param name="parseOptions">The parse options.</param>
    /// <param name="pages">The pages.</param>
    /// <returns>The result.</returns>
    private static GeneratorTestResult Run(Compilation compilation, CSharpParseOptions parseOptions, AdditionalText[] pages)
    {
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [new BindingGenerator().AsSourceGenerator()],
            pages,
            parseOptions,
            new AvaloniaOptions());
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);
        return new(driver, output, diagnostics);
    }

    /// <summary>Declares the controls as fields, as Avalonia's own generator writes them.</summary>
    /// <returns>The source.</returns>
    private static string Fields()
    {
        var lines = new List<string> { "namespace App {", "public partial class LoginView {" };
        for (var i = 0; i < Controls; i++)
        {
            lines.Add($"internal global::App.{ControlType(i)} C{i} = new global::App.{ControlType(i)}();");
        }

        lines.Add("} }");
        return string.Join("\n", lines);
    }

    /// <summary>Writes the page that names the controls.</summary>
    /// <returns>The page.</returns>
    private static string Xaml()
    {
        var lines = new List<string>
        {
            "<UserControl xmlns=\"https://github.com/avaloniaui\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" xmlns:a=\"using:App\" x:Class=\"App.LoginView\">",
            "<StackPanel>",
        };
        for (var i = 0; i < Controls; i++)
        {
            lines.Add($"<a:{ControlType(i)} x:Name=\"C{i}\" />");
        }

        lines.Add("</StackPanel>");
        lines.Add("</UserControl>");
        return string.Join("\n", lines);
    }

    /// <summary>Picks a control's type: every third control is a button.</summary>
    /// <param name="index">The control's index.</param>
    /// <returns>The type name.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string ControlType(int index) => (index % ControlKinds) is ControlKinds - 1 ? "Button" : "TextBox";

    /// <summary>A XAML page held in memory.</summary>
    /// <param name="text">The page's text.</param>
    private sealed class Page(string text) : AdditionalText
    {
        /// <inheritdoc/>
        public override string Path => "/App/LoginView.axaml";

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override SourceText GetText(CancellationToken cancellationToken = default) => SourceText.From(text);
    }

    /// <summary>Marks every additional file as an Avalonia XAML page.</summary>
    private sealed class AvaloniaOptions : AnalyzerConfigOptionsProvider
    {
        /// <inheritdoc/>
        public override AnalyzerConfigOptions GlobalOptions { get; } = new Value(null);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => GlobalOptions;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => new Value("AvaloniaXaml");
    }

    /// <summary>Options that carry Avalonia's item group, or nothing.</summary>
    /// <param name="itemGroup">The item group, or null for none.</param>
    private sealed class Value(string? itemGroup) : AnalyzerConfigOptions
    {
        /// <inheritdoc/>
        public override bool TryGetValue(string key, out string value)
        {
            value = itemGroup ?? string.Empty;
            return itemGroup is not null && string.Equals(key, "build_metadata.AdditionalFiles.SourceItemGroup", StringComparison.Ordinal);
        }
    }
}
