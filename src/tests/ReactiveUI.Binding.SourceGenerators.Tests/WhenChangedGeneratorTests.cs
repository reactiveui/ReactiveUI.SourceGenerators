// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>Snapshot tests for WhenChanged (after-change) invocation generation.</summary>
public class WhenChangedGeneratorTests
{
    /// <summary>The <c>WhenChangedDispatch.g.cs</c> name these tests generate against.</summary>
    private const string WhenChangedDispatchgcsName = "WhenChangedDispatch.g.cs";

    /// <summary>Verifies that a WhenChanged invocation on an INPC class generates PropertyChanged observation.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_INPC()
    {
        var source = SharedSourceReader.ReadScenario("WhenChanged/SinglePropertyINPC");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(WhenChangedGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies that the generator produces correct output when no invocations are present.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NoInvocations()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;

                              namespace TestApp
                              {
                                  public class MyViewModel : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;
                                      public string Name { get; set; } = "";
                                  }
                              }
                              """;

        var result =
            await TestHelper.TestPassWithResult(source, typeof(WhenChangedGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies WhenChanged on a ReactiveObject-based class.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_ReactiveObject()
    {
        var source = SharedSourceReader.ReadScenario("WhenChanged/SinglePropertyReactiveObject");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(WhenChangedGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies WhenChanged with two properties returns a tuple.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MultiProperty_TwoProperties()
    {
        var source = SharedSourceReader.ReadScenario("WhenChanged/MultiPropertyTwoProperties");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(WhenChangedGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies WhenChanged with three properties returns a tuple.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MultiProperty_ThreeProperties()
    {
        var source = SharedSourceReader.ReadScenario("WhenChanged/MultiPropertyThreeProperties");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(WhenChangedGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies WhenChanged with a selector function.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MultiProperty_WithSelector()
    {
        var source = SharedSourceReader.ReadScenario("WhenChanged/MultiPropertyWithSelector");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(WhenChangedGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies WhenChanged with a deep property chain like x => x.Child.Name.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepPropertyChain()
    {
        var source = SharedSourceReader.ReadScenario("WhenChanged/DeepPropertyChain");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(WhenChangedGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies WhenChanged with a nullable property type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableProperty()
    {
        var source = SharedSourceReader.ReadScenario("WhenChanged/NullableProperty");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(WhenChangedGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies WhenChanged with multiple ViewModels in the same compilation.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MultipleViewModels()
    {
        var source = SharedSourceReader.ReadScenario("WhenChanged/MultipleViewModels");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(WhenChangedGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies WhenChanged with an integer property type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IntProperty()
    {
        var source = SharedSourceReader.ReadScenario("WhenChanged/IntProperty");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(WhenChangedGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies WhenChanged with a 4-level deep property chain (ObjChain1 -> ObjChain2 -> ObjChain3 -> Model).</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FourLevelDeepChain()
    {
        var source = SharedSourceReader.ReadScenario("WhenChanged/FourLevelDeepChain");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(WhenChangedGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies WhenChanged with multiple invocations on the same ViewModel.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MultipleInvocations_SameViewModel()
    {
        var source = SharedSourceReader.ReadScenario("WhenChanged/MultipleInvocationsSameViewModel");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(WhenChangedGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies WhenChanged with a mix of deep chain and shallow properties in multi-property observation.
    /// Tests that Address.City uses Select/Switch deep chain pattern while Name uses shallow observation,
    /// both combined via CombineLatest with a selector function.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MultiProperty_WithDeepChains()
    {
        var source = SharedSourceReader.ReadScenario("WhenChanged/MultiPropertyWithDeepChains");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(WhenChangedGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies WhenChanged with the null-forgiving operator (x => x.Child!.Name).
    /// The generator must unwrap the PostfixUnaryExpressionSyntax (SuppressNullableWarning)
    /// to extract the correct property path.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullForgivingDeepChain()
    {
        var source = SharedSourceReader.ReadScenario("WhenChanged/NullForgivingDeepChain");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(WhenChangedGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies that WhenChanged on a WPF DependencyObject generates the correct observation code.
    /// Uses a stub DependencyObject to simulate WPF without requiring the WPF SDK.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WpfDependencyObject_Property()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;

                              using ReactiveUI.Binding;

                              namespace System.Windows
                              {
                                  public class DependencyObject {}
                              }

                              namespace TestApp
                              {
                                  public class MyWpfControl : System.Windows.DependencyObject, INotifyPropertyChanged
                                  {
                                      private string _text = string.Empty;
                                      public event PropertyChangedEventHandler? PropertyChanged;
                                      public string Text
                                      {
                                          get => _text;
                                          set
                                          {
                                              if (_text != value)
                                              {
                                                  _text = value;
                                                  PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
                                              }
                                          }
                                      }
                                  }

                                  public static class Scenario
                                  {
                                      public static IObservable<string> Execute(MyWpfControl control)
                                          => control.WhenChanged(x => x.Text);
                                  }
                              }
                              """;

        var result =
            await TestHelper.TestPassWithResult(source, typeof(WhenChangedGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies that WhenChanged on a WinForms Component generates the correct observation code.
    /// Uses a stub Component to simulate WinForms without requiring the WinForms SDK.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinFormsComponent_Property()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;

                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public class MyWinFormsControl : System.ComponentModel.Component, INotifyPropertyChanged
                                  {
                                      private string _text = string.Empty;
                                      public event PropertyChangedEventHandler? PropertyChanged;
                                      public event EventHandler? TextChanged;
                                      public string Text
                                      {
                                          get => _text;
                                          set
                                          {
                                              if (_text != value)
                                              {
                                                  _text = value;
                                                  PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
                                                  TextChanged?.Invoke(this, EventArgs.Empty);
                                              }
                                          }
                                      }
                                  }

                                  public static class Scenario
                                  {
                                      public static IObservable<string> Execute(MyWinFormsControl control)
                                          => control.WhenChanged(x => x.Text);
                                  }
                              }
                              """;

        var result =
            await TestHelper.TestPassWithResult(source, typeof(WhenChangedGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies that WhenChanged on an Android View generates the correct observation code.
    /// Uses a stub View to simulate Android without requiring the Android SDK.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AndroidView_Property()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;

                              using ReactiveUI.Binding;

                              namespace Android.Views
                              {
                                  public class View {}
                              }

                              namespace TestApp
                              {
                                  public class MyAndroidView : Android.Views.View, INotifyPropertyChanged
                                  {
                                      private string _text = string.Empty;
                                      public event PropertyChangedEventHandler? PropertyChanged;
                                      public string Text
                                      {
                                          get => _text;
                                          set
                                          {
                                              if (_text != value)
                                              {
                                                  _text = value;
                                                  PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
                                              }
                                          }
                                      }
                                  }

                                  public static class Scenario
                                  {
                                      public static IObservable<string> Execute(MyAndroidView view)
                                          => view.WhenChanged(x => x.Text);
                                  }
                              }
                              """;

        var result =
            await TestHelper.TestPassWithResult(source, typeof(WhenChangedGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies that WhenChanged on an NSObject (KVO) generates the correct observation code.
    /// Uses a stub NSObject to simulate Apple platforms without requiring the iOS/macOS SDK.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task KVO_NSObject_Property()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;

                              using ReactiveUI.Binding;

                              namespace Foundation
                              {
                                  public class NSString
                                  {
                                      private readonly string _value;
                                      public NSString(string value) { _value = value; }
                                      public static explicit operator NSString(string value) => new NSString(value);
                                  }
                                  public class NSDictionary {}
                                  public enum NSKeyValueObservingOptions { New = 1, Old = 2 }
                                  public class NSObject
                                  {
                                      public virtual void ObserveValue(NSString keyPath, NSObject ofObject, NSDictionary change, IntPtr context) {}
                                      public void AddObserver(NSObject observer, NSString keyPath, NSKeyValueObservingOptions options, IntPtr context) {}
                                      public void RemoveObserver(NSObject observer, NSString keyPath) {}
                                  }
                              }

                              namespace TestApp
                              {
                                  public class MyAppleView : Foundation.NSObject, INotifyPropertyChanged
                                  {
                                      private string _text = string.Empty;
                                      public event PropertyChangedEventHandler? PropertyChanged;
                                      public string Text
                                      {
                                          get => _text;
                                          set
                                          {
                                              if (_text != value)
                                              {
                                                  _text = value;
                                                  PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
                                              }
                                          }
                                      }
                                  }

                                  public static class Scenario
                                  {
                                      public static IObservable<string> Execute(MyAppleView view)
                                          => view.WhenChanged(x => x.Text);
                                  }
                              }
                              """;

        var result =
            await TestHelper.TestPassWithResult(source, typeof(WhenChangedGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies that WhenChanged on a WinUI DependencyObject generates the correct observation code.
    /// Uses a stub DependencyObject to simulate WinUI without requiring the WinUI SDK.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WinUIDependencyObject_Property()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;

                              using ReactiveUI.Binding;

                              namespace Microsoft.UI.Xaml
                              {
                                  public class DependencyProperty {}
                                  public class DependencyObject
                                  {
                                      public long RegisterPropertyChangedCallback(DependencyProperty dp, DependencyPropertyChangedCallback callback) => 0;
                                      public void UnregisterPropertyChangedCallback(DependencyProperty dp, long token) {}
                                  }
                                  public delegate void DependencyPropertyChangedCallback(DependencyObject sender, DependencyProperty dp);
                              }

                              namespace TestApp
                              {
                                  public class MyWinUIControl : Microsoft.UI.Xaml.DependencyObject, INotifyPropertyChanged
                                  {
                                      public static readonly Microsoft.UI.Xaml.DependencyProperty TextProperty = new Microsoft.UI.Xaml.DependencyProperty();
                                      private string _text = string.Empty;
                                      public event PropertyChangedEventHandler? PropertyChanged;
                                      public string Text
                                      {
                                          get => _text;
                                          set
                                          {
                                              if (_text != value)
                                              {
                                                  _text = value;
                                                  PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
                                              }
                                          }
                                      }
                                  }

                                  public static class Scenario
                                  {
                                      public static IObservable<string> Execute(MyWinUIControl control)
                                          => control.WhenChanged(x => x.Text);
                                  }
                              }
                              """;

        var result =
            await TestHelper.TestPassWithResult(source, typeof(WhenChangedGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies that WhenChanged observes a dependency property on an Uno control, whose <c>DependencyObject</c> is an
    /// interface on every head but Windows.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UnoInterfaceDependencyObject_Property()
    {
        const string source = """
                              using System;

                              using ReactiveUI.Binding;

                              namespace Microsoft.UI.Xaml
                              {
                                  public class DependencyProperty {}
                                  public interface DependencyObject
                                  {
                                      long RegisterPropertyChangedCallback(DependencyProperty dp, DependencyPropertyChangedCallback callback);
                                      void UnregisterPropertyChangedCallback(DependencyProperty dp, long token);
                                  }
                                  public delegate void DependencyPropertyChangedCallback(DependencyObject sender, DependencyProperty dp);
                              }

                              namespace TestApp
                              {
                                  public class MyUnoControl : Microsoft.UI.Xaml.DependencyObject
                                  {
                                      public static readonly Microsoft.UI.Xaml.DependencyProperty TextProperty = new Microsoft.UI.Xaml.DependencyProperty();
                                      public string Text { get; set; } = string.Empty;
                                      public long RegisterPropertyChangedCallback(Microsoft.UI.Xaml.DependencyProperty dp, Microsoft.UI.Xaml.DependencyPropertyChangedCallback callback) => 0;
                                      public void UnregisterPropertyChangedCallback(Microsoft.UI.Xaml.DependencyProperty dp, long token) {}
                                  }

                                  public static class Scenario
                                  {
                                      public static IObservable<string> Execute(MyUnoControl control)
                                          => control.WhenChanged(x => x.Text);
                                  }
                              }
                              """;

        var result =
            await TestHelper.TestPassWithResult(source, typeof(WhenChangedGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
        await Assert.That(result.GeneratedSources["WhenChangedDispatch.g.cs"]).Contains("RegisterPropertyChangedCallback");
    }

    /// <summary>
    /// Verifies that WhenChanged generates CallerFilePath dispatch when targeting pre-C# 10.
    /// CompilationSucceeds is omitted because the CallerFilePath stub signature is ambiguous
    /// with the runtime extension method in this test harness (both assemblies are referenced).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_INPC_CallerFilePath()
    {
        var source = SharedSourceReader.ReadScenario("WhenChanged/SinglePropertyINPC");
        var result = await TestHelper.TestPassWithResult(
            source,
            typeof(WhenChangedGeneratorTests),
            TestHelper.FallbackLanguageVersion(nullableEnabled: true));
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies that a method named "WhenChanged" on a non-ReactiveUI extension class is ignored
    /// by the generator. Exercises the extension class name mismatch guard in the extraction helpers
    /// (lines 419-424 of ExtractInvocationInfo).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExtensionClassNameMismatch_GeneratesNoDispatch()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;

                              namespace TestApp
                              {
                                  public class MyViewModel : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;
                                      public string Name { get; set; } = "";
                                  }

                                  public static class CustomExtensions
                                  {
                                      public static string WhenChanged<T>(this T obj, Func<T, string> selector)
                                          => selector(obj);
                                  }

                                  public static class Scenario
                                  {
                                      public static void Execute(MyViewModel vm)
                                      {
                                          // This calls CustomExtensions.WhenChanged, NOT ReactiveUIBindingExtensions.WhenChanged
                                          var result = vm.WhenChanged(x => x.Name);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source);

        // The generator should not produce any WhenChanged dispatch code
        // because the method belongs to CustomExtensions, not our extension class.
        await result.HasNoGeneratorDiagnostics();
        await result.DoesNotHaveGeneratedSource(WhenChangedDispatchgcsName);
    }

    /// <summary>
    /// Verifies that an identity lambda (x => x) with no member access produces no dispatch code.
    /// Exercises the segments.Count == 0 guard in the extraction helpers.ExtractPropertyPathFromLambda (line 578-581).
    /// The compilation will have errors because the lambda return type is the object itself, not a property type,
    /// but the generator should not crash and should produce no diagnostics.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IdentityLambda_GeneratesNoDispatch()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;
                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public class MyViewModel : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;
                                      public string Name { get; set; } = "";
                                  }

                                  public static class Scenario
                                  {
                                      public static void Execute(MyViewModel vm)
                                      {
                                          // Identity lambda — no member access, just x => x
                                          var obs = vm.WhenChanged(x => x);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source);

        // The generator should silently skip the identity lambda and produce no dispatch.
        await result.HasNoGeneratorDiagnostics();
        await result.DoesNotHaveGeneratedSource(WhenChangedDispatchgcsName);
    }

    /// <summary>
    /// Verifies that a block-body lambda (statement body) is skipped by the generator.
    /// Exercises the body == null guard in the extraction helpers.ExtractPropertyPathFromLambda (line 543-546)
    /// when the lambda body is a Block instead of an expression.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BlockBodyLambda_GeneratesNoDispatch()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;
                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public class MyViewModel : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;
                                      public string Name { get; set; } = "";
                                  }

                                  public static class Scenario
                                  {
                                      public static void Execute(MyViewModel vm)
                                      {
                                          // Block body lambda — has a statement body rather than expression body
                                          var obs = vm.WhenChanged(x => { return x.Name; });
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source);

        // The generator should silently skip block-body lambdas and produce no dispatch.
        await result.HasNoGeneratorDiagnostics();
        await result.DoesNotHaveGeneratedSource(WhenChangedDispatchgcsName);
    }

    /// <summary>Verifies that a lambda ending at a read-only field is skipped by the generator.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReadOnlyLeafFieldLambda_GeneratesNoDispatch()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;
                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public class MyViewModel : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;
                                      public readonly string _name = "";
                                  }

                                  public static class Scenario
                                  {
                                      public static void Execute(MyViewModel vm)
                                      {
                                          // Accessing a read-only field at the leaf
                                          var obs = vm.WhenChanged(x => x._name);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source);

        // The generator should silently skip field access lambdas and produce no dispatch.
        await result.HasNoGeneratorDiagnostics();
        await result.DoesNotHaveGeneratedSource(WhenChangedDispatchgcsName);
    }

    /// <summary>
    /// Verifies that a lambda accessing a private property is skipped by the generator.
    /// Exercises the DeclaredAccessibility check in the extraction helpers.ExtractPropertyPathFromLambda
    /// (lines 564-568). The compilation may have errors due to accessing a private member,
    /// but the generator should silently skip the invocation.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task PrivatePropertyLambda_GeneratesNoDispatch()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;
                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public class MyViewModel : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;
                                      private string SecretName { get; set; } = "";

                                      public void DoSomething()
                                      {
                                          // Accessing a private property from within the class
                                          var obs = this.WhenChanged(x => x.SecretName);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source);

        // The generator should silently skip private property access and produce no dispatch.
        await result.HasNoGeneratorDiagnostics();
        await result.DoesNotHaveGeneratedSource(WhenChangedDispatchgcsName);
    }

    /// <summary>
    /// Verifies that a lambda accessing a protected property is skipped by the generator.
    /// Exercises the DeclaredAccessibility check (not Public or Internal) in
    /// the extraction helpers.ExtractPropertyPathFromLambda (lines 564-568).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ProtectedPropertyLambda_GeneratesNoDispatch()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;
                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public class MyViewModel : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;
                                      protected string GuardedName { get; set; } = "";

                                      public void DoSomething()
                                      {
                                          // Accessing a protected property from within the class
                                          var obs = this.WhenChanged(x => x.GuardedName);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source);

        // The generator should silently skip protected property access and produce no dispatch.
        await result.HasNoGeneratorDiagnostics();
        await result.DoesNotHaveGeneratedSource(WhenChangedDispatchgcsName);
    }

    /// <summary>
    /// Verifies that a lambda accessing a method (not a property) on the chain is skipped.
    /// Exercises the memberSymbolInfo.Symbol is not IPropertySymbol guard in
    /// the extraction helpers.ExtractPropertyPathFromLambda (line 558-561) for method invocations.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MethodAccessLambda_GeneratesNoDispatch()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;
                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public class MyViewModel : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;
                                      public string Name { get; set; } = "";
                                      public string GetName() => Name;
                                  }

                                  public static class Scenario
                                  {
                                      public static void Execute(MyViewModel vm)
                                      {
                                          // Calling a method in the lambda, not accessing a property
                                          var obs = vm.WhenChanged(x => x.GetName());
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source);

        // The generator should silently skip method invocations in the lambda and produce no dispatch.
        await result.HasNoGeneratorDiagnostics();
        await result.DoesNotHaveGeneratedSource(WhenChangedDispatchgcsName);
    }
}
