// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Analyzer.Analyzers;
using ReactiveUI.Binding.Analyzer.Tests.Helpers;

namespace ReactiveUI.Binding.Analyzer.Tests;

/// <summary>Tests for <see cref="BindingInvocationAnalyzer"/> — RXUIBIND004 (before-change notification support).</summary>
public partial class BindingInvocationAnalyzerTests
{
    /// <summary>The diagnostic id reported when a type has no before-change notification support.</summary>
    private const string NoBeforeChangeSupportDiagnosticId = "RXUIBIND004";

    /// <summary>
    /// Verifies RXUIBIND004 is reported when WhenChanging is called on a type
    /// that only implements INotifyPropertyChanged (no INotifyPropertyChanging).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND004_INPCOnly_ReportsDiagnostic()
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
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanging(vm, x => x.Name);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var beforeChangeDiags = diagnostics.Where(static d => d.Id == NoBeforeChangeSupportDiagnosticId).ToArray();
        await Assert.That(beforeChangeDiags.Length).IsEqualTo(1);
    }

    /// <summary>Verifies RXUIBIND004 is NOT reported when WhenChanging is called on a type that implements INotifyPropertyChanging.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND004_INPChanging_NoDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class MyViewModel : INotifyPropertyChanged, INotifyPropertyChanging
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                                 public event PropertyChangingEventHandler? PropertyChanging;
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
        var beforeChangeDiags = diagnostics.Where(static d => d.Id == NoBeforeChangeSupportDiagnosticId).ToArray();
        await Assert.That(beforeChangeDiags.Length).IsEqualTo(0);
    }

    /// <summary>Verifies RXUIBIND004 is NOT reported for WhenChanged (only applies to WhenChanging).</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND004_WhenChanged_NoDiagnostic()
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
        var beforeChangeDiags = diagnostics.Where(static d => d.Id == NoBeforeChangeSupportDiagnosticId).ToArray();
        await Assert.That(beforeChangeDiags.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies RXUIBIND004 is NOT reported when WhenChanging is called on a type
    /// that implements IReactiveObject (supports before-change notifications).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND004_IReactiveObject_NoDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class ReactiveViewModel : ReactiveUI.IReactiveObject
                                             {
                                                 public string Name { get; set; } = "";
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     var vm = new ReactiveViewModel();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanging(vm, x => x.Name);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var beforeChangeDiags = diagnostics.Where(static d => d.Id == NoBeforeChangeSupportDiagnosticId).ToArray();
        await Assert.That(beforeChangeDiags.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies RXUIBIND004 is reported when WhenChanging is called on a WPF DependencyObject type
    /// (WPF does not support before-change notifications).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND004_WpfDependencyObject_ReportsDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace System.Windows
                                         {
                                             public class DependencyObject { }
                                         }

                                         namespace TestApp
                                         {
                                             public class WpfControl : System.Windows.DependencyObject
                                             {
                                                 public string Name { get; set; } = "";
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     var vm = new WpfControl();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanging(vm, x => x.Name);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var beforeChangeDiags = diagnostics.Where(static d => d.Id == NoBeforeChangeSupportDiagnosticId).ToArray();
        await Assert.That(beforeChangeDiags.Length).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies RXUIBIND004 is reported when WhenChanging is called on a WinUI DependencyObject type
    /// (WinUI does not support before-change notifications).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND004_WinUIDependencyObject_ReportsDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace Microsoft.UI.Xaml
                                         {
                                             public class DependencyObject { }
                                         }

                                         namespace TestApp
                                         {
                                             public class WinUIControl : Microsoft.UI.Xaml.DependencyObject
                                             {
                                                 public string Name { get; set; } = "";
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     var vm = new WinUIControl();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanging(vm, x => x.Name);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var beforeChangeDiags = diagnostics.Where(static d => d.Id == NoBeforeChangeSupportDiagnosticId).ToArray();
        await Assert.That(beforeChangeDiags.Length).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies RXUIBIND004 is reported when WhenChanging is called on an Uno control, which implements
    /// <c>DependencyObject</c> as an interface on every head but Windows.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND004_UnoInterfaceDependencyObject_ReportsDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace Microsoft.UI.Xaml
                                         {
                                             public interface DependencyObject { }
                                         }

                                         namespace TestApp
                                         {
                                             public class UnoControl : Microsoft.UI.Xaml.DependencyObject
                                             {
                                                 public string Name { get; set; } = "";
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     var vm = new UnoControl();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanging(vm, x => x.Name);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var beforeChangeDiags = diagnostics.Where(static d => d.Id == NoBeforeChangeSupportDiagnosticId).ToArray();
        await Assert.That(beforeChangeDiags.Length).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies RXUIBIND004 is reported when WhenChanging is called on a WinForms Component type
    /// (WinForms does not support before-change notifications).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND004_WinFormsComponent_ReportsDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace System.ComponentModel
                                         {
                                             public class Component { }
                                         }

                                         namespace TestApp
                                         {
                                             public class WinFormsControl : System.ComponentModel.Component
                                             {
                                                 public string Name { get; set; } = "";
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     var vm = new WinFormsControl();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanging(vm, x => x.Name);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var beforeChangeDiags = diagnostics.Where(static d => d.Id == NoBeforeChangeSupportDiagnosticId).ToArray();
        await Assert.That(beforeChangeDiags.Length).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies RXUIBIND004 is reported when WhenChanging is called on an Android View type
    /// (Android does not support before-change notifications).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND004_AndroidView_ReportsDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace Android.Views
                                         {
                                             public class View { }
                                         }

                                         namespace TestApp
                                         {
                                             public class AndroidControl : Android.Views.View
                                             {
                                                 public string Name { get; set; } = "";
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     var vm = new AndroidControl();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanging(vm, x => x.Name);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var beforeChangeDiags = diagnostics.Where(static d => d.Id == NoBeforeChangeSupportDiagnosticId).ToArray();
        await Assert.That(beforeChangeDiags.Length).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies RXUIBIND004 is NOT reported when WhenChanging is called on an Apple NSObject type
    /// (KVO supports before-change notifications).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND004_KVO_NSObject_NoDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace Foundation
                                         {
                                             public class NSObject { }
                                         }

                                         namespace TestApp
                                         {
                                             public class AppleView : Foundation.NSObject
                                             {
                                                 public string Name { get; set; } = "";
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     var vm = new AppleView();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanging(vm, x => x.Name);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var beforeChangeDiags = diagnostics.Where(static d => d.Id == NoBeforeChangeSupportDiagnosticId).ToArray();
        await Assert.That(beforeChangeDiags.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies RXUIBIND004 is reported when WhenChanging is called on a type with no
    /// recognized notification mechanism (unknown type falls through to no before-change support).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND004_UnknownType_ReportsDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class PlainObject
                                             {
                                                 public string Name { get; set; } = "";
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     var vm = new PlainObject();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanging(vm, x => x.Name);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var beforeChangeDiags = diagnostics.Where(static d => d.Id == NoBeforeChangeSupportDiagnosticId).ToArray();
        await Assert.That(beforeChangeDiags.Length).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies RXUIBIND004 is reported when WhenChanging is called on a type that inherits
    /// from a platform type through a deep inheritance chain (e.g., two levels deep from
    /// WPF DependencyObject). Tests the InheritsFrom base-type walk at line 132.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND004_InheritsFrom_DeepChain_ReportsDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace System.Windows
                                         {
                                             public class DependencyObject { }
                                         }

                                         namespace TestApp
                                         {
                                             public class WpfControlBase : System.Windows.DependencyObject
                                             {
                                                 public string BaseName { get; set; } = "";
                                             }

                                             public class WpfDerivedControl : WpfControlBase
                                             {
                                                 public string Name { get; set; } = "";
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     var vm = new WpfDerivedControl();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanging(vm, x => x.Name);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var beforeChangeDiags = diagnostics.Where(static d => d.Id == NoBeforeChangeSupportDiagnosticId).ToArray();
        await Assert.That(beforeChangeDiags.Length).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies that RXUIBIND004 is reported with the correct mechanism message
    /// when WhenChanging is called on an INPC-only type (no INotifyPropertyChanging).
    /// This validates the diagnostic message format argument contains the mechanism name.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND004_INPCOnly_DiagnosticMessageContainsMechanism()
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
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanging(vm, x => x.Name);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var beforeChangeDiags = diagnostics.Where(static d => d.Id == NoBeforeChangeSupportDiagnosticId).ToArray();
        await Assert.That(beforeChangeDiags.Length).IsEqualTo(1);
        var message = beforeChangeDiags[0].GetMessage();
        await Assert.That(message).Contains("INotifyPropertyChanged");
    }

    /// <summary>
    /// Verifies that RXUIBIND004 reports the correct mechanism message for WPF DependencyObject.
    /// This validates the "WPF DependencyObject" mechanism string in the diagnostic message.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND004_WpfDependencyObject_DiagnosticMessageContainsMechanism()
    {
        const string Source = Preamble + """

                                         namespace System.Windows
                                         {
                                             public class DependencyObject { }
                                         }

                                         namespace TestApp
                                         {
                                             public class WpfControl : System.Windows.DependencyObject
                                             {
                                                 public string Name { get; set; } = "";
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     var vm = new WpfControl();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanging(vm, x => x.Name);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var beforeChangeDiags = diagnostics.Where(static d => d.Id == NoBeforeChangeSupportDiagnosticId).ToArray();
        await Assert.That(beforeChangeDiags.Length).IsEqualTo(1);
        var message = beforeChangeDiags[0].GetMessage();
        await Assert.That(message).Contains("WPF DependencyObject");
    }

    /// <summary>
    /// Verifies that RXUIBIND004 reports the correct mechanism message for WinUI DependencyObject.
    /// This validates the "WinUI DependencyObject" mechanism string in the diagnostic message.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND004_WinUIDependencyObject_DiagnosticMessageContainsMechanism()
    {
        const string Source = Preamble + """

                                         namespace Microsoft.UI.Xaml
                                         {
                                             public class DependencyObject { }
                                         }

                                         namespace TestApp
                                         {
                                             public class WinUIControl : Microsoft.UI.Xaml.DependencyObject
                                             {
                                                 public string Name { get; set; } = "";
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     var vm = new WinUIControl();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanging(vm, x => x.Name);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var beforeChangeDiags = diagnostics.Where(static d => d.Id == NoBeforeChangeSupportDiagnosticId).ToArray();
        await Assert.That(beforeChangeDiags.Length).IsEqualTo(1);
        var message = beforeChangeDiags[0].GetMessage();
        await Assert.That(message).Contains("WinUI DependencyObject");
    }

    /// <summary>
    /// Verifies that RXUIBIND004 reports the correct mechanism message for WinForms Component.
    /// This validates the "WinForms Component" mechanism string in the diagnostic message.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND004_WinFormsComponent_DiagnosticMessageContainsMechanism()
    {
        const string Source = Preamble + """

                                         namespace System.ComponentModel
                                         {
                                             public class Component { }
                                         }

                                         namespace TestApp
                                         {
                                             public class WinFormsControl : System.ComponentModel.Component
                                             {
                                                 public string Name { get; set; } = "";
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     var vm = new WinFormsControl();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanging(vm, x => x.Name);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var beforeChangeDiags = diagnostics.Where(static d => d.Id == NoBeforeChangeSupportDiagnosticId).ToArray();
        await Assert.That(beforeChangeDiags.Length).IsEqualTo(1);
        var message = beforeChangeDiags[0].GetMessage();
        await Assert.That(message).Contains("WinForms Component");
    }

    /// <summary>
    /// Verifies that RXUIBIND004 reports the correct mechanism message for Android View.
    /// This validates the "Android View" mechanism string in the diagnostic message.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND004_AndroidView_DiagnosticMessageContainsMechanism()
    {
        const string Source = Preamble + """

                                         namespace Android.Views
                                         {
                                             public class View { }
                                         }

                                         namespace TestApp
                                         {
                                             public class AndroidControl : Android.Views.View
                                             {
                                                 public string Name { get; set; } = "";
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     var vm = new AndroidControl();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanging(vm, x => x.Name);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var beforeChangeDiags = diagnostics.Where(static d => d.Id == NoBeforeChangeSupportDiagnosticId).ToArray();
        await Assert.That(beforeChangeDiags.Length).IsEqualTo(1);
        var message = beforeChangeDiags[0].GetMessage();
        await Assert.That(message).Contains("Android View");
    }

    /// <summary>
    /// Verifies that RXUIBIND004 reports "unknown" mechanism when WhenChanging is called on
    /// a plain type with no recognized notification mechanism. This exercises the final
    /// fallthrough path in HasBeforeChangeSupport where mechanism is set to "unknown".
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND004_UnknownType_DiagnosticMessageContainsUnknown()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class PlainObject
                                             {
                                                 public string Name { get; set; } = "";
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     var vm = new PlainObject();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanging(vm, x => x.Name);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var beforeChangeDiags = diagnostics.Where(static d => d.Id == NoBeforeChangeSupportDiagnosticId).ToArray();
        await Assert.That(beforeChangeDiags.Length).IsEqualTo(1);
        var message = beforeChangeDiags[0].GetMessage();
        await Assert.That(message).Contains("unknown");
    }

    /// <summary>
    /// Verifies that RXUIBIND004 is NOT reported when WhenChanging is called on a type
    /// that inherits from NSObject through an intermediate class. This tests the InheritsFrom
    /// walk for the KVO/NSObject path which supports before-change notifications.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND004_KVO_NSObject_DeepInheritance_NoDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace Foundation
                                         {
                                             public class NSObject { }
                                         }

                                         namespace TestApp
                                         {
                                             public class AppleBase : Foundation.NSObject { }

                                             public class AppleDerived : AppleBase
                                             {
                                                 public string Name { get; set; } = "";
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     var vm = new AppleDerived();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanging(vm, x => x.Name);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var beforeChangeDiags = diagnostics.Where(static d => d.Id == NoBeforeChangeSupportDiagnosticId).ToArray();
        await Assert.That(beforeChangeDiags.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies that RXUIBIND004 is not reported for a non-generic WhenChanging method
    /// on the recognized extension class (e.g., a generated dispatch overload with concrete types).
    /// Exercises the <c>ExtractFirstTypeArgument == null</c> guard in <c>CheckBeforeChangeSupport</c>.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND004_NonGenericWhenChanging_NoDiagnostic()
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
                                      public static object WhenChanging(
                                          TestApp.MyViewModel obj,
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

                                  public class Usage
                                  {
                                      public void Test()
                                      {
                                          var vm = new MyViewModel();
                                          ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanging(vm);
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var beforeChangeDiags = diagnostics.Where(static d => d.Id == NoBeforeChangeSupportDiagnosticId).ToArray();
        await Assert.That(beforeChangeDiags.Length).IsEqualTo(0);
    }
}
