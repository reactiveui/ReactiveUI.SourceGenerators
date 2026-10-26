// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Analyzer.Analyzers;
using ReactiveUI.Binding.Analyzer.Tests.Helpers;

namespace ReactiveUI.Binding.Analyzer.Tests;

/// <summary>Tests for <see cref="TypeAnalyzer"/>.</summary>
public partial class TypeAnalyzerTests
{
    /// <summary>The diagnostic id reported when a type has no observable properties.</summary>
    private const string NoObservablePropertiesDiagnosticId = "RXUIBIND002";

    /// <summary>Common using directives and the generated stub class that the analyzer recognizes.</summary>
    private const string Preamble = """
                                    using System;
                                    using System.ComponentModel;
                                    using System.Linq.Expressions;

                                    namespace ReactiveUI.Binding
                                    {
                                        public static class __ReactiveUIGeneratedBindings
                                        {
                                            public static object WhenChanged<TObj, TReturn>(
                                                this TObj obj,
                                                Expression<Func<TObj, TReturn>> property)
                                                where TObj : class
                                                => throw new NotImplementedException();
                                        }
                                    }
                                    """;

    /// <summary>Verifies RXUIBIND002 is reported when the source type does not implement any observable mechanism.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND002_NoObservableMechanism_ReportsDiagnostic()
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
                                                     var obj = new PlainObject();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(obj, x => x.Name);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<TypeAnalyzer>(Source);
        await Assert.That(diagnostics.Length).IsEqualTo(1);
        await Assert.That(diagnostics[0].Id).IsEqualTo(NoObservablePropertiesDiagnosticId);
    }

    /// <summary>Verifies that an Unsafe call can observe a notification interface implemented only by the runtime type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND002_UnsafeInterfaceReceiver_NoDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace ReactiveUI.Binding
                                         {
                                             public static class ReactiveUIBindingExtensions
                                             {
                                                 public static object WhenAnyValueUnsafe<TObj, TReturn>(
                                                     this TObj obj,
                                                     Expression<Func<TObj, TReturn>> property)
                                                     where TObj : class
                                                     => throw new NotImplementedException();
                                             }
                                         }

                                         namespace TestApp
                                         {
                                             public interface IViewFor
                                             {
                                                 object ViewModel { get; }
                                             }

                                             public class Usage
                                             {
                                                 public void Test(IViewFor view)
                                                 {
                                                     ReactiveUI.Binding.ReactiveUIBindingExtensions.WhenAnyValueUnsafe(view, x => x.ViewModel);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<TypeAnalyzer>(Source);
        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }

    /// <summary>Verifies RXUIBIND002 is NOT reported when the source type implements INotifyPropertyChanged.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND002_INPC_NoDiagnostic()
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

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<TypeAnalyzer>(Source);
        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }

    /// <summary>Verifies RXUIBIND002 is NOT reported for empty source code.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND002_EmptySource_NoDiagnostics()
    {
        const string Source = """
                              namespace TestApp
                              {
                                  public class EmptyClass { }
                              }
                              """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<TypeAnalyzer>(Source);
        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies RXUIBIND002 is NOT reported when the source type implements both
    /// INotifyPropertyChanged and INotifyPropertyChanging.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND002_INPCWithChanging_NoDiagnostic()
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
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(vm, x => x.Name);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<TypeAnalyzer>(Source);
        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }

    /// <summary>Verifies RXUIBIND002 is NOT reported when the source contains no binding method invocations.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND002_NonMethodInvocation_NoDiagnostics()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class PlainObject
                                             {
                                                 public string Name { get; set; } = "";
                                                 public void DoWork() { }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<TypeAnalyzer>(Source);
        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies RXUIBIND002 is NOT reported when the source type inherits from a base class
    /// that implements INotifyPropertyChanged.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND002_InterfaceInheritance_NoDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class BaseViewModel : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                             }

                                             public class DerivedViewModel : BaseViewModel
                                             {
                                                 public string Name { get; set; } = "";
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     var derived = new DerivedViewModel();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(derived, x => x.Name);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<TypeAnalyzer>(Source);
        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }

    /// <summary>Verifies RXUIBIND002 is NOT reported when the source type implements IReactiveObject.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND002_IReactiveObject_NoDiagnostic()
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
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(vm, x => x.Name);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<TypeAnalyzer>(Source);
        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }

    /// <summary>Verifies RXUIBIND002 is NOT reported when the source type inherits from WPF DependencyObject.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND002_WpfDependencyObject_NoDiagnostic()
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
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(vm, x => x.Name);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<TypeAnalyzer>(Source);
        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }

    /// <summary>Verifies RXUIBIND002 is NOT reported when the source type inherits from WinUI DependencyObject.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND002_WinUIDependencyObject_NoDiagnostic()
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
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(vm, x => x.Name);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<TypeAnalyzer>(Source);
        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies RXUIBIND002 is NOT reported for an Uno control, which implements <c>DependencyObject</c> as an interface
    /// on every head but Windows.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND002_UnoInterfaceDependencyObject_NoDiagnostic()
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
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(vm, x => x.Name);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<TypeAnalyzer>(Source);
        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }

    /// <summary>Verifies RXUIBIND002 is NOT reported when the source type inherits from Apple NSObject (KVO).</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND002_KVO_NSObject_NoDiagnostic()
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
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(vm, x => x.Name);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<TypeAnalyzer>(Source);
        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }

    /// <summary>Verifies RXUIBIND002 is NOT reported when the source type inherits from WinForms Component.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND002_WinFormsComponent_NoDiagnostic()
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
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(vm, x => x.Name);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<TypeAnalyzer>(Source);
        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }

    /// <summary>Verifies RXUIBIND002 is NOT reported when the source type inherits from Android View.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND002_AndroidView_NoDiagnostic()
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
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(vm, x => x.Name);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<TypeAnalyzer>(Source);
        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies RXUIBIND002 is NOT reported when the source type is an abstract class
    /// that implements INotifyPropertyChanged. Abstract classes can still be used as
    /// type arguments in binding method calls through derived instances.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND002_AbstractClass_WithINPC_NoDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public abstract class AbstractViewModel : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                                 public string Name { get; set; } = "";
                                             }

                                             public class ConcreteViewModel : AbstractViewModel { }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     AbstractViewModel vm = new ConcreteViewModel();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(vm, x => x.Name);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<TypeAnalyzer>(Source);
        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }

    /// <summary>Verifies RXUIBIND002 IS reported when the source type is an abstract class without any observable mechanism.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND002_AbstractClass_NoObservableMechanism_ReportsDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public abstract class AbstractPlainObject
                                             {
                                                 public string Name { get; set; } = "";
                                             }

                                             public class ConcreteObject : AbstractPlainObject { }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     AbstractPlainObject obj = new ConcreteObject();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(obj, x => x.Name);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<TypeAnalyzer>(Source);
        await Assert.That(diagnostics.Length).IsEqualTo(1);
        await Assert.That(diagnostics[0].Id).IsEqualTo(NoObservablePropertiesDiagnosticId);
    }

    /// <summary>
    /// Verifies RXUIBIND002 is NOT reported when a generic class implementing INPC is used
    /// as the source type in a binding method call.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND002_GenericClass_WithINPC_NoDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class GenericViewModel<T> : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                                 public T Value { get; set; } = default!;
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     var vm = new GenericViewModel<string>();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(vm, x => x.Value);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<TypeAnalyzer>(Source);
        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }

    /// <summary>Verifies RXUIBIND002 IS reported when a generic class without any observable mechanism is used as the source type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND002_GenericClass_NoObservableMechanism_ReportsDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class GenericContainer<T>
                                             {
                                                 public T Value { get; set; } = default!;
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     var container = new GenericContainer<string>();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(container, x => x.Value);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<TypeAnalyzer>(Source);
        await Assert.That(diagnostics.Length).IsEqualTo(1);
        await Assert.That(diagnostics[0].Id).IsEqualTo(NoObservablePropertiesDiagnosticId);
    }

    /// <summary>
    /// Verifies RXUIBIND002 is NOT reported when the source type has observable properties
    /// by implementing INotifyPropertyChanged directly.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND002_ClassWithObservableProperties_NoDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class MyViewModel : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                                 public string FirstName { get; set; } = "";
                                                 public string LastName { get; set; } = "";
                                                 public int Age { get; set; }
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     var vm = new MyViewModel();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(vm, x => x.FirstName);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<TypeAnalyzer>(Source);
        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }

    /// <summary>Verifies RXUIBIND002 is NOT reported when the source type inherits INPC from a deep inheritance chain.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND002_DeepInheritanceChain_WithINPC_NoDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class Level0 : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                             }

                                             public class Level1 : Level0 { }

                                             public class Level2 : Level1
                                             {
                                                 public string Name { get; set; } = "";
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     var vm = new Level2();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(vm, x => x.Name);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<TypeAnalyzer>(Source);
        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies RXUIBIND002 is NOT reported for a non-binding method invocation.
    /// This exercises the IsBindingExtensionMethod false branch in AnalyzeInvocation.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND002_NonBindingMethodInvocation_NoDiagnostics()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public static class CustomExtensions
                                             {
                                                 public static string GetName<T>(this T obj) => "";
                                             }

                                             public class PlainObject
                                             {
                                                 public string Name { get; set; } = "";
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     var obj = new PlainObject();
                                                     obj.GetName();
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<TypeAnalyzer>(Source);
        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies RXUIBIND002 is NOT reported when the source type inherits from WPF
    /// DependencyObject through a deep inheritance chain (multi-level).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND002_DeepWpfInheritanceChain_NoDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace System.Windows
                                         {
                                             public class DependencyObject { }
                                         }

                                         namespace TestApp
                                         {
                                             public class WpfBase : System.Windows.DependencyObject { }
                                             public class WpfMiddle : WpfBase { }
                                             public class WpfLeaf : WpfMiddle
                                             {
                                                 public string Name { get; set; } = "";
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     var vm = new WpfLeaf();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(vm, x => x.Name);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<TypeAnalyzer>(Source);
        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies RXUIBIND002 is NOT reported when the source type implements IReactiveObject
    /// through an intermediate interface hierarchy.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND002_IReactiveObject_WithProperties_NoDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class ReactiveViewModel : ReactiveUI.IReactiveObject
                                             {
                                                 public string Name { get; set; } = "";
                                                 public int Count { get; set; }
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     var vm = new ReactiveViewModel();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(vm, x => x.Name);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<TypeAnalyzer>(Source);
        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies RXUIBIND002 is NOT reported when the source type inherits from
    /// Apple NSObject through a deep inheritance chain. This exercises the base-type
    /// walk iterating past multiple levels before finding NSObject.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND002_DeepKVO_NSObject_NoDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace Foundation
                                         {
                                             public class NSObject { }
                                         }

                                         namespace TestApp
                                         {
                                             public class AppleBase : Foundation.NSObject { }
                                             public class AppleMiddle : AppleBase { }
                                             public class AppleLeaf : AppleMiddle
                                             {
                                                 public string Name { get; set; } = "";
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     var vm = new AppleLeaf();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(vm, x => x.Name);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<TypeAnalyzer>(Source);
        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies RXUIBIND002 is NOT reported when the source type inherits from
    /// WinForms Component through an intermediate class. This exercises the base-type
    /// walk for the WinForms path through multiple levels.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND002_DeepWinFormsComponent_NoDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace System.ComponentModel
                                         {
                                             public class Component { }
                                         }

                                         namespace TestApp
                                         {
                                             public class WinFormsBase : System.ComponentModel.Component { }
                                             public class WinFormsMiddle : WinFormsBase { }
                                             public class WinFormsLeaf : WinFormsMiddle
                                             {
                                                 public string Name { get; set; } = "";
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     var vm = new WinFormsLeaf();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(vm, x => x.Name);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<TypeAnalyzer>(Source);
        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies RXUIBIND002 is NOT reported when the source type inherits from
    /// Android View through an intermediate class. This exercises the base-type
    /// walk for the Android path through multiple levels.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND002_DeepAndroidView_NoDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace Android.Views
                                         {
                                             public class View { }
                                         }

                                         namespace TestApp
                                         {
                                             public class AndroidBase : Android.Views.View { }
                                             public class AndroidMiddle : AndroidBase { }
                                             public class AndroidLeaf : AndroidMiddle
                                             {
                                                 public string Name { get; set; } = "";
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     var vm = new AndroidLeaf();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(vm, x => x.Name);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<TypeAnalyzer>(Source);
        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies RXUIBIND002 is NOT reported when the source type inherits from
    /// WinUI DependencyObject through a deep inheritance chain. This exercises the
    /// base-type walk iterating past multiple levels before finding WinUI DO.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND002_DeepWinUIDependencyObject_NoDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace Microsoft.UI.Xaml
                                         {
                                             public class DependencyObject { }
                                         }

                                         namespace TestApp
                                         {
                                             public class WinUIBase : Microsoft.UI.Xaml.DependencyObject { }
                                             public class WinUIMiddle : WinUIBase { }
                                             public class WinUILeaf : WinUIMiddle
                                             {
                                                 public string Name { get; set; } = "";
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     var vm = new WinUILeaf();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(vm, x => x.Name);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<TypeAnalyzer>(Source);
        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies RXUIBIND002 IS reported when a type inherits from a custom base class
    /// that does not implement any observable mechanism. The base-type walk should iterate
    /// through the chain without finding any match and fall through to report the diagnostic.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND002_DeepInheritanceChain_NoObservableMechanism_ReportsDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class Level0 { }
                                             public class Level1 : Level0 { }
                                             public class Level2 : Level1 { }
                                             public class Level3 : Level2
                                             {
                                                 public string Name { get; set; } = "";
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     var vm = new Level3();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(vm, x => x.Name);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<TypeAnalyzer>(Source);
        await Assert.That(diagnostics.Length).IsEqualTo(1);
        await Assert.That(diagnostics[0].Id).IsEqualTo(NoObservablePropertiesDiagnosticId);
    }

    /// <summary>
    /// Verifies RXUIBIND002 diagnostic message includes the source type name.
    /// This validates the message format argument is correctly passed.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND002_DiagnosticMessageContainsTypeName()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class UnobservableModel
                                             {
                                                 public string Name { get; set; } = "";
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     var obj = new UnobservableModel();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(obj, x => x.Name);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<TypeAnalyzer>(Source);
        await Assert.That(diagnostics.Length).IsEqualTo(1);
        var message = diagnostics[0].GetMessage();
        await Assert.That(message).Contains("UnobservableModel");
    }

    /// <summary>
    /// Verifies that RXUIBIND002 is not reported for a non-generic WhenChanged method
    /// on the recognized extension class (e.g., a generated dispatch overload with concrete types).
    /// Exercises the <c>ExtractFirstTypeArgument == null</c> guard in <c>TypeAnalyzer.AnalyzeInvocation</c>.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND002_NonGenericWhenChanged_NoDiagnostic()
    {
        const string Source = """
                              using System;
                              using System.ComponentModel;

                              namespace ReactiveUI.Binding
                              {
                                  public static class __ReactiveUIGeneratedBindings
                                  {
                                      // Non-generic generated dispatch overload (concrete types)
                                      public static object WhenChanged(
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
                                          ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(vm);
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<TypeAnalyzer>(Source);
        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies RXUIBIND002 stays silent for <c>BindTo</c>, whose first type argument is the value type of a
    /// stream the caller already built rather than an object anything observes.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND002_BindToStreamOfPlainValues_NoDiagnostic()
    {
        const string Source = """
                              using System;
                              using System.ComponentModel;
                              using System.Linq.Expressions;

                              namespace ReactiveUI.Binding
                              {
                                  public static class __ReactiveUIGeneratedBindings
                                  {
                                      public static IDisposable BindTo<TValue, TTarget, TTargetValue>(
                                          this IObservable<TValue> source,
                                          TTarget target,
                                          Expression<Func<TTarget, TTargetValue>> property)
                                          => throw new NotImplementedException();
                                  }
                              }

                              namespace TestApp
                              {
                                  public class MyView : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;
                                      public string Caption { get; set; } = "";
                                  }

                                  public class Usage
                                  {
                                      public void Test(IObservable<string> names)
                                      {
                                          var view = new MyView();
                                          ReactiveUI.Binding.__ReactiveUIGeneratedBindings.BindTo(names, view, v => v.Caption);
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<TypeAnalyzer>(Source);
        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }
}
