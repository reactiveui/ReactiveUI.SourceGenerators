// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Analyzer.Analyzers;
using ReactiveUI.Binding.Analyzer.Tests.Helpers;

namespace ReactiveUI.Binding.Analyzer.Tests;

/// <summary>Tests for <see cref="BindingInvocationAnalyzer"/> — general and cross-cutting cases.</summary>
public partial class BindingInvocationAnalyzerTests
{
    /// <summary>Placeholder test to verify the analyzer test infrastructure works.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EmptySource_NoDiagnostics()
    {
        const string Source = """
                              namespace TestApp
                              {
                                  public class EmptyClass { }
                              }
                              """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }

    /// <summary>Verifies that non-binding methods do not trigger any diagnostics.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NonBindingMethod_NoDiagnostics()
    {
        const string Source = """
                              using System;
                              using System.ComponentModel;

                              namespace TestApp
                              {
                                  public static class SomeExtensions
                                  {
                                      public static void DoSomething<T>(this T obj) { }
                                  }

                                  public class MyClass : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;
                                      public string Name { get; set; } = "";

                                      public void Test()
                                      {
                                          this.DoSomething();
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies that RXUIBIND001 is reported for non-inline lambda but RXUIBIND003 is not
    /// (since the analyzer only walks inline lambda bodies for private member access).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MultipleDiagnostics_NonInlineLambda_NoPrivateMemberCheck()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class MyViewModel : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                                 private string Secret { get; set; } = "";

                                                 public void Test()
                                                 {
                                                     Expression<Func<MyViewModel, string>> expr = x => x.Secret;
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(this, expr);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);

        // RXUIBIND001 for non-inline lambda
        var nonInlineDiags = diagnostics.Where(static d => d.Id == NonInlineLambdaDiagnosticId).ToArray();
        await Assert.That(nonInlineDiags.Length).IsEqualTo(1);

        // RXUIBIND003 should NOT be reported because the lambda is not inline
        var privateDiags = diagnostics.Where(static d => d.Id == "RXUIBIND003").ToArray();
        await Assert.That(privateDiags.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies that a regular (non-extension) method call does not trigger any diagnostics.
    /// This exercises the IsBindingExtensionMethod false branch where the method is not
    /// from __ReactiveUIGeneratedBindings.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RegularMethodCall_NotExtensionMethod_NoDiagnostics()
    {
        const string Source = """
                              using System;

                              namespace TestApp
                              {
                                  public class MyClass
                                  {
                                      public string Name { get; set; } = "";

                                      public void Test()
                                      {
                                          Console.WriteLine(Name);
                                          Name.ToString();
                                          var len = Name.Length;
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies that a block-body lambda (simple lambda with block body) exercises the
    /// body == null branch in CheckNonInlineLambda. A block lambda is still an inline lambda
    /// so RXUIBIND001 should NOT be reported, but the expression body extraction returns null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BlockBodyLambda_IsInlineLambda_NoDiagnosticForRXUIBIND001()
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
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(vm, x => { return x.Name; });
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var nonInlineDiags = diagnostics.Where(static d => d.Id == NonInlineLambdaDiagnosticId).ToArray();
        await Assert.That(nonInlineDiags.Length).IsEqualTo(0);
    }

    /// <summary>Verifies that a block-body parenthesized lambda is still considered inline and does NOT trigger RXUIBIND001.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ParenthesizedBlockBodyLambda_IsInlineLambda_NoDiagnosticForRXUIBIND001()
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
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(vm, (MyViewModel x) => { return x.Name; });
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var nonInlineDiags = diagnostics.Where(static d => d.Id == NonInlineLambdaDiagnosticId).ToArray();
        await Assert.That(nonInlineDiags.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies that no diagnostics are reported when source code contains no method invocations
    /// at all. This confirms the analyzer handles the case where there are zero invocation
    /// operations to analyze.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NoMethodInvocations_NoDiagnostics()
    {
        const string Source = """
                              namespace TestApp
                              {
                                  public class SimpleClass
                                  {
                                      public string Name { get; set; } = "";
                                      public int Age;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies no diagnostics when a non-binding extension method is called that happens
    /// to take a lambda argument. This exercises the IsBindingExtensionMethod false branch
    /// with a custom extension method that takes an expression.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CustomExtensionMethodWithLambda_NoDiagnostics()
    {
        const string Source = """
                              using System;
                              using System.Linq.Expressions;
                              using System.ComponentModel;

                              namespace TestApp
                              {
                                  public static class CustomExtensions
                                  {
                                      public static object CustomMethod<TObj, TReturn>(
                                          this TObj obj,
                                          Expression<Func<TObj, TReturn>> property)
                                          where TObj : class
                                          => throw new NotImplementedException();
                                  }

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
                                          vm.CustomMethod(x => x.Name);
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies that when BindOneWay is called with both inline lambdas, all path checks
    /// run against both lambdas. This exercises the full iteration loop in CheckPrivateMember
    /// and CheckUnsupportedPathSegment for multiple Expression arguments.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_BothInlineLambdas_AllChecksRun_NoDiagnostics()
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
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.BindOneWay(vm, view, x => x.Name, x => x.NameText);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies that a parenthesized lambda with expression body is treated identically
    /// to a simple lambda for all analyzer checks.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ParenthesizedExpressionLambda_PublicProperty_NoDiagnostics()
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
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(vm, (MyViewModel x) => x.Name);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }

    /// <summary>Verifies BindTo with an inline target lambda accessing a public property reports no diagnostics.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND_BindTo_InlineLambda_NoDiagnostic()
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
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.BindTo(source, view, x => x.NameText);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var bindDiags = diagnostics.Where(static d => d.Id.StartsWith("RXUIBIND", StringComparison.Ordinal)).ToArray();
        await Assert.That(bindDiags.Length).IsEqualTo(0);
    }
}
