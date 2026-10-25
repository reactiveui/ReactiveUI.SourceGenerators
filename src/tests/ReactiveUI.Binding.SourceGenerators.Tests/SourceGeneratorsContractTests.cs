// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>
/// Covers binding the members ReactiveUI.SourceGenerators writes, with that generator running beside this one as it does
/// in a build: neither sees the other's output, and the call sites still dispatch and run.
/// </summary>
public class SourceGeneratorsContractTests
{
    /// <summary>The placeholder the scenario names the runtime namespace with.</summary>
    internal const string RuntimeNamespace = "RUNTIME_NAMESPACE";

    /// <summary>The placeholder the scenario names ReactiveUI's <c>ReactiveObject</c> with; each flavour has its own.</summary>
    internal const string RuntimeReactiveObject = "RUNTIME_REACTIVE_OBJECT";

    /// <summary>A view model built from every ReactiveUI.SourceGenerators attribute, and call sites binding each member.</summary>
    internal const string Scenario = """
        using System;
        using System.Collections.ObjectModel;
        using System.ComponentModel;
        using System.Threading;
        using System.Threading.Tasks;
        using ReactiveUI.SourceGenerators;
        using RUNTIME_NAMESPACE;
        using RxObject = RUNTIME_REACTIVE_OBJECT;

        namespace Contract
        {
            public partial class PersonViewModel : RxObject
            {
                [Reactive]
                private string? _nickname;

                [Reactive(SetModifier = AccessModifier.Private)]
                private int _version;

                [ReactiveCollection]
                private ObservableCollection<string>? _tags;

                [BindableDerivedList]
                private ReadOnlyObservableCollection<string>? _derived;

                public PersonViewModel() =>
                    _shoutHelper = this.WhenAnyValue(x => x.Nickname, n => n?.ToUpperInvariant()).ToProperty(this, x => x.Shout);

                [Reactive]
                public partial string? FirstName { get; set; }

                [ObservableAsProperty]
                public partial string? Shout { get; }

                public int SaveCount { get; private set; }

                public void Bump() => Version++;

                [ReactiveCommand]
                private void Save() => SaveCount++;

                [ReactiveCommand]
                private Task<int> LoadAsync(int value, CancellationToken cancellationToken) => Task.FromResult(value * 2);
            }

            [IReactiveObject]
            public partial class PlainViewModel
            {
                [Reactive]
                private string? _title;
            }

            public class Button
            {
                public event EventHandler? Click;

                public void PerformClick() => Click?.Invoke(this, EventArgs.Empty);
            }

            public class PersonView : IViewFor<PersonViewModel>, INotifyPropertyChanged
            {
                private string? _text;

                public event PropertyChangedEventHandler? PropertyChanged;

                public PersonViewModel? ViewModel { get; set; }

                object? IViewFor.ViewModel
                {
                    get => ViewModel;
                    set => ViewModel = (PersonViewModel?)value;
                }

                public string? Text
                {
                    get => _text;
                    set
                    {
                        _text = value;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
                    }
                }

                public int VersionShown { get; set; }

                public Button SaveButton { get; } = new Button();
            }

            public static class Usage
            {
                public static string Run()
                {
                    var results = new System.Collections.Generic.List<string>();
                    var vm = new PersonViewModel();

                    string? nickname = null;
                    using var a = vm.WhenAnyValue(x => x.Nickname).Subscribe(v => nickname = v);
                    vm.Nickname = "Ada";
                    results.Add($"field:{nickname}");

                    string? first = null;
                    using var b = vm.WhenAnyValue(x => x.FirstName).Subscribe(v => first = v);
                    vm.FirstName = "Grace";
                    results.Add($"partial:{first}");

                    results.Add($"toProperty:{vm.Shout}");

                    string? changed = null;
                    using var c = vm.WhenAny(x => x.Nickname, change => change.Value).Subscribe(v => changed = v);
                    vm.Nickname = "Alan";
                    results.Add($"whenAny:{changed}");

                    int version = -1;
                    using var d = vm.WhenAnyValue(x => x.Version).Subscribe(v => version = v);
                    vm.Bump();
                    results.Add($"privateSetter:{version}");

                    int tags = -1;
                    using var e = vm.WhenAnyValue(x => x.Tags).Subscribe(v => tags = v?.Count ?? 0);
                    vm.Tags = new ObservableCollection<string> { "x", "y" };
                    results.Add($"collection:{tags}");

                    var derived = "unset";
                    using var f = vm.WhenAnyValue(x => x.Derived).Subscribe(v => derived = v is null ? "null" : "list");
                    results.Add($"derivedList:{derived}");

                    var plain = new PlainViewModel();
                    string? title = null;
                    using var g = plain.WhenAnyValue(x => x.Title).Subscribe(v => title = v);
                    plain.Title = "Lead";
                    results.Add($"iReactiveObject:{title}");

                    var view = new PersonView { ViewModel = vm };
                    using var h = view.Bind(vm, x => x.Nickname, v => v.Text);
                    view.Text = "Linus";
                    results.Add($"bind:{vm.Nickname}");

                    using var i = view.OneWayBind(vm, x => x.Version, v => v.VersionShown);
                    vm.Bump();
                    results.Add($"oneWayBind:{view.VersionShown}");

                    using var j = view.BindCommand(vm, x => x.SaveCommand, v => v.SaveButton);
                    view.SaveButton.PerformClick();
                    results.Add($"bindCommand:{vm.SaveCount}");

                    object? load = null;
                    using var k = vm.WhenAnyValue(x => x.LoadCommand).Subscribe(v => load = v);
                    results.Add($"typedCommand:{load is not null}");

                    return string.Join("|", results);
                }
            }
        }
        """;

    /// <summary>Every member ReactiveUI.SourceGenerators writes binds, observes and writes at run time, in both runtimes.</summary>
    /// <param name="useReactiveRuntime">Whether to reference the System.Reactive flavour rather than the lean one.</param>
    /// <param name="intercept">Whether the build opts into interceptors.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments(false, false)]
    [Arguments(true, false)]
    [Arguments(false, true)]
    [Arguments(true, true)]
    public async Task EveryGeneratedMember_BindsAtRunTime(bool useReactiveRuntime, bool intercept)
    {
        var result = RunBeside(Scenario, useReactiveRuntime, intercept);
        await Assert.That(result.CompilationErrors.Select(static d => d.ToString())).IsEmpty();

        var (assembly, context) = TestHelper.EmitAndLoad(result);
        try
        {
            var run = assembly.GetType("Contract.Usage")!.GetMethod("Run", BindingFlags.Public | BindingFlags.Static)!;
            await Assert.That((string)run.Invoke(null, null)!).IsEqualTo(
                "field:Ada|partial:Grace|toProperty:ADA|whenAny:Alan|privateSetter:1|collection:2|derivedList:null"
                + "|iReactiveObject:Lead|bind:Linus|oneWayBind:2|bindCommand:1|typedCommand:True");
        }
        finally
        {
            context.Unload();
        }
    }

    /// <summary>Runs this generator beside ReactiveUI.SourceGenerators' generators over a scenario.</summary>
    /// <param name="source">The scenario, naming the runtime namespace with a placeholder.</param>
    /// <param name="useReactiveRuntime">Whether to reference the System.Reactive flavour rather than the lean one.</param>
    /// <param name="intercept">Whether the build opts into interceptors.</param>
    /// <returns>The result of the generation pass.</returns>
    internal static GeneratorTestResult RunBeside(string source, bool useReactiveRuntime, bool intercept)
    {
        var parseOptions = intercept
            ? TestHelper.InterceptingParseOptionsFor(LanguageVersion.CSharp13)
            : TestHelper.ParseOptionsFor(LanguageVersion.CSharp13);
        var compilation = TestHelper.CreateCompilation(
            source
                .Replace(RuntimeNamespace, useReactiveRuntime ? "ReactiveUI.Binding.Reactive" : "ReactiveUI.Binding", StringComparison.Ordinal)
                .Replace(RuntimeReactiveObject, useReactiveRuntime ? "ReactiveUI.Reactive.ReactiveObject" : "ReactiveUI.ReactiveObject", StringComparison.Ordinal),
            parseOptions,
            useReactiveRuntime,
            "TestAssembly",
            [MetadataReference.CreateFromFile(typeof(ReactiveUI.SourceGenerators.ReactiveAttribute).Assembly.Location)]);

        return TestHelper.RunGeneratorBeside(compilation, parseOptions, SourceGeneratorsGenerators());
    }

    /// <summary>Creates ReactiveUI.SourceGenerators' generators, as its package loads them.</summary>
    /// <returns>The generators.</returns>
    private static ImmutableArray<ISourceGenerator> SourceGeneratorsGenerators() =>
    [
        new ReactiveUI.SourceGenerators.ReactiveGenerator().AsSourceGenerator(),
        new ReactiveUI.SourceGenerators.ReactiveCollectionGenerator().AsSourceGenerator(),
        new ReactiveUI.SourceGenerators.BindableDerivedListGenerator().AsSourceGenerator(),
        new ReactiveUI.SourceGenerators.ReactiveCommandGenerator().AsSourceGenerator(),
        new ReactiveUI.SourceGenerators.ReactiveObjectGenerator().AsSourceGenerator(),
    ];
}
