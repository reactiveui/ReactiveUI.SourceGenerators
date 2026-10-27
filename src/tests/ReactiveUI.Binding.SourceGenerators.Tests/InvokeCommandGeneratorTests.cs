// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>Snapshot tests for InvokeCommand (stream-driven command execution) invocation generation.</summary>
public class InvokeCommandGeneratorTests
{
    /// <summary>The <c>InvokeCommandDispatch.g.cs</c> name these tests generate against.</summary>
    private const string InvokeCommandDispatchgcsName = "InvokeCommandDispatch.g.cs";

    /// <summary>Verifies InvokeCommand observing a command property on the target.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CommandProperty()
    {
        var source = SharedSourceReader.ReadScenario("InvokeCommand/CommandProperty");
        var result = await TestHelper.TestPassWithResult(
            source,
            typeof(InvokeCommandGeneratorTests),
            LanguageVersion.CSharp10);

        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
        await result.HasGeneratedSource(InvokeCommandDispatchgcsName);
    }

    /// <summary>Verifies InvokeCommand reaching the command through a property chain.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepCommandPath()
    {
        var source = SharedSourceReader.ReadScenario("InvokeCommand/DeepCommandPath");
        var result = await TestHelper.TestPassWithResult(
            source,
            typeof(InvokeCommandGeneratorTests),
            LanguageVersion.CSharp10);

        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies InvokeCommand dispatches on file and line when the consumer predates C# 10.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CommandProperty_CallerFilePath()
    {
        var source = SharedSourceReader.ReadScenario("InvokeCommand/CommandProperty");
        var result = await TestHelper.TestPassWithResult(
            source,
            typeof(InvokeCommandGeneratorTests),
            LanguageVersion.CSharp7_3);

        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>A call to somebody else's <c>InvokeCommand</c> is not one of ours to generate for.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CustomExtension_GeneratesNoDispatch()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;
                              using System.Linq.Expressions;
                              using System.Windows.Input;

                              namespace TestApp
                              {
                                  public class MyViewModel : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;

                                      public ICommand? Save { get; set; }
                                  }

                                  public static class CustomExtensions
                                  {
                                      public static IDisposable InvokeCommand<T, TTarget>(
                                          this IObservable<T> source,
                                          TTarget target,
                                          Expression<Func<TTarget, ICommand?>> commandProperty) => throw new NotImplementedException();
                                  }

                                  public static class Scenario
                                  {
                                      public static IDisposable Execute(IObservable<string> values, MyViewModel vm)
                                      {
                                          return values.InvokeCommand(vm, x => x.Save);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10);

        await result.HasNoGeneratorDiagnostics();
        await result.DoesNotHaveGeneratedSource(InvokeCommandDispatchgcsName);
    }

    /// <summary>A call the model could not resolve names no method to read arguments against.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UnresolvedCall_GeneratesNoDispatch()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;
                              using System.Windows.Input;
                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public class MyViewModel : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;

                                      public ICommand? Save { get; set; }
                                  }

                                  public static class Scenario
                                  {
                                      public static void Execute(MyViewModel vm)
                                      {
                                          undefinedValues.InvokeCommand(vm, x => x.Save);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10);

        await result.HasNoGeneratorDiagnostics();
        await result.DoesNotHaveGeneratedSource(InvokeCommandDispatchgcsName);
    }

    /// <summary>A target typed as a type parameter generates no dispatch.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenericTargetParameter_GeneratesNoDispatch()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;
                              using System.Windows.Input;
                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public interface IHasCommand : INotifyPropertyChanged
                                  {
                                      ICommand? Save { get; }
                                  }

                                  public static class Scenario
                                  {
                                      public static IDisposable Execute<TTarget>(IObservable<string> values, TTarget target)
                                          where TTarget : class, IHasCommand
                                      {
                                          return values.InvokeCommand(target, x => x.Save);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10);

        await result.HasNoGeneratorDiagnostics();
        await result.CompilationSucceeds();
        await result.DoesNotHaveGeneratedSource(InvokeCommandDispatchgcsName);
    }

    /// <summary>A selector whose body is no property path is left to the stub, which throws.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SelectorWithoutAPropertyPath_GeneratesNoDispatch()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;
                              using System.Windows.Input;
                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public class MyViewModel : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;

                                      public ICommand? Save { get; set; }

                                      public ICommand? Resolve() => Save;
                                  }

                                  public static class Scenario
                                  {
                                      public static IDisposable Execute(IObservable<string> values, MyViewModel vm)
                                      {
                                          return values.InvokeCommand(vm, x => x.Resolve());
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10);

        await result.HasNoGeneratorDiagnostics();
        await result.CompilationSucceeds();
        await result.DoesNotHaveGeneratedSource(InvokeCommandDispatchgcsName);
    }

    /// <summary>A receiver that is no stream of values has nothing to drive an execution.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NonObservableReceiver_GeneratesNoDispatch()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;

                              namespace TestApp
                              {
                                  public class MyViewModel : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;

                                      public string Caption { get; set; } = "";
                                  }

                                  public static class ReactiveUIBindingExtensions
                                  {
                                      public static IDisposable InvokeCommand(this MyViewModel source, object target, object commandProperty) => null!;
                                  }

                                  public static class Scenario
                                  {
                                      public static void Execute(MyViewModel vm)
                                      {
                                          vm.InvokeCommand(vm, vm);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source);

        await result.HasNoGeneratorDiagnostics();
        await result.DoesNotHaveGeneratedSource(InvokeCommandDispatchgcsName);
    }

    /// <summary>A selector held in a variable names no path to read, so the call is left to the stub, which throws.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SelectorFromAVariable_GeneratesNoDispatch()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;
                              using System.Linq.Expressions;
                              using System.Windows.Input;
                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public class MyViewModel : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;

                                      public ICommand? Save { get; set; }
                                  }

                                  public static class Scenario
                                  {
                                      public static IDisposable Execute(IObservable<string> values, MyViewModel vm)
                                      {
                                          Expression<Func<MyViewModel, ICommand?>> selector = x => x.Save;
                                          return values.InvokeCommand(vm, selector);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10);

        await result.HasNoGeneratorDiagnostics();
        await result.DoesNotHaveGeneratedSource(InvokeCommandDispatchgcsName);
    }

    /// <summary>Two call sites spelling the same selector share one generated worker.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TwoCallSitesSharingASelector_ShareOneWorker()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;
                              using System.Windows.Input;
                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public class MyViewModel : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;

                                      public ICommand? Save { get; set; }
                                  }

                                  public static class Scenario
                                  {
                                      public static IDisposable First(IObservable<string> values, MyViewModel vm)
                                      {
                                          return values.InvokeCommand(vm, x => x.Save);
                                      }

                                      public static IDisposable Second(IObservable<string> values, MyViewModel vm)
                                      {
                                          return values.InvokeCommand(vm, x => x.Save);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10);

        await result.HasNoGeneratorDiagnostics();
        await result.CompilationSucceeds();

        var dispatch = result.GeneratedSources[InvokeCommandDispatchgcsName];
        var workers = dispatch.Split("private static global::System.IDisposable __InvokeCommand_").Length - 1;

        await Assert.That(workers).IsEqualTo(1);
    }

    /// <summary>The overload taking the command itself generates no dispatch.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CommandArgument_GeneratesNoDispatch()
    {
        const string source = """
                              using System;
                              using System.Windows.Input;
                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public static class Scenario
                                  {
                                      public static IDisposable Execute(IObservable<string> values, ICommand command)
                                      {
                                          return values.InvokeCommand(command);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10);

        await result.HasNoGeneratorDiagnostics();
        await result.CompilationSucceeds();
        await result.DoesNotHaveGeneratedSource(InvokeCommandDispatchgcsName);
    }
}
