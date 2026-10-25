// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>Snapshot tests for WhenChanging (before-change) invocation generation.</summary>
public class WhenChangingGeneratorTests
{
    /// <summary>Verifies that a WhenChanging invocation on an INPC+INPChanging class generates PropertyChanging observation.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_INPC()
    {
        var source = SharedSourceReader.ReadScenario("WhenChanging/SinglePropertyINPC");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(WhenChangingGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies WhenChanging on a ReactiveObject (which implements both INPC and INPChanging).</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_ReactiveObject()
    {
        var source = SharedSourceReader.ReadScenario("WhenChanging/SinglePropertyReactiveObject");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(WhenChangingGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies WhenChanging with two properties returns a tuple.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MultiProperty_TwoProperties()
    {
        var source = SharedSourceReader.ReadScenario("WhenChanging/MultiPropertyTwoProperties");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(WhenChangingGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies WhenChanging with a deep property chain.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepPropertyChain()
    {
        var source = SharedSourceReader.ReadScenario("WhenChanging/DeepPropertyChain");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(WhenChangingGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies WhenChanging with a selector function.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MultiProperty_WithSelector()
    {
        var source = SharedSourceReader.ReadScenario("WhenChanging/MultiPropertyWithSelector");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(WhenChangingGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies that WhenChanging on a type implementing only INotifyPropertyChanging (without INPC)
    /// generates the correct before-change observation code.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task INPChangingOnly_Property()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;

                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public class MyChangingViewModel : INotifyPropertyChanged, INotifyPropertyChanging
                                  {
                                      private string _name = string.Empty;
                                      public event PropertyChangedEventHandler? PropertyChanged;
                                      public event PropertyChangingEventHandler? PropertyChanging;
                                      public string Name
                                      {
                                          get => _name;
                                          set
                                          {
                                              if (_name != value)
                                              {
                                                  PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(Name)));
                                                  _name = value;
                                                  PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
                                              }
                                          }
                                      }
                                  }

                                  public static class Scenario
                                  {
                                      public static IObservable<string> Execute(MyChangingViewModel vm)
                                          => vm.WhenChanging(x => x.Name);
                                  }
                              }
                              """;

        var result =
            await TestHelper.TestPassWithResult(source, typeof(WhenChangingGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies that WhenChanging on a ReactiveObject (which implements both INPC and INPChanging)
    /// generates the correct before-change observation code using inline source.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveObject_Changing_Property()
    {
        const string source = """
                              using System;

                              using ReactiveUI;
                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public class MyReactiveViewModel : ReactiveObject
                                  {
                                      private string _name = string.Empty;
                                      public string Name
                                      {
                                          get => _name;
                                          set => this.RaiseAndSetIfChanged(ref _name, value);
                                      }
                                  }

                                  public static class Scenario
                                  {
                                      public static IObservable<string> Execute(MyReactiveViewModel vm)
                                          => vm.WhenChanging(x => x.Name);
                                  }
                              }
                              """;

        var result =
            await TestHelper.TestPassWithResult(source, typeof(WhenChangingGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies that WhenChanging generates CallerFilePath dispatch when targeting pre-C# 10.
    /// CompilationSucceeds is omitted because the CallerFilePath stub signature is ambiguous
    /// with the runtime extension method in this test harness (both assemblies are referenced).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_INPC_CallerFilePath()
    {
        var source = SharedSourceReader.ReadScenario("WhenChanging/SinglePropertyINPC");
        var result = await TestHelper.TestPassWithResult(
            source,
            typeof(WhenChangingGeneratorTests),
            TestHelper.FallbackLanguageVersion(nullableEnabled: true));
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies WhenChanging with a 4-level deep property chain (Level1 -> Level2 -> Level3 -> Model.Value).
    /// Exercises the deep chain loop in GenerateDeepChainObservation with multiple intermediate segments
    /// using PropertyChangingObservable at each level.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FourLevelDeepChain()
    {
        var source = SharedSourceReader.ReadScenario("WhenChanging/FourLevelDeepChain");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(WhenChangingGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies WhenChanging with three properties returns a tuple.
    /// Exercises GenerateShallowObservableVariable with isBeforeChange=true for 3 properties
    /// combined via CombineLatest.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MultiProperty_ThreeProperties()
    {
        var source = SharedSourceReader.ReadScenario("WhenChanging/MultiPropertyThreeProperties");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(WhenChangingGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies WhenChanging with a mix of deep chain and shallow properties in multi-property observation.
    /// Tests that Address.City uses Select/Switch deep chain pattern with PropertyChangingObservable
    /// while Name uses shallow PropertyChangingObservable, both combined via CombineLatest with a selector.
    /// Exercises GenerateDeepChainVariable with isBeforeChange=true.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MultiProperty_WithDeepChains()
    {
        var source = SharedSourceReader.ReadScenario("WhenChanging/MultiPropertyWithDeepChains");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(WhenChangingGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies that WhenChanging generates CallerFilePath dispatch for deep property chains
    /// when targeting pre-C# 10. Exercises the CallerFilePath dispatch code path for deep chains
    /// with isBeforeChange=true.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepPropertyChain_CallerFilePath()
    {
        var source = SharedSourceReader.ReadScenario("WhenChanging/DeepPropertyChain");
        var result = await TestHelper.TestPassWithResult(
            source,
            typeof(WhenChangingGeneratorTests),
            TestHelper.FallbackLanguageVersion(nullableEnabled: true));
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies that WhenChanging generates CallerFilePath dispatch for multi-property observations
    /// when targeting pre-C# 10. Exercises the CallerFilePath dispatch code path for multi-property
    /// with isBeforeChange=true.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MultiProperty_TwoProperties_CallerFilePath()
    {
        var source = SharedSourceReader.ReadScenario("WhenChanging/MultiPropertyTwoProperties");
        var result = await TestHelper.TestPassWithResult(
            source,
            typeof(WhenChangingGeneratorTests),
            TestHelper.FallbackLanguageVersion(nullableEnabled: true));
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies that WhenChanging generates CallerFilePath dispatch for four-level deep chains
    /// when targeting pre-C# 10. Exercises the CallerFilePath dispatch combined with the deep chain
    /// loop for multiple intermediate segments using PropertyChangingObservable.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FourLevelDeepChain_CallerFilePath()
    {
        var source = SharedSourceReader.ReadScenario("WhenChanging/FourLevelDeepChain");
        var result = await TestHelper.TestPassWithResult(
            source,
            typeof(WhenChangingGeneratorTests),
            TestHelper.FallbackLanguageVersion(nullableEnabled: true));
        await result.HasNoGeneratorDiagnostics();
    }
}
