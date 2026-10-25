// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>Snapshot tests for BindOneWay (one-way binding) invocation generation.</summary>
public class BindOneWayGeneratorTests
{
    /// <summary>The <c>BindOneWayDispatch.g.cs</c> name these tests generate against.</summary>
    private const string BindOneWayDispatchgcsName = "BindOneWayDispatch.g.cs";

    /// <summary>Verifies BindOneWay with same-type string property binding.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_StringToString()
    {
        var source = SharedSourceReader.ReadScenario("BindOneWay/SinglePropertyStringToString");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(BindOneWayGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies BindOneWay with int property binding.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_IntToInt()
    {
        var source = SharedSourceReader.ReadScenario("BindOneWay/SinglePropertyIntToInt");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(BindOneWayGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies BindOneWay with multiple bindings on the same source/target pair.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MultipleBindings()
    {
        var source = SharedSourceReader.ReadScenario("BindOneWay/MultipleBindings");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(BindOneWayGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies BindOneWay on a ReactiveObject-based ViewModel.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveObject_Source()
    {
        var source = SharedSourceReader.ReadScenario("BindOneWay/ReactiveObjectSource");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(BindOneWayGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies BindOneWay with a conversion function from int to string.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_WithConverter()
    {
        var source = SharedSourceReader.ReadScenario("BindOneWay/SinglePropertyWithConverter");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(BindOneWayGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies BindOneWay with a scheduler parameter.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_WithScheduler()
    {
        var source = SharedSourceReader.ReadScenario("BindOneWay/SinglePropertyWithScheduler");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(BindOneWayGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies that BindOneWay generates CallerFilePath dispatch when targeting pre-C# 10.
    /// CompilationSucceeds is omitted because the CallerFilePath stub signature is ambiguous
    /// with the runtime extension method in this test harness (both assemblies are referenced).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_StringToString_CallerFilePath()
    {
        var source = SharedSourceReader.ReadScenario("BindOneWay/SinglePropertyStringToString");
        var result = await TestHelper.TestPassWithResult(
            source,
            typeof(BindOneWayGeneratorTests),
            TestHelper.FallbackLanguageVersion(nullableEnabled: true));
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies BindOneWay with both a conversion function and a scheduler parameter.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_WithConverterAndScheduler()
    {
        var source = SharedSourceReader.ReadScenario("BindOneWay/SinglePropertyWithConverterAndScheduler");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(BindOneWayGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies BindOneWay with multiple bindings sharing the same type signature to cover the else-if dispatch branch.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MultipleSameTypeBindings()
    {
        var source = SharedSourceReader.ReadScenario("BindOneWay/MultipleSameTypeBindings");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(BindOneWayGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies that BindOneWay with multiple same-type bindings generates CallerFilePath dispatch when targeting pre-C# 10.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MultipleSameTypeBindings_CallerFilePath()
    {
        var source = SharedSourceReader.ReadScenario("BindOneWay/MultipleSameTypeBindings");
        var result = await TestHelper.TestPassWithResult(
            source,
            typeof(BindOneWayGeneratorTests),
            TestHelper.FallbackLanguageVersion(nullableEnabled: true));
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies that a method named "BindOneWay" on a non-ReactiveUI extension class is ignored
    /// by the generator. Exercises the extension class name mismatch guard in
    /// the extraction helpers.ExtractBindInvocation (lines 297-302).
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

                                  public class MyView : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;
                                      public string NameText { get; set; } = "";
                                  }

                                  public static class CustomBindingExtensions
                                  {
                                      public static IDisposable BindOneWay<TSource, TTarget>(
                                          this TSource source,
                                          TTarget target,
                                          Func<TSource, string> sourceProp,
                                          Func<TTarget, string> targetProp)
                                          => null!;
                                  }

                                  public static class Scenario
                                  {
                                      public static void Execute(MyViewModel vm, MyView view)
                                      {
                                          // This calls CustomBindingExtensions.BindOneWay, NOT our extension class
                                          var binding = vm.BindOneWay(view, x => x.Name, x => x.NameText);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source);

        // The generator should not produce any BindOneWay dispatch code
        // because the method belongs to CustomBindingExtensions, not our extension class.
        await result.HasNoGeneratorDiagnostics();
        await result.DoesNotHaveGeneratedSource(BindOneWayDispatchgcsName);
    }

    /// <summary>
    /// Verifies that BindOneWay with an identity lambda (x => x) as the source property
    /// produces no dispatch code. Exercises the segments.Count == 0 guard in
    /// the extraction helpers.ExtractPropertyPathFromLambda (line 578-581) for bind invocations.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IdentityLambda_Source_GeneratesNoDispatch()
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

                                  public class MyView : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;
                                      public string NameText { get; set; } = "";
                                  }

                                  public static class Scenario
                                  {
                                      public static void Execute(MyViewModel vm, MyView view)
                                      {
                                          // Identity lambda for source property — no member access
                                          var binding = vm.BindOneWay(view, x => x, x => x.NameText);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source);

        // The generator should silently skip the identity lambda and produce no dispatch.
        await result.HasNoGeneratorDiagnostics();
        await result.DoesNotHaveGeneratedSource(BindOneWayDispatchgcsName);
    }

    /// <summary>
    /// Verifies that BindOneWay with a block-body lambda for the source property
    /// produces no dispatch code. Exercises the body == null guard in
    /// the extraction helpers.ExtractPropertyPathFromLambda (line 543-546) for bind invocations.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BlockBodyLambda_Source_GeneratesNoDispatch()
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

                                  public class MyView : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;
                                      public string NameText { get; set; } = "";
                                  }

                                  public static class Scenario
                                  {
                                      public static void Execute(MyViewModel vm, MyView view)
                                      {
                                          // Block body lambda for source property
                                          var binding = vm.BindOneWay(view, x => { return x.Name; }, x => x.NameText);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source);

        // The generator should silently skip block-body lambdas and produce no dispatch.
        await result.HasNoGeneratorDiagnostics();
        await result.DoesNotHaveGeneratedSource(BindOneWayDispatchgcsName);
    }

    /// <summary>Verifies that BindOneWay with a source path ending at a read-only field produces no dispatch code.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReadOnlyLeafFieldLambda_Source_GeneratesNoDispatch()
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

                                  public class MyView : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;
                                      public string NameText { get; set; } = "";
                                  }

                                  public static class Scenario
                                  {
                                      public static void Execute(MyViewModel vm, MyView view)
                                      {
                                          // Source path ends at a read-only field
                                          var binding = vm.BindOneWay(view, x => x._name, x => x.NameText);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source);

        // The generator should silently skip field access lambdas and produce no dispatch.
        await result.HasNoGeneratorDiagnostics();
        await result.DoesNotHaveGeneratedSource(BindOneWayDispatchgcsName);
    }

    /// <summary>
    /// Verifies BindOneWay from an NSObject source, whose observation instantiates the KVO helper
    /// classes. Those helpers are declared in a file of their own, so a compilation that reaches them
    /// only through a binding - never through WhenChanged - still compiles.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task KVO_NSObject_Source()
    {
        var source = ApplePlatformSource.BindingScenario("view.BindOneWay(vm, x => x.Text, x => x.Name)");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(BindOneWayGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
        await result.GeneratedSourceContains("BindOneWayDispatch.g.cs", ApplePlatformSource.KvoObservable);
    }
}
