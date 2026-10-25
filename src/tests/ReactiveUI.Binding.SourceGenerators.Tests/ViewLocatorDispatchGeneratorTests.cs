// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>Snapshot tests for the view locator dispatch generator.</summary>
public class ViewLocatorDispatchGeneratorTests
{
    /// <summary>The hint name of the generated view dispatch file.</summary>
    private const string DispatchHintName = "ViewDispatch.g.cs";

    /// <summary>Verifies that a single IViewFor&lt;T&gt; implementation generates correct dispatch code.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public Task SingleViewForImplementation()
    {
        const string source = """
                              using System.ComponentModel;

                              namespace TestApp
                              {
                                  public class LoginViewModel : INotifyPropertyChanged
                                  {
                                      public string UserName { get; set; }
                                      public event PropertyChangedEventHandler PropertyChanged;
                                  }

                                  public class LoginView : ReactiveUI.Binding.IViewFor<LoginViewModel>
                                  {
                                      public LoginViewModel ViewModel { get; set; }
                                      object ReactiveUI.Binding.IViewFor.ViewModel
                                      {
                                          get => ViewModel;
                                          set => ViewModel = (LoginViewModel)value;
                                      }
                                  }
                              }
                              """;

        return TestHelper.TestPass(source, typeof(ViewLocatorDispatchGeneratorTests));
    }

    /// <summary>Verifies that a C# 10 consumer registers the dispatch as a module initializer.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public Task ModuleInitializerRegistration()
    {
        const string source = """
                              using System.ComponentModel;

                              namespace TestApp
                              {
                                  public class LoginViewModel : INotifyPropertyChanged
                                  {
                                      public string UserName { get; set; }
                                      public event PropertyChangedEventHandler PropertyChanged;
                                  }

                                  public class LoginView : ReactiveUI.Binding.IViewFor<LoginViewModel>
                                  {
                                      public LoginViewModel ViewModel { get; set; }
                                      object ReactiveUI.Binding.IViewFor.ViewModel
                                      {
                                          get => ViewModel;
                                          set => ViewModel = (LoginViewModel)value;
                                      }
                                  }
                              }
                              """;

        return TestHelper.TestPass(source, typeof(ViewLocatorDispatchGeneratorTests), LanguageVersion.CSharp10);
    }

    /// <summary>Verifies that multiple IViewFor&lt;T&gt; implementations generate multiple dispatch branches.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public Task MultipleViewForImplementations()
    {
        const string source = """
                              using System.ComponentModel;

                              namespace TestApp
                              {
                                  public class LoginViewModel : INotifyPropertyChanged
                                  {
                                      public string UserName { get; set; }
                                      public event PropertyChangedEventHandler PropertyChanged;
                                  }

                                  public class MainViewModel : INotifyPropertyChanged
                                  {
                                      public string Title { get; set; }
                                      public event PropertyChangedEventHandler PropertyChanged;
                                  }

                                  public class LoginView : ReactiveUI.Binding.IViewFor<LoginViewModel>
                                  {
                                      public LoginViewModel ViewModel { get; set; }
                                      object ReactiveUI.Binding.IViewFor.ViewModel
                                      {
                                          get => ViewModel;
                                          set => ViewModel = (LoginViewModel)value;
                                      }
                                  }

                                  public class MainView : ReactiveUI.Binding.IViewFor<MainViewModel>
                                  {
                                      public MainViewModel ViewModel { get; set; }
                                      object ReactiveUI.Binding.IViewFor.ViewModel
                                      {
                                          get => ViewModel;
                                          set => ViewModel = (MainViewModel)value;
                                      }
                                  }
                              }
                              """;

        return TestHelper.TestPass(source, typeof(ViewLocatorDispatchGeneratorTests));
    }

    /// <summary>Verifies that a view without a parameterless constructor generates service-locator-only dispatch.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public Task ViewWithoutParameterlessConstructor()
    {
        const string source = """
                              using System.ComponentModel;

                              namespace TestApp
                              {
                                  public class SettingsViewModel : INotifyPropertyChanged
                                  {
                                      public string Theme { get; set; }
                                      public event PropertyChangedEventHandler PropertyChanged;
                                  }

                                  public class SettingsView : ReactiveUI.Binding.IViewFor<SettingsViewModel>
                                  {
                                      private readonly string _config;

                                      public SettingsView(string config)
                                      {
                                          _config = config;
                                      }

                                      public SettingsViewModel ViewModel { get; set; }
                                      object ReactiveUI.Binding.IViewFor.ViewModel
                                      {
                                          get => ViewModel;
                                          set => ViewModel = (SettingsViewModel)value;
                                      }
                                  }
                              }
                              """;

        return TestHelper.TestPass(source, typeof(ViewLocatorDispatchGeneratorTests));
    }

    /// <summary>Verifies that abstract classes implementing IViewFor&lt;T&gt; are excluded from dispatch.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public Task AbstractViewIsExcluded()
    {
        const string source = """
                              using System.ComponentModel;

                              namespace TestApp
                              {
                                  public class BaseViewModel : INotifyPropertyChanged
                                  {
                                      public string Name { get; set; }
                                      public event PropertyChangedEventHandler PropertyChanged;
                                  }

                                  public abstract class BaseView : ReactiveUI.Binding.IViewFor<BaseViewModel>
                                  {
                                      public BaseViewModel ViewModel { get; set; }
                                      object ReactiveUI.Binding.IViewFor.ViewModel
                                      {
                                          get => ViewModel;
                                          set => ViewModel = (BaseViewModel)value;
                                      }
                                  }
                              }
                              """;

        // Abstract views should not produce ViewDispatch.g.cs
        var result = TestHelper.RunGenerator(source);
        return GeneratorSnapshot.VerifyAsync(result.Driver, "VDG", "AbstractExcl");
    }

    /// <summary>Verifies that a view with a private constructor generates service-locator-only dispatch (no direct construction).</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public Task ViewWithPrivateConstructor()
    {
        const string source = """
                              using System.ComponentModel;

                              namespace TestApp
                              {
                                  public class PrivateCtorViewModel : INotifyPropertyChanged
                                  {
                                      public string Data { get; set; }
                                      public event PropertyChangedEventHandler PropertyChanged;
                                  }

                                  public class PrivateCtorView : ReactiveUI.Binding.IViewFor<PrivateCtorViewModel>
                                  {
                                      private PrivateCtorView() { }

                                      public PrivateCtorViewModel ViewModel { get; set; }
                                      object ReactiveUI.Binding.IViewFor.ViewModel
                                      {
                                          get => ViewModel;
                                          set => ViewModel = (PrivateCtorViewModel)value;
                                      }
                                  }
                              }
                              """;

        return TestHelper.TestPass(source, typeof(ViewLocatorDispatchGeneratorTests));
    }

    /// <summary>Verifies that a class not implementing IViewFor produces no ViewDispatch output.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public Task NonViewForClass_NoDispatch()
    {
        const string source = """
                              using System.ComponentModel;

                              namespace TestApp
                              {
                                  public class PlainViewModel : INotifyPropertyChanged
                                  {
                                      public string Name { get; set; }
                                      public event PropertyChangedEventHandler PropertyChanged;
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source);
        return GeneratorSnapshot.VerifyAsync(result.Driver, "VDG", "NoViewFor");
    }

    /// <summary>Verifies that duplicate IViewFor&lt;T&gt; implementations for the same view model are deduplicated.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public Task DuplicateViewModelsAreDeduplicated()
    {
        const string source = """
                              using System.ComponentModel;

                              namespace TestApp
                              {
                                  public class SharedViewModel : INotifyPropertyChanged
                                  {
                                      public string Data { get; set; }
                                      public event PropertyChangedEventHandler PropertyChanged;
                                  }

                                  public class DesktopView : ReactiveUI.Binding.IViewFor<SharedViewModel>
                                  {
                                      public SharedViewModel ViewModel { get; set; }
                                      object ReactiveUI.Binding.IViewFor.ViewModel
                                      {
                                          get => ViewModel;
                                          set => ViewModel = (SharedViewModel)value;
                                      }
                                  }

                                  public class MobileView : ReactiveUI.Binding.IViewFor<SharedViewModel>
                                  {
                                      public SharedViewModel ViewModel { get; set; }
                                      object ReactiveUI.Binding.IViewFor.ViewModel
                                      {
                                          get => ViewModel;
                                          set => ViewModel = (SharedViewModel)value;
                                      }
                                  }
                              }
                              """;

        return TestHelper.TestPass(source, typeof(ViewLocatorDispatchGeneratorTests));
    }

    /// <summary>Verifies that a view marked with [ExcludeFromViewRegistration] is not included in dispatch.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public Task ExcludedViewIsSkipped()
    {
        const string source = """
                              using System.ComponentModel;

                              namespace TestApp
                              {
                                  public class ExcludedViewModel : INotifyPropertyChanged
                                  {
                                      public string Name { get; set; }
                                      public event PropertyChangedEventHandler PropertyChanged;
                                  }

                                  [ReactiveUI.Binding.ExcludeFromViewRegistration]
                                  public class ExcludedView : ReactiveUI.Binding.IViewFor<ExcludedViewModel>
                                  {
                                      public ExcludedViewModel ViewModel { get; set; }
                                      object ReactiveUI.Binding.IViewFor.ViewModel
                                      {
                                          get => ViewModel;
                                          set => ViewModel = (ExcludedViewModel)value;
                                      }
                                  }
                              }
                              """;

        // Excluded views should not produce ViewDispatch.g.cs
        var result = TestHelper.RunGenerator(source);
        return GeneratorSnapshot.VerifyAsync(result.Driver, "VDG", "ExclAttr");
    }

    /// <summary>Verifies that a view marked with [SingleInstanceView] generates singleton dispatch code.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public Task SingleInstanceViewGeneratesSingletonCache()
    {
        const string source = """
                              using System.ComponentModel;

                              namespace TestApp
                              {
                                  public class SingletonViewModel : INotifyPropertyChanged
                                  {
                                      public string Name { get; set; }
                                      public event PropertyChangedEventHandler PropertyChanged;
                                  }

                                  [ReactiveUI.Binding.SingleInstanceView]
                                  public class SingletonView : ReactiveUI.Binding.IViewFor<SingletonViewModel>
                                  {
                                      public SingletonViewModel ViewModel { get; set; }
                                      object ReactiveUI.Binding.IViewFor.ViewModel
                                      {
                                          get => ViewModel;
                                          set => ViewModel = (SingletonViewModel)value;
                                      }
                                  }
                              }
                              """;

        return TestHelper.TestPass(source, typeof(ViewLocatorDispatchGeneratorTests));
    }

    /// <summary>Verifies that a view marked with [ViewContract] generates contract-aware dispatch code.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public Task ViewContractGeneratesContractDispatch()
    {
        const string source = """
                              using System.ComponentModel;

                              namespace TestApp
                              {
                                  public class ContractViewModel : INotifyPropertyChanged
                                  {
                                      public string Name { get; set; }
                                      public event PropertyChangedEventHandler PropertyChanged;
                                  }

                                  [ReactiveUI.Binding.ViewContract("compact")]
                                  public class CompactView : ReactiveUI.Binding.IViewFor<ContractViewModel>
                                  {
                                      public ContractViewModel ViewModel { get; set; }
                                      object ReactiveUI.Binding.IViewFor.ViewModel
                                      {
                                          get => ViewModel;
                                          set => ViewModel = (ContractViewModel)value;
                                      }
                                  }
                              }
                              """;

        return TestHelper.TestPass(source, typeof(ViewLocatorDispatchGeneratorTests));
    }

    /// <summary>Verifies a contract view's check is emitted before the default view's.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public Task DefaultAndContractViewsDispatchCorrectly()
    {
        const string source = """
                              using System.ComponentModel;

                              namespace TestApp
                              {
                                  public class DashboardViewModel : INotifyPropertyChanged
                                  {
                                      public string Title { get; set; }
                                      public event PropertyChangedEventHandler PropertyChanged;
                                  }

                                  public class DashboardView : ReactiveUI.Binding.IViewFor<DashboardViewModel>
                                  {
                                      public DashboardViewModel ViewModel { get; set; }
                                      object ReactiveUI.Binding.IViewFor.ViewModel
                                      {
                                          get => ViewModel;
                                          set => ViewModel = (DashboardViewModel)value;
                                      }
                                  }

                                  [ReactiveUI.Binding.ViewContract("compact")]
                                  public class CompactDashboardView : ReactiveUI.Binding.IViewFor<DashboardViewModel>
                                  {
                                      public DashboardViewModel ViewModel { get; set; }
                                      object ReactiveUI.Binding.IViewFor.ViewModel
                                      {
                                          get => ViewModel;
                                          set => ViewModel = (DashboardViewModel)value;
                                      }
                                  }
                              }
                              """;

        return TestHelper.TestPass(source, typeof(ViewLocatorDispatchGeneratorTests));
    }

    /// <summary>Verifies a view model with only contract views emits no default return.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public Task MultipleContractViewsWithoutDefault()
    {
        const string source = """
                              using System.ComponentModel;

                              namespace TestApp
                              {
                                  public class ThemeViewModel : INotifyPropertyChanged
                                  {
                                      public string Name { get; set; }
                                      public event PropertyChangedEventHandler PropertyChanged;
                                  }

                                  [ReactiveUI.Binding.ViewContract("light")]
                                  public class LightThemeView : ReactiveUI.Binding.IViewFor<ThemeViewModel>
                                  {
                                      public ThemeViewModel ViewModel { get; set; }
                                      object ReactiveUI.Binding.IViewFor.ViewModel
                                      {
                                          get => ViewModel;
                                          set => ViewModel = (ThemeViewModel)value;
                                      }
                                  }

                                  [ReactiveUI.Binding.ViewContract("dark")]
                                  public class DarkThemeView : ReactiveUI.Binding.IViewFor<ThemeViewModel>
                                  {
                                      public ThemeViewModel ViewModel { get; set; }
                                      object ReactiveUI.Binding.IViewFor.ViewModel
                                      {
                                          get => ViewModel;
                                          set => ViewModel = (ThemeViewModel)value;
                                      }
                                  }
                              }
                              """;

        return TestHelper.TestPass(source, typeof(ViewLocatorDispatchGeneratorTests));
    }

    /// <summary>Verifies an open generic view base is not dispatched while its concrete subclass is.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OpenGenericViewBaseIsSkippedAndConcreteSubclassIsDispatched()
    {
        const string source = """
                              using System.ComponentModel;

                              namespace TestApp
                              {
                                  public class TodoViewModel : INotifyPropertyChanged
                                  {
                                      public string Title { get; set; }
                                      public event PropertyChangedEventHandler PropertyChanged;
                                  }

                                  public class ViewBase<TViewModel> : ReactiveUI.Binding.IViewFor<TViewModel>
                                      where TViewModel : class
                                  {
                                      public TViewModel ViewModel { get; set; }
                                      object ReactiveUI.Binding.IViewFor.ViewModel
                                      {
                                          get => ViewModel;
                                          set => ViewModel = (TViewModel)value;
                                      }
                                  }

                                  public abstract class AbstractViewBase<TViewModel> : ReactiveUI.Binding.IViewFor<TViewModel>
                                      where TViewModel : class
                                  {
                                      public TViewModel ViewModel { get; set; }
                                      object ReactiveUI.Binding.IViewFor.ViewModel
                                      {
                                          get => ViewModel;
                                          set => ViewModel = (TViewModel)value;
                                      }
                                  }

                                  public class TodoView : ViewBase<TodoViewModel>
                                  {
                                  }

                                  public class TodoDetailView : AbstractViewBase<TodoViewModel>
                                  {
                                  }
                              }
                              """;

        var result = await TestHelper.TestPassWithResult(source, typeof(ViewLocatorDispatchGeneratorTests));

        await result.CompilationSucceeds();
        await result.GeneratedSourceContains(DispatchHintName, "global::TestApp.TodoView");
        await result.GeneratedSourceDoesNotContain(DispatchHintName, "TViewModel");
    }

    /// <summary>Verifies a generic view closed over its view model is not dispatched.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenericViewWithFixedViewModelIsSkipped()
    {
        const string source = """
                              using System.ComponentModel;

                              namespace TestApp
                              {
                                  public class NoteViewModel : INotifyPropertyChanged
                                  {
                                      public string Title { get; set; }
                                      public event PropertyChangedEventHandler PropertyChanged;
                                  }

                                  public class NoteView<TSkin> : ReactiveUI.Binding.IViewFor<NoteViewModel>
                                  {
                                      public NoteViewModel ViewModel { get; set; }
                                      object ReactiveUI.Binding.IViewFor.ViewModel
                                      {
                                          get => ViewModel;
                                          set => ViewModel = (NoteViewModel)value;
                                      }
                                  }

                                  public class Host<T>
                                  {
                                      public class NestedNoteView : ReactiveUI.Binding.IViewFor<NoteViewModel>
                                      {
                                          public NoteViewModel ViewModel { get; set; }
                                          object ReactiveUI.Binding.IViewFor.ViewModel
                                          {
                                              get => ViewModel;
                                              set => ViewModel = (NoteViewModel)value;
                                          }
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source);

        await result.CompilationSucceeds();
        await result.DoesNotHaveGeneratedSource(DispatchHintName);
    }

    /// <summary>Verifies a private nested view is not dispatched, since the generated resolver cannot name it.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task PrivateNestedViewOrViewModelIsSkipped()
    {
        const string source = """
                              using System.ComponentModel;

                              namespace TestApp
                              {
                                  public class NoteViewModel : INotifyPropertyChanged
                                  {
                                      public string Title { get; set; }
                                      public event PropertyChangedEventHandler PropertyChanged;
                                  }

                                  public class Host
                                  {
                                      private sealed class HiddenNoteView : ReactiveUI.Binding.IViewFor<NoteViewModel>
                                      {
                                          public NoteViewModel ViewModel { get; set; }
                                          object ReactiveUI.Binding.IViewFor.ViewModel
                                          {
                                              get => ViewModel;
                                              set => ViewModel = (NoteViewModel)value;
                                          }
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source);

        await result.CompilationSucceeds();
        await result.DoesNotHaveGeneratedSource(DispatchHintName);
    }

    /// <summary>Verifies [SingleInstanceView] on a view without a parameterless constructor generates no singleton cache.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public Task SingleInstanceViewWithoutParameterlessCtor()
    {
        const string source = """
                              using System.ComponentModel;

                              namespace TestApp
                              {
                                  public class NoCtorSingletonViewModel : INotifyPropertyChanged
                                  {
                                      public string Name { get; set; }
                                      public event PropertyChangedEventHandler PropertyChanged;
                                  }

                                  [ReactiveUI.Binding.SingleInstanceView]
                                  public class NoCtorSingletonView : ReactiveUI.Binding.IViewFor<NoCtorSingletonViewModel>
                                  {
                                      private readonly string _cfg;
                                      public NoCtorSingletonView(string cfg) { _cfg = cfg; }

                                      public NoCtorSingletonViewModel ViewModel { get; set; }
                                      object ReactiveUI.Binding.IViewFor.ViewModel
                                      {
                                          get => ViewModel;
                                          set => ViewModel = (NoCtorSingletonViewModel)value;
                                      }
                                  }
                              }
                              """;

        return TestHelper.TestPass(source, typeof(ViewLocatorDispatchGeneratorTests));
    }
}
