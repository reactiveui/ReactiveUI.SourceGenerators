// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.RuntimeExecution;

/// <summary>
/// Call sites that spell their selectors the same way but differ in another argument. Expression-text dispatch
/// tells call sites apart by their selectors, so anything else that changes what a call site binds has to be
/// told apart too: each call site here has to bind what it asked for, whichever of them comes first.
/// </summary>
public class SharedSelectorCallSiteRuntimeTests
{
    /// <summary>The models and scenario entry points every case runs against.</summary>
    private const string ScenarioSource = """
        using System;
        using System.ComponentModel;
        using System.Threading.Tasks;
        using System.Windows.Input;
        using ReactiveUI.Binding;

        namespace TestApp
        {
            public class RecordingCommand : ICommand
            {
                public int Calls;

                public object LastParameter;

                public event EventHandler CanExecuteChanged;

                public bool CanExecute(object parameter) { return true; }

                public void Execute(object parameter) { Calls++; LastParameter = parameter; }
            }

            public class Button
            {
                public event EventHandler Click;

                public event EventHandler Tapped;

                public ICommand Command { get; set; }

                public object CommandParameter { get; set; }

                public void PerformClick() { var handler = Click; if (handler != null) { handler(this, EventArgs.Empty); } }

                public void PerformTap() { var handler = Tapped; if (handler != null) { handler(this, EventArgs.Empty); } }
            }

            public class Person : INotifyPropertyChanged
            {
                private RecordingCommand _save;
                private string _name = "alpha";
                private string _title = "beta";
                private int _count;
                private Interaction<string, bool> _confirm = new Interaction<string, bool>();

                public event PropertyChangedEventHandler PropertyChanged;

                public RecordingCommand Save
                {
                    get { return _save; }
                    set { _save = value; Changed("Save"); }
                }

                public string Name
                {
                    get { return _name; }
                    set { _name = value; Changed("Name"); }
                }

                public string Title
                {
                    get { return _title; }
                    set { _title = value; Changed("Title"); }
                }

                public int Count
                {
                    get { return _count; }
                    set { _count = value; Changed("Count"); }
                }

                public Interaction<string, bool> Confirm
                {
                    get { return _confirm; }
                    set { _confirm = value; Changed("Confirm"); }
                }

                private void Changed(string name)
                {
                    var handler = PropertyChanged;
                    if (handler != null) { handler(this, new PropertyChangedEventArgs(name)); }
                }
            }

            public class Label : INotifyPropertyChanged
            {
                private string _text = "";

                public event PropertyChangedEventHandler PropertyChanged;

                public string Text
                {
                    get { return _text; }
                    set
                    {
                        _text = value;
                        var handler = PropertyChanged;
                        if (handler != null) { handler(this, new PropertyChangedEventArgs("Text")); }
                    }
                }
            }

            public class PersonView : Label, IViewFor<Person>
            {
                public Button SaveButton { get; } = new Button();

                public Person ViewModel { get; set; }

                object IViewFor.ViewModel
                {
                    get { return ViewModel; }
                    set { ViewModel = (Person)value; }
                }
            }

            public static class Usage
            {
                public static string CommandPlainThenEvent()
                {
                    var person = new Person { Save = new RecordingCommand() };
                    var plain = new PersonView { ViewModel = person };
                    var withEvent = new PersonView { ViewModel = person };
                    using (plain.BindCommand(person, x => x.Save, x => x.SaveButton))
                    using (withEvent.BindCommand(person, x => x.Save, x => x.SaveButton, toEvent: "Click"))
                    {
                        withEvent.SaveButton.PerformClick();
                        return (plain.SaveButton.Command == person.Save) + "|" + (withEvent.SaveButton.Command == null) + "|" + person.Save.Calls;
                    }
                }

                public static string CommandEventThenPlain()
                {
                    var person = new Person { Save = new RecordingCommand() };
                    var withEvent = new PersonView { ViewModel = person };
                    var plain = new PersonView { ViewModel = person };
                    using (withEvent.BindCommand(person, x => x.Save, x => x.SaveButton, toEvent: "Click"))
                    using (plain.BindCommand(person, x => x.Save, x => x.SaveButton))
                    {
                        withEvent.SaveButton.PerformClick();
                        return (plain.SaveButton.Command == person.Save) + "|" + (withEvent.SaveButton.Command == null) + "|" + person.Save.Calls;
                    }
                }

                public static string CommandTwoEvents()
                {
                    var person = new Person { Save = new RecordingCommand() };
                    var clicked = new PersonView { ViewModel = person };
                    var tapped = new PersonView { ViewModel = person };
                    using (clicked.BindCommand(person, x => x.Save, x => x.SaveButton, toEvent: "Click"))
                    using (tapped.BindCommand(person, x => x.Save, x => x.SaveButton, toEvent: "Tapped"))
                    {
                        tapped.SaveButton.PerformTap();
                        var afterTap = person.Save.Calls;
                        clicked.SaveButton.PerformTap();
                        var afterWrongEvent = person.Save.Calls;
                        clicked.SaveButton.PerformClick();
                        return afterTap + "|" + afterWrongEvent + "|" + person.Save.Calls;
                    }
                }

                public static string CommandParameterExpressions()
                {
                    var person = new Person { Save = new RecordingCommand() };
                    var byName = new PersonView { ViewModel = person };
                    var byTitle = new PersonView { ViewModel = person };
                    using (byName.BindCommand(person, x => x.Save, x => x.SaveButton, x => x.Name))
                    using (byTitle.BindCommand(person, x => x.Save, x => x.SaveButton, x => x.Title))
                    {
                        return byName.SaveButton.CommandParameter + "|" + byTitle.SaveButton.CommandParameter;
                    }
                }

                public static string InteractionHandlerResults()
                {
                    var person = new Person();
                    var first = new PersonView { ViewModel = person };
                    var second = new PersonView { ViewModel = person };
                    using (first.BindInteraction(person, x => x.Confirm, context =>
                    {
                        context.SetOutput(true);
                        return new ReactiveUI.Primitives.Advanced.ImmediateReturnSignal<int>(1);
                    }))
                    using (second.BindInteraction(person, x => x.Confirm, context =>
                    {
                        context.SetOutput(true);
                        return new ReactiveUI.Primitives.Advanced.ImmediateReturnSignal<string>("done");
                    }))
                    {
                        return person.Confirm.Handle("q").GetAwaiter().GetResult().ToString();
                    }
                }

                public static string BindToWithAndWithoutHint()
                {
                    var plain = new Label();
                    var hinted = new Label();
                    IObservable<int> source = new ReactiveUI.Primitives.Advanced.ImmediateReturnSignal<int>(7);
                    using (source.BindTo(plain, x => x.Text))
                    using (source.BindTo(hinted, x => x.Text, conversionHint: "D3"))
                    {
                        return plain.Text + "|" + hinted.Text;
                    }
                }

                public static string BindOneWayConversionsAndSchedulers()
                {
                    var person = new Person { Name = "alpha" };
                    var plain = new Label();
                    var converted = new Label();
                    var scheduled = new Label();
                    var scheduler = ReactiveUI.Primitives.Concurrency.Sequencer.Immediate;
                    Func<string, string> convert = name => "c" + name;
                    using (person.BindOneWay(plain, x => x.Name, x => x.Text))
                    using (person.BindOneWay(converted, x => x.Name, x => x.Text, convert))
                    using (person.BindOneWay(scheduled, x => x.Name, x => x.Text, scheduler))
                    {
                        return plain.Text + "|" + converted.Text + "|" + scheduled.Text;
                    }
                }

                public static string WhenAnyValueWithAndWithoutSelector()
                {
                    var person = new Person { Name = "alpha" };
                    string plain = null;
                    string selected = null;
                    Func<string, string> select = name => "s" + name;
                    using (person.WhenAnyValue(x => x.Name).Subscribe(new Sink(value => plain = value)))
                    using (person.WhenAnyValue(x => x.Name, select).Subscribe(new Sink(value => selected = value)))
                    {
                        return plain + "|" + selected;
                    }
                }

                private sealed class Sink : IObserver<string>
                {
                    private readonly Action<string> _onNext;

                    public Sink(Action<string> onNext) { _onNext = onNext; }

                    public void OnNext(string value) { _onNext(value); }

                    public void OnError(Exception error) { }

                    public void OnCompleted() { }
                }
            }
        }
        """;

    /// <summary>Each call site binds what it asked for, although its selectors read the same as another's.</summary>
    /// <param name="scenario">The scenario entry point to run.</param>
    /// <param name="expected">What the scenario reports when every call site bound its own arguments.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [Arguments("CommandPlainThenEvent", "True|True|1")]
    [Arguments("CommandEventThenPlain", "True|True|1")]
    [Arguments("CommandTwoEvents", "1|1|2")]
    [Arguments("CommandParameterExpressions", "alpha|beta")]
    [Arguments("InteractionHandlerResults", "True")]
    [Arguments("BindToWithAndWithoutHint", "7|007")]
    [Arguments("BindOneWayConversionsAndSchedulers", "alpha|calpha|alpha")]
    [Arguments("WhenAnyValueWithAndWithoutSelector", "alpha|salpha")]
    public async Task CallSitesSharingSelectors_EachBindTheirOwnArguments(string scenario, string expected)
    {
        var result = TestHelper.RunGenerator(ScenarioSource, LanguageVersion.CSharp10);
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
}
