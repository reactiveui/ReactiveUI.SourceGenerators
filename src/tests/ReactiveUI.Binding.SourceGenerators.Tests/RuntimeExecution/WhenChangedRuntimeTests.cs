// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.RuntimeExecution;

/// <summary>
/// Tests that verify the generator produces correct WhenChanged dispatch code
/// and that it compiles and contains expected structure.
/// </summary>
public class WhenChangedRuntimeTests
{
    /// <summary>The <c>WhenChangedDispatch.g.cs</c> name these tests generate against.</summary>
    private const string WhenChangedDispatchgcsName = "WhenChangedDispatch.g.cs";

    /// <summary>Verifies that single-property observation emits direct dispatch without a registration layer.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_GeneratesDirectDispatch()
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

                                      private string _name = "";
                                      public string Name
                                      {
                                          get => _name;
                                          set
                                          {
                                              if (_name != value)
                                              {
                                                  _name = value;
                                                  PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
                                              }
                                          }
                                      }
                                  }

                                  public class Usage
                                  {
                                      public void Test()
                                      {
                                          var vm = new MyViewModel();
                                          var obs = vm.WhenChanged(x => x.Name);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10);

        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
        await result.HasGeneratedSource(WhenChangedDispatchgcsName);
        await result.DoesNotHaveGeneratedSource("GeneratedBinderRegistration.g.cs");
        await result.GeneratedSourceContains(WhenChangedDispatchgcsName, "PropertyChanged");
        await result.GeneratedSourceContains(WhenChangedDispatchgcsName, "Name");
    }

    /// <summary>Verifies that multi-property WhenChanged generates CombineLatest observation code.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MultiProperty_GeneratesCombineLatest()
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
                                          var obs = vm.WhenChanged(x => x.Name, x => x.Age);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10);

        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
        await result.HasGeneratedSource(WhenChangedDispatchgcsName);
        await result.GeneratedSourceContains(WhenChangedDispatchgcsName, "CombineLatest");
    }

    /// <summary>Verifies that deep chain WhenChanged generates Switch-based re-subscription code.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepChain_GeneratesSwitchPattern()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;
                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public class ChildModel : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;
                                      public string Name { get; set; } = "";
                                  }

                                  public class ParentViewModel : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;
                                      public ChildModel Child { get; set; } = new();
                                  }

                                  public class Usage
                                  {
                                      public void Test()
                                      {
                                          var vm = new ParentViewModel();
                                          var obs = vm.WhenChanged(x => x.Child.Name);
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10);

        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
        await result.HasGeneratedSource(WhenChangedDispatchgcsName);
        await result.GeneratedSourceContains(WhenChangedDispatchgcsName, "Switch");
    }

    /// <summary>Verifies that WhenChanged with a selector generates the selector invocation.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithSelector_GeneratesSelectorInvocation()
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
                                      public string FirstName { get; set; } = "";
                                      public string LastName { get; set; } = "";
                                  }

                                  public class Usage
                                  {
                                      public void Test()
                                      {
                                          var vm = new MyViewModel();
                                          var obs = vm.WhenChanged(x => x.FirstName, x => x.LastName, (f, l) => $"{f} {l}");
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10);

        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
        await result.HasGeneratedSource(WhenChangedDispatchgcsName);
        await result.GeneratedSourceContains(WhenChangedDispatchgcsName, "selector");
    }
}
