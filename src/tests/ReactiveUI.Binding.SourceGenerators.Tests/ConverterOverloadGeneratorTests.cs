// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>Snapshot tests for the binding overloads that take an <c>IBindingTypeConverter</c>.</summary>
public class ConverterOverloadGeneratorTests
{
    /// <summary>One call site per binding API, each naming a converter, a hint and a scheduler.</summary>
    private const string Source = """
        using System;
        using System.ComponentModel;
        using ReactiveUI.Binding;

        namespace TestApp
        {
            public class Person : INotifyPropertyChanged
            {
                public event PropertyChangedEventHandler PropertyChanged;

                public int Count { get; set; }
            }

            public class PersonView : INotifyPropertyChanged, IViewFor<Person>
            {
                public event PropertyChangedEventHandler PropertyChanged;

                public Person ViewModel { get; set; }

                object IViewFor.ViewModel
                {
                    get { return ViewModel; }
                    set { ViewModel = (Person)value; }
                }

                public string Text { get; set; }
            }

            public static class Usage
            {
                public static void Run(
                    Person person,
                    PersonView view,
                    IBindingTypeConverter forward,
                    IBindingTypeConverter reverse,
                    ReactiveUI.Primitives.Concurrency.ISequencer scheduler)
                {
                    person.BindOneWay(view, x => x.Count, v => v.Text, forward, "format", scheduler);
                    person.BindTwoWay(view, x => x.Count, v => v.Text, forward, reverse, "format", scheduler);
                    view.OneWayBind(person, x => x.Count, v => v.Text, forward);
                    view.Bind(person, x => x.Count, v => v.Text, forward, reverse, "format");
                }
            }
        }
        """;

    /// <summary>Verifies each converter overload is dispatched by file and line and carries its converters to the binding.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EveryApi()
    {
        var result = await TestHelper.TestPassWithResult(Source, typeof(ConverterOverloadGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies the converter overloads carry no expression-text dispatch, which the stubs do not declare.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EveryApi_DispatchesByFileAndLine()
    {
        var result = TestHelper.RunGenerator(Source, LanguageVersion.CSharp10);

        await result.GeneratedSourceContains("BindOneWayDispatch.g.cs", "callerLineNumber ==");
        await result.GeneratedSourceDoesNotContain("BindOneWayDispatch.g.cs", "sourcePropertyExpression");
        await result.GeneratedSourceContains("BindDispatch.g.cs", "callerLineNumber ==");
        await result.GeneratedSourceDoesNotContain("BindDispatch.g.cs", "viewModelPropertyExpression");
    }
}
