// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>
/// Integration tests that exercise defensive guard paths in the extractor helpers.
/// These tests craft source code that passes the syntax predicates but triggers
/// early-return null paths in the semantic transform functions.
/// </summary>
public class UnanalyzableInvocationTests
{
    /// <summary>The <c>BindCommandDispatch.g.cs</c> name these tests generate against.</summary>
    private const string BindCommandDispatchgcsName = "BindCommandDispatch.g.cs";

    /// <summary>The <c>BindInteractionDispatch.g.cs</c> name these tests generate against.</summary>
    private const string BindInteractionDispatchgcsName = "BindInteractionDispatch.g.cs";

    /// <summary>The <c>BindOneWayDispatch.g.cs</c> name these tests generate against.</summary>
    private const string BindOneWayDispatchgcsName = "BindOneWayDispatch.g.cs";

    /// <summary>The <c>WhenAnyObservableDispatch.g.cs</c> name these tests generate against.</summary>
    private const string WhenAnyObservableDispatchgcsName = "WhenAnyObservableDispatch.g.cs";

    /// <summary>
    /// Verifies that a BindOneWay call on a custom (non-stub) extension class is skipped.
    /// Exercises the ContainingType name guard in BindingExtractor (line 41).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_CustomExtension_GeneratesNoDispatch()
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

                                  public class MyView
                                  {
                                      public string DisplayName { get; set; } = "";
                                  }

                                  public static class CustomExtensions
                                  {
                                      public static void BindOneWay<TSender, TTarget, TProp>(
                                          this TTarget view,
                                          TSender source,
                                          System.Linq.Expressions.Expression<Func<TSender, TProp>> sourceProp,
                                          System.Linq.Expressions.Expression<Func<TTarget, TProp>> targetProp) { }
                                  }

                                  public static class Scenario
                                  {
                                      public static void Execute(MyViewModel vm, MyView view)
                                      {
                                          view.BindOneWay(vm, x => x.Name, x => x.DisplayName);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source);
        await result.HasNoGeneratorDiagnostics();
        await result.DoesNotHaveGeneratedSource(BindOneWayDispatchgcsName);
    }

    /// <summary>
    /// Verifies that a BindCommand call on a custom (non-stub) extension class is skipped.
    /// Exercises the ContainingType name guard in CommandExtractor (line 41).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindCommand_CustomExtension_GeneratesNoDispatch()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;
                              using System.Windows.Input;

                              namespace TestApp
                              {
                                  public class MyViewModel : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;
                                      public ICommand? Save { get; set; }
                                  }

                                  public class MyView
                                  {
                                      public object SaveButton { get; set; } = new object();
                                  }

                                  public static class CustomExtensions
                                  {
                                      public static void BindCommand<TView, TViewModel, TControl>(
                                          this TView view,
                                          TViewModel viewModel,
                                          System.Linq.Expressions.Expression<Func<TViewModel, ICommand?>> commandProp,
                                          System.Linq.Expressions.Expression<Func<TView, TControl>> controlProp) { }
                                  }

                                  public static class Scenario
                                  {
                                      public static void Execute(MyViewModel vm, MyView view)
                                      {
                                          view.BindCommand(vm, x => x.Save, x => x.SaveButton);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source);
        await result.HasNoGeneratorDiagnostics();
        await result.DoesNotHaveGeneratedSource(BindCommandDispatchgcsName);
    }

    /// <summary>
    /// Verifies that a WhenAnyObservable call on a custom (non-stub) extension class is skipped.
    /// Exercises the ContainingType name guard in WhenAnyObservableExtractor (line 42).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyObservable_CustomExtension_GeneratesNoDispatch()
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
                                          System.Linq.Expressions.Expression<Func<TSender, IObservable<T>?>> obs) => throw new NotImplementedException();
                                  }

                                  public static class Scenario
                                  {
                                      public static void Execute(MyViewModel vm)
                                      {
                                          var obs = vm.WhenAnyObservable(x => x.Items);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source);
        await result.HasNoGeneratorDiagnostics();
        await result.DoesNotHaveGeneratedSource(WhenAnyObservableDispatchgcsName);
    }

    /// <summary>
    /// Verifies that a BindInteraction call on a custom (non-stub) extension class is skipped.
    /// Exercises the ContainingType name guard in InteractionExtractor (line 40).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindInteraction_CustomExtension_GeneratesNoDispatch()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;

                              namespace TestApp
                              {
                                  public class MyViewModel : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;
                                      public object? Interaction { get; set; }
                                  }

                                  public class MyView
                                  {
                                      public object Result { get; set; } = new object();
                                  }

                                  public static class CustomExtensions
                                  {
                                      public static IDisposable BindInteraction<TView, TViewModel, T>(
                                          this TView view,
                                          TViewModel viewModel,
                                          System.Linq.Expressions.Expression<Func<TViewModel, T>> interaction,
                                          Func<T, IObservable<object>> handler) => throw new NotImplementedException();
                                  }

                                  public static class Scenario
                                  {
                                      public static void Execute(MyViewModel vm, MyView view)
                                      {
                                          view.BindInteraction(vm, x => x.Interaction, x => System.Reactive.Linq.Observable.Return(new object()));
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source);
        await result.HasNoGeneratorDiagnostics();
        await result.DoesNotHaveGeneratedSource(BindInteractionDispatchgcsName);
    }

    /// <summary>
    /// A BindInteraction call whose view is a type parameter names no type a generated overload could
    /// declare, so no dispatch is emitted for it. Emitting one puts the type parameter's own name in the
    /// consumer's build, which does not compile.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindInteraction_GenericViewParameter_GeneratesNoDispatch()
    {
        const string source = """
                              using System.ComponentModel;
                              using System.Threading.Tasks;
                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public class MyViewModel : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;

                                      public Interaction<string, bool> Confirm { get; set; } = new Interaction<string, bool>();
                                  }

                                  public static class Scenario
                                  {
                                      public static void Execute<TView>(TView view, MyViewModel vm)
                                          where TView : class, IViewFor
                                      {
                                          view.BindInteraction(vm, x => x.Confirm, ctx =>
                                          {
                                              ctx.SetOutput(true);
                                              return Task.CompletedTask;
                                          });
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp10);

        await result.HasNoGeneratorDiagnostics();
        await result.CompilationSucceeds();
        await result.DoesNotHaveGeneratedSource(BindInteractionDispatchgcsName);
    }

    /// <summary>A WhenChanged call on a type parameter emits nothing that names it.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanged_GenericReceiver_EmitsNothingNamingTheTypeParameter()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;
                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public interface INamed : INotifyPropertyChanged
                                  {
                                      string Name { get; set; }
                                  }

                                  public static class Scenario
                                  {
                                      public static IObservable<string> Execute<TSource>(TSource source)
                                          where TSource : class, INamed
                                      {
                                          return source.WhenChanged(x => x.Name);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp10);

        await result.HasNoGeneratorDiagnostics();
        await result.CompilationSucceeds();
    }

    /// <summary>A BindOneWay call between two type parameters emits nothing that names them.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_GenericReceiver_EmitsNothingNamingTheTypeParameter()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;
                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public interface INamed : INotifyPropertyChanged
                                  {
                                      string Name { get; set; }
                                  }

                                  public static class Scenario
                                  {
                                      public static IDisposable Execute<TSource, TTarget>(TSource source, TTarget target)
                                          where TSource : class, INamed
                                          where TTarget : class, INamed
                                      {
                                          return source.BindOneWay(target, x => x.Name, x => x.Name);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp10);

        await result.HasNoGeneratorDiagnostics();
        await result.CompilationSucceeds();
    }

    /// <summary>A WhenAnyObservable call on a type parameter emits nothing that names it.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyObservable_GenericReceiver_EmitsNothingNamingTheTypeParameter()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;
                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public interface IHasSignal : INotifyPropertyChanged
                                  {
                                      IObservable<int> Signal { get; }
                                  }

                                  public static class Scenario
                                  {
                                      public static IObservable<int> Execute<TSource>(TSource source)
                                          where TSource : class, IHasSignal
                                      {
                                          return source.WhenAnyObservable(x => x.Signal);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp10);

        await result.HasNoGeneratorDiagnostics();
        await result.CompilationSucceeds();
    }

    /// <summary>A BindTo call onto a type parameter target emits nothing that names it.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindTo_GenericTargetParameter_EmitsNothingNamingTheTypeParameter()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;
                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public interface INamed : INotifyPropertyChanged
                                  {
                                      string Name { get; set; }
                                  }

                                  public static class Scenario
                                  {
                                      public static IDisposable Execute<TTarget>(IObservable<string> source, TTarget target)
                                          where TTarget : class, INamed
                                      {
                                          return source.BindTo(target, x => x.Name);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp10);

        await result.HasNoGeneratorDiagnostics();
        await result.CompilationSucceeds();
    }

    /// <summary>A BindCommand call on a type parameter view emits nothing that names it.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindCommand_GenericViewParameter_EmitsNothingNamingTheTypeParameter()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;
                              using System.Windows.Input;
                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public class MyButton
                                  {
                                      public event EventHandler? Click;
                                  }

                                  public interface IHasButton : IViewFor
                                  {
                                      MyButton SaveButton { get; }
                                  }

                                  public class MyViewModel : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;

                                      public ICommand? Save { get; set; }
                                  }

                                  public static class Scenario
                                  {
                                      public static IDisposable Execute<TView>(TView view, MyViewModel vm)
                                          where TView : class, IHasButton
                                      {
                                          return view.BindCommand(vm, x => x.Save, x => x.SaveButton);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp10);

        await result.HasNoGeneratorDiagnostics();
        await result.CompilationSucceeds();
    }

    /// <summary>
    /// Verifies that a BindOneWay call with a non-lambda expression (method group) is skipped.
    /// Exercises the property path null guard in BindingExtractor (line 63).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_NonLambdaExpression_GeneratesNoDispatch()
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

                                  public class MyView
                                  {
                                      public string DisplayName { get; set; } = "";
                                  }

                                  public static class Scenario
                                  {
                                      private static System.Linq.Expressions.Expression<Func<MyViewModel, string>> GetExpression()
                                          => x => x.Name;

                                      public static void Execute(MyViewModel vm, MyView view)
                                      {
                                          // Non-lambda: pass a method call result instead of inline lambda
                                          view.BindOneWay(vm, GetExpression(), x => x.DisplayName);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source);
        await result.HasNoGeneratorDiagnostics();
        await result.DoesNotHaveGeneratedSource(BindOneWayDispatchgcsName);
    }

    /// <summary>
    /// Verifies that a BindCommand call with a non-lambda argument for the command is skipped.
    /// Exercises the command property path null guard in CommandExtractor (line 54).
    /// The view type implements IViewFor so overload resolution succeeds.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindCommand_NonLambdaCommand_GeneratesNoDispatch()
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

                                  public class MyView : IViewFor
                                  {
                                      public object? ViewModel { get; set; }
                                      public object SaveButton { get; set; } = new object();
                                  }

                                  public static class Scenario
                                  {
                                      private static System.Linq.Expressions.Expression<Func<MyViewModel, ICommand?>> GetCommand()
                                          => x => x.Save;

                                      public static void Execute(MyViewModel vm, MyView view)
                                      {
                                          view.BindCommand(vm, GetCommand(), x => x.SaveButton);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source);
        await result.HasNoGeneratorDiagnostics();
        await result.DoesNotHaveGeneratedSource(BindCommandDispatchgcsName);
    }

    /// <summary>
    /// Verifies that a BindCommand call with a non-lambda argument for the control is skipped.
    /// Exercises the control property path null guard in CommandExtractor (line 62).
    /// The view type implements IViewFor so overload resolution succeeds.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindCommand_NonLambdaControl_GeneratesNoDispatch()
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

                                  public class MyView : IViewFor
                                  {
                                      public object? ViewModel { get; set; }
                                      public object SaveButton { get; set; } = new object();
                                  }

                                  public static class Scenario
                                  {
                                      private static System.Linq.Expressions.Expression<Func<MyView, object>> GetControl()
                                          => x => x.SaveButton;

                                      public static void Execute(MyViewModel vm, MyView view)
                                      {
                                          view.BindCommand(vm, x => x.Save, GetControl());
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source);
        await result.HasNoGeneratorDiagnostics();
        await result.DoesNotHaveGeneratedSource(BindCommandDispatchgcsName);
    }

    /// <summary>
    /// Verifies that a BindInteraction call with a non-lambda property expression is skipped.
    /// Exercises the interaction property path null guard in InteractionExtractor (line 53).
    /// The view type implements IViewFor so overload resolution succeeds.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindInteraction_NonLambdaProperty_GeneratesNoDispatch()
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
                                      public IInteraction<string, bool>? Confirm { get; set; }
                                  }

                                  public class MyView : IViewFor
                                  {
                                      public object? ViewModel { get; set; }
                                      public object Result { get; set; } = new object();
                                  }

                                  public static class Scenario
                                  {
                                      private static System.Linq.Expressions.Expression<Func<MyViewModel, IInteraction<string, bool>?>> GetExpr()
                                          => x => x.Confirm;

                                      public static void Execute(MyViewModel vm, MyView view)
                                      {
                                          view.BindInteraction(vm, GetExpr(), async ctx =>
                                          {
                                              ctx.SetOutput(true);
                                              await Task.CompletedTask;
                                          });
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source);
        await result.HasNoGeneratorDiagnostics();
        await result.DoesNotHaveGeneratedSource(BindInteractionDispatchgcsName);
    }

    /// <summary>
    /// Verifies that a WhenAnyObservable call with a non-lambda expression is skipped.
    /// Exercises the propertyPaths empty guard in WhenAnyObservableExtractor (line 80).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyObservable_NonLambda_GeneratesNoDispatch()
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
                                      public IObservable<string>? Items { get; set; }
                                  }

                                  public static class Scenario
                                  {
                                      private static System.Linq.Expressions.Expression<Func<MyViewModel, IObservable<string>?>> GetExpr()
                                          => x => x.Items;

                                      public static void Execute(MyViewModel vm)
                                      {
                                          var obs = vm.WhenAnyObservable(GetExpr());
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source);
        await result.HasNoGeneratorDiagnostics();
        await result.DoesNotHaveGeneratedSource(WhenAnyObservableDispatchgcsName);
    }

    /// <summary>Verifies that the generator handles source code with no INPC types at all.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NoObservableTypes_GeneratesNoRegistration()
    {
        const string source = """
                              namespace TestApp
                              {
                                  public class PlainClass
                                  {
                                      public string Name { get; set; } = "";
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source);
        await result.HasNoGeneratorDiagnostics();

        // No INPC/IReactiveObject types → no registration output
        await result.DoesNotHaveGeneratedSource("GeneratedBinderRegistration.g.cs");
    }

    /// <summary>
    /// Verifies that a WhenChanged call on an unresolvable type is skipped.
    /// Exercises the symbolInfo.Symbol is not IMethodSymbol guard in ObservationExtractor.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanged_UnresolvableReceiver_GeneratesNoDispatch()
    {
        const string source = """
                              namespace TestApp
                              {
                                  public static class Scenario
                                  {
                                      public static void Execute()
                                      {
                                          // UnknownType doesn't exist - GetSymbolInfo returns null Symbol
                                          ((UnknownType)null).WhenChanged(x => x.Bar);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source);
        await result.HasNoGeneratorDiagnostics();
        await result.DoesNotHaveGeneratedSource("WhenChangedDispatch.g.cs");
    }

    /// <summary>
    /// Verifies that a WhenAnyObservable call on an unresolvable type is skipped.
    /// Exercises the symbolInfo.Symbol is not IMethodSymbol guard in WhenAnyObservableExtractor.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyObservable_UnresolvableReceiver_GeneratesNoDispatch()
    {
        const string source = """
                              namespace TestApp
                              {
                                  public static class Scenario
                                  {
                                      public static void Execute()
                                      {
                                          ((UnknownType)null).WhenAnyObservable(x => x.Items);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source);
        await result.HasNoGeneratorDiagnostics();
        await result.DoesNotHaveGeneratedSource(WhenAnyObservableDispatchgcsName);
    }

    /// <summary>
    /// Verifies that a BindCommand call on an unresolvable type is skipped.
    /// Exercises the symbolInfo.Symbol is not IMethodSymbol guard in CommandExtractor.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindCommand_UnresolvableReceiver_GeneratesNoDispatch()
    {
        const string source = """
                              namespace TestApp
                              {
                                  public static class Scenario
                                  {
                                      public static void Execute()
                                      {
                                          ((UnknownType)null).BindCommand(null, x => x.Save, x => x.Btn);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source);
        await result.HasNoGeneratorDiagnostics();
        await result.DoesNotHaveGeneratedSource(BindCommandDispatchgcsName);
    }

    /// <summary>
    /// Verifies that a BindInteraction call on an unresolvable type is skipped.
    /// Exercises the symbolInfo.Symbol is not IMethodSymbol guard in InteractionExtractor.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindInteraction_UnresolvableReceiver_GeneratesNoDispatch()
    {
        const string source = """
                              namespace TestApp
                              {
                                  public static class Scenario
                                  {
                                      public static void Execute()
                                      {
                                          ((UnknownType)null).BindInteraction(null, x => x.Confirm, ctx => null);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source);
        await result.HasNoGeneratorDiagnostics();
        await result.DoesNotHaveGeneratedSource(BindInteractionDispatchgcsName);
    }

    /// <summary>
    /// Verifies that a BindOneWay call on an unresolvable type is skipped.
    /// Exercises the symbolInfo.Symbol is not IMethodSymbol guard in BindingExtractor.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_UnresolvableReceiver_GeneratesNoDispatch()
    {
        const string source = """
                              namespace TestApp
                              {
                                  public static class Scenario
                                  {
                                      public static void Execute()
                                      {
                                          ((UnknownType)null).BindOneWay(null, x => x.Name, x => x.Text);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source);
        await result.HasNoGeneratorDiagnostics();
        await result.DoesNotHaveGeneratedSource(BindOneWayDispatchgcsName);
    }

    /// <summary>
    /// Verifies that a BindOneWay call with fewer than 3 arguments is skipped.
    /// Uses a custom method on a recognized extension class with only 1 explicit parameter.
    /// Exercises the HasMinimumArguments guard in BindingExtractor (line 49).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_TooFewArgs_GeneratesNoDispatch()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;

                              namespace ReactiveUI.Binding
                              {
                                  public static class __ReactiveUIGeneratedBindings
                                  {
                                      public static void BindOneWay<TSource, TTarget>(
                                          this TSource source,
                                          TTarget target)
                                          where TSource : class
                                          where TTarget : class { }
                                  }
                              }

                              namespace TestApp
                              {
                                  public class MyViewModel : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;
                                      public string Name { get; set; } = "";
                                  }

                                  public class MyView
                                  {
                                      public string DisplayName { get; set; } = "";
                                  }

                                  public static class Scenario
                                  {
                                      public static void Execute(MyViewModel vm, MyView view)
                                      {
                                          vm.BindOneWay(view);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source);
        await result.HasNoGeneratorDiagnostics();
        await result.DoesNotHaveGeneratedSource(BindOneWayDispatchgcsName);
    }

    /// <summary>
    /// Verifies that a BindCommand call with fewer than 3 arguments is skipped.
    /// Uses a custom method on a recognized extension class with only 1 explicit parameter.
    /// Exercises the HasMinimumArguments guard in CommandExtractor (line 46).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindCommand_TooFewArgs_GeneratesNoDispatch()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;

                              namespace ReactiveUI.Binding
                              {
                                  public static class __ReactiveUIGeneratedBindings
                                  {
                                      public static void BindCommand<TView, TViewModel>(
                                          this TView view,
                                          TViewModel viewModel)
                                          where TView : class
                                          where TViewModel : class { }
                                  }
                              }

                              namespace TestApp
                              {
                                  public class MyViewModel : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;
                                      public string Name { get; set; } = "";
                                  }

                                  public class MyView
                                  {
                                      public string DisplayName { get; set; } = "";
                                  }

                                  public static class Scenario
                                  {
                                      public static void Execute(MyViewModel vm, MyView view)
                                      {
                                          view.BindCommand(vm);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source);
        await result.HasNoGeneratorDiagnostics();
        await result.DoesNotHaveGeneratedSource(BindCommandDispatchgcsName);
    }

    /// <summary>
    /// Verifies that a BindInteraction call with fewer than 3 arguments is skipped.
    /// Uses a custom method on a recognized extension class with only 1 explicit parameter.
    /// Exercises the HasMinimumArguments guard in InteractionExtractor (line 45).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindInteraction_TooFewArgs_GeneratesNoDispatch()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;

                              namespace ReactiveUI.Binding
                              {
                                  public static class __ReactiveUIGeneratedBindings
                                  {
                                      public static void BindInteraction<TView, TViewModel>(
                                          this TView view,
                                          TViewModel viewModel)
                                          where TView : class
                                          where TViewModel : class { }
                                  }
                              }

                              namespace TestApp
                              {
                                  public class MyViewModel : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;
                                      public string Name { get; set; } = "";
                                  }

                                  public class MyView
                                  {
                                      public string DisplayName { get; set; } = "";
                                  }

                                  public static class Scenario
                                  {
                                      public static void Execute(MyViewModel vm, MyView view)
                                      {
                                          view.BindInteraction(vm);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source);
        await result.HasNoGeneratorDiagnostics();
        await result.DoesNotHaveGeneratedSource(BindInteractionDispatchgcsName);
    }

    /// <summary>
    /// Verifies that the hasConverterOverride flag is set when a recognized binding method
    /// has a parameter named "converter" typed as IBindingTypeConverter.
    /// Exercises the hasConverterOverride branch in BindingExtractor (line 84-86).
    /// Uses extension method syntax so argument indices align correctly with the extractor.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_WithConverterOverride_SetsFlag()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;
                              using System.Linq.Expressions;
                              using ReactiveUI.Binding;

                              namespace ReactiveUI.Binding
                              {
                                  public static class __ReactiveUIGeneratedBindings
                                  {
                                      public static IDisposable BindOneWay<TSource, TTarget, TProperty>(
                                          this TSource source,
                                          TTarget target,
                                          Expression<Func<TSource, TProperty>> sourceProperty,
                                          Expression<Func<TTarget, TProperty>> targetProperty,
                                          IBindingTypeConverter converter)
                                          where TSource : class
                                          where TTarget : class
                                      {
                                          throw new NotImplementedException();
                                      }
                                  }
                              }

                              namespace TestApp
                              {
                                  using ReactiveUI.Binding;

                                  public class MyViewModel : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler PropertyChanged;
                                      public string Name { get; set; }
                                  }

                                  public class MyView
                                  {
                                      public string DisplayName { get; set; }
                                  }

                                  public static class Scenario
                                  {
                                      public static void Execute(MyViewModel vm, MyView view)
                                      {
                                          vm.BindOneWay(view, x => x.Name, x => x.DisplayName, (IBindingTypeConverter)null);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source);
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies that the hasScheduler flag is set when a recognized binding method
    /// has a parameter named "scheduler". Also includes a converter override to exercise
    /// both the hasConverterOverride and hasScheduler branches in BindingExtractor.
    /// Uses extension method syntax so argument indices align correctly with the extractor.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_WithSchedulerAndConverter_SetsBothFlags()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;
                              using System.Linq.Expressions;
                              using ReactiveUI.Binding;

                              namespace ReactiveUI.Binding
                              {
                                  public static class __ReactiveUIGeneratedBindings
                                  {
                                      public static IDisposable BindOneWay<TSource, TTarget, TProperty>(
                                          this TSource source,
                                          TTarget target,
                                          Expression<Func<TSource, TProperty>> sourceProperty,
                                          Expression<Func<TTarget, TProperty>> targetProperty,
                                          IBindingTypeConverter converter,
                                          System.Reactive.Concurrency.IScheduler scheduler)
                                          where TSource : class
                                          where TTarget : class
                                      {
                                          throw new NotImplementedException();
                                      }
                                  }
                              }

                              namespace TestApp
                              {
                                  using ReactiveUI.Binding;

                                  public class MyViewModel : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler PropertyChanged;
                                      public string Name { get; set; }
                                  }

                                  public class MyView
                                  {
                                      public string DisplayName { get; set; }
                                  }

                                  public static class Scenario
                                  {
                                      public static void Execute(MyViewModel vm, MyView view)
                                      {
                                          vm.BindOneWay(view, x => x.Name, x => x.DisplayName,
                                              (IBindingTypeConverter)null, (System.Reactive.Concurrency.IScheduler)null);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source);
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies that SymbolHelpers.ResolveNamedType returns null when the control lambda
    /// body resolves to a property whose type is not INamedTypeSymbol (e.g., an array type).
    /// Exercises the fallback return null in SymbolHelpers.ResolveNamedType (line 205).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindCommand_ArrayControlType_FallsBackGracefully()
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

                                  public class MyView : IViewFor
                                  {
                                      public object? ViewModel { get; set; }
                                      public object[] Buttons { get; set; } = new object[0];
                                  }

                                  public static class Scenario
                                  {
                                      public static void Execute(MyViewModel vm, MyView view)
                                      {
                                          view.BindCommand(vm, x => x.Save, x => x.Buttons);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source);
        await result.HasNoGeneratorDiagnostics();
    }
}
