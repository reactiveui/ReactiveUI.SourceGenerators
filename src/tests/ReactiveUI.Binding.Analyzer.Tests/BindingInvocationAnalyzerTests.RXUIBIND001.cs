// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Analyzer.Analyzers;
using ReactiveUI.Binding.Analyzer.Tests.Helpers;

namespace ReactiveUI.Binding.Analyzer.Tests;

/// <summary>Tests for <see cref="BindingInvocationAnalyzer"/> — RXUIBIND001 (expression must be an inline lambda).</summary>
public partial class BindingInvocationAnalyzerTests
{
    /// <summary>The diagnostic id reported for a non-inline lambda.</summary>
    private const string NonInlineLambdaDiagnosticId = "RXUIBIND001";

    /// <summary>Diagnostic count for a call whose source and target selectors are both non-inline.</summary>
    private const int BothSelectorsNonInlineCount = 2;

    /// <summary>Verifies RXUIBIND001 is reported when a non-inline lambda (variable reference) is passed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND001_NonInlineLambda_Variable_ReportsDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class MyViewModel : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                                 public string Name { get; set; } = "";
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     var vm = new MyViewModel();
                                                     Expression<Func<MyViewModel, string>> expr = x => x.Name;
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(vm, expr);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        await Assert.That(diagnostics.Length).IsEqualTo(1);
        await Assert.That(diagnostics[0].Id).IsEqualTo(NonInlineLambdaDiagnosticId);
    }

    /// <summary>Verifies RXUIBIND001 is NOT reported when an inline lambda is passed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND001_InlineLambda_NoDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class MyViewModel : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                                 public string Name { get; set; } = "";
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     var vm = new MyViewModel();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(vm, x => x.Name);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }

    /// <summary>Verifies RXUIBIND001 is reported for WhenChanging with non-inline lambda.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND001_WhenChanging_NonInlineLambda_ReportsDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class MyViewModel : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                                 public string Name { get; set; } = "";
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     var vm = new MyViewModel();
                                                     Expression<Func<MyViewModel, string>> expr = x => x.Name;
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanging(vm, expr);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);

        // Filter to only RXUIBIND001 (WhenChanging on INPC-only also fires RXUIBIND004)
        var nonInlineDiags = diagnostics.Where(static d => d.Id == NonInlineLambdaDiagnosticId).ToArray();
        await Assert.That(nonInlineDiags.Length).IsEqualTo(1);
    }

    /// <summary>Verifies RXUIBIND001 is reported for BindOneWay with non-inline source lambda.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND001_BindOneWay_NonInlineLambda_ReportsDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class MyViewModel : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                                 public string Name { get; set; } = "";
                                             }

                                             public class MyView : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                                 public string NameText { get; set; } = "";
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     var vm = new MyViewModel();
                                                     var view = new MyView();
                                                     Expression<Func<MyViewModel, string>> expr = x => x.Name;
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.BindOneWay(vm, view, expr, x => x.NameText);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var nonInlineDiags = diagnostics.Where(static d => d.Id == NonInlineLambdaDiagnosticId).ToArray();
        await Assert.That(nonInlineDiags.Length).IsEqualTo(1);
    }

    /// <summary>Verifies RXUIBIND001 is reported for BindTwoWay with non-inline target lambda.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND001_BindTwoWay_NonInlineLambda_ReportsDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class MyViewModel : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                                 public string Name { get; set; } = "";
                                             }

                                             public class MyView : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                                 public string NameText { get; set; } = "";
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     var vm = new MyViewModel();
                                                     var view = new MyView();
                                                     Expression<Func<MyView, string>> targetExpr = x => x.NameText;
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.BindTwoWay(vm, view, x => x.Name, targetExpr);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var nonInlineDiags = diagnostics.Where(static d => d.Id == NonInlineLambdaDiagnosticId).ToArray();
        await Assert.That(nonInlineDiags.Length).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies RXUIBIND001 and RXUIBIND006 behavior with non-inline lambdas in BindTwoWay.
    /// The RXUIBIND001 is reported for non-inline expressions, but RXUIBIND006 is NOT checked
    /// because the non-lambda expression is not a LambdaExpressionSyntax.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND001_BindTwoWay_BothNonInline_ReportsMultipleDiagnostics()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class MyViewModel : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                                 public string Name { get; set; } = "";
                                             }

                                             public class MyView : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                                 public string NameText { get; set; } = "";
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     var vm = new MyViewModel();
                                                     var view = new MyView();
                                                     Expression<Func<MyViewModel, string>> srcExpr = x => x.Name;
                                                     Expression<Func<MyView, string>> tgtExpr = x => x.NameText;
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.BindTwoWay(vm, view, srcExpr, tgtExpr);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var nonInlineDiags = diagnostics.Where(static d => d.Id == NonInlineLambdaDiagnosticId).ToArray();

        // Both source and target Expression<Func<>> args are non-inline
        await Assert.That(nonInlineDiags.Length).IsEqualTo(BothSelectorsNonInlineCount);
    }

    /// <summary>Verifies RXUIBIND001 is reported for non-inline lambda in BindInteraction.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND001_BindInteraction_NonInlineLambda_ReportsDiagnostic()
    {
        const string Source = InteractionCommandPreamble + """

                                                           namespace TestApp
                                                           {
                                                               public class MyInteraction : ReactiveUI.Binding.IInteraction<string, bool> { }

                                                               public class MyViewModel : INotifyPropertyChanged
                                                               {
                                                                   public event PropertyChangedEventHandler? PropertyChanged;
                                                                   public MyInteraction Confirm { get; set; } = new();
                                                               }

                                                               public class MyView : ReactiveUI.Binding.IViewFor, INotifyPropertyChanged
                                                               {
                                                                   public event PropertyChangedEventHandler? PropertyChanged;
                                                                   public object? ViewModel { get; set; }
                                                               }

                                                               public class Usage
                                                               {
                                                                   public void Test()
                                                                   {
                                                                       var vm = new MyViewModel();
                                                                       var view = new MyView();
                                                                       Expression<Func<MyViewModel, ReactiveUI.Binding.IInteraction<string, bool>>> expr = x => x.Confirm;
                                                                       ReactiveUI.Binding.ReactiveUIBindingExtensions.BindInteraction(
                                                                           view, vm, expr, ctx => Task.CompletedTask);
                                                                   }
                                                               }
                                                           }
                                                           """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var nonInlineDiags = diagnostics.Where(static d => d.Id == NonInlineLambdaDiagnosticId).ToArray();
        await Assert.That(nonInlineDiags.Length).IsEqualTo(1);
    }

    /// <summary>Verifies RXUIBIND001 is reported for BindTo with a non-inline target lambda.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND001_BindTo_NonInlineLambda_ReportsDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class MyView : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                                 public string NameText { get; set; } = "";
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     IObservable<string> source = null!;
                                                     var view = new MyView();
                                                     Expression<Func<MyView, string>> expr = x => x.NameText;
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.BindTo(source, view, expr);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var nonInlineDiags = diagnostics.Where(static d => d.Id == NonInlineLambdaDiagnosticId).ToArray();
        await Assert.That(nonInlineDiags.Length).IsEqualTo(1);
    }
}
