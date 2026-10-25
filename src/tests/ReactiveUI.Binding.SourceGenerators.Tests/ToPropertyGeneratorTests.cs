// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>Snapshot tests for ToProperty (observable-backed read-only property) invocation generation.</summary>
public class ToPropertyGeneratorTests
{
    /// <summary>The <c>ToPropertyDispatch.g.cs</c> name these tests generate against.</summary>
    private const string ToPropertyDispatchName = "ToPropertyDispatch.g.cs";

    /// <summary>A partial view model with its own field-like event is raised through an accessor the generator adds.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task PartialEvent() => VerifyScenario("ToProperty/PartialEvent");

    /// <summary>A partial view model is raised through its base class's protected event-args methods.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task PartialProtectedBase() => VerifyScenario("ToProperty/PartialProtectedBase");

    /// <summary>A view model that is not partial is raised through its public raise method.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task PublicRaiseMethod() => VerifyScenario("ToProperty/PublicRaiseMethod");

    /// <summary>A ReactiveUI object is raised through ReactiveUI's public raise extensions.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task ReactiveObject() => VerifyScenario("ToProperty/ReactiveObject");

    /// <summary>A property named by <c>nameof</c> dispatches on the argument's text.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NameOf() => VerifyScenario("ToProperty/NameOf");

    /// <summary>An initial value, deferred subscription and scheduler are forwarded to the helper.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task InitialValueDeferScheduler() => VerifyScenario("ToProperty/InitialValueDeferScheduler");

    /// <summary>An initial value factory is forwarded and the helper is also returned through the out parameter.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task FactoryOutResult() => VerifyScenario("ToProperty/FactoryOutResult");

    /// <summary>Below C# 10 the overload dispatches on the caller's file and line.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task PartialEvent_CallerFilePath()
    {
        var source = SharedSourceReader.ReadScenario("ToProperty/PartialEvent");
        var result = await TestHelper.TestPassWithResult(
            source,
            typeof(ToPropertyGeneratorTests),
            TestHelper.FallbackLanguageVersion(nullableEnabled: true));

        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>A type whose notifications generated code cannot raise produces no dispatch, so the stub throws.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UnraisableType_GeneratesNoDispatch()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;
                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public class MyViewModel : INotifyPropertyChanged
                                  {
                                      private readonly ObservableAsPropertyHelper<string> _name;

                                      public MyViewModel(IObservable<string> names) => _name = names.ToProperty(this, x => x.Name);

                                      public event PropertyChangedEventHandler PropertyChanged;

                                      public string Name => _name.Value;

                                      protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10);
        await result.DoesNotHaveGeneratedSource(ToPropertyDispatchName);
    }

    /// <summary>A selector that reaches past the source object names no property of it and produces no dispatch.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepSelector_GeneratesNoDispatch()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;
                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public partial class MyViewModel : INotifyPropertyChanged
                                  {
                                      public MyViewModel(IObservable<int> lengths) => lengths.ToProperty(this, x => x.Name.Length);

                                      public event PropertyChangedEventHandler PropertyChanged;

                                      public string Name { get; set; } = "";
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10);
        await result.DoesNotHaveGeneratedSource(ToPropertyDispatchName);
    }

    /// <summary>A ToProperty call on a custom extension class is not one of ours and is skipped.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CustomExtension_GeneratesNoDispatch()
    {
        const string source = """
                              using System;

                              namespace TestApp
                              {
                                  public partial class MyViewModel : System.ComponentModel.INotifyPropertyChanged
                                  {
                                      public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

                                      public string Name { get; set; } = "";
                                  }

                                  public static class CustomExtensions
                                  {
                                      public static object ToProperty<TObj, TRet>(
                                          this IObservable<TRet> target,
                                          TObj source,
                                          System.Linq.Expressions.Expression<Func<TObj, TRet>> property) => null;
                                  }

                                  public static class Scenario
                                  {
                                      public static void Execute(IObservable<string> names, MyViewModel vm) => names.ToProperty(vm, x => x.Name);
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10);
        await result.DoesNotHaveGeneratedSource(ToPropertyDispatchName);
    }

    /// <summary>Runs a shared scenario at C# 10, verifies its snapshots and compiles the result.</summary>
    /// <param name="scenarioPath">The scenario path under SharedScenarios.</param>
    /// <param name="testName">The calling test, which names its snapshots.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    private static async Task VerifyScenario(string scenarioPath, [CallerMemberName] string testName = "")
    {
        var source = SharedSourceReader.ReadScenario(scenarioPath);
        var result = await TestHelper.TestPassWithResult(source, typeof(ToPropertyGeneratorTests), LanguageVersion.CSharp10, testName);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }
}
