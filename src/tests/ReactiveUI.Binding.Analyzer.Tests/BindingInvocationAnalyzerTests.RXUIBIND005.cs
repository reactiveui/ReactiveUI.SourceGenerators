// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Analyzer.Analyzers;
using ReactiveUI.Binding.Analyzer.Tests.Helpers;

namespace ReactiveUI.Binding.Analyzer.Tests;

/// <summary>Tests for <see cref="BindingInvocationAnalyzer"/> — RXUIBIND005 (INotifyDataErrorInfo validation).</summary>
public partial class BindingInvocationAnalyzerTests
{
    /// <summary>The diagnostic id reported when validation binding requires the runtime engine.</summary>
    private const string ValidationNotGeneratedDiagnosticId = "RXUIBIND005";

    /// <summary>Verifies RXUIBIND005 is reported when BindOneWay source type implements INotifyDataErrorInfo.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND005_BindOneWay_WithDataErrorInfo_ReportsDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class MyViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                                 public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
                                                 public bool HasErrors => false;
                                                 public IEnumerable GetErrors(string? propertyName) => Array.Empty<string>();
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
        var validationDiags = diagnostics.Where(static d => d.Id == ValidationNotGeneratedDiagnosticId).ToArray();
        await Assert.That(validationDiags.Length).IsEqualTo(1);
    }

    /// <summary>Verifies RXUIBIND005 is reported for BindTwoWay when source implements INotifyDataErrorInfo.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND005_BindTwoWay_WithDataErrorInfo_ReportsDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class MyViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                                 public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
                                                 public bool HasErrors => false;
                                                 public IEnumerable GetErrors(string? propertyName) => Array.Empty<string>();
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
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.BindTwoWay(vm, view, x => x.Name, x => x.NameText);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var validationDiags = diagnostics.Where(static d => d.Id == ValidationNotGeneratedDiagnosticId).ToArray();
        await Assert.That(validationDiags.Length).IsEqualTo(1);
    }

    /// <summary>Verifies RXUIBIND005 is NOT reported when source type does not implement INotifyDataErrorInfo.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND005_NoDataErrorInfo_NoDiagnostic()
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
        var validationDiags = diagnostics.Where(static d => d.Id == ValidationNotGeneratedDiagnosticId).ToArray();
        await Assert.That(validationDiags.Length).IsEqualTo(0);
    }

    /// <summary>Verifies RXUIBIND005 is NOT reported for WhenChanged (only applies to binding methods).</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND005_WhenChanged_NoDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class MyViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                                 public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
                                                 public bool HasErrors => false;
                                                 public IEnumerable GetErrors(string? propertyName) => Array.Empty<string>();
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
        var validationDiags = diagnostics.Where(static d => d.Id == ValidationNotGeneratedDiagnosticId).ToArray();
        await Assert.That(validationDiags.Length).IsEqualTo(0);
    }

    /// <summary>Verifies that RXUIBIND005 is NOT reported for WhenChanging (only applies to BindOneWay/BindTwoWay).</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND005_WhenChanging_NoDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class MyViewModel : INotifyPropertyChanged, INotifyPropertyChanging, INotifyDataErrorInfo
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                                 public event PropertyChangingEventHandler? PropertyChanging;
                                                 public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
                                                 public bool HasErrors => false;
                                                 public IEnumerable GetErrors(string? propertyName) => Array.Empty<string>();
                                                 public string Name { get; set; } = "";
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     var vm = new MyViewModel();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanging(vm, x => x.Name);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var validationDiags = diagnostics.Where(static d => d.Id == ValidationNotGeneratedDiagnosticId).ToArray();
        await Assert.That(validationDiags.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies that RXUIBIND005 is reported for BindTwoWay when the source type implements
    /// INotifyDataErrorInfo through a base class. Tests the AllInterfaces walk for inherited
    /// interface detection.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND005_BindTwoWay_InheritedDataErrorInfo_ReportsDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class BaseViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                                 public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
                                                 public bool HasErrors => false;
                                                 public IEnumerable GetErrors(string? propertyName) => Array.Empty<string>();
                                             }

                                             public class DerivedViewModel : BaseViewModel
                                             {
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
                                                     var vm = new DerivedViewModel();
                                                     var view = new MyView();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.BindTwoWay(vm, view, x => x.Name, x => x.NameText);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var validationDiags = diagnostics.Where(static d => d.Id == ValidationNotGeneratedDiagnosticId).ToArray();
        await Assert.That(validationDiags.Length).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies RXUIBIND005 is NOT reported for BindTwoWay when the source type does NOT
    /// implement INotifyDataErrorInfo. This exercises the AllInterfaces loop fallthrough
    /// path in CheckValidationSupport where no interface matches.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND005_BindTwoWay_NoDataErrorInfo_NoDiagnostic()
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
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.BindTwoWay(vm, view, x => x.Name, x => x.NameText);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var validationDiags = diagnostics.Where(static d => d.Id == ValidationNotGeneratedDiagnosticId).ToArray();
        await Assert.That(validationDiags.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies RXUIBIND005 diagnostic message includes the source type name when
    /// BindOneWay is called on a type implementing INotifyDataErrorInfo.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND005_BindOneWay_DiagnosticMessageContainsTypeName()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class ValidatingViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                                 public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
                                                 public bool HasErrors => false;
                                                 public IEnumerable GetErrors(string? propertyName) => Array.Empty<string>();
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
                                                     var vm = new ValidatingViewModel();
                                                     var view = new MyView();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.BindOneWay(vm, view, x => x.Name, x => x.NameText);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var validationDiags = diagnostics.Where(static d => d.Id == ValidationNotGeneratedDiagnosticId).ToArray();
        await Assert.That(validationDiags.Length).IsEqualTo(1);
        var message = validationDiags[0].GetMessage();
        await Assert.That(message).Contains("ValidatingViewModel");
    }

    /// <summary>
    /// Verifies that RXUIBIND005 is not reported for a non-generic BindOneWay method
    /// on the recognized extension class (e.g., a generated dispatch overload with concrete types).
    /// Exercises the <c>ExtractFirstTypeArgument == null</c> guard in <c>CheckValidationSupport</c>.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND005_NonGenericBindOneWay_NoDiagnostic()
    {
        const string Source = """
                              using System;
                              using System.ComponentModel;
                              using System.Linq.Expressions;

                              namespace ReactiveUI.Binding
                              {
                                  public static class __ReactiveUIGeneratedBindings
                                  {
                                      // Non-generic generated dispatch overload (concrete types)
                                      public static IDisposable BindOneWay(
                                          TestApp.MyViewModel source,
                                          TestApp.MyView target,
                                          string callerFilePath = "",
                                          int callerLineNumber = 0)
                                          => throw new NotImplementedException();
                                  }
                              }

                              namespace TestApp
                              {
                                  public class MyViewModel : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;
                                      public string Name { get; set; } = "";
                                  }

                                  public class MyView
                                  {
                                      public string DisplayName { get; set; } = "";
                                  }

                                  public class Usage
                                  {
                                      public void Test()
                                      {
                                          var vm = new MyViewModel();
                                          var view = new MyView();
                                          ReactiveUI.Binding.__ReactiveUIGeneratedBindings.BindOneWay(vm, view);
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var validationDiags = diagnostics.Where(static d => d.Id == ValidationNotGeneratedDiagnosticId).ToArray();
        await Assert.That(validationDiags.Length).IsEqualTo(0);
    }
}
