// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>Generated observations and bindings construct public Primitives operators directly.</summary>
public class PrimitivesOperatorGenerationTests
{
    /// <summary>The generated operator types compile against the consumer's selected runtime.</summary>
    /// <param name="scenario">The shared binding scenario.</param>
    /// <param name="hintName">The generated dispatch file.</param>
    /// <param name="operatorName">The concrete operator required by that scenario.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [Arguments("WhenChanged/MultiPropertyTwoProperties", "WhenChangedDispatch.g.cs", "CombineLatestSignal<")]
    [Arguments("WhenChanged/DeepPropertyChain", "WhenChangedDispatch.g.cs", "UniqueSignal<")]
    [Arguments("WhenAny/SinglePropertyINPC", "WhenAnyDispatch.g.cs", "MapSignal<")]
    [Arguments("WhenAny/MultiPropertyTwoProperties", "WhenAnyDispatch.g.cs", "CombineLatestSignal<")]
    [Arguments("WhenAnyObservable/TwoObservablesMerge", "WhenAnyObservableDispatch.g.cs", "MergeSignal<")]
    [Arguments("WhenAnyObservable/TwoObservablesWithSelector", "WhenAnyObservableDispatch.g.cs", "CombineLatestSignal<")]
    [Arguments("BindOneWay/SinglePropertyWithScheduler", "BindOneWayDispatch.g.cs", "WitnessOnSignal<")]
    public async Task Generate_OperatorScenario_ConstructsConcreteOperator(string scenario, string hintName, string operatorName)
    {
        var source = SharedSourceReader.ReadScenario(scenario);
        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10);

        await result.CompilationSucceeds();
        await Assert.That(result.GeneratedSources[hintName]).Contains(operatorName);
    }

    /// <summary>Wide-arity observations use the allocation-preserving public factory.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Generate_ThreeSources_UsesCombineLatestFactory()
    {
        var source = SharedSourceReader.ReadScenario("WhenChanged/MultiPropertyThreeProperties");
        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10);

        await result.CompilationSucceeds();
        await Assert.That(result.GeneratedSources["WhenChangedDispatch.g.cs"]).Contains("LinqExtensions.CombineLatest(");
    }

    /// <summary>Three observable properties compile as a merged sequence or a projected combination in either runtime.</summary>
    /// <param name="useReactiveRuntime">Whether the consumer references the System.Reactive runtime.</param>
    /// <param name="hasSelector">Whether the consumer combines the latest values with a selector.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [Arguments(false, false)]
    [Arguments(false, true)]
    [Arguments(true, false)]
    [Arguments(true, true)]
    public async Task Generate_ThreeObservableProperties_CompilesMergeAndSelectorOperators(bool useReactiveRuntime, bool hasSelector)
    {
        var runtimeNamespace = useReactiveRuntime ? "ReactiveUI.Binding.Reactive" : "ReactiveUI.Binding";
        var selector = hasSelector ? ", (first, second, third) => first + second + third" : string.Empty;
        var source = $$"""
                       using System;
                       using System.ComponentModel;
                       using {{runtimeNamespace}};

                       namespace TestApp
                       {
                           public class ViewModel : INotifyPropertyChanged
                           {
                               public event PropertyChangedEventHandler? PropertyChanged;
                               public IObservable<int> First { get; set; } = null!;
                               public IObservable<int> Second { get; set; } = null!;
                               public IObservable<int> Third { get; set; } = null!;
                           }

                           public static class Scenario
                           {
                               public static IObservable<int> Observe(ViewModel model) =>
                                   model.WhenAnyObservable(x => x.First, x => x.Second, x => x.Third{{selector}});
                           }
                       }
                       """;
        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10, null, useReactiveRuntime);

        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
        var dispatch = result.GeneratedSources["WhenAnyObservableDispatch.g.cs"];
        await Assert.That(dispatch).Contains(hasSelector
            ? "LinqExtensions.CombineLatest("
            : "MergeSignal<int>(new global::System.IObservable<int>[] {");
        await Assert.That(dispatch).Contains("__switched2");
    }
}
