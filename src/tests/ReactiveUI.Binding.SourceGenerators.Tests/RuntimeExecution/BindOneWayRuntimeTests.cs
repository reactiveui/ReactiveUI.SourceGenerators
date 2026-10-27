// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.RuntimeExecution;

/// <summary>
/// Tests that verify the generator produces correct BindOneWay dispatch code
/// and that it compiles and contains expected structure.
/// </summary>
public class BindOneWayRuntimeTests
{
    /// <summary>The <c>BindOneWayDispatch.g.cs</c> name these tests generate against.</summary>
    private const string BindOneWayDispatchgcsName = "BindOneWayDispatch.g.cs";

    /// <summary>The <c>BindTwoWayDispatch.g.cs</c> name these tests generate against.</summary>
    private const string BindTwoWayDispatchgcsName = "BindTwoWayDispatch.g.cs";

    /// <summary>Verifies that BindOneWay emits direct dispatch without a registration layer.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StringBinding_GeneratesDirectDispatch()
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
                                      public string NameText { get; set; } = "";
                                  }

                                  public class Usage
                                  {
                                      public void Test()
                                      {
                                          var vm = new MyViewModel();
                                          var view = new MyView();
                                          var binding = vm.BindOneWay(view, x => x.Name, x => x.NameText);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10);

        await result.HasNoGeneratorDiagnostics();
        await result.HasGeneratedSource(BindOneWayDispatchgcsName);
        await result.DoesNotHaveGeneratedSource("GeneratedBinderRegistration.g.cs");
        await result.GeneratedSourceContains(BindOneWayDispatchgcsName, "NameText");
        await result.GeneratedSourceContains(BindOneWayDispatchgcsName, "Name");
    }

    /// <summary>Verifies that WhenChanging generates dispatch code with PropertyChanging subscription.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanging_GeneratesPropertyChangingSubscription()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;
                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public class MyViewModel : INotifyPropertyChanged, INotifyPropertyChanging
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;
                                      public event PropertyChangingEventHandler? PropertyChanging;
                                      public string Name { get; set; } = "";
                                  }

                                  public class Usage
                                  {
                                      public void Test()
                                      {
                                          var vm = new MyViewModel();
                                          var obs = vm.WhenChanging(x => x.Name);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10);

        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
        await result.HasGeneratedSource("WhenChangingDispatch.g.cs");
        await result.GeneratedSourceContains("WhenChangingDispatch.g.cs", "PropertyChanging");
    }

    /// <summary>Verifies that WhenAnyValue generates dispatch code.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyValue_GeneratesDispatch()
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
                                      public int Age { get; set; }
                                  }

                                  public class Usage
                                  {
                                      public void Test()
                                      {
                                          var vm = new MyViewModel();
                                          var obs = vm.WhenAnyValue(x => x.Name, x => x.Age);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10);

        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
        await result.HasGeneratedSource("WhenAnyValueDispatch.g.cs");
        await result.GeneratedSourceContains("WhenAnyValueDispatch.g.cs", "CombineLatest");
    }

    /// <summary>Verifies that BindTwoWay generates dispatch code for two-way binding.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindTwoWay_GeneratesDispatch()
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
                                      public string NameText { get; set; } = "";
                                  }

                                  public class Usage
                                  {
                                      public void Test()
                                      {
                                          var vm = new MyViewModel();
                                          var view = new MyView();
                                          var binding = vm.BindTwoWay(view, x => x.Name, x => x.NameText);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10);

        await result.HasNoGeneratorDiagnostics();
        await result.HasGeneratedSource(BindTwoWayDispatchgcsName);
        await result.GeneratedSourceContains(BindTwoWayDispatchgcsName, "Name");
        await result.GeneratedSourceContains(BindTwoWayDispatchgcsName, "NameText");
    }
}
