// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Analyzer.Analyzers;
using ReactiveUI.Binding.Analyzer.Tests.Helpers;

namespace ReactiveUI.Binding.Analyzer.Tests;

/// <summary>Tests for <see cref="BindingInvocationAnalyzer"/> — RXUIBIND008 (interaction type).</summary>
public partial class BindingInvocationAnalyzerTests
{
    /// <summary>The diagnostic id reported for an invalid interaction type.</summary>
    private const string InvalidInteractionTypeDiagnosticId = "RXUIBIND008";

    /// <summary>
    /// Verifies RXUIBIND008 is reported when the interaction property type does not implement IInteraction.
    /// Uses the unconstrained BindInteraction overload so the call compiles despite the wrong property type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND008_InvalidInteractionType_ReportsDiagnostic()
    {
        const string Source = InteractionCommandPreamble + """

                                                           namespace TestApp
                                                           {
                                                               public class MyViewModel : INotifyPropertyChanged
                                                               {
                                                                   public event PropertyChangedEventHandler? PropertyChanged;
                                                                   public string NotAnInteraction { get; set; } = "";
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
                                                                       ReactiveUI.Binding.ReactiveUIBindingExtensions.BindInteraction(
                                                                           view, vm, x => x.NotAnInteraction, ctx => Task.CompletedTask);
                                                                   }
                                                               }
                                                           }
                                                           """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var interactionDiags = diagnostics.Where(static d => d.Id == InvalidInteractionTypeDiagnosticId).ToArray();
        await Assert.That(interactionDiags.Length).IsEqualTo(1);
    }

    /// <summary>Verifies RXUIBIND008 is not reported when the property is a valid IInteraction type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND008_ValidInteractionType_NoDiagnostic()
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
                                                                       ReactiveUI.Binding.ReactiveUIBindingExtensions.BindInteraction(
                                                                           view, vm, x => x.Confirm, ctx => Task.CompletedTask);
                                                                   }
                                                               }
                                                           }
                                                           """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var interactionDiags = diagnostics.Where(static d => d.Id == InvalidInteractionTypeDiagnosticId).ToArray();
        await Assert.That(interactionDiags.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies RXUIBIND008 is not reported when the property type IS IInteraction directly (not a class implementing it).
    /// Covers the direct generic type check branch in ImplementsOpenGenericInterface.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND008_DirectInteractionInterface_NoDiagnostic()
    {
        const string Source = InteractionCommandPreamble + """

                                                           namespace TestApp
                                                           {
                                                               public class MyViewModel : INotifyPropertyChanged
                                                               {
                                                                   public event PropertyChangedEventHandler? PropertyChanged;
                                                                   public ReactiveUI.Binding.IInteraction<string, bool> Confirm { get; set; } = default!;
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
                                                                       ReactiveUI.Binding.ReactiveUIBindingExtensions.BindInteraction(
                                                                           view, vm, x => x.Confirm, ctx => Task.CompletedTask);
                                                                   }
                                                               }
                                                           }
                                                           """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var interactionDiags = diagnostics.Where(static d => d.Id == InvalidInteractionTypeDiagnosticId).ToArray();
        await Assert.That(interactionDiags.Length).IsEqualTo(0);
    }

    /// <summary>Verifies RXUIBIND008 check uses parenthesized lambda for BindInteraction property path.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND008_ParenthesizedLambda_ValidInteraction_NoDiagnostic()
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
                                                                       ReactiveUI.Binding.ReactiveUIBindingExtensions.BindInteraction(
                                                                           view, vm, (x) => x.Confirm, ctx => Task.CompletedTask);
                                                                   }
                                                               }
                                                           }
                                                           """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var interactionDiags = diagnostics.Where(static d => d.Id == InvalidInteractionTypeDiagnosticId).ToArray();
        await Assert.That(interactionDiags.Length).IsEqualTo(0);
    }
}
