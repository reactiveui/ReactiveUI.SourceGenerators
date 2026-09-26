// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections;
using System.Reflection;
using System.Runtime.Loader;
using ReactiveUI.SourceGenerators.WinForms;

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>
/// Runs the generated Windows Forms hosts against ReactiveUI.Binding 8.4.0 in each flavour: the hosts follow their
/// properties through Binding's <c>ObservedProperty</c>, show their default content, and swap in the view for a routed or
/// hosted view model.
/// </summary>
/// <remarks>
/// A ReactiveUI built on ReactiveUI.Binding declares no <c>IViewFor</c> of its own; no such release is published yet, so
/// the few ReactiveUI types a host touches are declared in source, and Binding is the real package.
/// </remarks>
public class ControlHostObservedPropertyTests
{
    /// <summary>The ReactiveUI types a host touches, as a ReactiveUI built on ReactiveUI.Binding declares them.</summary>
    private const string ReactiveUISource = """
        using System;
        using System.Collections.Generic;
        using System.ComponentModel;
        using System.Runtime.CompilerServices;

        namespace ReactiveUI
        {
            public interface IReactiveObject : INotifyPropertyChanged, INotifyPropertyChanging
            {
                void RaisePropertyChanging(PropertyChangingEventArgs args);

                void RaisePropertyChanged(PropertyChangedEventArgs args);
            }

            public static class IReactiveObjectExtensions
            {
                public static TRet RaiseAndSetIfChanged<TObj, TRet>(this TObj source, ref TRet field, TRet value, [CallerMemberName] string? name = null)
                    where TObj : IReactiveObject
                {
                    if (EqualityComparer<TRet>.Default.Equals(field, value))
                    {
                        return value;
                    }

                    source.RaisePropertyChanging(new(name));
                    field = value;
                    source.RaisePropertyChanged(new(name));
                    return value;
                }

                public static void SubscribePropertyChangedEvents<TObj>(this TObj source) where TObj : IReactiveObject { }

                public static void SubscribePropertyChangingEvents<TObj>(this TObj source) where TObj : IReactiveObject { }
            }

            public interface IScreen
            {
                RoutingState Router { get; }
            }

            public interface IRoutableViewModel
            {
                string? UrlPathSegment { get; }

                IScreen HostScreen { get; }
            }

            public sealed class RoutingState
            {
                private readonly List<IObserver<IRoutableViewModel>> _observers = new();

                public IObservable<IRoutableViewModel> CurrentViewModel => new Current(this);

                public void Navigate(IRoutableViewModel viewModel)
                {
                    foreach (var observer in _observers.ToArray())
                    {
                        observer.OnNext(viewModel);
                    }
                }

                private sealed class Current(RoutingState state) : IObservable<IRoutableViewModel>
                {
                    public IDisposable Subscribe(IObserver<IRoutableViewModel> observer)
                    {
                        state._observers.Add(observer);
                        return new Unsubscribe(() => state._observers.Remove(observer));
                    }
                }

                private sealed class Unsubscribe(Func<bool> remove) : IDisposable
                {
                    public void Dispose() => remove();
                }
            }

            public static class RxApp
            {
                public static IObserver<Exception> DefaultExceptionHandler { get; } = new Thrower();

                private sealed class Thrower : IObserver<Exception>
                {
                    public void OnCompleted() { }

                    public void OnError(Exception error) => throw error;

                    public void OnNext(Exception value) => throw value;
                }
            }
        }
        """;

    /// <summary>The hosts, a view and a view locator; <c>BINDING_NAMESPACE</c> is the ReactiveUI.Binding flavour.</summary>
    private const string HostsSource = """
        using System.ComponentModel;
        using System.Windows.Forms;
        using ReactiveUI.SourceGenerators.WinForms;

        namespace Hosts
        {
            [RoutedControlHost("System.Windows.Forms.UserControl")]
            public partial class RoutedHost
            {
                private IContainer? components;

                private void InitializeComponent() { }
            }

            [ViewModelControlHost("System.Windows.Forms.UserControl")]
            public partial class ViewModelHost
            {
                private IContainer? components;

                private void InitializeComponent() { }
            }

            public sealed class Shell : ReactiveUI.IScreen
            {
                public ReactiveUI.RoutingState Router { get; } = new();
            }

            public sealed class PageViewModel : ReactiveUI.IRoutableViewModel
            {
                public string? UrlPathSegment => "page";

                public ReactiveUI.IScreen HostScreen { get; } = new Shell();
            }

            public sealed class PageView : UserControl, BINDING_NAMESPACE.IViewFor<PageViewModel>
            {
                public PageViewModel? ViewModel { get; set; }

                object? BINDING_NAMESPACE.IViewFor.ViewModel { get => ViewModel; set => ViewModel = (PageViewModel?)value; }
            }

            public sealed class Locator : BINDING_NAMESPACE.IViewLocator
            {
                public BINDING_NAMESPACE.IViewFor? ResolveView<TViewModel>(TViewModel viewModel, string? contract)
                    where TViewModel : class => ResolveView((object?)viewModel, contract);

                public BINDING_NAMESPACE.IViewFor? ResolveView(object? viewModel, string? contract) =>
                    viewModel is PageViewModel ? new PageView() : null;

                public BINDING_NAMESPACE.IViewFor? ResolveViewUnsafe(object? viewModel, string? contract) => ResolveView(viewModel, contract);
            }
        }
        """;

    /// <summary>The lean ReactiveUI.Binding namespace.</summary>
    private const string Binding = "ReactiveUI.Binding";

    /// <summary>The System.Reactive ReactiveUI.Binding namespace.</summary>
    private const string BindingReactive = "ReactiveUI.Binding.Reactive";

    /// <summary>The hosts' default content property.</summary>
    private const string DefaultContent = nameof(DefaultContent);

    /// <summary>The view-model host's view model property, and a view's.</summary>
    private const string ViewModel = nameof(ViewModel);

    /// <summary>Both hosts follow their properties through <c>ObservedProperty</c>, and no longer write their own observable.</summary>
    /// <param name="bindingNamespace">The ReactiveUI.Binding flavour.</param>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    [Arguments(Binding)]
    [Arguments(BindingReactive)]
    public async Task HostsFollowTheirPropertiesThroughObservedProperty(string bindingNamespace)
    {
        var (_, generated) = Generate(bindingNamespace);

        foreach (var source in generated)
        {
            await Assert.That(source).Contains($"global::{bindingNamespace}.ObservedProperty.Create(this, static x => x.DefaultContent, static x => x.DefaultContent)");
            await Assert.That(source).Contains($"global::{bindingNamespace}.ObservedProperty.Switch(global::{bindingNamespace}.ObservedProperty.Create(this, static x => x.ViewContractObservable");
            await Assert.That(source).DoesNotContain("PropertyObservable<");
            await Assert.That(source).DoesNotContain("WhenAny");
        }
    }

    /// <summary>The routed host shows its default content, then the view for the view model the router navigates to.</summary>
    /// <param name="bindingNamespace">The ReactiveUI.Binding flavour.</param>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    [Arguments(Binding)]
    [Arguments(BindingReactive)]
    public async Task RoutedHostShowsDefaultContentThenTheRoutedView(string bindingNamespace)
    {
        var assembly = Compile(bindingNamespace);
        var host = Create(assembly, "Hosts.RoutedHost");
        var defaultContent = Create(assembly, "System.Windows.Forms.UserControl");
        Set(host, nameof(ViewLocator), Create(assembly, "Hosts.Locator"));

        Set(host, DefaultContent, defaultContent);
        await AssertShows(host, defaultContent);

        var router = Create(assembly, "ReactiveUI.RoutingState");
        Set(host, "Router", router);
        _ = router.GetType().GetMethod("Navigate")!.Invoke(router, [Create(assembly, "Hosts.PageViewModel")]);

        await AssertShowsPageView(host);
    }

    /// <summary>The view-model host shows its default content, then the view for the view model it is given.</summary>
    /// <param name="bindingNamespace">The ReactiveUI.Binding flavour.</param>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    [Arguments(Binding)]
    [Arguments(BindingReactive)]
    public async Task ViewModelHostShowsDefaultContentThenTheViewForItsViewModel(string bindingNamespace)
    {
        var assembly = Compile(bindingNamespace);
        var host = Create(assembly, "Hosts.ViewModelHost");
        var defaultContent = Create(assembly, "System.Windows.Forms.UserControl");
        Set(host, nameof(ViewLocator), Create(assembly, "Hosts.Locator"));

        Set(host, DefaultContent, defaultContent);
        await AssertShows(host, defaultContent);

        var viewModel = Create(assembly, "Hosts.PageViewModel");
        Set(host, ViewModel, viewModel);

        await AssertShowsPageView(host);
        await Assert.That(Get(ShownControls(host)[0]!, ViewModel)).IsSameReferenceAs(viewModel);
    }

    /// <summary>Gets the source of the hosts, a view and view locator, for a flavour.</summary>
    /// <param name="bindingNamespace">The ReactiveUI.Binding flavour.</param>
    /// <returns>The source.</returns>
    private static string GetHostsSource(string bindingNamespace) =>
        HostsSource.Replace("BINDING_NAMESPACE", bindingNamespace, StringComparison.Ordinal);

    /// <summary>Runs both host generators over a flavour's source, the desktop stubs and ReactiveUI.Binding.</summary>
    /// <param name="bindingNamespace">The ReactiveUI.Binding flavour.</param>
    /// <returns>The output compilation and the generated host sources.</returns>
    private static (Compilation Output, List<string> GeneratedSources) Generate(string bindingNamespace)
    {
        var bindingAssembly = bindingNamespace == Binding
            ? typeof(ReactiveUI.Binding.ObservedProperty).Assembly
            : typeof(ReactiveUI.Binding.Reactive.ObservedProperty).Assembly;
        var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp13);
        var compilation = CSharpCompilation.Create(
            $"ControlHostObservedProperty.{bindingNamespace}",
            [
                CSharpSyntaxTree.ParseText(ReactiveUISource, parseOptions),
                CSharpSyntaxTree.ParseText(GetHostsSource(bindingNamespace), parseOptions),
                CSharpSyntaxTree.ParseText(TestCompilationReferences.WindowsDesktopStubs, parseOptions),
            ],
            TestCompilationReferences.CreateForAssemblies(
                typeof(object).Assembly,
                typeof(System.ComponentModel.Component).Assembly,
                typeof(System.ComponentModel.INotifyPropertyChanged).Assembly,
                typeof(System.Linq.Expressions.Expression).Assembly,
                typeof(RoutedControlHostAttribute).Assembly,
                bindingAssembly),
            new(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        var driver = CSharpGeneratorDriver
            .Create([new RoutedControlHostGenerator().AsSourceGenerator(), new ViewModelControlHostGenerator().AsSourceGenerator()], parseOptions: parseOptions)
            .RunGeneratorsAndUpdateCompilation(compilation, out var output, out _);

        var generated = new List<string>();
        foreach (var result in driver.GetRunResult().Results)
        {
            foreach (var file in result.GeneratedSources)
            {
                generated.Add(file.SourceText.ToString());
            }
        }

        return (output, generated);
    }

    /// <summary>Generates, compiles and loads a flavour's hosts.</summary>
    /// <param name="bindingNamespace">The ReactiveUI.Binding flavour.</param>
    /// <returns>The loaded assembly.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "SES1402", Justification = "The test loads the assembly it has just compiled from its own source.")]
    private static Assembly Compile(string bindingNamespace)
    {
        var (output, _) = Generate(bindingNamespace);
        using var image = new MemoryStream();
        var emit = output.Emit(image);
        if (!emit.Success)
        {
            throw new InvalidOperationException(string.Join(Environment.NewLine, emit.Diagnostics));
        }

        image.Position = 0;
        return new AssemblyLoadContext($"{nameof(ControlHostObservedPropertyTests)}.{bindingNamespace}", isCollectible: true).LoadFromStream(image);
    }

    /// <summary>Creates an instance of a type from the compiled assembly.</summary>
    /// <param name="assembly">The compiled assembly.</param>
    /// <param name="typeName">The type's full name.</param>
    /// <returns>The instance.</returns>
    private static object Create(Assembly assembly, string typeName) => Activator.CreateInstance(assembly.GetType(typeName, throwOnError: true)!)!;

    /// <summary>Reads a public property.</summary>
    /// <param name="target">The object.</param>
    /// <param name="name">The property name.</param>
    /// <returns>The property's value.</returns>
    private static object Get(object target, string name) => target.GetType().GetProperty(name)!.GetValue(target)!;

    /// <summary>Sets a public property.</summary>
    /// <param name="target">The object.</param>
    /// <param name="name">The property name.</param>
    /// <param name="value">The value.</param>
    private static void Set(object target, string name, object? value) => target.GetType().GetProperty(name)!.SetValue(target, value);

    /// <summary>Lists the controls a host shows.</summary>
    /// <param name="host">The host.</param>
    /// <returns>The controls.</returns>
    private static List<object?> ShownControls(object host)
    {
        var controls = new List<object?>();
        foreach (var control in (IEnumerable)Get(host, "Controls"))
        {
            controls.Add(control);
        }

        return controls;
    }

    /// <summary>Asserts that a host shows exactly one control, the given one.</summary>
    /// <param name="host">The host.</param>
    /// <param name="control">The control it should show.</param>
    /// <returns>A task to monitor the async.</returns>
    private static async Task AssertShows(object host, object control)
    {
        var shown = ShownControls(host);
        await Assert.That(shown.Count).IsEqualTo(1);
        await Assert.That(shown[0]).IsSameReferenceAs(control);
    }

    /// <summary>Asserts that a host shows exactly one control, a page view.</summary>
    /// <param name="host">The host.</param>
    /// <returns>A task to monitor the async.</returns>
    private static async Task AssertShowsPageView(object host)
    {
        var shown = ShownControls(host);
        await Assert.That(shown.Count).IsEqualTo(1);
        await Assert.That(shown[0]!.GetType().FullName).IsEqualTo("Hosts.PageView");
    }
}
