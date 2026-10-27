// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Analyzer.Analyzers;
using ReactiveUI.Binding.Analyzer.Tests.Helpers;

namespace ReactiveUI.Binding.Analyzer.Tests;

/// <summary>Tests for <see cref="BindingInvocationAnalyzer"/> — RXUIBIND007 (BindCommand bindable event).</summary>
public partial class BindingInvocationAnalyzerTests
{
    /// <summary>The diagnostic id reported when a control has no default bindable event.</summary>
    private const string NoBindableEventDiagnosticId = "RXUIBIND007";

    /// <summary>Verifies RXUIBIND007 is reported when the control has no default bindable event.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND007_NoBindableEvent_ReportsDiagnostic()
    {
        const string Source = InteractionCommandPreamble + """

                                                           namespace TestApp
                                                           {
                                                               public class MyViewModel : INotifyPropertyChanged
                                                               {
                                                                   public event PropertyChangedEventHandler? PropertyChanged;
                                                                   public ICommand? Save { get; set; }
                                                               }

                                                               public class NoEventControl { }

                                                               public class MyView : ReactiveUI.Binding.IViewFor, INotifyPropertyChanged
                                                               {
                                                                   public event PropertyChangedEventHandler? PropertyChanged;
                                                                   public object? ViewModel { get; set; }
                                                                   public NoEventControl Button { get; } = new();
                                                               }

                                                               public class Usage
                                                               {
                                                                   public void Test()
                                                                   {
                                                                       var vm = new MyViewModel();
                                                                       var view = new MyView();
                                                                       ReactiveUI.Binding.ReactiveUIBindingExtensions.BindCommand(
                                                                           view, vm, x => x.Save, x => x.Button);
                                                                   }
                                                               }
                                                           }
                                                           """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var eventDiags = diagnostics.Where(static d => d.Id == NoBindableEventDiagnosticId).ToArray();
        await Assert.That(eventDiags.Length).IsEqualTo(1);
    }

    /// <summary>Verifies RXUIBIND007 is not reported when the control has a Click event.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND007_ControlWithClickEvent_NoDiagnostic()
    {
        const string Source = InteractionCommandPreamble + """

                                                           namespace TestApp
                                                           {
                                                               public class MyViewModel : INotifyPropertyChanged
                                                               {
                                                                   public event PropertyChangedEventHandler? PropertyChanged;
                                                                   public ICommand? Save { get; set; }
                                                               }

                                                               public class MyButton
                                                               {
                                                                   public event EventHandler? Click;
                                                               }

                                                               public class MyView : ReactiveUI.Binding.IViewFor, INotifyPropertyChanged
                                                               {
                                                                   public event PropertyChangedEventHandler? PropertyChanged;
                                                                   public object? ViewModel { get; set; }
                                                                   public MyButton Button { get; } = new();
                                                               }

                                                               public class Usage
                                                               {
                                                                   public void Test()
                                                                   {
                                                                       var vm = new MyViewModel();
                                                                       var view = new MyView();
                                                                       ReactiveUI.Binding.ReactiveUIBindingExtensions.BindCommand(
                                                                           view, vm, x => x.Save, x => x.Button);
                                                                   }
                                                               }
                                                           }
                                                           """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var eventDiags = diagnostics.Where(static d => d.Id == NoBindableEventDiagnosticId).ToArray();
        await Assert.That(eventDiags.Length).IsEqualTo(0);
    }

    /// <summary>Verifies RXUIBIND007 is not reported when toEvent is explicitly specified.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND007_WithExplicitToEvent_NoDiagnostic()
    {
        const string Source = InteractionCommandPreamble + """

                                                           namespace TestApp
                                                           {
                                                               public class MyViewModel : INotifyPropertyChanged
                                                               {
                                                                   public event PropertyChangedEventHandler? PropertyChanged;
                                                                   public ICommand? Save { get; set; }
                                                               }

                                                               public class NoEventControl { }

                                                               public class MyView : ReactiveUI.Binding.IViewFor, INotifyPropertyChanged
                                                               {
                                                                   public event PropertyChangedEventHandler? PropertyChanged;
                                                                   public object? ViewModel { get; set; }
                                                                   public NoEventControl Button { get; } = new();
                                                               }

                                                               public class Usage
                                                               {
                                                                   public void Test()
                                                                   {
                                                                       var vm = new MyViewModel();
                                                                       var view = new MyView();
                                                                       ReactiveUI.Binding.ReactiveUIBindingExtensions.BindCommand(
                                                                           view, vm, x => x.Save, x => x.Button, "CustomEvent");
                                                                   }
                                                               }
                                                           }
                                                           """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var eventDiags = diagnostics.Where(static d => d.Id == NoBindableEventDiagnosticId).ToArray();
        await Assert.That(eventDiags.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies RXUIBIND007 is reported when the control has a property named Click but no Click event.
    /// Covers the branch where GetMembers("Click") returns a non-event member.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND007_ControlWithClickPropertyNotEvent_ReportsDiagnostic()
    {
        const string Source = InteractionCommandPreamble + """

                                                           namespace TestApp
                                                           {
                                                               public class MyViewModel : INotifyPropertyChanged
                                                               {
                                                                   public event PropertyChangedEventHandler? PropertyChanged;
                                                                   public ICommand? Save { get; set; }
                                                               }

                                                               public class ClickPropertyControl
                                                               {
                                                                   public string Click { get; set; } = "";
                                                               }

                                                               public class MyView : ReactiveUI.Binding.IViewFor, INotifyPropertyChanged
                                                               {
                                                                   public event PropertyChangedEventHandler? PropertyChanged;
                                                                   public object? ViewModel { get; set; }
                                                                   public ClickPropertyControl Button { get; } = new();
                                                               }

                                                               public class Usage
                                                               {
                                                                   public void Test()
                                                                   {
                                                                       var vm = new MyViewModel();
                                                                       var view = new MyView();
                                                                       ReactiveUI.Binding.ReactiveUIBindingExtensions.BindCommand(
                                                                           view, vm, x => x.Save, x => x.Button);
                                                                   }
                                                               }
                                                           }
                                                           """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var eventDiags = diagnostics.Where(static d => d.Id == NoBindableEventDiagnosticId).ToArray();
        await Assert.That(eventDiags.Length).IsEqualTo(1);
    }

    /// <summary>Verifies RXUIBIND007 is not reported when the control has a TouchUpInside event.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND007_ControlWithTouchUpInsideEvent_NoDiagnostic()
    {
        const string Source = InteractionCommandPreamble + """

                                                           namespace TestApp
                                                           {
                                                               public class MyViewModel : INotifyPropertyChanged
                                                               {
                                                                   public event PropertyChangedEventHandler? PropertyChanged;
                                                                   public ICommand? Save { get; set; }
                                                               }

                                                               public class TouchButton
                                                               {
                                                                   public event EventHandler? TouchUpInside;
                                                               }

                                                               public class MyView : ReactiveUI.Binding.IViewFor, INotifyPropertyChanged
                                                               {
                                                                   public event PropertyChangedEventHandler? PropertyChanged;
                                                                   public object? ViewModel { get; set; }
                                                                   public TouchButton Button { get; } = new();
                                                               }

                                                               public class Usage
                                                               {
                                                                   public void Test()
                                                                   {
                                                                       var vm = new MyViewModel();
                                                                       var view = new MyView();
                                                                       ReactiveUI.Binding.ReactiveUIBindingExtensions.BindCommand(
                                                                           view, vm, x => x.Save, x => x.Button);
                                                                   }
                                                               }
                                                           }
                                                           """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var eventDiags = diagnostics.Where(static d => d.Id == NoBindableEventDiagnosticId).ToArray();
        await Assert.That(eventDiags.Length).IsEqualTo(0);
    }

    /// <summary>Verifies RXUIBIND007 is not reported when the control has a MouseUp event.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND007_ControlWithMouseUpEvent_NoDiagnostic()
    {
        const string Source = InteractionCommandPreamble + """

                                                           namespace TestApp
                                                           {
                                                               public class MyViewModel : INotifyPropertyChanged
                                                               {
                                                                   public event PropertyChangedEventHandler? PropertyChanged;
                                                                   public ICommand? Save { get; set; }
                                                               }

                                                               public class MouseButton
                                                               {
                                                                   public event EventHandler? MouseUp;
                                                               }

                                                               public class MyView : ReactiveUI.Binding.IViewFor, INotifyPropertyChanged
                                                               {
                                                                   public event PropertyChangedEventHandler? PropertyChanged;
                                                                   public object? ViewModel { get; set; }
                                                                   public MouseButton Button { get; } = new();
                                                               }

                                                               public class Usage
                                                               {
                                                                   public void Test()
                                                                   {
                                                                       var vm = new MyViewModel();
                                                                       var view = new MyView();
                                                                       ReactiveUI.Binding.ReactiveUIBindingExtensions.BindCommand(
                                                                           view, vm, x => x.Save, x => x.Button);
                                                                   }
                                                               }
                                                           }
                                                           """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var eventDiags = diagnostics.Where(static d => d.Id == NoBindableEventDiagnosticId).ToArray();
        await Assert.That(eventDiags.Length).IsEqualTo(0);
    }

    /// <summary>Verifies RXUIBIND007 is not reported when the control has a Pressed event.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND007_ControlWithPressedEvent_NoDiagnostic()
    {
        const string Source = InteractionCommandPreamble + """

                                                           namespace TestApp
                                                           {
                                                               public class MyViewModel : INotifyPropertyChanged
                                                               {
                                                                   public event PropertyChangedEventHandler? PropertyChanged;
                                                                   public ICommand? Save { get; set; }
                                                               }

                                                               public class PressButton
                                                               {
                                                                   public event EventHandler? Pressed;
                                                               }

                                                               public class MyView : ReactiveUI.Binding.IViewFor, INotifyPropertyChanged
                                                               {
                                                                   public event PropertyChangedEventHandler? PropertyChanged;
                                                                   public object? ViewModel { get; set; }
                                                                   public PressButton Button { get; } = new();
                                                               }

                                                               public class Usage
                                                               {
                                                                   public void Test()
                                                                   {
                                                                       var vm = new MyViewModel();
                                                                       var view = new MyView();
                                                                       ReactiveUI.Binding.ReactiveUIBindingExtensions.BindCommand(
                                                                           view, vm, x => x.Save, x => x.Button);
                                                                   }
                                                               }
                                                           }
                                                           """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var eventDiags = diagnostics.Where(static d => d.Id == NoBindableEventDiagnosticId).ToArray();
        await Assert.That(eventDiags.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies RXUIBIND007 still reports when toEvent is passed as an empty string
    /// (the check requires a non-empty string to suppress the diagnostic).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND007_WithEmptyToEvent_ReportsDiagnostic()
    {
        const string Source = InteractionCommandPreamble + """

                                                           namespace TestApp
                                                           {
                                                               public class MyViewModel : INotifyPropertyChanged
                                                               {
                                                                   public event PropertyChangedEventHandler? PropertyChanged;
                                                                   public ICommand? Save { get; set; }
                                                               }

                                                               public class NoEventControl { }

                                                               public class MyView : ReactiveUI.Binding.IViewFor, INotifyPropertyChanged
                                                               {
                                                                   public event PropertyChangedEventHandler? PropertyChanged;
                                                                   public object? ViewModel { get; set; }
                                                                   public NoEventControl Button { get; } = new();
                                                               }

                                                               public class Usage
                                                               {
                                                                   public void Test()
                                                                   {
                                                                       var vm = new MyViewModel();
                                                                       var view = new MyView();
                                                                       ReactiveUI.Binding.ReactiveUIBindingExtensions.BindCommand(
                                                                           view, vm, x => x.Save, x => x.Button, toEvent: "");
                                                                   }
                                                               }
                                                           }
                                                           """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var eventDiags = diagnostics.Where(static d => d.Id == NoBindableEventDiagnosticId).ToArray();
        await Assert.That(eventDiags.Length).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies RXUIBIND007 uses parenthesized lambda for BindCommand control path.
    /// Covers the ParenthesizedLambdaExpressionSyntax branch in GetLambdaBody.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND007_ParenthesizedLambda_ControlWithClickEvent_NoDiagnostic()
    {
        const string Source = InteractionCommandPreamble + """

                                                           namespace TestApp
                                                           {
                                                               public class MyViewModel : INotifyPropertyChanged
                                                               {
                                                                   public event PropertyChangedEventHandler? PropertyChanged;
                                                                   public ICommand? Save { get; set; }
                                                               }

                                                               public class MyButton
                                                               {
                                                                   public event EventHandler? Click;
                                                               }

                                                               public class MyView : ReactiveUI.Binding.IViewFor, INotifyPropertyChanged
                                                               {
                                                                   public event PropertyChangedEventHandler? PropertyChanged;
                                                                   public object? ViewModel { get; set; }
                                                                   public MyButton Button { get; } = new();
                                                               }

                                                               public class Usage
                                                               {
                                                                   public void Test()
                                                                   {
                                                                       var vm = new MyViewModel();
                                                                       var view = new MyView();
                                                                       ReactiveUI.Binding.ReactiveUIBindingExtensions.BindCommand(
                                                                           view, vm, (x) => x.Save, (x) => x.Button);
                                                                   }
                                                               }
                                                           }
                                                           """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var eventDiags = diagnostics.Where(static d => d.Id == NoBindableEventDiagnosticId).ToArray();
        await Assert.That(eventDiags.Length).IsEqualTo(0);
    }
}
