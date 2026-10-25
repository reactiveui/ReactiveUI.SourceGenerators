// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.RuntimeExecution;

/// <summary>
/// Paths through instance fields. Controls named in XAML or by a designer are exposed as fields, so a view binding
/// usually reads through one, as in <c>v =&gt; v.NameBox.Text</c>. A field raises no notification, so the link is
/// read once and the rest of the path is observed as usual.
/// </summary>
public class FieldPathLinkRuntimeTests
{
    /// <summary>The models and scenario entry points every case runs against.</summary>
    private const string ScenarioSource = """
        using System;
        using System.ComponentModel;
        using System.Windows.Input;
        using ReactiveUI.Binding;

        namespace TestApp
        {
            public class TextBox : INotifyPropertyChanged
            {
                private string _text = "";

                public event PropertyChangedEventHandler PropertyChanged;

                public string Text
                {
                    get { return _text; }
                    set { _text = value; var handler = PropertyChanged; if (handler != null) { handler(this, new PropertyChangedEventArgs("Text")); } }
                }
            }

            public class Button
            {
                public event EventHandler Click;

                public void PerformClick() { var handler = Click; if (handler != null) { handler(this, EventArgs.Empty); } }
            }

            public class RecordingCommand : ICommand
            {
                public int Calls;

                public event EventHandler CanExecuteChanged;

                public bool CanExecute(object parameter) { return true; }

                public void Execute(object parameter) { Calls++; }
            }

            public class Person : INotifyPropertyChanged
            {
                private string _name = "alpha";

                public event PropertyChangedEventHandler PropertyChanged;

                public RecordingCommand Save { get; } = new RecordingCommand();

                public string Name
                {
                    get { return _name; }
                    set { _name = value; var handler = PropertyChanged; if (handler != null) { handler(this, new PropertyChangedEventArgs("Name")); } }
                }
            }

            public class PersonView : IViewFor<Person>, INotifyPropertyChanged
            {
                public Person ViewModel { get; set; }

                object IViewFor.ViewModel { get { return ViewModel; } set { ViewModel = (Person)value; } }

                internal TextBox NameBox = new TextBox();

                internal readonly TextBox TitleBox = new TextBox();

                internal Button SaveButton = new Button();

                public event PropertyChangedEventHandler PropertyChanged;
            }

            public static class Usage
            {
                public static string ObserveThroughField()
                {
                    var view = new PersonView();
                    var seen = "";
                    using (view.WhenChanged(x => x.NameBox.Text).Subscribe(new Sink(v => seen += "|" + v)))
                    {
                        view.NameBox.Text = "one";
                        view.NameBox.Text = "two";
                    }

                    return seen;
                }

                public static string ObserveThroughReadOnlyField()
                {
                    var view = new PersonView();
                    var seen = "";
                    using (view.WhenChanged(x => x.TitleBox.Text).Subscribe(new Sink(v => seen += "|" + v)))
                    {
                        view.TitleBox.Text = "one";
                    }

                    return seen;
                }

                public static string BindOneWayToFieldControl()
                {
                    var person = new Person();
                    var view = new PersonView();
                    using (person.BindOneWay(view, x => x.Name, x => x.NameBox.Text))
                    {
                        person.Name = "beta";
                        return view.NameBox.Text;
                    }
                }

                public static string BindTwoWayToFieldControl()
                {
                    var person = new Person();
                    var view = new PersonView();
                    using (person.BindTwoWay(view, x => x.Name, x => x.NameBox.Text))
                    {
                        view.NameBox.Text = "gamma";
                        return person.Name;
                    }
                }

                public static string BindCommandToFieldControl()
                {
                    var person = new Person();
                    var view = new PersonView { ViewModel = person };
                    using (view.BindCommand(person, x => x.Save, v => v.SaveButton))
                    {
                        view.SaveButton.PerformClick();
                        return person.Save.Calls.ToString();
                    }
                }
            }

            public sealed class Sink : IObserver<string>
            {
                private readonly Action<string> _onNext;

                public Sink(Action<string> onNext) { _onNext = onNext; }

                public void OnNext(string value) { _onNext(value); }

                public void OnError(Exception error) { }

                public void OnCompleted() { }
            }
        }
        """;

    /// <summary>Each field path generates and runs, observing and writing the control's property.</summary>
    /// <param name="scenario">The scenario entry point to run.</param>
    /// <param name="expected">What the scenario reports.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [Arguments("ObserveThroughField", "||one|two")]
    [Arguments("ObserveThroughReadOnlyField", "||one")]
    [Arguments("BindOneWayToFieldControl", "beta")]
    [Arguments("BindTwoWayToFieldControl", "gamma")]
    [Arguments("BindCommandToFieldControl", "1")]
    public async Task FieldPaths_GenerateAndRun(string scenario, string expected)
    {
        var result = TestHelper.RunGenerator(ScenarioSource, LanguageVersion.CSharp10);
        await result.HasNoGeneratorDiagnostics();
        await result.CompilationSucceeds();

        var (assembly, context) = TestHelper.EmitAndLoad(result);
        try
        {
            var run = assembly.GetType("TestApp.Usage")!.GetMethod(scenario, BindingFlags.Public | BindingFlags.Static)!;
            await Assert.That((string?)run.Invoke(null, null)).IsEqualTo(expected);
        }
        finally
        {
            context.Unload();
        }
    }

    /// <summary>A read-only field at the end of a written path is left to the runtime stub rather than assigned.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ReadOnlyLeafField_WrittenPath_GeneratesNothing()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;
                              using ReactiveUI.Binding;

                              namespace TestApp
                              {
                                  public class Person : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;
                                      public string Name { get; set; } = "";
                                  }

                                  public class PersonView
                                  {
                                      internal readonly string Title = "";
                                  }

                                  public static class Usage
                                  {
                                      public static IDisposable Run(Person person, PersonView view) =>
                                          person.BindOneWay(view, x => x.Name, x => x.Title);
                                  }
                              }
                              """;

        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10);

        await result.HasNoGeneratorDiagnostics();
        await result.CompilationSucceeds();
        await result.DoesNotHaveGeneratedSource("BindOneWayDispatch.g.cs");
    }
}
