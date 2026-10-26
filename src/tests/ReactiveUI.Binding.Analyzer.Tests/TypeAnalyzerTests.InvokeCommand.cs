// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Analyzer.Analyzers;
using ReactiveUI.Binding.Analyzer.Tests.Helpers;

namespace ReactiveUI.Binding.Analyzer.Tests;

/// <summary>
/// Tests for <see cref="TypeAnalyzer"/> over <c>InvokeCommand</c>, whose observed object is not the type
/// argument every other API names it with: the first one is the value type of a stream the caller already built.
/// </summary>
public partial class TypeAnalyzerTests
{
    /// <summary>The stub class as it declares <c>InvokeCommand</c>.</summary>
    private const string InvokeCommandPreamble = """
                                                 using System;
                                                 using System.ComponentModel;
                                                 using System.Linq.Expressions;
                                                 using System.Windows.Input;

                                                 namespace ReactiveUI.Binding
                                                 {
                                                     public static class __ReactiveUIGeneratedBindings
                                                     {
                                                         public static IDisposable InvokeCommand<T, TTarget>(
                                                             this IObservable<T> source,
                                                             TTarget target,
                                                             Expression<Func<TTarget, ICommand>> commandProperty)
                                                             where TTarget : class
                                                             => throw new NotImplementedException();
                                                     }
                                                 }
                                                 """;

    /// <summary>The type holding the command is the one the diagnostic is about.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND002_InvokeCommandOnATypeThatDoesNotNotify_ReportsTheTargetType()
    {
        const string Source = InvokeCommandPreamble + """

                                                      namespace TestApp
                                                      {
                                                          public class PlainObject
                                                          {
                                                              public ICommand Save { get; set; } = null!;
                                                          }

                                                          public class Usage
                                                          {
                                                              public void Test(IObservable<string> values)
                                                              {
                                                                  var obj = new PlainObject();
                                                                  ReactiveUI.Binding.__ReactiveUIGeneratedBindings.InvokeCommand(values, obj, x => x.Save);
                                                              }
                                                          }
                                                      }
                                                      """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<TypeAnalyzer>(Source);

        await Assert.That(diagnostics.Length).IsEqualTo(1);
        await Assert.That(diagnostics[0].Id).IsEqualTo(NoObservablePropertiesDiagnosticId);
        await Assert.That(diagnostics[0].GetMessage()).Contains("PlainObject");
    }

    /// <summary>A notifying target reports nothing, and the stream's value type is not mistaken for it.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND002_InvokeCommandOnANotifyingType_NoDiagnostic()
    {
        const string Source = InvokeCommandPreamble + """

                                                      namespace TestApp
                                                      {
                                                          public class MyViewModel : INotifyPropertyChanged
                                                          {
                                                              public event PropertyChangedEventHandler? PropertyChanged;

                                                              public ICommand Save { get; set; } = null!;
                                                          }

                                                          public class Usage
                                                          {
                                                              public void Test(IObservable<string> values)
                                                              {
                                                                  var viewModel = new MyViewModel();
                                                                  ReactiveUI.Binding.__ReactiveUIGeneratedBindings.InvokeCommand(values, viewModel, x => x.Save);
                                                              }
                                                          }
                                                      }
                                                      """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<TypeAnalyzer>(Source);

        await Assert.That(diagnostics.Any(static d => d.Id == NoObservablePropertiesDiagnosticId)).IsFalse();
    }
}
