// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>
/// Tests the binding calls a consumer writes through the stub's declaring class -
/// <c>ReactiveUIBindingExtensions.WhenChanged(vm, x => x.Name)</c> rather than <c>vm.WhenChanged(x => x.Name)</c>.
/// </summary>
/// <remarks>
/// Every extractor reads the bound object from the receiver and the lambdas from the arguments after it, which
/// a call naming the class shifts by one: the receiver is the class itself and each argument sits one place
/// along. Generating from that produces a member declaring a parameter of a static type, which does not
/// compile - so the consumer's whole build fails over generated code they cannot edit, including every
/// unrelated call site in the project. Nothing could serve these call sites anyway: a call that names the
/// class resolves against that class's members, so neither a generated overload nor an interceptor matching
/// the reduced form is a candidate. They belong on the runtime stub.
/// </remarks>
public class DeclaringClassInvocationTests
{
    /// <summary>The <c>WhenChangedDispatch.g.cs</c> name these tests generate against.</summary>
    private const string WhenChangedDispatchFileName = "WhenChangedDispatch.g.cs";

    /// <summary>The <c>WhenAnyObservableDispatch.g.cs</c> name these tests generate against.</summary>
    private const string WhenAnyObservableDispatchFileName = "WhenAnyObservableDispatch.g.cs";

    /// <summary>The <c>BindOneWayDispatch.g.cs</c> name these tests generate against.</summary>
    private const string BindOneWayDispatchFileName = "BindOneWayDispatch.g.cs";

    /// <summary>The <c>BindToDispatch.g.cs</c> name these tests generate against.</summary>
    private const string BindToDispatchFileName = "BindToDispatch.g.cs";

    /// <summary>The <c>BindCommandDispatch.g.cs</c> name these tests generate against.</summary>
    private const string BindCommandDispatchFileName = "BindCommandDispatch.g.cs";

    /// <summary>The <c>BindInteractionDispatch.g.cs</c> name these tests generate against.</summary>
    private const string BindInteractionDispatchFileName = "BindInteractionDispatch.g.cs";

    /// <summary>A <c>WhenChanged</c> call named through its declaring class emits no dispatch.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanged_NamedThroughItsDeclaringClass_GeneratesNoDispatch()
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
                                      public static IObservable<string> Execute(MyViewModel vm)
                                      {
                                          return ReactiveUIBindingExtensions.WhenChanged(vm, x => x.Name);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10);

        await result.HasNoGeneratorDiagnostics();
        await result.CompilationSucceeds();
        await result.DoesNotHaveGeneratedSource(WhenChangedDispatchFileName);
    }

    /// <summary>A <c>WhenAnyObservable</c> call named through its declaring class emits no dispatch.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyObservable_NamedThroughItsDeclaringClass_GeneratesNoDispatch()
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

                                      public IObservable<int>? Signal { get; set; }
                                  }

                                  public static class Scenario
                                  {
                                      public static IObservable<int> Execute(MyViewModel vm)
                                      {
                                          return ReactiveUIBindingExtensions.WhenAnyObservable(vm, x => x.Signal);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10);

        await result.HasNoGeneratorDiagnostics();
        await result.CompilationSucceeds();
        await result.DoesNotHaveGeneratedSource(WhenAnyObservableDispatchFileName);
    }

    /// <summary>A <c>BindOneWay</c> call named through its declaring class emits no dispatch.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_NamedThroughItsDeclaringClass_GeneratesNoDispatch()
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

                                      public string DisplayName { get; set; } = "";
                                  }

                                  public static class Scenario
                                  {
                                      public static IDisposable Execute(MyViewModel vm, MyView view)
                                      {
                                          return ReactiveUIBindingExtensions.BindOneWay(vm, view, x => x.Name, x => x.DisplayName);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10);

        await result.HasNoGeneratorDiagnostics();
        await result.CompilationSucceeds();
        await result.DoesNotHaveGeneratedSource(BindOneWayDispatchFileName);
    }

    /// <summary>A <c>BindTo</c> call named through its declaring class emits no dispatch.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindTo_NamedThroughItsDeclaringClass_GeneratesNoDispatch()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;
                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public class MyView : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;

                                      public string DisplayName { get; set; } = "";
                                  }

                                  public static class Scenario
                                  {
                                      public static IDisposable Execute(IObservable<string> values, MyView view)
                                      {
                                          return ReactiveUIBindingExtensions.BindTo(values, view, x => x.DisplayName);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10);

        await result.HasNoGeneratorDiagnostics();
        await result.CompilationSucceeds();
        await result.DoesNotHaveGeneratedSource(BindToDispatchFileName);
    }

    /// <summary>A <c>BindCommand</c> call named through its declaring class emits no dispatch.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindCommand_NamedThroughItsDeclaringClass_GeneratesNoDispatch()
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

                                  public class MyViewModel : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;

                                      public ICommand? Save { get; set; }
                                  }

                                  public class MyView : IViewFor
                                  {
                                      public object? ViewModel { get; set; }

                                      public MyButton SaveButton { get; set; } = new MyButton();
                                  }

                                  public static class Scenario
                                  {
                                      public static IDisposable Execute(MyViewModel vm, MyView view)
                                      {
                                          return ReactiveUIBindingExtensions.BindCommand(view, vm, x => x.Save, x => x.SaveButton);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10);

        await result.HasNoGeneratorDiagnostics();
        await result.CompilationSucceeds();
        await result.DoesNotHaveGeneratedSource(BindCommandDispatchFileName);
    }

    /// <summary>A <c>BindInteraction</c> call named through its declaring class emits no dispatch.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindInteraction_NamedThroughItsDeclaringClass_GeneratesNoDispatch()
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

                                      public Interaction<string, bool> Confirm { get; set; } = new Interaction<string, bool>();
                                  }

                                  public class MyView : IViewFor
                                  {
                                      public object? ViewModel { get; set; }
                                  }

                                  public static class Scenario
                                  {
                                      public static IDisposable Execute(MyViewModel vm, MyView view)
                                      {
                                          return ReactiveUIBindingExtensions.BindInteraction(view, vm, x => x.Confirm, Handle);
                                      }

                                      private static Task Handle(IInteractionContext<string, bool> context)
                                      {
                                          context.SetOutput(true);
                                          return Task.CompletedTask;
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10);

        await result.HasNoGeneratorDiagnostics();
        await result.CompilationSucceeds();
        await result.DoesNotHaveGeneratedSource(BindInteractionDispatchFileName);
    }
}
