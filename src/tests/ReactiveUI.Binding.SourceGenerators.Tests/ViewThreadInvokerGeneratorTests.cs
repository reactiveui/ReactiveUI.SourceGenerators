// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>Snapshot tests for the invoker a generated binding carries for a WPF, WinForms, MAUI or Avalonia target.</summary>
public class ViewThreadInvokerGeneratorTests
{
    /// <summary>A view model with one notifying property.</summary>
    private const string ViewModelSource = """

                                           namespace TestApp
                                           {
                                               public class MyViewModel : INotifyPropertyChanged
                                               {
                                                   private string _name = "";

                                                   public event PropertyChangedEventHandler? PropertyChanged;

                                                   public string Name
                                                   {
                                                       get => _name;
                                                       set
                                                       {
                                                           _name = value;
                                                           PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
                                                       }
                                                   }

                                                   public System.Windows.Input.ICommand? Save { get; set; }
                                               }
                                           }

                                           """;

    /// <summary>The WPF types the generated invoker calls.</summary>
    private const string WpfStubs = """

                                    namespace System.Windows.Threading
                                    {
                                        public enum DispatcherPriority
                                        {
                                            Normal = 9,
                                        }

                                        public class Dispatcher
                                        {
                                            public object BeginInvoke(DispatcherPriority priority, Delegate method, object? arg) => new object();
                                        }

                                        public class DispatcherObject
                                        {
                                            public Dispatcher Dispatcher { get; } = new Dispatcher();

                                            public bool CheckAccess() => true;
                                        }
                                    }

                                    """ + RuntimeInvokerStandIns.Wpf;

    /// <summary>The WinForms types the generated invoker calls.</summary>
    private const string WinFormsStubs = """

                                         namespace System.Windows.Forms
                                         {
                                             public class Control
                                             {
                                                 public bool InvokeRequired { get; set; }

                                                 public IAsyncResult? BeginInvoke(Delegate method, params object?[] args) => null;
                                             }
                                         }

                                         """ + RuntimeInvokerStandIns.WinForms;

    /// <summary>The MAUI types the generated invoker calls.</summary>
    private const string MauiStubs = """

                                     namespace Microsoft.Maui.Dispatching
                                     {
                                         public interface IDispatcher
                                         {
                                             bool IsDispatchRequired { get; }

                                             bool Dispatch(System.Action action);
                                         }
                                     }

                                     namespace Microsoft.Maui.Controls
                                     {
                                         public class BindableObject
                                         {
                                             public Microsoft.Maui.Dispatching.IDispatcher? Dispatcher { get; set; }
                                         }
                                     }

                                     """ + RuntimeInvokerStandIns.Maui;

    /// <summary>The Avalonia types the generated invoker calls.</summary>
    private const string AvaloniaStubs = """

                                         namespace Avalonia.Threading
                                         {
                                             public class Dispatcher
                                             {
                                                 public void Post(Action<object?> callback, object? state)
                                                 {
                                                 }
                                             }
                                         }

                                         namespace Avalonia
                                         {
                                             public class AvaloniaObject
                                             {
                                                 public Avalonia.Threading.Dispatcher Dispatcher { get; } = new Avalonia.Threading.Dispatcher();

                                                 public bool CheckAccess() => true;
                                             }
                                         }

                                         """ + RuntimeInvokerStandIns.Avalonia;

    /// <summary>The Uno types the generated invoker calls.</summary>
    private const string UnoStubs = """

                                    namespace Microsoft.UI.Dispatching
                                    {
                                        public class DispatcherQueue
                                        {
                                            public bool HasThreadAccess => true;

                                            public bool TryEnqueue(Action callback) => true;
                                        }
                                    }

                                    namespace Microsoft.UI.Xaml
                                    {
                                        public class DependencyObject
                                        {
                                            public Microsoft.UI.Dispatching.DispatcherQueue DispatcherQueue { get; } = new Microsoft.UI.Dispatching.DispatcherQueue();
                                        }
                                    }

                                    """ + RuntimeInvokerStandIns.Uno;

    /// <summary>The imports every test source starts with.</summary>
    private const string Imports = """
                                   using System;
                                   using System.ComponentModel;
                                   using ReactiveUI.Binding;

                                   """;

    /// <summary>A one-way binding onto a WPF target carries the WPF invoker.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_ToAWpfTarget_CarriesTheWpfInvoker()
    {
        const string usage = """
                             namespace TestApp
                             {
                                 public class MyView : System.Windows.Threading.DispatcherObject
                                 {
                                     public string Text { get; set; } = "";
                                 }

                                 public static class Usage
                                 {
                                     public static object Bind(MyViewModel viewModel, MyView view) =>
                                         viewModel.BindOneWay(view, x => x.Name, x => x.Text);
                                 }
                             }
                             """;

        var result = await TestHelper.TestPassWithResult(
            Imports + usage + ViewModelSource + WpfStubs,
            typeof(ViewThreadInvokerGeneratorTests),
            LanguageVersion.CSharp10);

        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>A stream bound onto a WinForms control carries the WinForms invoker.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindTo_ToAWinFormsControl_CarriesTheWinFormsInvoker()
    {
        const string usage = """
                             namespace TestApp
                             {
                                 public class MyControl : System.Windows.Forms.Control
                                 {
                                     public string Text { get; set; } = "";
                                 }

                                 public static class Usage
                                 {
                                     public static object Bind(IObservable<string> values, MyControl control) =>
                                         values.BindTo(control, x => x.Text);
                                 }
                             }
                             """;

        var result = await TestHelper.TestPassWithResult(
            Imports + usage + ViewModelSource + WinFormsStubs,
            typeof(ViewThreadInvokerGeneratorTests),
            LanguageVersion.CSharp10);

        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>A two-way binding onto a MAUI target carries the MAUI invoker for the target only.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindTwoWay_ToAMauiTarget_CarriesTheMauiInvokerForTheTargetOnly()
    {
        const string usage = """
                             namespace TestApp
                             {
                                 public class MyEntry : Microsoft.Maui.Controls.BindableObject, INotifyPropertyChanged
                                 {
                                     private string _text = "";

                                     public event PropertyChangedEventHandler? PropertyChanged;

                                     public string Text
                                     {
                                         get => _text;
                                         set
                                         {
                                             _text = value;
                                             PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
                                         }
                                     }
                                 }

                                 public static class Usage
                                 {
                                     public static object Bind(MyViewModel viewModel, MyEntry entry) =>
                                         viewModel.BindTwoWay(entry, x => x.Name, x => x.Text);
                                 }
                             }
                             """;

        var result = await TestHelper.TestPassWithResult(
            Imports + usage + ViewModelSource + MauiStubs,
            typeof(ViewThreadInvokerGeneratorTests),
            LanguageVersion.CSharp10);

        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>A one-way binding onto an Avalonia target carries the Avalonia invoker.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_ToAnAvaloniaTarget_CarriesTheAvaloniaInvoker()
    {
        const string usage = """
                             namespace TestApp
                             {
                                 public class MyTextBox : Avalonia.AvaloniaObject
                                 {
                                     public string Text { get; set; } = "";
                                 }

                                 public static class Usage
                                 {
                                     public static object Bind(MyViewModel viewModel, MyTextBox textBox) =>
                                         viewModel.BindOneWay(textBox, x => x.Name, x => x.Text);
                                 }
                             }
                             """;

        var result = await TestHelper.TestPassWithResult(
            Imports + usage + ViewModelSource + AvaloniaStubs,
            typeof(ViewThreadInvokerGeneratorTests),
            LanguageVersion.CSharp10);

        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>A one-way binding onto an Uno target carries the Uno invoker.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_ToAnUnoTarget_CarriesTheUnoInvoker()
    {
        const string usage = """
                             namespace TestApp
                             {
                                 public class MyTextBlock : Microsoft.UI.Xaml.DependencyObject
                                 {
                                     public string Text { get; set; } = "";
                                 }

                                 public static class Usage
                                 {
                                     public static object Bind(MyViewModel viewModel, MyTextBlock textBlock) =>
                                         viewModel.BindOneWay(textBlock, x => x.Name, x => x.Text);
                                 }
                             }
                             """;

        var result = await TestHelper.TestPassWithResult(
            Imports + usage + ViewModelSource + UnoStubs,
            typeof(ViewThreadInvokerGeneratorTests),
            LanguageVersion.CSharp10);

        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>A view-first one-way binding and a command binding on a WPF view carry the WPF invoker.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ViewFirstBindings_OnAWpfView_CarryTheWpfInvoker()
    {
        const string usage = """
                             namespace TestApp
                             {
                                 public class CommandButton
                                 {
                                     public System.Windows.Input.ICommand? Command { get; set; }

                                     public object? CommandParameter { get; set; }
                                 }

                                 public class MyView : System.Windows.Threading.DispatcherObject, IViewFor, INotifyPropertyChanged
                                 {
                                     public event PropertyChangedEventHandler? PropertyChanged;

                                     public object? ViewModel { get; set; }

                                     public string Title { get; set; } = "";

                                     public CommandButton SaveButton { get; } = new CommandButton();
                                 }

                                 public static class Usage
                                 {
                                     public static object BindTitle(MyView view, MyViewModel viewModel) =>
                                         view.OneWayBind(viewModel, x => x.Name, x => x.Title);

                                     public static object BindSave(MyView view, MyViewModel viewModel) =>
                                         view.BindCommand(viewModel, x => x.Save, x => x.SaveButton);
                                 }
                             }
                             """;

        var result = await TestHelper.TestPassWithResult(
            Imports + usage + ViewModelSource + WpfStubs,
            typeof(ViewThreadInvokerGeneratorTests),
            LanguageVersion.CSharp10);

        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>A command binding on a plain view writes its command on the thread of the WPF control it binds.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindCommand_OnAPlainViewWithAWpfControl_CarriesTheWpfInvokerForTheControl()
    {
        const string usage = """
                             namespace TestApp
                             {
                                 public class CommandButton : System.Windows.Threading.DispatcherObject
                                 {
                                     public System.Windows.Input.ICommand? Command { get; set; }

                                     public object? CommandParameter { get; set; }
                                 }

                                 public class MyView : IViewFor, INotifyPropertyChanged
                                 {
                                     public event PropertyChangedEventHandler? PropertyChanged;

                                     public object? ViewModel { get; set; }

                                     public CommandButton SaveButton { get; } = new CommandButton();
                                 }

                                 public static class Usage
                                 {
                                     public static object BindSave(MyView view, MyViewModel viewModel) =>
                                         view.BindCommand(viewModel, x => x.Save, x => x.SaveButton);
                                 }
                             }
                             """;

        var result = await TestHelper.TestPassWithResult(
            Imports + usage + ViewModelSource + WpfStubs,
            typeof(ViewThreadInvokerGeneratorTests),
            LanguageVersion.CSharp10);

        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>The generated invokers compile against the System.Reactive flavour of the runtime.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Invokers_CompileAgainstTheSystemReactiveRuntime()
    {
        const string usage = """
                             namespace TestApp
                             {
                                 public class MyView : System.Windows.Threading.DispatcherObject
                                 {
                                     public string Text { get; set; } = "";
                                 }

                                 public class MyControl : System.Windows.Forms.Control
                                 {
                                     public string Text { get; set; } = "";
                                 }

                                 public class MyEntry : Microsoft.Maui.Controls.BindableObject
                                 {
                                     public string Text { get; set; } = "";
                                 }

                                 public class MyTextBox : Avalonia.AvaloniaObject
                                 {
                                     public string Text { get; set; } = "";
                                 }

                                 public static class Usage
                                 {
                                     public static object BindView(MyViewModel viewModel, MyView view) =>
                                         viewModel.BindOneWay(view, x => x.Name, x => x.Text);

                                     public static object BindControl(MyViewModel viewModel, MyControl control) =>
                                         viewModel.BindOneWay(control, x => x.Name, x => x.Text);

                                     public static object BindEntry(MyViewModel viewModel, MyEntry entry) =>
                                         viewModel.BindOneWay(entry, x => x.Name, x => x.Text);

                                     public static object BindTextBox(MyViewModel viewModel, MyTextBox textBox) =>
                                         viewModel.BindOneWay(textBox, x => x.Name, x => x.Text);
                                 }
                             }
                             """;

        var source = (Imports + usage + ViewModelSource + WpfStubs + WinFormsStubs + MauiStubs + AvaloniaStubs)
            .Replace("using ReactiveUI.Binding;", "using ReactiveUI.Binding.Reactive;", StringComparison.Ordinal)
            .Replace("namespace ReactiveUI.Binding.", "namespace ReactiveUI.Binding.Reactive.", StringComparison.Ordinal)
            .Replace("global::ReactiveUI.Binding.IViewThreadInvoker", "global::ReactiveUI.Binding.Reactive.IViewThreadInvoker", StringComparison.Ordinal);
        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10, null, true);

        await result.CompilationSucceeds();
    }
}
