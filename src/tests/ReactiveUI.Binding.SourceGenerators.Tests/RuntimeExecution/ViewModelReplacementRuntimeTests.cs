// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.RuntimeExecution;

/// <summary>
/// Covers a view whose view model is replaced after the binding is made. Setting the view model after
/// construction and swapping it later is the ordinary view lifecycle - a router does it on every navigation -
/// so a binding that holds the instance it was handed stops tracking the view without saying so.
/// </summary>
public class ViewModelReplacementRuntimeTests
{
    /// <summary>The value the second view model publishes after it replaces the first.</summary>
    private const string ValueFromReplacement = "second";

    /// <summary>A view that notifies when its view model changes, bound once and then given a new one.</summary>
    private const string ViewModelReplacementSource = """
                                                      using System;
                                                      using System.ComponentModel;
                                                      using ReactiveUI.Binding;

                                                      namespace TestApp
                                                      {
                                                          public class MyViewModel : INotifyPropertyChanged
                                                          {
                                                              private string _name = "";

                                                              public event PropertyChangedEventHandler PropertyChanged;

                                                              public string Name
                                                              {
                                                                  get { return _name; }
                                                                  set
                                                                  {
                                                                      _name = value;
                                                                      var handler = PropertyChanged;
                                                                      if (handler != null)
                                                                      {
                                                                          handler(this, new PropertyChangedEventArgs("Name"));
                                                                      }
                                                                  }
                                                              }
                                                          }

                                                          public class MyView : INotifyPropertyChanged, IViewFor<MyViewModel>
                                                          {
                                                              private MyViewModel _viewModel;

                                                              public event PropertyChangedEventHandler PropertyChanged;

                                                              public string Text { get; set; } = "";

                                                              public MyViewModel ViewModel
                                                              {
                                                                  get { return _viewModel; }
                                                                  set
                                                                  {
                                                                      _viewModel = value;
                                                                      var handler = PropertyChanged;
                                                                      if (handler != null)
                                                                      {
                                                                          handler(this, new PropertyChangedEventArgs("ViewModel"));
                                                                      }
                                                                  }
                                                              }

                                                              object IViewFor.ViewModel
                                                              {
                                                                  get { return ViewModel; }
                                                                  set { ViewModel = (MyViewModel)value; }
                                                              }
                                                          }

                                                          public static class Usage
                                                          {
                                                              public static string Run()
                                                              {
                                                                  var view = new MyView();
                                                                  view.ViewModel = new MyViewModel { Name = "first" };

                                                                  var binding = view.OneWayBind(view.ViewModel, vm => vm.Name, v => v.Text);

                                                                  view.ViewModel = new MyViewModel { Name = "ignored" };
                                                                  view.ViewModel.Name = "second";

                                                                  return view.Text;
                                                              }
                                                          }
                                                      }
                                                      """;

    /// <summary>A view whose command binding is made once and is then given a new view model.</summary>
    private const string CommandReplacementSource = """
                                                    using System;
                                                    using System.ComponentModel;
                                                    using System.Windows.Input;
                                                    using ReactiveUI.Binding;

                                                    namespace TestApp
                                                    {
                                                        public class RecordingCommand : ICommand
                                                        {
                                                            private readonly Action _onExecute;

                                                            public RecordingCommand(Action onExecute)
                                                            {
                                                                _onExecute = onExecute;
                                                            }

                                                            public event EventHandler CanExecuteChanged;

                                                            public bool CanExecute(object parameter)
                                                            {
                                                                return true;
                                                            }

                                                            public void Execute(object parameter)
                                                            {
                                                                _onExecute();
                                                            }
                                                        }

                                                        public class MyButton
                                                        {
                                                            public event EventHandler Click;

                                                            public void PerformClick()
                                                            {
                                                                var handler = Click;
                                                                if (handler != null)
                                                                {
                                                                    handler(this, EventArgs.Empty);
                                                                }
                                                            }
                                                        }

                                                        public class MyViewModel : INotifyPropertyChanged
                                                        {
                                                            public event PropertyChangedEventHandler PropertyChanged;

                                                            public ICommand Save { get; set; }
                                                        }

                                                        public class MyView : INotifyPropertyChanged, IViewFor<MyViewModel>
                                                        {
                                                            private MyViewModel _viewModel;

                                                            public event PropertyChangedEventHandler PropertyChanged;

                                                            public MyButton SaveButton { get; } = new MyButton();

                                                            public MyViewModel ViewModel
                                                            {
                                                                get { return _viewModel; }
                                                                set
                                                                {
                                                                    _viewModel = value;
                                                                    var handler = PropertyChanged;
                                                                    if (handler != null)
                                                                    {
                                                                        handler(this, new PropertyChangedEventArgs("ViewModel"));
                                                                    }
                                                                }
                                                            }

                                                            object IViewFor.ViewModel
                                                            {
                                                                get { return ViewModel; }
                                                                set { ViewModel = (MyViewModel)value; }
                                                            }
                                                        }

                                                        public static class Usage
                                                        {
                                                            public static string Run()
                                                            {
                                                                var invoked = "";
                                                                var view = new MyView();
                                                                view.ViewModel = new MyViewModel();
                                                                view.ViewModel.Save = new RecordingCommand(() => invoked = "first");

                                                                var binding = view.BindCommand(view.ViewModel, vm => vm.Save, v => v.SaveButton);

                                                                var replacement = new MyViewModel();
                                                                replacement.Save = new RecordingCommand(() => invoked = "second");
                                                                view.ViewModel = replacement;

                                                                view.SaveButton.PerformClick();

                                                                return invoked;
                                                            }
                                                        }
                                                    }
                                                    """;

    /// <summary>A view whose interaction binding is made once and is then given a new view model.</summary>
    private const string InteractionReplacementSource = """
                                                        using System;
                                                        using System.ComponentModel;
                                                        using System.Threading.Tasks;
                                                        using ReactiveUI.Binding;

                                                        namespace TestApp
                                                        {
                                                            public class MyViewModel : INotifyPropertyChanged
                                                            {
                                                                public event PropertyChangedEventHandler PropertyChanged;

                                                                public Interaction<string, string> Confirm { get; set; } = new Interaction<string, string>();
                                                            }

                                                            public class MyView : INotifyPropertyChanged, IViewFor<MyViewModel>
                                                            {
                                                                private MyViewModel _viewModel;

                                                                public event PropertyChangedEventHandler PropertyChanged;

                                                                public MyViewModel ViewModel
                                                                {
                                                                    get { return _viewModel; }
                                                                    set
                                                                    {
                                                                        _viewModel = value;
                                                                        var handler = PropertyChanged;
                                                                        if (handler != null)
                                                                        {
                                                                            handler(this, new PropertyChangedEventArgs("ViewModel"));
                                                                        }
                                                                    }
                                                                }

                                                                object IViewFor.ViewModel
                                                                {
                                                                    get { return ViewModel; }
                                                                    set { ViewModel = (MyViewModel)value; }
                                                                }
                                                            }

                                                            public static class Usage
                                                            {
                                                                public static string Run()
                                                                {
                                                                    var view = new MyView();
                                                                    view.ViewModel = new MyViewModel();

                                                                    var binding = view.BindInteraction(view.ViewModel, vm => vm.Confirm, ctx =>
                                                                    {
                                                                        ctx.SetOutput("second");
                                                                        return Task.CompletedTask;
                                                                    });

                                                                    view.ViewModel = new MyViewModel();

                                                                    return view.ViewModel.Confirm.Handle("ask").GetAwaiter().GetResult();
                                                                }
                                                            }
                                                        }
                                                        """;

    /// <summary>
    /// A binding follows the view's current view model. Holding the instance the call was handed leaves the
    /// replaced view model driving the view, with nothing raised to say the binding went stale.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task OneWayBind_AfterTheViewModelIsReplaced_TracksTheNewViewModel() =>
        AssertTracksReplacement(ViewModelReplacementSource);

    /// <summary>The two-way binding follows the view's current view model in the view-model-to-view direction.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task Bind_AfterTheViewModelIsReplaced_TracksTheNewViewModel() =>
        AssertTracksReplacement(
            ViewModelReplacementSource.Replace(
                "view.OneWayBind(view.ViewModel, vm => vm.Name, v => v.Text)",
                "view.Bind(view.ViewModel, vm => vm.Name, v => v.Text)",
                StringComparison.Ordinal));

    /// <summary>A one-way binding created while the view has no view model follows the first assigned model.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task OneWayBind_WhenViewModelInitiallyNull_TracksTheAssignedViewModel() =>
        AssertTracksReplacement(WithNullViewModel(ViewModelReplacementSource, "view.OneWayBind"));

    /// <summary>A two-way binding created while the view has no view model follows the first assigned model.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task Bind_WhenViewModelInitiallyNull_TracksTheAssignedViewModel() =>
        AssertTracksReplacement(WithNullViewModel(ToBind(ViewModelReplacementSource), "view.Bind"));

    /// <summary>
    /// A one-way binding onto a control the view holds in a field, as a control named in XAML is, follows the view's
    /// view model when the view has none yet.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task OneWayBind_ToAFieldControl_WhenViewModelInitiallyNull_TracksTheAssignedViewModel() =>
        AssertTracksReplacement(WithNullViewModel(ToFieldControl(ViewModelReplacementSource), "view.OneWayBind"));

    /// <summary>A two-way binding onto a control the view holds in a field follows the view model it is given later.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task Bind_ToAFieldControl_WhenViewModelInitiallyNull_TracksTheAssignedViewModel() =>
        AssertTracksReplacement(WithNullViewModel(ToFieldControl(ToBind(ViewModelReplacementSource)), "view.Bind"));

    /// <summary>
    /// A command binding follows the view's current view model, so clicking the control invokes the command the
    /// view model on display exposes rather than the one belonging to the view model it replaced.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task BindCommand_AfterTheViewModelIsReplaced_InvokesTheNewViewModelsCommand() =>
        AssertTracksReplacement(CommandReplacementSource);

    /// <summary>A command binding created while the view has no view model follows the first assigned model.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task BindCommand_WhenViewModelInitiallyNull_InvokesTheAssignedCommand() =>
        AssertTracksReplacement(CommandReplacementSource.Replace(
            "var binding = view.BindCommand",
            "view.ViewModel = null; var binding = view.BindCommand",
            StringComparison.Ordinal));

    /// <summary>
    /// An interaction binding follows the view's current view model, so the handler answers the interaction the
    /// view model on display raises rather than staying registered against the one it replaced.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task BindInteraction_AfterTheViewModelIsReplaced_HandlesTheNewViewModelsInteraction() =>
        AssertTracksReplacement(InteractionReplacementSource);

    /// <summary>An interaction binding created while the view has no view model follows the first assigned model.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task BindInteraction_WhenViewModelInitiallyNull_HandlesTheAssignedInteraction() =>
        AssertTracksReplacement(InteractionReplacementSource.Replace(
            "var binding = view.BindInteraction",
            "view.ViewModel = null; var binding = view.BindInteraction",
            StringComparison.Ordinal));

    /// <summary>Makes the one-way replacement scenario bind two-way.</summary>
    /// <param name="source">The one-way scenario.</param>
    /// <returns>The same scenario, calling <c>Bind</c>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string ToBind(string source) =>
        source.Replace("view.OneWayBind(", "view.Bind(", StringComparison.Ordinal);

    /// <summary>Clears the view's view model just before the scenario makes its binding.</summary>
    /// <param name="source">The scenario.</param>
    /// <param name="call">The binding call the scenario makes, such as <c>view.OneWayBind</c>.</param>
    /// <returns>The same scenario, binding while the view holds no view model.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string WithNullViewModel(string source, string call) =>
        source.Replace($"var binding = {call}", $"view.ViewModel = null; var binding = {call}", StringComparison.Ordinal);

    /// <summary>Moves the bound view property onto a control the view holds in a field.</summary>
    /// <param name="source">The scenario, binding <c>v =&gt; v.Text</c>.</param>
    /// <returns>The same scenario, binding <c>v =&gt; v.NameBox.Text</c>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string ToFieldControl(string source) =>
        source
            .Replace("public string Text { get; set; } = \"\";", "public TextBox NameBox = new TextBox();", StringComparison.Ordinal)
            .Replace("public static class Usage", "public class TextBox { public string Text { get; set; } = \"\"; }\n\npublic static class Usage", StringComparison.Ordinal)
            .Replace("v => v.Text)", "v => v.NameBox.Text)", StringComparison.Ordinal)
            .Replace("return view.Text;", "return view.NameBox.Text;", StringComparison.Ordinal);

    /// <summary>Runs a replacement scenario and asserts the view ends up showing the replacement's value.</summary>
    /// <param name="source">The scenario source to generate, compile and run.</param>
    /// <returns>A task representing the asynchronous assertion.</returns>
    private static async Task AssertTracksReplacement(string source)
    {
        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10);
        await result.CompilationSucceeds();

        var (assembly, context) = TestHelper.EmitAndLoad(result);
        var run = assembly.GetType("TestApp.Usage")!.GetMethod("Run", BindingFlags.Public | BindingFlags.Static)!;

        var outcome = (string?)run.Invoke(null, null);

        context.Unload();

        await Assert.That(outcome).IsEqualTo(ValueFromReplacement);
    }
}
