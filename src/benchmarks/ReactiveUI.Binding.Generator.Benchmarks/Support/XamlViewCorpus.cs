// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using ReactiveUI.Binding.SourceGenerators;

namespace ReactiveUI.Binding.Generator.Benchmarks.Support;

/// <summary>Builds Avalonia-style views that name their controls in XAML and bind them all in one activation lambda.</summary>
/// <remarks>
/// A named control is a field Avalonia's own generator writes, and each view model has a <c>[Reactive]</c> field. Either
/// one makes the binding generator read the call sites from its private copy of the compilation. With the XAML pages
/// left out, the views declare their controls as fields, as a WPF build does, and the call sites are read from the
/// consumer's compilation.
/// </remarks>
internal static class XamlViewCorpus
{
    /// <summary>The assembly name of the consumer compilation.</summary>
    private const string AssemblyName = "XamlViews";

    /// <summary>The source files each view adds: its view model and its code-behind.</summary>
    private const int FilesPerView = 2;

    /// <summary>The directive every corpus file starts with.</summary>
    private const string NullableDirective = "#nullable enable";

    /// <summary>The controls, the activation stub and the attribute ReactiveUI.SourceGenerators would supply.</summary>
    private const string Support = """
        #nullable enable
        using System;
        using System.Collections.Generic;
        using System.ComponentModel;
        using System.Windows.Input;

        namespace ReactiveUI.SourceGenerators
        {
            [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
            public sealed class ReactiveAttribute : Attribute { }
        }

        namespace Bench.Controls
        {
            public abstract class Control : INotifyPropertyChanged
            {
                public event PropertyChangedEventHandler? PropertyChanged;
                protected void Raise(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }

            public class TextBox : Control { private string? _text; public string? Text { get => _text; set { _text = value; Raise(nameof(Text)); } } }
            public class TextBlock : Control { private string? _text; public string? Text { get => _text; set { _text = value; Raise(nameof(Text)); } } }
            public class Button : Control { public ICommand? Command { get; set; } public event EventHandler? Click; public void PerformClick() => Click?.Invoke(this, EventArgs.Empty); }
        }

        namespace Bench
        {
            public sealed class Disposables : IDisposable
            {
                private readonly List<IDisposable> _items = new List<IDisposable>();
                public void Add(IDisposable item) => _items.Add(item);
                public void Dispose() { foreach (var item in _items) { item.Dispose(); } }
            }

            public static class ActivationExtensions
            {
                public static void WhenActivated(this object view, Action<Disposables> block) => block(new Disposables());
                public static T DisposeWith<T>(this T item, Disposables disposables) where T : IDisposable { disposables.Add(item); return item; }
            }
        }
        """;

    /// <summary>The control each binding targets, in rotation: a text box bound both ways, a text block, a button.</summary>
    private static readonly string[] Controls = ["TextBox", "TextBlock", "Button"];

    /// <summary>The binding each control gets, in the same rotation.</summary>
    private static readonly string[] Calls = ["Bind", "OneWayBind", "BindCommand"];

    /// <summary>The control member each binding writes, in the same rotation; a command binds the control itself.</summary>
    private static readonly string[] Targets = [".Text", ".Text", string.Empty];

    /// <summary>The view model property type each binding reads, in the same rotation.</summary>
    private static readonly string[] PropertyTypes = ["string?", "string?", "ICommand?"];

    /// <summary>Builds the consumer compilation.</summary>
    /// <param name="views">The number of views.</param>
    /// <param name="bindings">The number of controls each view names and binds.</param>
    /// <param name="xaml">Whether the controls come from XAML pages, which is the path that reads a copy of the compilation.</param>
    /// <param name="pages">The XAML pages, empty when <paramref name="xaml"/> is false.</param>
    /// <returns>The compilation.</returns>
    internal static CSharpCompilation Build(int views, int bindings, bool xaml, out ImmutableArray<AdditionalText> pages)
    {
        var parseOptions = GeneratorHarness.ParseOptions(false);
        var trees = new List<SyntaxTree>((views * FilesPerView) + 1) { CSharpSyntaxTree.ParseText(Support, parseOptions, "Support.cs") };
        var pageBuilder = ImmutableArray.CreateBuilder<AdditionalText>(xaml ? views : 0);
        for (var view = 0; view < views; view++)
        {
            trees.Add(CSharpSyntaxTree.ParseText(ViewModel(view, bindings, xaml), parseOptions, $"ViewModel{view}.cs"));
            trees.Add(CSharpSyntaxTree.ParseText(CodeBehind(view, bindings, xaml), parseOptions, $"View{view}.axaml.cs"));
            if (xaml)
            {
                pageBuilder.Add(new InMemoryText($"View{view}.axaml", Page(view, bindings)));
            }
        }

        pages = pageBuilder.MoveToImmutable();
        var compilation = GeneratorHarness.BuildCompilation(AssemblyName, [.. trees]);
        return compilation.AddReferences(RuntimeDependencies(compilation));
    }

    /// <summary>Writes the members Avalonia's and ReactiveUI.SourceGenerators' generators would add.</summary>
    /// <param name="views">The number of views.</param>
    /// <param name="bindings">The number of controls each view names.</param>
    /// <returns>The source, which lets the generated output be compiled as a full build would.</returns>
    internal static SyntaxTree OtherGeneratorsOutput(int views, int bindings)
    {
        var lines = new List<string> { NullableDirective };
        for (var view = 0; view < views; view++)
        {
            lines.Add($"namespace Bench.ViewModels {{ public partial class ViewModel{view} {{ public string Title {{ get; set; }} = string.Empty; }} }}");
            lines.Add($"namespace Bench.Views {{ public partial class View{view} {{");
            AddFields(lines, bindings);
            lines.Add("} }");
        }

        return CSharpSyntaxTree.ParseText(string.Join("\n", lines), GeneratorHarness.ParseOptions(false), "OtherGenerators.g.cs");
    }

    /// <summary>Creates a cold driver that sees the XAML pages as Avalonia marks them.</summary>
    /// <param name="pages">The XAML pages.</param>
    /// <returns>The driver.</returns>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    internal static GeneratorDriver CreateDriver(ImmutableArray<AdditionalText> pages) =>
        CSharpGeneratorDriver.Create(
            [new BindingGenerator().AsSourceGenerator()],
            pages,
            GeneratorHarness.ParseOptions(false),
            new PageOptionsProvider());

    /// <summary>Lists the runtime library's dependencies the compilation does not reference yet.</summary>
    /// <param name="compilation">The compilation.</param>
    /// <returns>The references to add.</returns>
    /// <remarks>
    /// The generated code names types from Splat, ReactiveUI.Primitives and ReactiveUI.Disposables, so the output can
    /// only be checked to compile with all of their assemblies referenced.
    /// </remarks>
    private static List<MetadataReference> RuntimeDependencies(Compilation compilation)
    {
        var present = new HashSet<string>(StringComparer.Ordinal);
        foreach (var reference in compilation.References)
        {
            _ = present.Add(Path.GetFileName(reference.Display ?? string.Empty));
        }

        var extra = new List<MetadataReference>();
        foreach (var path in Directory.GetFiles(AppContext.BaseDirectory, "*.dll"))
        {
            var name = Path.GetFileName(path);
            if (IsRuntimeDependency(name) && present.Add(name))
            {
                extra.Add(MetadataReference.CreateFromFile(path));
            }
        }

        return extra;
    }

    /// <summary>Determines whether an assembly file belongs to one of the runtime library's dependencies.</summary>
    /// <param name="name">The file name.</param>
    /// <returns><see langword="true"/> for a Splat, ReactiveUI.Primitives or ReactiveUI.Disposables assembly.</returns>
    private static bool IsRuntimeDependency(string name) =>
        name.StartsWith("Splat", StringComparison.Ordinal)
        || name.StartsWith("ReactiveUI.Primitives", StringComparison.Ordinal)
        || name.StartsWith("ReactiveUI.Disposables", StringComparison.Ordinal);

    /// <summary>Writes a view model with a property per control, and a <c>[Reactive]</c> field when XAML is on.</summary>
    /// <param name="view">The view's index.</param>
    /// <param name="bindings">The number of controls.</param>
    /// <param name="xaml">Whether the view model carries a <c>[Reactive]</c> field.</param>
    /// <returns>The source.</returns>
    private static string ViewModel(int view, int bindings, bool xaml)
    {
        var lines = new List<string>
        {
            NullableDirective,
            "using System.ComponentModel;",
            "using System.Windows.Input;",
            "namespace Bench.ViewModels {",
            $"public partial class ViewModel{view} : INotifyPropertyChanged {{",
            "public event PropertyChangedEventHandler? PropertyChanged;",
        };
        if (xaml)
        {
            lines.Add("[ReactiveUI.SourceGenerators.Reactive] private string _title = string.Empty;");
        }

        for (var i = 0; i < bindings; i++)
        {
            lines.Add($"public {PropertyTypes[i % PropertyTypes.Length]} P{i} {{ get; set; }}");
        }

        lines.Add("} }");
        return string.Join("\n", lines);
    }

    /// <summary>Writes a view's code-behind, binding every control in one activation lambda.</summary>
    /// <param name="view">The view's index.</param>
    /// <param name="bindings">The number of controls.</param>
    /// <param name="xaml">Whether the controls come from the page; otherwise the class declares them.</param>
    /// <returns>The source.</returns>
    private static string CodeBehind(int view, int bindings, bool xaml)
    {
        var lines = new List<string>
        {
            NullableDirective,
            "using ReactiveUI.Binding;",
            "using Bench.ViewModels;",
            "namespace Bench.Views {",
            $"public partial class View{view} : IViewFor<ViewModel{view}> {{",
            $"public ViewModel{view}? ViewModel {{ get; set; }}",
            $"object? IViewFor.ViewModel {{ get => ViewModel; set => ViewModel = (ViewModel{view}?)value; }}",
        };
        if (!xaml)
        {
            AddFields(lines, bindings);
        }

        lines.Add($"public View{view}() {{");
        lines.Add("this.WhenActivated(disposables => {");
        for (var i = 0; i < bindings; i++)
        {
            lines.Add($"this.{Calls[i % Calls.Length]}(ViewModel, vm => vm.P{i}, v => v.C{i}{Targets[i % Targets.Length]}).DisposeWith(disposables);");
        }

        lines.Add("}); } } }");
        return string.Join("\n", lines);
    }

    /// <summary>Adds a field per control, as Avalonia's generator or a WPF build declares them.</summary>
    /// <param name="lines">The lines to add to.</param>
    /// <param name="bindings">The number of controls.</param>
    private static void AddFields(List<string> lines, int bindings)
    {
        for (var i = 0; i < bindings; i++)
        {
            var type = Controls[i % Controls.Length];
            lines.Add($"internal global::Bench.Controls.{type} C{i} = new global::Bench.Controls.{type}();");
        }
    }

    /// <summary>Writes a view's XAML page, naming every control.</summary>
    /// <param name="view">The view's index.</param>
    /// <param name="bindings">The number of controls.</param>
    /// <returns>The page.</returns>
    private static string Page(int view, int bindings)
    {
        var lines = new List<string>
        {
            $"<UserControl xmlns=\"https://github.com/avaloniaui\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" xmlns:c=\"using:Bench.Controls\" x:Class=\"Bench.Views.View{view}\">",
            "<StackPanel>",
        };
        for (var i = 0; i < bindings; i++)
        {
            lines.Add($"<c:{Controls[i % Controls.Length]} x:Name=\"C{i}\" />");
        }

        lines.Add("</StackPanel>");
        lines.Add("</UserControl>");
        return string.Join("\n", lines);
    }

    /// <summary>An additional file held in memory.</summary>
    /// <param name="path">The file's path.</param>
    /// <param name="content">The file's text.</param>
    private sealed class InMemoryText(string path, string content) : AdditionalText
    {
        /// <summary>The file's text.</summary>
        private readonly SourceText _text = SourceText.From(content, Encoding.UTF8);

        /// <inheritdoc/>
        public override string Path { get; } = path;

        /// <inheritdoc/>
        public override SourceText GetText(CancellationToken cancellationToken = default) => _text;
    }

    /// <summary>Marks every additional file as an Avalonia XAML page.</summary>
    private sealed class PageOptionsProvider : AnalyzerConfigOptionsProvider
    {
        /// <summary>The options every file carries: the item metadata Avalonia marks its XAML files with.</summary>
        private static readonly AnalyzerConfigOptions PageOptions = new Options("build_metadata.AdditionalFiles.SourceItemGroup", "AvaloniaXaml");

        /// <summary>The build's options, which set nothing.</summary>
        private static readonly AnalyzerConfigOptions NoOptions = new Options(null, null);

        /// <inheritdoc/>
        public override AnalyzerConfigOptions GlobalOptions => NoOptions;

        /// <inheritdoc/>
        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => NoOptions;

        /// <inheritdoc/>
        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => PageOptions;
    }

    /// <summary>Options holding at most one key.</summary>
    /// <param name="optionKey">The key, or null for none.</param>
    /// <param name="optionValue">The key's value.</param>
    private sealed class Options(string? optionKey, string? optionValue) : AnalyzerConfigOptions
    {
        /// <inheritdoc/>
        public override bool TryGetValue(string key, [NotNullWhen(true)] out string? value)
        {
            value = string.Equals(optionKey, key, StringComparison.Ordinal) ? optionValue : null;
            return value is not null;
        }
    }
}
