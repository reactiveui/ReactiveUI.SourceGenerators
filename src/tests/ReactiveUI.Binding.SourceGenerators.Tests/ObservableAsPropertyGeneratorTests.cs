// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>Snapshot tests for the bodies and helper fields written for <c>[ObservableAsProperty]</c> partial properties.</summary>
public class ObservableAsPropertyGeneratorTests
{
    /// <summary>Splitting text on something that appears once gives this many parts.</summary>
    private const int PartsForOneOccurrence = 2;

    /// <summary>A view model with an initial value on each property, and a source it pushes values through by hand.</summary>
    private const string GaugeScenario = """
                                         using System;
                                         using System.ComponentModel;
                                         using ReactiveUI.Binding;

                                         namespace TestApp
                                         {
                                             public sealed class Source<T> : IObservable<T>
                                             {
                                                 private IObserver<T>? _observer;

                                                 public IDisposable Subscribe(IObserver<T> observer)
                                                 {
                                                     _observer = observer;
                                                     return new Nothing();
                                                 }

                                                 public void Push(T value) => _observer!.OnNext(value);

                                                 private sealed class Nothing : IDisposable
                                                 {
                                                     public void Dispose()
                                                     {
                                                     }
                                                 }
                                             }

                                             public partial class GaugeViewModel : INotifyPropertyChanged
                                             {
                                                 public GaugeViewModel(Source<string>? names, Source<double?>? ratios, bool byName)
                                                 {
                                                     if (names != null)
                                                     {
                                                         _nameHelper = byName ? names.ToProperty(this, nameof(Name)) : names.ToProperty(this, x => x.Name);
                                                     }

                                                     if (ratios != null)
                                                     {
                                                         _ratioHelper = ratios.ToProperty(this, x => x.Ratio);
                                                     }
                                                 }

                                                 public event PropertyChangedEventHandler? PropertyChanged;

                                                 [ObservableAsProperty(InitialValue = "start")]
                                                 public partial string Name { get; }

                                                 [ObservableAsProperty(InitialValue = "1.5d", ReadOnly = true, UseProtected = true)]
                                                 public partial double? Ratio { get; }

                                                 [ObservableAsProperty]
                                                 public partial string Label { get; }
                                             }

                                             public static class Usage
                                             {
                                                 public static bool Run()
                                                 {
                                                     var idle = new GaugeViewModel(null, null, false);
                                                     if (idle.Name != "start" || idle.Ratio != 1.5d || idle.Label != string.Empty)
                                                     {
                                                         return false;
                                                     }

                                                     foreach (var byName in new[] { false, true })
                                                     {
                                                         var names = new Source<string>();
                                                         var ratios = new Source<double?>();
                                                         var model = new GaugeViewModel(names, ratios, byName);
                                                         var raised = 0;
                                                         model.PropertyChanged += (sender, args) => raised += args.PropertyName == "Name" ? 1 : 0;
                                                         names.Push("changed");
                                                         ratios.Push(2.5d);
                                                         if (model.Name != "changed" || model.Ratio != 2.5d || raised != 1)
                                                         {
                                                             return false;
                                                         }
                                                     }

                                                     return true;
                                                 }
                                             }
                                         }
                                         """;

    /// <summary>A partial property gets a helper field and a body, and ToProperty assigns the helper.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task PartialProperty_WithToProperty()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;
                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public partial class PersonViewModel : INotifyPropertyChanged
                                  {
                                      public PersonViewModel(IObservable<string> names, IObservable<int?> ages)
                                      {
                                          _fullNameHelper = names.ToProperty(this, x => x.FullName);
                                          _ageHelper = ages.ToProperty(this, x => x.Age);
                                      }

                                      public event PropertyChangedEventHandler? PropertyChanged;

                                      [ObservableAsProperty]
                                      public partial string FullName { get; }

                                      [ObservableAsProperty]
                                      internal partial int? Age { get; }
                                  }
                              }
                              """;

        var result = await TestHelper.TestPassWithResult(source, typeof(ObservableAsPropertyGeneratorTests), LanguageVersion.CSharp13);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>A generic type nested in a partial type repeats both declarations around the members.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NestedGenericType()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;
                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public partial class Outer
                                  {
                                      public partial class Holder<T> : INotifyPropertyChanged
                                      {
                                          public Holder(IObservable<T> values) => _currentHelper = values.ToProperty(this, x => x.Current);

                                          public event PropertyChangedEventHandler? PropertyChanged;

                                          [ObservableAsProperty]
                                          public partial T Current { get; }
                                      }
                                  }
                              }
                              """;

        var result = await TestHelper.TestPassWithResult(source, typeof(ObservableAsPropertyGeneratorTests), LanguageVersion.CSharp13);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Two types of the same name in different namespaces get separate files (ReactiveUI.SourceGenerators #131).</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SameNameDifferentNamespaces()
    {
        const string source = """
                              using System.ComponentModel;
                              using ReactiveUI.Binding;

                              namespace First
                              {
                                  public partial class Widget : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;

                                      [ObservableAsProperty]
                                      public partial int Count { get; }
                                  }
                              }

                              namespace Second
                              {
                                  public partial class Widget : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;

                                      [ObservableAsProperty]
                                      public partial string? Label { get; }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp13);
        await result.HasGeneratedSource("First.Widget.ObservableAsProperties.g.cs");
        await result.HasGeneratedSource("Second.Widget.ObservableAsProperties.g.cs");
        await result.CompilationSucceeds();
    }

    /// <summary>A property three types deep is wrapped in every containing declaration (ReactiveUI.SourceGenerators #133).</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ThreeLevelsNested()
    {
        const string source = """
                              using System.ComponentModel;
                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public partial class A
                                  {
                                      public partial class B
                                      {
                                          public partial class C : INotifyPropertyChanged
                                          {
                                              public event PropertyChangedEventHandler? PropertyChanged;

                                              [ObservableAsProperty]
                                              public partial int Depth { get; }
                                          }
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp13);
        await result.HasGeneratedSource("TestApp.A.B.C.ObservableAsProperties.g.cs");
        await result.CompilationSucceeds();
    }

    /// <summary>
    /// A type with more than one type parameter names its file without the space its display name puts between
    /// them, so the hint name stays a plain file name.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TwoTypeParameters_NamesTheFileWithoutSpaces()
    {
        const string source = """
                              using System.ComponentModel;
                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public partial class Pair<TKey, TValue> : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;

                                      [ObservableAsProperty]
                                      public partial int Count { get; }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp13);
        await result.HasGeneratedSource("TestApp.Pair[TKey,TValue].ObservableAsProperties.g.cs");
        await result.CompilationSucceeds();
    }

    /// <summary>A property that is not a partial get-only declaration is left alone.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NonPartialOrSettable_GeneratesNothing()
    {
        const string source = """
                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public partial class PersonViewModel
                                  {
                                      [ObservableAsProperty]
                                      public string Name { get; } = "";

                                      [ObservableAsProperty]
                                      public string Title { get; set; } = "";
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp13);
        await result.DoesNotHaveGeneratedSource("TestApp.PersonViewModel.ObservableAsProperties.g.cs");
    }

    /// <summary>
    /// The attribute's options shape the helper and the value returned before it is assigned: a readonly or protected
    /// helper, an expression held in a backing field, a string value written as a literal, and an empty string for a
    /// non-nullable string with no initial value.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Options()
    {
        const string source = """
                              using System.ComponentModel;
                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public partial class GaugeViewModel : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;

                                      [ObservableAsProperty(InitialValue = "x", ReadOnly = false, UseProtected = true)]
                                      public partial string Code { get; }

                                      [ObservableAsProperty(InitialValue = "1.1d", ReadOnly = true)]
                                      public partial double? Ratio { get; }

                                      [ObservableAsProperty]
                                      public partial string Label { get; }

                                      [ObservableAsProperty]
                                      public partial string? Note { get; }
                                  }
                              }
                              """;

        var result = await TestHelper.TestPassWithResult(source, typeof(ObservableAsPropertyGeneratorTests), LanguageVersion.CSharp13);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Before its helper is assigned the property returns its initial value, and once <c>ToProperty</c> assigns it in the
    /// constructor, by lambda or by name, each value updates the property and raises <c>PropertyChanged</c>.
    /// </summary>
    /// <param name="useReactiveRuntime">Whether to build against the System.Reactive flavour.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task Options_InitialValueUntilAssignedThenToPropertyValues(bool useReactiveRuntime)
    {
        var source = GaugeScenario;
        if (useReactiveRuntime)
        {
            source = source.Replace("using ReactiveUI.Binding;", "using ReactiveUI.Binding.Reactive;", StringComparison.Ordinal);
        }

        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp13, null, useReactiveRuntime);
        await result.CompilationSucceeds();
        var (assembly, context) = TestHelper.EmitAndLoad(result);
        try
        {
            var run = assembly.GetType("TestApp.Usage")!.GetMethod("Run", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)!;
            await Assert.That((bool)run.Invoke(null, null)!).IsTrue();
        }
        finally
        {
            context.Unload();
        }
    }

    /// <summary>
    /// An initial value expression binds as it was written: the generated file repeats the declaring file's extern alias,
    /// its usings and its namespace's usings, each once however many properties need them.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task InitialValue_BindsWithTheDeclaringFilesDirectives()
    {
        const string Source = """
                              extern alias lib;
                              using System.ComponentModel;
                              using ReactiveUI.Binding;
                              using lib::Lib;

                              namespace TestApp
                              {
                                  using System.Collections.Generic;

                                  public partial class Holder : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;

                                      [ObservableAsProperty(InitialValue = "Defaults.Size")]
                                      public partial int Size { get; }

                                      [ObservableAsProperty(InitialValue = "new List<int>()")]
                                      public partial List<int> Items { get; }
                                  }
                              }
                              """;

        var library = TestHelper.CompileToReference("namespace Lib { public static class Defaults { public const int Size = 4; } }", "Lib", LanguageVersion.CSharp13);
        var result = TestHelper.RunGenerator(Source, LanguageVersion.CSharp13, null, false, [library.WithAliases(["lib"])]);
        await result.CompilationSucceeds();
        var generated = result.GeneratedSources["TestApp.Holder.ObservableAsProperties.g.cs"];
        await Assert.That(generated).Contains("extern alias lib;\nusing System.ComponentModel;\nusing ReactiveUI.Binding;\nusing lib::Lib;\nusing System.Collections.Generic;\n");
        await Assert.That(generated.Split("using lib::Lib;").Length).IsEqualTo(PartsForOneOccurrence);
    }

    /// <summary>A static partial property has no instance to hold a helper, so nothing is generated for it.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StaticPartialProperty_GeneratesNothing()
    {
        const string source = """
                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public partial class Settings
                                  {
                                      [ObservableAsProperty]
                                      public static partial int Count { get; }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp13);
        await result.DoesNotHaveGeneratedSource("TestApp.Settings.ObservableAsProperties.g.cs");
    }

    /// <summary>A record is reopened as a record, and one file holds every marked property it declares.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Record_WithSeveralProperties()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;
                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public partial record Reading : INotifyPropertyChanged
                                  {
                                      public Reading(IObservable<int> values, IObservable<string> units)
                                      {
                                          _valueHelper = values.ToProperty(this, x => x.Value);
                                          _unitHelper = units.ToProperty(this, x => x.Unit);
                                      }

                                      public event PropertyChangedEventHandler? PropertyChanged;

                                      [ObservableAsProperty]
                                      public partial int Value { get; }

                                      [ObservableAsProperty(InitialValue = "kg")]
                                      public partial string Unit { get; }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp13);
        await result.HasGeneratedSource("TestApp.Reading.ObservableAsProperties.g.cs");
        await result.CompilationSucceeds();
    }

    /// <summary>A partial property in a type nested inside a type that is not partial cannot be implemented.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NonPartialContainer_GeneratesNothing()
    {
        const string source = """
                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public class Outer
                                  {
                                      public partial class PersonViewModel
                                      {
                                          [ObservableAsProperty]
                                          public partial string Name { get; }
                                      }
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp13);
        await result.DoesNotHaveGeneratedSource("TestApp.Outer.PersonViewModel.ObservableAsProperties.g.cs");
    }
}
