// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>
/// Covers the analyzer and code fix that move the <c>[ObservableAsProperty]</c> forms of ReactiveUI's older source
/// generator (a field, a method, an observable property) onto partial properties.
/// </summary>
public class ObservableAsPropertyMigrationTests
{
    /// <summary>The diagnostic reported for a member the generator writes nothing for.</summary>
    private const string NeedsPartialProperty = "RXUIBIND018";

    /// <summary>The diagnostic reported for a marked method that takes parameters.</summary>
    private const string MethodHasParameters = "RXUIBIND019";

    /// <summary>Splitting text on something that appears once gives this many parts.</summary>
    private const int PartsForOneOccurrence = 2;

    /// <summary>A source whose values a test pushes by hand.</summary>
    private const string SourceType = """
        namespace TestApp
        {
            public sealed class Source<T> : System.IObservable<T>
            {
                private System.IObserver<T>? _observer;

                public System.IDisposable Subscribe(System.IObserver<T> observer)
                {
                    _observer = observer;
                    return new Nothing();
                }

                public void Push(T value) => _observer!.OnNext(value);

                private sealed class Nothing : System.IDisposable
                {
                    public void Dispose()
                    {
                    }
                }
            }
        }
        """;

    /// <summary>Every field shape ReactiveUI.SourceGenerators' own tests covered, in plain, generic and nested types.</summary>
    private const string FieldForms = """
        using System;
        using System.Collections.Generic;
        using System.ComponentModel;
        using ReactiveUI.Binding;

        namespace TestApp
        {
            public record Point(int X, int Y);

            public partial class FieldViewModel : INotifyPropertyChanged
            {
                public FieldViewModel(Source<int> counts, Source<string> codes)
                {
                    _countHelper = counts.ToProperty(this, x => x.Count);
                    m_codeHelper = codes.ToProperty(this, x => x.Code);
                }

                public event PropertyChangedEventHandler? PropertyChanged;

                public string Shout => m_code.ToUpperInvariant();

                [ObservableAsProperty] private int _count = 10;
                [ObservableAsProperty] private string _title = "start";
                [ObservableAsProperty] private int[] _values = new[] { 1, 2 };
                [ObservableAsProperty] private (int Id, string Name) _pair = (1, "one");
                [ObservableAsProperty] private Func<int>? _factory;
                [ObservableAsProperty] private DayOfWeek _day = DayOfWeek.Monday;
                [ObservableAsProperty] private List<string> _names = new List<string>();
                [ObservableAsProperty] private int? _maybe;
                [ObservableAsProperty] private DateTimeOffset _when;
                [ObservableAsProperty] private TimeSpan _span = TimeSpan.FromSeconds(1);
                [ObservableAsProperty] private Point _point = new Point(1, 2);
                [ObservableAsProperty(UseProtected = true)] private string m_code = "c";
            }

            public partial class Holder<T> : INotifyPropertyChanged
            {
                public event PropertyChangedEventHandler? PropertyChanged;

                [ObservableAsProperty] private T _current = default!;
            }

            public class Level1
            {
                public class Level2
                {
                    public class Level3 : INotifyPropertyChanged
                    {
                        public event PropertyChangedEventHandler? PropertyChanged;

                        [ObservableAsProperty] private int _depth = 3;
                    }
                }
            }

            public static class Usage
            {
                public static string Run()
                {
                    var counts = new Source<int>();
                    var codes = new Source<string>();
                    var model = new FieldViewModel(counts, codes);
                    if (model.Title != "start") return "string";
                    if (model.Values.Length != 2) return "array";
                    if (model.Pair.Name != "one") return "tuple";
                    if (model.Factory != null) return "delegate";
                    if (model.Day != DayOfWeek.Monday) return "enum";
                    if (model.Names.Count != 0) return "generic";
                    if (model.Maybe != null) return "nullable";
                    if (model.When != default(DateTimeOffset)) return "time";
                    if (model.Span != TimeSpan.FromSeconds(1)) return "span";
                    if (model.Point != new Point(1, 2)) return "record";
                    counts.Push(5);
                    codes.Push("z");
                    if (model.Count != 5) return "count";
                    if (model.Code != "z" || model.Shout != "Z") return "prefixed field";
                    if (new Holder<int>().Current != 0) return "generic class";
                    if (new Level1.Level2.Level3().Depth != 3) return "nested";
                    return string.Empty;
                }
            }
        }
        """;

    /// <summary>A method and an observable property, built into properties by the generated <c>InitializeOAPH</c>.</summary>
    private const string SourceForms = """
        using System;
        using System.ComponentModel;
        using ReactiveUI.Binding;

        namespace TestApp
        {
            public partial class SourceViewModel : INotifyPropertyChanged
            {
                private readonly Source<string> _names;
                private readonly Source<int> _counts;

                public SourceViewModel(Source<string> names, Source<int> counts)
                {
                    _names = names;
                    _counts = counts;
                    InitializeOAPH();
                }

                public event PropertyChangedEventHandler? PropertyChanged;

                /// <summary>The names.</summary>
                [ObservableAsProperty(InitialValue = "none", ReadOnly = true)]
                [property: System.ComponentModel.Description("shown")]
                public IObservable<string> Name() => _names;

                [ObservableAsProperty(PropertyName = "Total")]
                public IObservable<int> Counts => _counts;
            }

            public static class Usage
            {
                public static string Run()
                {
                    var names = new Source<string>();
                    var counts = new Source<int>();
                    var model = new SourceViewModel(names, counts);
                    string? seen = null;
                    using var subscription = global::ReactiveUI.Primitives.SubscribeExtensions.Subscribe(
                        model.WhenAnyValue(x => x.NameProperty),
                        value => seen = value);
                    names.Push("Ada");
                    counts.Push(3);
                    if (model.NameProperty != "Ada" || seen != "Ada") return "method";
                    if (model.Total != 3) return "observable property";
                    return string.Empty;
                }
            }
        }
        """;

    /// <summary>The analyzer names the form of each member the generator writes nothing for, so the fix knows how to rewrite it.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyzer_ReportsEachFormItCannotGenerate()
    {
        const string Source = """
            using System;
            using System.ComponentModel;
            using ReactiveUI.Binding;

            namespace TestApp
            {
                public partial class Mixed : INotifyPropertyChanged
                {
                    public event PropertyChangedEventHandler? PropertyChanged;

                    [ObservableAsProperty] private int _field;
                    [ObservableAsProperty] private static int _staticField;
                    [ObservableAsProperty] public IObservable<int> Method() => null!;
                    [ObservableAsProperty] public IObservable<int> WithArgument(int value) => null!;
                    [ObservableAsProperty] public int NotObservable() => 0;
                    [ObservableAsProperty] public static IObservable<int> StaticMethod() => null!;
                    [ObservableAsProperty] public IObservable<int> Stream => null!;
                    [ObservableAsProperty] public int Plain { get; } = 0;
                    [ObservableAsProperty] public partial int Generated { get; }
                    [ObservableAsProperty] public static partial int StaticPartial { get; }
                }
            }
            """;

        var diagnostics = await CodeFixRunner.AnalyzeAsync(Source, LanguageVersion.CSharp13);
        var reported = diagnostics
            .Select(static d => $"{d.Id} {d.GetMessage().Split('\'')[1]} {(d.Properties.TryGetValue("Form", out var form) ? form : "-")}")
            .ToArray();

        await Assert.That(reported).IsEquivalentTo(
            [
                $"{NeedsPartialProperty} _field Field",
                $"{NeedsPartialProperty} _staticField -",
                $"{NeedsPartialProperty} Method Method",
                $"{MethodHasParameters} WithArgument -",
                $"{NeedsPartialProperty} NotObservable -",
                $"{NeedsPartialProperty} StaticMethod -",
                $"{NeedsPartialProperty} Stream ObservableProperty",
                $"{NeedsPartialProperty} Plain -",
                $"{NeedsPartialProperty} StaticPartial -",
            ]);
    }

    /// <summary>
    /// The System.Reactive flavour's attribute is recognised among a member's other attributes, and a project that
    /// references neither runtime is not analysed.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyzer_RecognisesEitherFlavour()
    {
        const string Source = """
            using System;
            using ReactiveUI.Binding.Reactive;

            namespace TestApp
            {
                public partial class Reactive
                {
                    [Obsolete]
                    [ObservableAsProperty]
                    private int _field;
                }
            }
            """;

        var reactive = await CodeFixRunner.AnalyzeAsync(Source, LanguageVersion.CSharp13, CodeFixRunner.Runtime.Reactive);
        var none = await CodeFixRunner.AnalyzeAsync(Source, LanguageVersion.CSharp13, CodeFixRunner.Runtime.None);

        await Assert.That(reactive.Select(static d => d.Id)).IsEquivalentTo([NeedsPartialProperty]);
        await Assert.That(none).IsEmpty();
    }

    /// <summary>
    /// A type holding every kind of member is rewritten where the fix applies and left alone elsewhere: a partial
    /// property, other members, a method's own attributes, a positional argument and <c>Inheritance = None</c>.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Fix_LeavesWhatItDoesNotRewrite()
    {
        const string Source = """
            using System;
            using ReactiveUI.Binding;

            namespace TestApp
            {
                public partial class Mixed
                {
                    [ObservableAsProperty] public partial int Generated { get; }
                    [ObservableAsProperty] public static IObservable<int> StaticStream => null!;
                    public int Plain { get; set; }
                    public void Work() { }
                    [Obsolete, ObservableAsProperty] public IObservable<int> Ticks() => null!;
                    [ObservableAsProperty(Inheritance = ObservableAsPropertyInheritance.None)] private int _none;
                    [ObservableAsProperty("positional", ReadOnly = true)] private int _odd;
                }
            }
            """;

        var fixedSource = await CodeFixRunner.FixFirstAsync(Source, LanguageVersion.CSharp13);
        await Assert.That(fixedSource!).Contains("[ObservableAsProperty] public partial int Generated { get; }");
        await Assert.That(fixedSource!).Contains("[ObservableAsProperty] public static IObservable<int> StaticStream => null!;");
        await Assert.That(fixedSource!).Contains("[Obsolete] public IObservable<int> Ticks() => null!;");
        await Assert.That(fixedSource!).Contains("public partial int None { get; }");
        await Assert.That(fixedSource!).Contains("[ObservableAsProperty(ReadOnly = true)]");
        await Assert.That(fixedSource!).Contains("public partial int Odd { get; }");
    }

    /// <summary>A method and an observable property keep their declarations, gain a property each, and are assigned in <c>InitializeOAPH</c>.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Fix_RewritesSourceForms()
    {
        const string Expected = """
            using System;
            using System.ComponentModel;
            using ReactiveUI.Binding;

            namespace TestApp
            {
                public partial class SourceViewModel : INotifyPropertyChanged
                {
                    private readonly Source<string> _names;
                    private readonly Source<int> _counts;

                    public SourceViewModel(Source<string> names, Source<int> counts)
                    {
                        _names = names;
                        _counts = counts;
                        InitializeOAPH();
                    }

                    public event PropertyChangedEventHandler? PropertyChanged;

                    /// <summary>The names.</summary>
                    public IObservable<string> Name() => _names;

                    [ObservableAsProperty(InitialValue = "none")]
                    [System.ComponentModel.Description("shown")]
                    public partial string NameProperty { get; }

                    public IObservable<int> Counts => _counts;

                    [ObservableAsProperty]
                    public partial int Total { get; }

                    /// <summary>Assigns the helpers of the properties built from observables. Call it from the constructor.</summary>
                    protected void InitializeOAPH()
                    {
                        _namePropertyHelper = Name().ToProperty(this, nameof(NameProperty));
                        _totalHelper = Counts.ToProperty(this, nameof(Total));
                    }
                }
            }
            """;

        var fixedSource = await CodeFixRunner.FixFirstAsync(ClassOnly(SourceForms), LanguageVersion.CSharp13);
        await Assert.That(fixedSource).IsEqualTo(Expected);
    }

    /// <summary>
    /// Every field shape compiles once rewritten and keeps its value: the initializer is returned until the helper is
    /// assigned, and the helper then supplies each value. A type the fix writes into, and the types around it, become partial.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FixAll_FieldForms_CompileAndKeepTheirValues()
    {
        var fixedSource = await CodeFixRunner.FixAllAsync($"{FieldForms}\n{SourceType}", LanguageVersion.CSharp13);
        await Assert.That(fixedSource).IsNotNull();
        await Assert.That(fixedSource!).Contains("public partial class Level1");
        await Assert.That(fixedSource!).Contains("public string Shout => Code.ToUpperInvariant();");
        await Assert.That(fixedSource!).Contains("_codeHelper = codes.ToProperty(this, x => x.Code);");
        await AssertRuns(fixedSource!);
    }

    /// <summary>A method and an observable property run through <c>InitializeOAPH</c>, and <c>WhenAnyValue</c> observes the property.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Fix_SourceForms_RunThroughInitializeOAPH()
    {
        var fixedSource = await CodeFixRunner.FixFirstAsync($"{SourceForms}\n{SourceType}", LanguageVersion.CSharp13);
        await AssertRuns(fixedSource!);
    }

    /// <summary>Partial properties need C# 13, so the fix is not offered below it.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Fix_NotOfferedBelowCSharp13()
    {
        var fixedSource = await CodeFixRunner.FixFirstAsync(FieldForms, LanguageVersion.CSharp12);
        await Assert.That(fixedSource).IsNull();
    }

    /// <summary>
    /// The field's <c>Inheritance</c> becomes a modifier, whichever enum names it; a string field set to something other
    /// than a literal is left alone; and an existing <c>InitializeOAPH</c> gains the new assignment.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Fix_InheritanceComputedStringAndExistingInitializer()
    {
        const string Source = """
            using System;
            using ReactiveUI.Binding;

            namespace TestApp
            {
                public partial class Derived : Base
                {
                    [ObservableAsProperty(Inheritance = ObservableAsPropertyInheritance.Virtual)] private int _a;
                    [ObservableAsProperty(Inheritance = ObservableAsPropertyInheritance.Override)] private int _b;
                    [ObservableAsProperty(Inheritance = InheritanceModifier.New)] private int _c;
                    [ObservableAsProperty] private string _computed = Environment.NewLine;
                    [ObservableAsProperty] private int _x, _y;
                    [ObservableAsProperty] private int _;
                    [ObservableAsProperty] private int Upper;
                    [ObservableAsProperty] public IObservable<int> Ticks() => null!;

                    protected void InitializeOAPH()
                    {
                    }
                }
            }
            """;

        var fixedSource = await CodeFixRunner.FixFirstAsync(Source, LanguageVersion.CSharp13);
        await Assert.That(fixedSource!).Contains("public virtual partial int A { get; }");
        await Assert.That(fixedSource!).Contains("public override partial int B { get; }");
        await Assert.That(fixedSource!).Contains("public new partial int C { get; }");
        await Assert.That(fixedSource!).Contains("private string _computed = Environment.NewLine;");
        await Assert.That(fixedSource!).Contains("private int _x, _y;");
        await Assert.That(fixedSource!).Contains("private int _;");
        await Assert.That(fixedSource!).Contains("private int Upper;");
        await Assert.That(fixedSource!).Contains("            _ticksPropertyHelper = Ticks().ToProperty(this, nameof(TicksProperty));\n        }");
        await Assert.That(fixedSource!.Split("InitializeOAPH()").Length).IsEqualTo(PartsForOneOccurrence);
    }

    /// <summary>A type with no modifiers becomes partial, and a document written on one line keeps working.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Fix_TypeWithoutModifiersOnOneLine()
    {
        const string Source = "namespace TestApp { class Bare { [ReactiveUI.Binding.ObservableAsProperty] int _size; } }";

        var fixedSource = await CodeFixRunner.FixFirstAsync(Source, LanguageVersion.CSharp13);
        await Assert.That(fixedSource!).Contains("partial class Bare");
        await Assert.That(fixedSource!).Contains("public partial int Size { get; }");
    }

    /// <summary>Fix-all leaves the document unchanged when no diagnostic in it names a form it rewrites.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FixAll_NothingToRewrite()
    {
        const string Source = """
            using ReactiveUI.Binding;

            namespace TestApp
            {
                public partial class Settings
                {
                    [ObservableAsProperty] private static int _count;
                }
            }
            """;

        var fixedSource = await CodeFixRunner.FixAllAsync(Source, LanguageVersion.CSharp13);
        await Assert.That(fixedSource ?? Source).IsEqualTo(Source);
    }

    /// <summary>Removes the usage class, whose calls the expected text of a rewrite does not need.</summary>
    /// <param name="source">The source forms.</param>
    /// <returns>The source without its usage class.</returns>
    private static string ClassOnly(string source)
    {
        var start = source.IndexOf("\n    public static class Usage", StringComparison.Ordinal);
        var end = source.LastIndexOf('}');
        return $"{source[..start]}{source[end..]}";
    }

    /// <summary>Generates from a fixed source, compiles it, and checks its usage returns no failure.</summary>
    /// <param name="fixedSource">The fixed source.</param>
    /// <returns>A task representing the asynchronous assertion.</returns>
    private static async Task AssertRuns(string fixedSource)
    {
        var result = TestHelper.RunGenerator(fixedSource, LanguageVersion.CSharp13);
        await result.CompilationSucceeds();
        var (assembly, context) = TestHelper.EmitAndLoad(result);
        try
        {
            var run = assembly.GetType("TestApp.Usage")!.GetMethod("Run", BindingFlags.Public | BindingFlags.Static)!;
            await Assert.That((string)run.Invoke(null, null)!).IsEqualTo(string.Empty);
        }
        finally
        {
            context.Unload();
        }
    }
}
