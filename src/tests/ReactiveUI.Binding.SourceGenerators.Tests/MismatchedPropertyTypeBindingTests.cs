// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>
/// Covers binding a property to one of a different type without passing a converter. The registry exists to
/// serve exactly this - a number shown in a text box is the archetypal binding - so the two sides not lining up
/// is a conversion to resolve, not an assignment to emit.
/// </summary>
public class MismatchedPropertyTypeBindingTests
{
    /// <summary>The registry checked before using a generated conversion.</summary>
    private const string ConverterName = "BindingConverters.Current.TypedConverters.TryGetConverter";

    /// <summary>A view model exposing a number bound to a view exposing text, with no converter supplied.</summary>
    private const string NumberToTextSource = """
                                              using System;
                                              using System.ComponentModel;
                                              using ReactiveUI.Binding;

                                              namespace Consumer
                                              {
                                                  public class MyViewModel : INotifyPropertyChanged
                                                  {
                                                      public event PropertyChangedEventHandler PropertyChanged;

                                                      public int Age { get; set; }
                                                  }

                                                  public class MyView : INotifyPropertyChanged, IViewFor<MyViewModel>
                                                  {
                                                      public event PropertyChangedEventHandler PropertyChanged;

                                                      public string AgeText { get; set; } = "";

                                                      public MyViewModel ViewModel { get; set; }

                                                      object IViewFor.ViewModel
                                                      {
                                                          get { return ViewModel; }
                                                          set { ViewModel = (MyViewModel)value; }
                                                      }
                                                  }

                                                  public static class Usage
                                                  {
                                                      public static IDisposable Wire(MyView view, MyViewModel viewModel)
                                                      {
                                                          return view.OneWayBind(viewModel, vm => vm.Age, v => v.AgeText);
                                                      }
                                                  }
                                              }
                                              """;

    /// <summary>The same pair of types bound two-way, where each direction crosses the gap the other way.</summary>
    private static readonly string TwoWayNumberToTextSource = NumberToTextSource.Replace(
        "view.OneWayBind(viewModel, vm => vm.Age, v => v.AgeText)",
        "view.Bind(viewModel, vm => vm.Age, v => v.AgeText)",
        StringComparison.Ordinal);

    /// <summary>
    /// A binding whose two sides have different types compiles. Assigning the source value straight across is a
    /// type error inside a generated file, where the consumer can neither see nor fix it.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OneWayBind_BetweenDifferentPropertyTypes_Compiles()
    {
        var result = TestHelper.RunGenerator(NumberToTextSource, LanguageVersion.CSharp10);

        await result.CompilationSucceeds();
    }

    /// <summary>The conversion is resolved from the registry, the way the runtime engine resolves it.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OneWayBind_BetweenDifferentPropertyTypes_ResolvesAConverter()
    {
        var result = TestHelper.RunGenerator(NumberToTextSource, LanguageVersion.CSharp10);

        await result.GeneratedSourceContains("OneWayBindDispatch.g.cs", ConverterName);
    }

    /// <summary>
    /// A two-way binding whose two sides have different types compiles. Both directions have to convert, since
    /// each one assigns across the same type gap in the opposite direction.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Bind_BetweenDifferentPropertyTypes_Compiles()
    {
        var result = TestHelper.RunGenerator(TwoWayNumberToTextSource, LanguageVersion.CSharp10);

        await result.CompilationSucceeds();
    }

    /// <summary>Both directions of a two-way binding resolve their conversion from the registry.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Bind_BetweenDifferentPropertyTypes_ResolvesConvertersBothWays()
    {
        var result = TestHelper.RunGenerator(TwoWayNumberToTextSource, LanguageVersion.CSharp10);

        await result.GeneratedSourceContains("BindDispatch.g.cs", $"{ConverterName}(typeof(int), typeof(string))");
        await result.GeneratedSourceContains("BindDispatch.g.cs", $"{ConverterName}(typeof(string), typeof(int))");
    }
}
