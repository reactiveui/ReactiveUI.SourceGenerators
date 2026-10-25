// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

/// <summary>
/// Call sites whose types generated code cannot name: an anonymous type, a private or protected nested type, or a type
/// built from one. Each scenario exposes <c>TestApp.Scenario.Run()</c>, which returns the values the call delivered.
/// </summary>
internal static partial class UnnameableTypeScenarios
{
    /// <summary>The name of the type each scenario runs through.</summary>
    internal const string ScenarioTypeName = "TestApp.Scenario";

    /// <summary>The diagnostic reported for a call that names a type generated code cannot reach.</summary>
    internal const string UnreachableTypeId = "RXUIBIND015";

    /// <summary>The diagnostic reported for a call that has no generated binding.</summary>
    internal const string NoGeneratedBindingId = "RXUIBIND021";

    /// <summary>A WhenAnyValue call whose selector builds an anonymous type.</summary>
    internal const string AnonymousWhenAnyValueResult = """
        public static class Scenario
        {
            public static int[] Run()
            {
                var vm = new Vm { A = 1, B = 2 };
                var recorder = Recorder.Watch(vm.WhenAnyValue(x => x.A, x => x.B, (a, b) => new { a, b }));
                vm.A = 3;
                vm.B = 4;
                return recorder.Values.Select(v => (v.a * 10) + v.b).ToArray();
            }
        }
        """;

    /// <summary>A WhenAnyValue call whose selector builds a private nested type.</summary>
    internal const string PrivateWhenAnyValueResult = """
        public static class Scenario
        {
            public static int[] Run()
            {
                var vm = new Vm { A = 1 };
                var recorder = Recorder.Watch(vm.WhenAnyValue(x => x.A, a => new Result(a)));
                vm.A = 3;
                return recorder.Values.Select(v => v.Value).ToArray();
            }

            private sealed class Result
            {
                public Result(int value) => Value = value;

                public int Value { get; }
            }
        }
        """;

    /// <summary>A WhenAnyValue call whose selector builds a list closed over a private nested type.</summary>
    internal const string PrivateTypeArgumentResult = """
        public static class Scenario
        {
            public static int[] Run()
            {
                var vm = new Vm { A = 1 };
                var recorder = Recorder.Watch(vm.WhenAnyValue(x => x.A, a => new List<Result> { new Result(a) }));
                vm.A = 3;
                return recorder.Values.Select(v => v[0].Value).ToArray();
            }

            protected sealed class Result
            {
                public Result(int value) => Value = value;

                public int Value { get; }
            }
        }
        """;

    /// <summary>A WhenAnyValue call whose selector builds a tuple holding a private nested type.</summary>
    internal const string PrivateTupleResult = """
        public static class Scenario
        {
            public static int[] Run()
            {
                var vm = new Vm { A = 1 };
                var recorder = Recorder.Watch(vm.WhenAnyValue(x => x.A, a => (a, new Result(a * 2))));
                vm.A = 3;
                return recorder.Values.Select(v => v.Item1 + v.Item2.Value).ToArray();
            }

            private sealed class Result
            {
                public Result(int value) => Value = value;

                public int Value { get; }
            }
        }
        """;

    /// <summary>A WhenAny call whose selector builds an anonymous type from the observed change.</summary>
    internal const string AnonymousWhenAnyResult = """
        public static class Scenario
        {
            public static int[] Run()
            {
                var vm = new Vm { A = 1 };
                var recorder = Recorder.Watch(vm.WhenAny(x => x.A, change => new { Seen = change.Value }));
                vm.A = 3;
                return recorder.Values.Select(v => v.Seen).ToArray();
            }
        }
        """;

    /// <summary>A WhenChanged call whose conversion builds an anonymous type.</summary>
    internal const string AnonymousWhenChangedResult = """
        public static class Scenario
        {
            public static int[] Run()
            {
                var vm = new Vm { A = 1, B = 2 };
                var recorder = Recorder.Watch(vm.WhenChanged(x => x.A, x => x.B, (a, b) => new { Sum = a + b }));
                vm.A = 3;
                return recorder.Values.Select(v => v.Sum).ToArray();
            }
        }
        """;

    /// <summary>A WhenChanging call whose conversion builds an anonymous type.</summary>
    internal const string AnonymousWhenChangingResult = """
        public static class Scenario
        {
            public static int[] Run()
            {
                var vm = new Vm { A = 1, B = 2 };
                var recorder = Recorder.Watch(vm.WhenChanging(x => x.A, x => x.B, (a, b) => new { Before = a + b }));
                vm.A = 3;
                return recorder.Values.Select(v => v.Before).ToArray();
            }
        }
        """;

    /// <summary>The same WhenChanging call with a named result, which the anonymous one has to match value for value.</summary>
    internal const string NamedWhenChangingResult = """
        public static class Scenario
        {
            public static int[] Run()
            {
                var vm = new Vm { A = 1, B = 2 };
                var recorder = Recorder.Watch(vm.WhenChanging(x => x.A, x => x.B, (a, b) => a + b));
                vm.A = 3;
                return recorder.Values.ToArray();
            }
        }
        """;

    /// <summary>A WhenAnyObservable call whose selector builds an anonymous type.</summary>
    internal const string AnonymousWhenAnyObservableResult = """
        public static class Scenario
        {
            public static int[] Run()
            {
                var first = new Subject<int>();
                var second = new Subject<int>();
                var vm = new Vm { First = first, Second = second };
                var recorder = Recorder.Watch(vm.WhenAnyObservable(x => x.First, x => x.Second, (a, b) => new { a, b }));
                first.OnNext(1);
                second.OnNext(2);
                first.OnNext(3);
                return recorder.Values.Select(v => (v.a * 10) + v.b).ToArray();
            }
        }
        """;

    /// <summary>A WhenAnyValue call on a private nested view model, made inside the partial class that declares it.</summary>
    internal const string HostedPrivateSource = """
        public static partial class Scenario
        {
            public static int[] Run()
            {
                var vm = new Inner { A = 1 };
                var recorder = Recorder.Watch(vm.WhenAnyValue(x => x.A));
                vm.A = 3;
                return recorder.Values.ToArray();
            }

            private sealed class Inner : INotifyPropertyChanged
            {
                private int _a;

                public event PropertyChangedEventHandler? PropertyChanged;

                public int A
                {
                    get => _a;
                    set
                    {
                        _a = value;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(A)));
                    }
                }
            }
        }
        """;

    /// <summary>A WhenAnyValue call on a private nested view model whose declaring class is not partial.</summary>
    internal const string UnhostablePrivateSource = """
        public static class Scenario
        {
            public static int[] Run()
            {
                var vm = new Inner { A = 1 };
                var recorder = Recorder.Watch(vm.WhenAnyValue(x => x.A));
                vm.A = 3;
                return recorder.Values.ToArray();
            }

            private sealed class Inner : INotifyPropertyChanged
            {
                public event PropertyChangedEventHandler? PropertyChanged;

                public int A { get; set; }
            }
        }
        """;

    /// <summary>A WhenAnyValue call on a file-local view model, which no other file can name.</summary>
    internal const string FileLocalSource = """
        public static class Scenario
        {
            public static int[] Run()
            {
                var vm = new FileVm { A = 1 };
                var recorder = Recorder.Watch(vm.WhenAnyValue(x => x.A));
                vm.A = 3;
                return recorder.Values.ToArray();
            }
        }

        file sealed class FileVm : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged;

            public int A { get; set; }
        }
        """;

    /// <summary>A WhenAnyValue call on a private nested view model, made inside a file-local partial class.</summary>
    internal const string FileLocalHost = """
        public static class Scenario
        {
            public static int[] Run() => FileHost.Run();
        }

        file static partial class FileHost
        {
            public static int[] Run()
            {
                var vm = new Inner { A = 1 };
                var recorder = Recorder.Watch(vm.WhenAnyValue(x => x.A));
                vm.A = 3;
                return recorder.Values.ToArray();
            }

            private sealed class Inner : INotifyPropertyChanged
            {
                public event PropertyChangedEventHandler? PropertyChanged;

                public int A { get; set; }
            }
        }
        """;

    /// <summary>A WhenAnyValue call on a private nested view model, made inside a generic partial class.</summary>
    internal const string GenericHost = """
        public static class Scenario
        {
            public static int[] Run() => GenericHost<int>.Run();
        }

        public static partial class GenericHost<T>
        {
            public static int[] Run()
            {
                var vm = new Inner { A = 1 };
                var recorder = Recorder.Watch(vm.WhenAnyValue(x => x.A));
                vm.A = 3;
                return recorder.Values.ToArray();
            }

            private sealed class Inner : INotifyPropertyChanged
            {
                public event PropertyChangedEventHandler? PropertyChanged;

                public int A { get; set; }
            }
        }
        """;

    /// <summary>A WhenChanged call on a private nested view model, written through the stub's declaring class.</summary>
    internal const string HostedStaticFormCall = """
        public static partial class Scenario
        {
            public static int[] Run()
            {
                var vm = new Inner { A = 1 };
                var recorder = Recorder.Watch(ReactiveUIBindingExtensions.WhenChanged(vm, x => x.A));
                vm.A = 3;
                return recorder.Values.ToArray();
            }

            private sealed class Inner : INotifyPropertyChanged
            {
                public event PropertyChangedEventHandler? PropertyChanged;

                public int A { get; set; }
            }
        }
        """;

    /// <summary>A BindOneWay call with a scheduler between private nested types, made inside the partial class that declares them.</summary>
    internal const string HostedSchedulerOverload = """
        public static partial class Scenario
        {
            public static int[] Run()
            {
                var source = new Source { A = 1 };
                var target = new Target();
                using (source.BindOneWay(target, x => x.A, x => x.B, global::ReactiveUI.Primitives.Concurrency.ImmediateSequencer.Instance))
                {
                    source.A = 3;
                }

                return new[] { target.B };
            }

            private sealed class Source : INotifyPropertyChanged
            {
                public event PropertyChangedEventHandler? PropertyChanged;

                public int A { get; set; }
            }

            private sealed class Target
            {
                public int B { get; set; }
            }
        }
        """;

    /// <summary>A BindCommand call between a private nested view and view model, made inside the partial class that declares them.</summary>
    internal const string HostedPrivateBindCommand = """
        public static partial class Scenario
        {
            public static int[] Run()
            {
                var executed = 0;
                var view = new View { ViewModel = new ViewModel { Save = new RecordingCommand(() => executed++) } };
                using (view.BindCommand(view.ViewModel, vm => vm.Save, v => v.SaveButton))
                {
                    view.SaveButton.PerformClick();
                }

                view.SaveButton.PerformClick();
                return new[] { executed };
            }

            private sealed class ViewModel : INotifyPropertyChanged
            {
                public event PropertyChangedEventHandler? PropertyChanged;

                public ICommand? Save { get; set; }
            }

            private sealed class View : INotifyPropertyChanged, IViewFor<ViewModel>
            {
                public event PropertyChangedEventHandler? PropertyChanged;

                public ViewModel? ViewModel { get; set; }

                object? IViewFor.ViewModel
                {
                    get => ViewModel;
                    set => ViewModel = (ViewModel?)value;
                }

                public MyButton SaveButton { get; } = new MyButton();
            }
        }
        """;

    /// <summary>
    /// One call to every generated API between a private nested view and view model, made inside the partial class that
    /// declares them. Returns, in order: the last WhenChanged, WhenAny and WhenAnyObservable values; 1 when WhenChanging
    /// delivered; the values BindOneWay, OneWayBind and BindTo wrote; the InvokeCommand and BindInteraction counts; 1 when
    /// BindTwoWay and Bind carried an edit both ways; the ToProperty value; and the value of a WhenChanged call on a
    /// public view model, which is generated the usual way beside the hosted calls.
    /// </summary>
    internal const string HostedEveryApi = """
        public static partial class Scenario
        {
            public static int[] Run()
            {
                var executed = 0;
                var handled = 0;
                var ticks = new Subject<int>();
                var pushes = new Subject<int>();
                var vm = new ViewModel { Age = 1, Name = "a", Ticks = ticks, Save = new RecordingCommand(() => executed++) };
                var view = new View { ViewModel = vm };
                vm.Attach();

                var changed = Recorder.Watch(vm.WhenChanged(x => x.Age));
                var unhosted = Recorder.Watch(new Vm { A = 4 }.WhenChanged(x => x.A));
                var changing = Recorder.Watch(vm.WhenChanging(x => x.Age));
                var any = Recorder.Watch(vm.WhenAny(x => x.Age, change => change.Value));
                var anyObservable = Recorder.Watch(vm.WhenAnyObservable(x => x.Ticks));
                var bindings = new[]
                {
                    vm.BindOneWay(view, x => x.Age, v => v.AgeCopy),
                    vm.BindTwoWay(view, x => x.Name, v => v.NameCopy),
                    view.OneWayBind(vm, x => x.Age, v => v.AgeFromView),
                    view.Bind(vm, x => x.Name, v => v.NameFromView),
                    pushes.BindTo(view, v => v.Pushed),
                    pushes.InvokeCommand(vm, x => x.Save),
                    view.BindInteraction(vm, x => x.Confirm, context =>
                    {
                        handled++;
                        context.SetOutput(true);
                        return System.Threading.Tasks.Task.CompletedTask;
                    }),
                };

                vm.Age = 2;
                ticks.OnNext(5);
                pushes.OnNext(7);
                view.NameCopy = "b";
                _ = Recorder.Watch(vm.Confirm.Handle("question"));

                foreach (var binding in bindings)
                {
                    binding.Dispose();
                }

                return new[]
                {
                    changed.Values.Last(),
                    any.Values.Last(),
                    anyObservable.Values.Last(),
                    changing.Values.Count > 0 ? 1 : 0,
                    view.AgeCopy,
                    view.AgeFromView,
                    view.Pushed,
                    executed,
                    handled,
                    vm.Name == "b" && view.NameFromView == "b" ? 1 : 0,
                    vm.Doubled,
                    unhosted.Values.Last(),
                };
            }

            private sealed partial class ViewModel : INotifyPropertyChanged, INotifyPropertyChanging
            {
                private int _age;
                private string? _name;
                private ObservableAsPropertyHelper<int>? _doubled;

                public event PropertyChangedEventHandler? PropertyChanged;

                public event PropertyChangingEventHandler? PropertyChanging;

                public int Age
                {
                    get => _age;
                    set
                    {
                        PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(Age)));
                        _age = value;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Age)));
                    }
                }

                public string? Name
                {
                    get => _name;
                    set
                    {
                        PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(Name)));
                        _name = value;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
                    }
                }

                public int Doubled => _doubled?.Value ?? 0;

                public IObservable<int>? Ticks { get; set; }

                public ICommand? Save { get; set; }

                public IInteraction<string, bool> Confirm { get; } = new Interaction<string, bool>();

                public void Attach() => _doubled = this.WhenAnyValue(x => x.Age, a => a * 2).ToProperty(this, x => x.Doubled);
            }

            private sealed class View : INotifyPropertyChanged, IViewFor<ViewModel>
            {
                private int _ageCopy;
                private string? _nameCopy;
                private int _ageFromView;
                private string? _nameFromView;
                private int _pushed;

                public event PropertyChangedEventHandler? PropertyChanged;

                public ViewModel? ViewModel { get; set; }

                object? IViewFor.ViewModel
                {
                    get => ViewModel;
                    set => ViewModel = (ViewModel?)value;
                }

                public int AgeCopy { get => _ageCopy; set => Set(ref _ageCopy, value); }

                public string? NameCopy { get => _nameCopy; set => Set(ref _nameCopy, value); }

                public int AgeFromView { get => _ageFromView; set => Set(ref _ageFromView, value); }

                public string? NameFromView { get => _nameFromView; set => Set(ref _nameFromView, value); }

                public int Pushed { get => _pushed; set => Set(ref _pushed, value); }

                private void Set<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string name = "")
                {
                    field = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
                }
            }
        }
        """;

    /// <summary>The usings, view model, and recorder every scenario shares.</summary>
    private const string Shared = """
        using System;
        using System.Collections.Generic;
        using System.ComponentModel;
        using System.Linq;
        using System.Windows.Input;
        using ReactiveUI.Binding;

        namespace TestApp
        {
            public sealed class Vm : INotifyPropertyChanged, INotifyPropertyChanging
            {
                private int _a;
                private int _b;

                public event PropertyChangedEventHandler? PropertyChanged;

                public event PropertyChangingEventHandler? PropertyChanging;

                public int A
                {
                    get => _a;
                    set
                    {
                        PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(A)));
                        _a = value;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(A)));
                    }
                }

                public int B
                {
                    get => _b;
                    set
                    {
                        PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(B)));
                        _b = value;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(B)));
                    }
                }

                public IObservable<int>? First { get; set; }

                public IObservable<int>? Second { get; set; }
            }

            public sealed class Subject<T> : IObservable<T>
            {
                private readonly List<IObserver<T>> _observers = new List<IObserver<T>>();

                public void OnNext(T value)
                {
                    foreach (var observer in _observers.ToArray())
                    {
                        observer.OnNext(value);
                    }
                }

                public IDisposable Subscribe(IObserver<T> observer)
                {
                    _observers.Add(observer);
                    return new Unsubscriber(() => _observers.Remove(observer));
                }
            }

            public sealed class Unsubscriber : IDisposable
            {
                private readonly Action _dispose;

                public Unsubscriber(Action dispose) => _dispose = dispose;

                public void Dispose() => _dispose();
            }

            public sealed class Recorder<T> : IObserver<T>
            {
                public List<T> Values { get; } = new List<T>();

                public void OnNext(T value) => Values.Add(value);

                public void OnError(Exception error) => throw error;

                public void OnCompleted()
                {
                }
            }

            public sealed class RecordingCommand : ICommand
            {
                private readonly Action _onExecute;

                public RecordingCommand(Action onExecute) => _onExecute = onExecute;

                public event EventHandler? CanExecuteChanged;

                public bool CanExecute(object? parameter) => true;

                public void Execute(object? parameter) => _onExecute();
            }

            public sealed class MyButton
            {
                public event EventHandler? Click;

                public void PerformClick() => Click?.Invoke(this, EventArgs.Empty);
            }

            public static class Recorder
            {
                public static Recorder<T> Watch<T>(IObservable<T> source)
                {
                    var recorder = new Recorder<T>();
                    source.Subscribe(recorder);
                    return recorder;
                }
            }

        SCENARIO
        }
        """;

    /// <summary>Finds a scenario by the name of the constant that holds it.</summary>
    /// <param name="name">The constant's name.</param>
    /// <returns>The scenario's <c>Scenario</c> type.</returns>
    /// <exception cref="ArgumentOutOfRangeException">No scenario has the name.</exception>
    internal static string Named(string name) => name switch
    {
        nameof(AnonymousWhenAnyValueResult) => AnonymousWhenAnyValueResult,
        nameof(PrivateWhenAnyValueResult) => PrivateWhenAnyValueResult,
        nameof(PrivateTypeArgumentResult) => PrivateTypeArgumentResult,
        nameof(PrivateTupleResult) => PrivateTupleResult,
        nameof(AnonymousWhenAnyResult) => AnonymousWhenAnyResult,
        nameof(AnonymousWhenChangedResult) => AnonymousWhenChangedResult,
        nameof(AnonymousWhenChangingResult) => AnonymousWhenChangingResult,
        nameof(AnonymousWhenAnyObservableResult) => AnonymousWhenAnyObservableResult,
        nameof(HostedPrivateSource) => HostedPrivateSource,
        nameof(HostedPrivateBindCommand) => HostedPrivateBindCommand,
        nameof(HostedEveryApi) => HostedEveryApi,
        nameof(UnhostablePrivateSource) => UnhostablePrivateSource,
        nameof(FileLocalSource) => FileLocalSource,
        nameof(TypeParameterReceiver) => TypeParameterReceiver,
        nameof(TypeParameterValue) => TypeParameterValue,
        nameof(TypeParameterResult) => TypeParameterResult,
        nameof(TypeParameterViewBase) => TypeParameterViewBase,
        nameof(TypeParameterEveryApi) => TypeParameterEveryApi,
        nameof(TypeParameterInsideArray) => TypeParameterInsideArray,
        nameof(ConstraintNamesAnotherTypeParameter) => ConstraintNamesAnotherTypeParameter,
        nameof(UnmappableTypeParameter) => UnmappableTypeParameter,
        nameof(HostedPrivateMember) => HostedPrivateMember,
        nameof(UnhostablePrivateMember) => UnhostablePrivateMember,
        nameof(ComputedPath) => ComputedPath,
        nameof(StaticFormCall) => StaticFormCall,
        nameof(StoredPath) => StoredPath,
        nameof(StoredCommandParameter) => StoredCommandParameter,
        nameof(FileLocalHost) => FileLocalHost,
        nameof(GenericHost) => GenericHost,
        nameof(HostedStaticFormCall) => HostedStaticFormCall,
        nameof(HostedSchedulerOverload) => HostedSchedulerOverload,
        _ => throw new ArgumentOutOfRangeException(nameof(name), name, "No scenario has this name."),
    };

    /// <summary>Places a scenario beside the shared view model and recorder.</summary>
    /// <param name="scenario">The scenario's <c>Scenario</c> type.</param>
    /// <returns>The complete source.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string Source(string scenario) => Shared.Replace("SCENARIO", scenario, StringComparison.Ordinal);

    /// <summary>Runs the generator over a scenario, opted into interception or not.</summary>
    /// <param name="scenario">The scenario's <c>Scenario</c> type.</param>
    /// <param name="intercept">Whether the build lists the generated namespace for interception.</param>
    /// <returns>The generator result.</returns>
    internal static GeneratorTestResult Generate(string scenario, bool intercept)
    {
        var parseOptions = intercept
            ? TestHelper.InterceptingParseOptionsFor(LanguageVersion.CSharp11)
            : TestHelper.ParseOptionsFor(LanguageVersion.CSharp11);
        var compilation = TestHelper.CreateCompilation(Source(scenario), parseOptions, false, "TestAssembly", []);

        return TestHelper.RunGenerator(compilation, parseOptions, "TestApp", true);
    }

    /// <summary>Gets the errors of a compilation that come from generated files.</summary>
    /// <param name="result">The generator result.</param>
    /// <returns>The errors located in generated source.</returns>
    internal static ImmutableArray<Diagnostic> GeneratedCodeErrors(GeneratorTestResult result) =>
    [
        .. result.OutputCompilation.Emit(Stream.Null).Diagnostics
            .Where(static d => d.Severity == DiagnosticSeverity.Error
                && d.Location.SourceTree?.FilePath.EndsWith(".g.cs", StringComparison.Ordinal) == true),
    ];
}
