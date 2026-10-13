// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.RuntimeExecution;

/// <summary>Executes bindings on a generic view base supplied by another assembly.</summary>
public class ReferencedViewModelReplacementRuntimeTests
{
    /// <summary>The separately compiled view assembly name.</summary>
    private const string ExternalAssemblyName = "ExternalViews";

    /// <summary>The generic view implemented in the referenced assembly.</summary>
    private const string BaseSource = """
        using System.ComponentModel;
        using ReactiveUI.Binding;
        namespace ExternalViews
        {
            public class ViewBase<T> : IViewFor<T>, INotifyPropertyChanged where T : class
            {
                private T _viewModel;
                public event PropertyChangedEventHandler PropertyChanged;
                public T ViewModel
                {
                    get { return _viewModel; }
                    set { _viewModel = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ViewModel")); }
                }
                object IViewFor.ViewModel { get { return ViewModel; } set { ViewModel = (T)value; } }
            }
        }
        """;

    /// <summary>The consumer binds once and follows every view model transition.</summary>
    private const string InteractionSource = """
        using System;
        using System.Threading.Tasks;
        using ReactiveUI.Binding;
        namespace TestApp
        {
            public class Model
            {
                public Interaction<string, string> Confirm { get; } = new Interaction<string, string>();
            }
            public class View : ExternalViews.ViewBase<Model> { }
            public static class Usage
            {
                public static async Task<string> Run()
                {
                    var view = new View();
                    var first = new Model();
                    var second = new Model();
                    var later = new Model();
                    var binding = view.BindInteraction(view.ViewModel, vm => vm.Confirm, ctx =>
                    {
                        ctx.SetOutput("handled");
                        return Task.CompletedTask;
                    });
                    view.ViewModel = first;
                    var a = await Answer(first);
                    view.ViewModel = second;
                    var b = await Answer(first);
                    var c = await Answer(second);
                    view.ViewModel = null;
                    var d = await Answer(second);
                    view.ViewModel = later;
                    var e = await Answer(later);
                    binding.Dispose();
                    var f = await Answer(later);
                    view.ViewModel = first;
                    var g = await Answer(first);
                    return string.Join(",", a, b, c, d, e, f, g);
                }
                private static async Task<string> Answer(Model model)
                {
                    try { return await model.Confirm.Handle("ask"); }
                    catch (UnhandledInteractionException<string, string>) { return "unhandled"; }
                }
            }
        }
        """;

    /// <summary>Property bindings follow the referenced base's current model.</summary>
    private const string PropertySource = """
        using System;
        using System.ComponentModel;
        using System.Threading.Tasks;
        using ReactiveUI.Binding;
        namespace TestApp
        {
            public class Model : INotifyPropertyChanged
            {
                private string _name;
                public event PropertyChangedEventHandler PropertyChanged;
                public string Name { get { return _name; } set { _name = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Name")); } }
            }
            public class View : ExternalViews.ViewBase<Model>
            {
                public string Text { get; set; } = "empty";
            }
            public static class Usage
            {
                public static Task<string> Run()
                {
                    var view = new View();
                    var first = new Model { Name = "first" };
                    var second = new Model { Name = "second" };
                    var later = new Model { Name = "later" };
                    var binding = view.OneWayBind(view.ViewModel, vm => vm.Name, v => v.Text);
                    view.ViewModel = first;
                    var a = view.Text;
                    view.ViewModel = second;
                    first.Name = "stale";
                    var b = view.Text;
                    view.ViewModel = null;
                    second.Name = "stale";
                    view.ViewModel = later;
                    var c = view.Text;
                    binding.Dispose();
                    later.Name = "disposed";
                    view.ViewModel = first;
                    return Task.FromResult(string.Join(",", a, b, c, view.Text));
                }
            }
        }
        """;

    /// <summary>The file-and-line dispatcher observes the inherited typed view model.</summary>
    /// <returns>The asynchronous test.</returns>
    [Test]
    public async Task BindInteraction_ReferencedGenericViewBase_GeneratesForCSharp73()
    {
        var reference = TestHelper.CompileToReference(BaseSource, ExternalAssemblyName, LanguageVersion.CSharp7_3);
        var result = TestHelper.RunGenerator(InteractionSource, LanguageVersion.CSharp7_3, null, false, [reference]);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
        await result.GeneratedSourceContains("BindInteractionDispatch.g.cs", "\"ViewModel\"");
    }

    /// <summary>Moves a task handler between view models and removes it on disposal.</summary>
    /// <param name="observableHandler">Whether the handler returns an observable.</param>
    /// <param name="initiallyAssigned">Whether the binding starts with a view model.</param>
    /// <param name="reactiveRuntime">Whether to use the System.Reactive runtime flavour.</param>
    /// <returns>The asynchronous test.</returns>
    [Test]
    [MatrixDataSource]
    public async Task BindInteraction_ReferencedGenericViewBase_TracksTheFullLifecycle(
        [Matrix(false, true)] bool observableHandler,
        [Matrix(false, true)] bool initiallyAssigned,
        [Matrix(false, true)] bool reactiveRuntime)
    {
        var source = observableHandler
            ? InteractionSource.Replace("return Task.CompletedTask;", "return new Completed();", StringComparison.Ordinal)
                .Replace(
                    "public class Model",
                    "public class Completed : IObservable<int>, IDisposable { "
                    + "public IDisposable Subscribe(IObserver<int> observer) { observer.OnNext(0); observer.OnCompleted(); return this; } "
                    + "public void Dispose() { } } public class Model",
                    StringComparison.Ordinal)
            : InteractionSource;
        if (initiallyAssigned)
        {
            source = source.Replace("var binding =", "view.ViewModel = first; var binding =", StringComparison.Ordinal);
        }

        await AssertLifecycle(source, "handled,unhandled,handled,unhandled,handled,unhandled,unhandled", reactiveRuntime);
    }

    /// <summary>Property bindings observe a model inherited from the referenced view base.</summary>
    /// <param name="twoWay">Whether to use the two-way binding API.</param>
    /// <returns>The asynchronous test.</returns>
    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task PropertyBinding_ReferencedGenericViewBase_TracksTheFullLifecycle(bool twoWay)
    {
        var source = twoWay ? PropertySource.Replace("view.OneWayBind(", "view.Bind(", StringComparison.Ordinal) : PropertySource;
        await AssertLifecycle(source, "first,second,later,later");
    }

    /// <summary>Command bindings follow the referenced base and stop invoking after disposal.</summary>
    /// <returns>The asynchronous test.</returns>
    [Test]
    public async Task BindCommand_ReferencedGenericViewBase_TracksTheFullLifecycle()
    {
        const string source = """
            using System;
            using System.Threading.Tasks;
            using System.Windows.Input;
            using ReactiveUI.Binding;
            namespace TestApp
            {
                public class Command : ICommand
                {
                    private readonly Action<object> _execute;
                    public Command(Action<object> execute) { _execute = execute; }
                    public event EventHandler CanExecuteChanged;
                    public bool CanExecute(object p) { return true; }
                    public void Execute(object p) { _execute(p); }
                }
                public class Model { public ICommand Save { get; set; } public string Parameter { get; set; } }
                public class Button
                {
                    public event EventHandler Click;
                    public void Invoke() { Click?.Invoke(this, EventArgs.Empty); }
                }
                public class View : ExternalViews.ViewBase<Model> { public Button SaveButton { get; } = new Button(); }
                public static class Usage
                {
                    public static Task<string> Run()
                    {
                        var trace = "";
                        var view = new View();
                        var first = new Model { Save = new Command(p => trace += "first:" + p + ","), Parameter = "a" };
                        var second = new Model { Save = new Command(p => trace += "second:" + p + ","), Parameter = "b" };
                        var later = new Model { Save = new Command(p => trace += "later:" + p + ","), Parameter = "c" };
                        var binding = view.BindCommand(view.ViewModel, vm => vm.Save, v => v.SaveButton, vm => vm.Parameter);
                        view.SaveButton.Invoke();
                        view.ViewModel = first;
                        view.SaveButton.Invoke();
                        view.ViewModel = second;
                        view.SaveButton.Invoke();
                        view.ViewModel = null;
                        view.SaveButton.Invoke();
                        view.ViewModel = later;
                        view.SaveButton.Invoke();
                        binding.Dispose();
                        view.SaveButton.Invoke();
                        view.ViewModel = first;
                        view.SaveButton.Invoke();
                        return Task.FromResult(trace);
                    }
                }
            }
            """;
        await AssertLifecycle(source, "first:a,second:b,later:c,");
    }

    /// <summary>Generates and executes the consumer with a separately emitted view assembly.</summary>
    /// <param name="source">The consumer source.</param>
    /// <param name="expected">The expected lifecycle trace.</param>
    /// <param name="reactiveRuntime">Whether to use the System.Reactive runtime flavour.</param>
    /// <returns>The asynchronous assertion.</returns>
    private static async Task AssertLifecycle(string source, string expected, bool reactiveRuntime = false)
    {
        var baseSource = BaseSource;
        if (reactiveRuntime)
        {
            baseSource = baseSource.Replace("using ReactiveUI.Binding;", "using ReactiveUI.Binding.Reactive;", StringComparison.Ordinal);
            source = source.Replace("using ReactiveUI.Binding;", "using ReactiveUI.Binding.Reactive;", StringComparison.Ordinal);
        }

        var reference = reactiveRuntime
            ? TestHelper.CreateCompilation(baseSource, LanguageVersion.CSharp10, true, ExternalAssemblyName, []).ToMetadataReference()
            : TestHelper.CompileToReference(baseSource, ExternalAssemblyName, LanguageVersion.CSharp10);
        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10, null, reactiveRuntime, [reference]);
        await result.CompilationSucceeds();
        var (assembly, context) = TestHelper.EmitAndLoad(result, false, (CompilationReference)reference);
        try
        {
            var method = assembly.GetType("TestApp.Usage")!.GetMethod("Run", BindingFlags.Public | BindingFlags.Static)!;
            var outcome = await (Task<string>)method.Invoke(null, null)!;
            await Assert.That(outcome).IsEqualTo(expected);
        }
        finally
        {
            context.Unload();
        }
    }
}
