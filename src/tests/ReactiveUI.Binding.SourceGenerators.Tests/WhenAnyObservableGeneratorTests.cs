// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>Snapshot tests for WhenAnyObservable (Switch/Merge/CombineLatest) invocation generation.</summary>
public class WhenAnyObservableGeneratorTests
{
    /// <summary>Verifies WhenAnyObservable with a single observable property (Switch pattern).</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleObservable()
    {
        var source = SharedSourceReader.ReadScenario("WhenAnyObservable/SingleObservable");
        var result = await TestHelper.TestPassWithResult(
            source,
            typeof(WhenAnyObservableGeneratorTests),
            LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies WhenAnyObservable with two same-type observable properties (Merge pattern).</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TwoObservables_Merge()
    {
        var source = SharedSourceReader.ReadScenario("WhenAnyObservable/TwoObservablesMerge");
        var result = await TestHelper.TestPassWithResult(
            source,
            typeof(WhenAnyObservableGeneratorTests),
            LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies WhenAnyObservable with two different-type observable properties and a selector (CombineLatest pattern).</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TwoObservables_WithSelector()
    {
        var source = SharedSourceReader.ReadScenario("WhenAnyObservable/TwoObservablesWithSelector");
        var result = await TestHelper.TestPassWithResult(
            source,
            typeof(WhenAnyObservableGeneratorTests),
            LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies that WhenAnyObservable generates CallerFilePath dispatch when targeting pre-C# 10.
    /// CompilationSucceeds is omitted because the CallerFilePath stub signature is ambiguous
    /// with the runtime extension method in this test harness (both assemblies are referenced).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleObservable_CallerFilePath()
    {
        var source = SharedSourceReader.ReadScenario("WhenAnyObservable/SingleObservable");
        var result = await TestHelper.TestPassWithResult(
            source,
            typeof(WhenAnyObservableGeneratorTests),
            TestHelper.FallbackLanguageVersion(nullableEnabled: true));
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies WhenAnyObservable with a deep property chain in the Switch pattern (x => x.Child.MyCommand).</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleObservable_DeepChain()
    {
        var source = SharedSourceReader.ReadScenario("WhenAnyObservable/DeepObservableSwitch");
        var result = await TestHelper.TestPassWithResult(
            source,
            typeof(WhenAnyObservableGeneratorTests),
            LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies WhenAnyObservable with deep property chains in a Merge pattern.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TwoObservables_DeepChain_Merge()
    {
        var source = SharedSourceReader.ReadScenario("WhenAnyObservable/DeepObservableMerge");
        var result = await TestHelper.TestPassWithResult(
            source,
            typeof(WhenAnyObservableGeneratorTests),
            LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies WhenAnyObservable with deep property chains in a CombineLatest pattern with selector.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TwoObservables_DeepChain_CombineLatest()
    {
        var source = SharedSourceReader.ReadScenario("WhenAnyObservable/DeepObservableCombineLatest");
        var result = await TestHelper.TestPassWithResult(
            source,
            typeof(WhenAnyObservableGeneratorTests),
            LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies WhenAnyObservable with multiple invocations sharing the same type signature to cover the else-if dispatch branch.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MultipleInvocations_SameTypeSignature()
    {
        var source = SharedSourceReader.ReadScenario("WhenAnyObservable/MultipleInvocationsSameType");
        var result = await TestHelper.TestPassWithResult(
            source,
            typeof(WhenAnyObservableGeneratorTests),
            LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies that a method named "WhenAnyObservable" on a non-ReactiveUI extension class is ignored
    /// by the generator. Exercises the extension class name mismatch guard in
    /// the extraction helpers.ExtractWhenAnyObservableInvocation (lines 183-188).
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
                                      public IObservable<string>? Items { get; set; }
                                  }

                                  public static class CustomExtensions
                                  {
                                      public static IObservable<T> WhenAnyObservable<TSender, T>(
                                          this TSender sender,
                                          Func<TSender, IObservable<T>?> observable)
                                          => null!;
                                  }

                                  public static class Scenario
                                  {
                                      public static void Execute(MyViewModel vm)
                                      {
                                          // This calls CustomExtensions.WhenAnyObservable, NOT our extension class
                                          var obs = vm.WhenAnyObservable(x => x.Items);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source);

        // The generator should not produce any WhenAnyObservable dispatch code
        // because the method belongs to CustomExtensions, not our extension class.
        await result.HasNoGeneratorDiagnostics();
        await result.DoesNotHaveGeneratedSource("WhenAnyObservableDispatch.g.cs");
    }
}
