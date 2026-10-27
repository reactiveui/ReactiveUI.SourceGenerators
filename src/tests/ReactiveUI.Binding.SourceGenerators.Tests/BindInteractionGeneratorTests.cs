// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>Snapshot tests for BindInteraction invocation generation.</summary>
public class BindInteractionGeneratorTests
{
    /// <summary>The <c>BindInteractionDispatch.g.cs</c> name these tests generate against.</summary>
    private const string BindInteractionDispatchgcsName = "BindInteractionDispatch.g.cs";

    /// <summary>The task-handler scenario used by both runtime flavors.</summary>
    private const string TaskHandlerScenario = "BindInteraction/TaskHandler";

    /// <summary>Verifies BindInteraction with a task-based handler.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TaskHandler()
    {
        var source = SharedSourceReader.ReadScenario(TaskHandlerScenario);
        var result =
            await TestHelper.TestPassWithResult(
                source,
                typeof(BindInteractionGeneratorTests),
                LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Generates an interaction binding against the System.Reactive runtime flavor.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TaskHandler_ReactiveRuntime()
    {
        var source = SharedSourceReader.ReadScenario(TaskHandlerScenario)
            .Replace("using ReactiveUI.Binding;", "using ReactiveUI.Binding.Reactive;", StringComparison.Ordinal);
        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10, null, true);

        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
        await result.GeneratedSourceContains(BindInteractionDispatchgcsName, "global::ReactiveUI.Binding.Reactive.IInteraction<");
    }

    /// <summary>Verifies BindInteraction with an observable-based handler.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObservableHandler()
    {
        var source = SharedSourceReader.ReadScenario("BindInteraction/ObservableHandler");
        var result =
            await TestHelper.TestPassWithResult(
                source,
                typeof(BindInteractionGeneratorTests),
                LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies BindInteraction with a deep property path (Child.Confirm).</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepPropertyPath()
    {
        var source = SharedSourceReader.ReadScenario("BindInteraction/DeepPropertyPath");
        var result =
            await TestHelper.TestPassWithResult(
                source,
                typeof(BindInteractionGeneratorTests),
                LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies BindInteraction with a ViewModel that does not implement INPC.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NonINPCViewModel()
    {
        var source = SharedSourceReader.ReadScenario("BindInteraction/NonINPCViewModel");
        var result =
            await TestHelper.TestPassWithResult(
                source,
                typeof(BindInteractionGeneratorTests),
                LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies BindInteraction generates CallerFilePath-based dispatch when targeting pre-C# 10.
    /// CompilationSucceeds is omitted because the CallerFilePath stub signature is ambiguous
    /// with the runtime extension method in this test harness (both assemblies are referenced).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CallerFilePathFallback()
    {
        var source = SharedSourceReader.ReadScenario(TaskHandlerScenario);
        var result = await TestHelper.TestPassWithResult(
            source,
            typeof(BindInteractionGeneratorTests),
            TestHelper.FallbackLanguageVersion(nullableEnabled: true));
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies BindInteraction generates CallerFilePath-based dispatch for ObservableHandler.
    /// Covers the CallerFilePath overload branch with observable handler type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CallerFilePathFallback_ObservableHandler()
    {
        var source = SharedSourceReader.ReadScenario("BindInteraction/ObservableHandler");
        var result = await TestHelper.TestPassWithResult(
            source,
            typeof(BindInteractionGeneratorTests),
            TestHelper.FallbackLanguageVersion(nullableEnabled: true));
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies BindInteraction generates CallerFilePath-based dispatch for DeepPropertyPath.
    /// Covers the deep path observation branch in the CallerFilePath codepath.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CallerFilePathFallback_DeepPropertyPath()
    {
        var source = SharedSourceReader.ReadScenario("BindInteraction/DeepPropertyPath");
        var result = await TestHelper.TestPassWithResult(
            source,
            typeof(BindInteractionGeneratorTests),
            TestHelper.FallbackLanguageVersion(nullableEnabled: true));
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies BindInteraction generates CallerFilePath-based dispatch for NonINPCViewModel.
    /// Covers the non-INPC return-observable branch in the CallerFilePath codepath.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CallerFilePathFallback_NonINPCViewModel()
    {
        var source = SharedSourceReader.ReadScenario("BindInteraction/NonINPCViewModel");
        var result = await TestHelper.TestPassWithResult(
            source,
            typeof(BindInteractionGeneratorTests),
            TestHelper.FallbackLanguageVersion(nullableEnabled: true));
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>A BindInteraction whose property argument is not a lambda resolves no interaction types.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindInteraction_PropertyArgumentNotALambda_GeneratesNoDispatch()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;
                              using System.Linq.Expressions;
                              using System.Threading.Tasks;
                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public class MyViewModel : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;

                                      public object? Confirm { get; set; }
                                  }

                                  public class MyView : IViewFor
                                  {
                                      public object? ViewModel { get; set; }
                                  }

                                  public static class Scenario
                                  {
                                      public static void Execute(MyView view, MyViewModel vm, Expression<Func<MyViewModel, object?>> property)
                                      {
                                          view.BindInteraction(vm, property, x => Task.CompletedTask);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10);
        await result.DoesNotHaveGeneratedSource(BindInteractionDispatchgcsName);
    }

    /// <summary>A BindInteraction whose property lambda has a statement body resolves no interaction types.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindInteraction_PropertyLambdaWithStatementBody_GeneratesNoDispatch()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;
                              using System.Threading.Tasks;
                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public class MyViewModel : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;

                                      public object? Confirm { get; set; }
                                  }

                                  public class MyView : IViewFor
                                  {
                                      public object? ViewModel { get; set; }
                                  }

                                  public static class Scenario
                                  {
                                      public static void Execute(MyView view, MyViewModel vm)
                                      {
                                          view.BindInteraction(vm, x => { throw new NotSupportedException(); }, x => Task.CompletedTask);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10);
        await result.DoesNotHaveGeneratedSource(BindInteractionDispatchgcsName);
    }
}
