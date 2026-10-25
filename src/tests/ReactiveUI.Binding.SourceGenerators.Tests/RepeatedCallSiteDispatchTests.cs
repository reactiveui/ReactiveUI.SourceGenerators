// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>
/// Binding the same pair of properties from more than one place is ordinary, and what the two dispatch
/// mechanisms can tell apart differs: expression text is shared by such call sites, while file and line are
/// not. These scenarios pin that each mechanism generates exactly the bodies it can actually reach.
/// </summary>
public partial class RepeatedCallSiteDispatchTests
{
    /// <summary>The dispatch file BindOneWay call sites are generated into.</summary>
    private const string DispatchFileName = "BindOneWayDispatch.g.cs";

    /// <summary>The declaration that opens a generated binding method.</summary>
    private const string BindingMethodDeclaration = "private static global::System.IDisposable __BindOneWay_";

    /// <summary>The number of bodies a dispatch that reaches both call sites keeps.</summary>
    private const int BodyPerCallSite = 2;

    /// <summary>The number of times one kept BindCommand body is named: its branch and its declaration.</summary>
    private const int NamedOncePerBranchAndDeclaration = 2;

    /// <summary>The root namespace these scenarios are generated under.</summary>
    private const string ProbeRootNamespace = "RepeatedProbe";

    /// <summary>Two call sites binding the same property pair, spelled identically.</summary>
    private const string RepeatedBindingSource = """
                                                 using System;
                                                 using System.ComponentModel;
                                                 using ReactiveUI.Binding;

                                                 namespace RepeatedProbe
                                                 {
                                                     public class ProbeViewModel : INotifyPropertyChanged
                                                     {
                                                         public event PropertyChangedEventHandler PropertyChanged;

                                                         public string Name { get; set; }
                                                     }

                                                     public class ProbeView : INotifyPropertyChanged
                                                     {
                                                         public event PropertyChangedEventHandler PropertyChanged;

                                                         public string DisplayName { get; set; }
                                                     }

                                                     public class ProbeHost
                                                     {
                                                         private ProbeViewModel _source = new ProbeViewModel();

                                                         private ProbeView _target = new ProbeView();

                                                         public IDisposable First()
                                                         {
                                                             return _source.BindOneWay(_target, x => x.Name, x => x.DisplayName);
                                                         }

                                                         public IDisposable Second()
                                                         {
                                                             return _source.BindOneWay(_target, x => x.Name, x => x.DisplayName);
                                                         }
                                                     }
                                                 }
                                                 """;

    /// <summary>Two BindTo call sites applying the same stream to the same property, spelled identically.</summary>
    private const string RepeatedBindToSource = """
                              using System;
                              using System.ComponentModel;
                              using ReactiveUI.Binding;

                              namespace RepeatedProbe
                              {
                                  public class ProbeView : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler PropertyChanged;

                                      public string Caption { get; set; }
                                  }

                                  public static class Scenario
                                  {
                                      public static IDisposable First(IObservable<string> source, ProbeView view)
                                      {
                                          return source.BindTo(view, x => x.Caption);
                                      }

                                      public static IDisposable Second(IObservable<string> source, ProbeView view)
                                      {
                                          return source.BindTo(view, x => x.Caption);
                                      }
                                  }
                              }
                              """;

    /// <summary>Two ToProperty call sites naming the same property by string, and two naming it by selector.</summary>
    private const string RepeatedToPropertySource = """
                              using System;
                              using System.ComponentModel;
                              using ReactiveUI.Binding;

                              namespace RepeatedProbe
                              {
                                  public class ProbeViewModel : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler PropertyChanged;

                                      public string Caption => "";

                                      internal void RaisePropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
                                  }

                                  public static class Scenario
                                  {
                                      public static ObservableAsPropertyHelper<string> First(IObservable<string> source, ProbeViewModel vm)
                                      {
                                          return source.ToProperty(vm, nameof(ProbeViewModel.Caption));
                                      }

                                      public static ObservableAsPropertyHelper<string> Second(IObservable<string> source, ProbeViewModel vm)
                                      {
                                          return source.ToProperty(vm, nameof(ProbeViewModel.Caption));
                                      }
                                  }
                              }
                              """;

    /// <summary>Two BindCommand call sites binding the same command to the same control, spelled identically.</summary>
    private const string RepeatedBindCommandSource = """
                              using System;
                              using System.ComponentModel;
                              using System.Windows.Input;
                              using ReactiveUI.Binding;

                              namespace RepeatedProbe
                              {
                                  public class ProbeViewModel : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler PropertyChanged;

                                      public ICommand Save { get; set; }
                                  }

                                  public class ProbeButton
                                  {
                                      public event EventHandler Clicked;

                                      public bool IsEnabled { get; set; }
                                  }

                                  public class ProbeView : IViewFor<ProbeViewModel>
                                  {
                                      public ProbeViewModel ViewModel { get; set; }

                                      object IViewFor.ViewModel
                                      {
                                          get { return ViewModel; }
                                          set { ViewModel = (ProbeViewModel)value; }
                                      }

                                      public ProbeButton Button { get; set; }
                                  }

                                  public static class Scenario
                                  {
                                      public static void First(ProbeView view, ProbeViewModel vm)
                                      {
                                          view.BindCommand(vm, x => x.Save, x => x.Button);
                                      }

                                      public static void Second(ProbeView view, ProbeViewModel vm)
                                      {
                                          view.BindCommand(vm, x => x.Save, x => x.Button);
                                      }
                                  }
                              }
                              """;

    /// <summary>Two BindInteraction call sites binding the same interaction, spelled identically.</summary>
    private const string RepeatedBindInteractionSource = """
                              using System;
                              using System.ComponentModel;
                              using System.Threading.Tasks;
                              using ReactiveUI.Binding;

                              namespace RepeatedProbe
                              {
                                  public class ProbeViewModel : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler PropertyChanged;

                                      public Interaction<string, bool> Confirm { get; set; }
                                  }

                                  public class ProbeView : IViewFor<ProbeViewModel>
                                  {
                                      public ProbeViewModel ViewModel { get; set; }

                                      object IViewFor.ViewModel
                                      {
                                          get { return ViewModel; }
                                          set { ViewModel = (ProbeViewModel)value; }
                                      }
                                  }

                                  public static class Scenario
                                  {
                                      public static IDisposable First(ProbeView view, ProbeViewModel vm)
                                      {
                                          return view.BindInteraction(vm, x => x.Confirm, ctx => Task.CompletedTask);
                                      }

                                      public static IDisposable Second(ProbeView view, ProbeViewModel vm)
                                      {
                                          return view.BindInteraction(vm, x => x.Confirm, ctx => Task.CompletedTask);
                                      }
                                  }
                              }
                              """;

    /// <summary>
    /// Expression-text dispatch cannot tell the two call sites apart, so a second body would be unreachable
    /// and is not emitted.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RepeatedCallSites_UnderExpressionDispatch_GenerateOneBindingMethod()
    {
        var result = TestHelper.RunGenerator(
            RepeatedBindingSource,
            LanguageVersion.CSharp10,
            ProbeRootNamespace);

        await result.CompilationSucceeds();
        await result.GeneratedSourceContainsCount(DispatchFileName, BindingMethodDeclaration, 1);
    }

    /// <summary>File-and-line dispatch reaches each call site separately, so both keep a body of their own.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RepeatedCallSites_UnderFileAndLineDispatch_GenerateABindingMethodEach()
    {
        var result = TestHelper.RunGenerator(
            RepeatedBindingSource,
            LanguageVersion.CSharp7_3,
            ProbeRootNamespace);

        await result.GeneratedSourceContainsCount(DispatchFileName, BindingMethodDeclaration, BodyPerCallSite);
    }

    /// <summary>Two BindTo call sites spelled identically share one generated binding method.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RepeatedBindTo_UnderExpressionDispatch_GeneratesOneBindingMethod()
    {
        var result = TestHelper.RunGenerator(RepeatedBindToSource, LanguageVersion.CSharp10, ProbeRootNamespace);

        await result.CompilationSucceeds();
        await result.GeneratedSourceContainsCount(
            "BindToDispatch.g.cs",
            "private static global::System.IDisposable __BindTo_",
            1);
    }

    /// <summary>Two BindCommand call sites spelled identically share one generated binding method.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RepeatedBindCommand_UnderExpressionDispatch_GeneratesOneBindingMethod()
    {
        var result = TestHelper.RunGenerator(RepeatedBindCommandSource, LanguageVersion.CSharp10, ProbeRootNamespace);

        await result.GeneratedSourceContainsCount(
            "BindCommandDispatch.g.cs",
            "__BindCommand_",
            NamedOncePerBranchAndDeclaration);
    }

    /// <summary>Two BindInteraction call sites spelled identically share one generated binding method.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RepeatedBindInteraction_UnderExpressionDispatch_GeneratesOneBindingMethod()
    {
        var result = TestHelper.RunGenerator(RepeatedBindInteractionSource, LanguageVersion.CSharp10, ProbeRootNamespace);

        await result.GeneratedSourceContainsCount(
            "BindInteractionDispatch.g.cs",
            "__BindInteraction_",
            NamedOncePerBranchAndDeclaration);
    }
}
