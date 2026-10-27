// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.RuntimeExecution;

/// <summary>
/// Runs every binding and observation API with its selectors written as <c>static</c> lambdas. Dispatch under
/// expression-text matching compares the text the compiler captured, which carries the modifier, so each API
/// has to reach its generated binding whichever way the selector was spelled.
/// </summary>
public class StaticLambdaDispatchRuntimeTests
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
            public class Recorder<T> : IObserver<T>
            {
                private readonly System.Collections.Generic.List<string> _seen = new System.Collections.Generic.List<string>();

                public string Joined { get { return string.Join(",", _seen); } }

                public void OnNext(T value) { _seen.Add(value == null ? "null" : value.ToString()); }

                public void OnError(Exception error) { }

                public void OnCompleted() { }
            }

            public class RecordingCommand : ICommand
            {
                public int Calls;

                public event EventHandler CanExecuteChanged;

                public bool CanExecute(object parameter) { return true; }

                public void Execute(object parameter) { Calls++; }
            }

            public class Button
            {
                public event EventHandler Click;

                public void PerformClick() { var handler = Click; if (handler != null) { handler(this, EventArgs.Empty); } }
            }

            public class Person : INotifyPropertyChanged, INotifyPropertyChanging
            {
                private string _name = "";
                private RecordingCommand _save;
                private IObservable<string> _stream;
                private Interaction<string, bool> _confirm = new Interaction<string, bool>();

                public event PropertyChangedEventHandler PropertyChanged;

                public event PropertyChangingEventHandler PropertyChanging;

                public string Name
                {
                    get { return _name; }
                    set
                    {
                        Changing("Name");
                        _name = value;
                        Changed("Name");
                    }
                }

                public RecordingCommand Save
                {
                    get { return _save; }
                    set { Changing("Save"); _save = value; Changed("Save"); }
                }

                public IObservable<string> Stream
                {
                    get { return _stream; }
                    set { Changing("Stream"); _stream = value; Changed("Stream"); }
                }

                public Interaction<string, bool> Confirm
                {
                    get { return _confirm; }
                    set { Changing("Confirm"); _confirm = value; Changed("Confirm"); }
                }

                private void Changing(string name)
                {
                    var handler = PropertyChanging;
                    if (handler != null) { handler(this, new PropertyChangingEventArgs(name)); }
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

                public Button SaveButton { get; } = new Button();

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
                public Person ViewModel { get; set; }

                object IViewFor.ViewModel
                {
                    get { return ViewModel; }
                    set { ViewModel = (Person)value; }
                }
            }

            public static class Usage
            {
                public static string WhenChanged()
                {
                    var person = new Person { Name = "a" };
                    var recorder = new Recorder<string>();
                    using (person.WhenChanged(__S__x => x.Name).Subscribe(recorder)) { person.Name = "b"; }
                    return recorder.Joined;
                }

                public static string WhenChanging()
                {
                    var person = new Person { Name = "a" };
                    var recorder = new Recorder<string>();
                    using (person.WhenChanging(__S__x => x.Name).Subscribe(recorder)) { person.Name = "b"; }
                    return recorder.Joined;
                }

                public static string WhenAnyValue()
                {
                    var person = new Person { Name = "a" };
                    var recorder = new Recorder<string>();
                    using (person.WhenAnyValue(__S__x => x.Name).Subscribe(recorder)) { person.Name = "b"; }
                    return recorder.Joined;
                }

                public static string WhenAny()
                {
                    var person = new Person { Name = "a" };
                    var recorder = new Recorder<string>();
                    using (person.WhenAny(__S__x => x.Name, __S__c => c.Value).Subscribe(recorder)) { person.Name = "b"; }
                    return recorder.Joined;
                }

                public static string WhenAnyObservable()
                {
                    var person = new Person { Stream = new ReactiveUI.Primitives.Advanced.ImmediateReturnSignal<string>("hi") };
                    var recorder = new Recorder<string>();
                    using (person.WhenAnyObservable(__S__x => x.Stream).Subscribe(recorder)) { }
                    return recorder.Joined;
                }

                public static string BindOneWay()
                {
                    var person = new Person { Name = "a" };
                    var label = new Label();
                    using (person.BindOneWay(label, __S__x => x.Name, __S__x => x.Text)) { person.Name = "b"; }
                    return label.Text;
                }

                public static string BindTwoWay()
                {
                    var person = new Person { Name = "a" };
                    var label = new Label();
                    using (person.BindTwoWay(label, __S__x => x.Name, __S__x => x.Text)) { label.Text = "c"; }
                    return person.Name;
                }

                public static string OneWayBind()
                {
                    var person = new Person { Name = "a" };
                    var view = new PersonView { ViewModel = person };
                    using (view.OneWayBind(person, __S__x => x.Name, __S__x => x.Text)) { person.Name = "b"; }
                    return view.Text;
                }

                public static string Bind()
                {
                    var person = new Person { Name = "a" };
                    var view = new PersonView { ViewModel = person };
                    using (view.Bind(person, __S__x => x.Name, __S__x => x.Text)) { view.Text = "c"; }
                    return person.Name;
                }

                public static string BindTo()
                {
                    var label = new Label();
                    IObservable<string> source = new ReactiveUI.Primitives.Advanced.ImmediateReturnSignal<string>("hello");
                    using (source.BindTo(label, __S__x => x.Text)) { }
                    return label.Text;
                }

                public static string InvokeCommand()
                {
                    var person = new Person { Save = new RecordingCommand() };
                    IObservable<string> values = new ReactiveUI.Primitives.Advanced.ImmediateReturnSignal<string>("go");
                    using (values.InvokeCommand(person, __S__x => x.Save)) { }
                    return person.Save.Calls.ToString();
                }

                public static string BindCommand()
                {
                    var person = new Person { Save = new RecordingCommand() };
                    var view = new PersonView { ViewModel = person };
                    using (view.BindCommand(person, __S__x => x.Save, __S__x => x.SaveButton)) { view.SaveButton.PerformClick(); }
                    return person.Save.Calls.ToString();
                }

                public static string BindInteraction()
                {
                    var person = new Person();
                    var view = new PersonView { ViewModel = person };
                    using (view.BindInteraction(person, __S__x => x.Confirm, context =>
                    {
                        context.SetOutput(true);
                        return Task.CompletedTask;
                    }))
                    {
                        return person.Confirm.Handle("q").GetAwaiter().GetResult().ToString();
                    }
                }
            }
        }
        """;

    /// <summary>Selectors written with <c>static</c> reach the same generated binding as those written without.</summary>
    /// <param name="api">The API the scenario exercises.</param>
    /// <param name="expected">What the scenario reports when its binding was reached.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [Arguments("WhenChanged", "a,b")]
    [Arguments("WhenChanging", "a,a")]
    [Arguments("WhenAnyValue", "a,b")]
    [Arguments("WhenAny", "a,b")]
    [Arguments("WhenAnyObservable", "hi")]
    [Arguments("BindOneWay", "b")]
    [Arguments("BindTwoWay", "c")]
    [Arguments("OneWayBind", "b")]
    [Arguments("Bind", "c")]
    [Arguments("BindTo", "hello")]
    [Arguments("InvokeCommand", "1")]
    [Arguments("BindCommand", "1")]
    [Arguments("BindInteraction", "True")]
    public async Task StaticSelectors_ReachTheGeneratedBinding(string api, string expected) =>
        await Assert.That(await RunAsync(api, "static ")).IsEqualTo(expected);

    /// <summary>Selectors written without <c>static</c> report the same outcome, so the two spellings are interchangeable.</summary>
    /// <param name="api">The API the scenario exercises.</param>
    /// <param name="expected">What the scenario reports when its binding was reached.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [Arguments("WhenChanged", "a,b")]
    [Arguments("WhenChanging", "a,a")]
    [Arguments("WhenAnyValue", "a,b")]
    [Arguments("WhenAny", "a,b")]
    [Arguments("WhenAnyObservable", "hi")]
    [Arguments("BindOneWay", "b")]
    [Arguments("BindTwoWay", "c")]
    [Arguments("OneWayBind", "b")]
    [Arguments("Bind", "c")]
    [Arguments("BindTo", "hello")]
    [Arguments("InvokeCommand", "1")]
    [Arguments("BindCommand", "1")]
    [Arguments("BindInteraction", "True")]
    public async Task PlainSelectors_ReachTheGeneratedBinding(string api, string expected) =>
        await Assert.That(await RunAsync(api, string.Empty)).IsEqualTo(expected);

    /// <summary>Generates, compiles and runs one scenario with its selectors spelled as given.</summary>
    /// <param name="api">The scenario entry point to run.</param>
    /// <param name="modifier">What each selector lambda opens with.</param>
    /// <returns>The value the scenario's entry point returned.</returns>
    private static async Task<string?> RunAsync(string api, string modifier)
    {
        var source = ScenarioSource.Replace("__S__", modifier, StringComparison.Ordinal);
        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10);
        await result.CompilationSucceeds();

        var (assembly, context) = TestHelper.EmitAndLoad(result);
        try
        {
            var run = assembly.GetType("TestApp.Usage")!.GetMethod(api, BindingFlags.Public | BindingFlags.Static)!;
            return (string?)run.Invoke(null, null);
        }
        finally
        {
            context.Unload();
        }
    }
}
