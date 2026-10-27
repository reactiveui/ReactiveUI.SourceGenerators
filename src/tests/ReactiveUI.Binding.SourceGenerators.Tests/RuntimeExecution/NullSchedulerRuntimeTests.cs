// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.RuntimeExecution;

/// <summary>
/// Passes an explicit null scheduler to every property-binding overload that declares one. The parameter is
/// nullable, and null leaves the write to the thread that owns the target.
/// </summary>
public class NullSchedulerRuntimeTests
{
    /// <summary>The models and scenario entry points every case runs against.</summary>
    private const string ScenarioSource = """
        using System;
        using System.ComponentModel;
        using ReactiveUI.Binding;
        using ReactiveUI.Primitives.Concurrency;

        namespace TestApp
        {
            public class Person : INotifyPropertyChanged
            {
                private string _name = "a";

                public event PropertyChangedEventHandler PropertyChanged;

                public string Name
                {
                    get { return _name; }
                    set
                    {
                        _name = value;
                        var handler = PropertyChanged;
                        if (handler != null) { handler(this, new PropertyChangedEventArgs("Name")); }
                    }
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
                public Person ViewModel { get; set; }

                object IViewFor.ViewModel
                {
                    get { return ViewModel; }
                    set { ViewModel = (Person)value; }
                }
            }

            public static class Usage
            {
                public static string BindOneWay()
                {
                    var person = new Person(); var label = new Label(); ISequencer scheduler = null;
                    using (person.BindOneWay(label, x => x.Name, x => x.Text, scheduler)) { person.Name = "b"; }
                    return label.Text;
                }

                public static string BindOneWayConverted()
                {
                    var person = new Person(); var label = new Label(); ISequencer scheduler = null;
                    Func<string, string> convert = name => "c" + name;
                    using (person.BindOneWay(label, x => x.Name, x => x.Text, convert, scheduler)) { person.Name = "b"; }
                    return label.Text;
                }

                public static string BindTwoWay()
                {
                    var person = new Person(); var label = new Label(); ISequencer scheduler = null;
                    using (person.BindTwoWay(label, x => x.Name, x => x.Text, scheduler)) { person.Name = "b"; label.Text = "z"; }
                    return person.Name + "|" + label.Text;
                }

                public static string BindTwoWayConverted()
                {
                    var person = new Person(); var label = new Label(); ISequencer scheduler = null;
                    Func<string, string> forward = name => "c" + name;
                    Func<string, string> reverse = text => text.Substring(1);
                    using (person.BindTwoWay(label, x => x.Name, x => x.Text, forward, reverse, scheduler)) { person.Name = "b"; }
                    return label.Text;
                }

                public static string OneWayBind()
                {
                    var person = new Person(); var view = new PersonView { ViewModel = person }; ISequencer scheduler = null;
                    Func<string, string> select = name => "s" + name;
                    using (view.OneWayBind(person, x => x.Name, v => v.Text, select, scheduler)) { person.Name = "b"; }
                    return view.Text;
                }

                public static string Bind()
                {
                    var person = new Person(); var view = new PersonView { ViewModel = person }; ISequencer scheduler = null;
                    Func<string, string> forward = name => "c" + name;
                    Func<string, string> reverse = text => text.Substring(1);
                    using (view.Bind(person, x => x.Name, v => v.Text, forward, reverse, scheduler)) { person.Name = "b"; view.Text = "cz"; }
                    return view.Text + "|" + person.Name;
                }
            }
        }
        """;

    /// <summary>An explicit null scheduler binds and writes, as an omitted one does.</summary>
    /// <param name="scenario">The scenario entry point to run.</param>
    /// <param name="expected">What the scenario reports when the binding wrote through.</param>
    /// <param name="languageVersion">The language version the consumer builds at.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [Arguments("BindOneWay", "b", LanguageVersion.CSharp10)]
    [Arguments("BindOneWayConverted", "cb", LanguageVersion.CSharp10)]
    [Arguments("BindTwoWay", "z|z", LanguageVersion.CSharp10)]
    [Arguments("BindTwoWayConverted", "cb", LanguageVersion.CSharp10)]
    [Arguments("OneWayBind", "sb", LanguageVersion.CSharp10)]
    [Arguments("Bind", "cz|z", LanguageVersion.CSharp10)]
    [Arguments("BindOneWay", "b", LanguageVersion.CSharp7_3)]
    [Arguments("BindTwoWayConverted", "cb", LanguageVersion.CSharp7_3)]
    [Arguments("OneWayBind", "sb", LanguageVersion.CSharp7_3)]
    [Arguments("Bind", "cz|z", LanguageVersion.CSharp7_3)]
    public async Task NullScheduler_WritesOnTheThreadThatOwnsTheTarget(string scenario, string expected, LanguageVersion languageVersion)
    {
        var result = TestHelper.RunGenerator(ScenarioSource, languageVersion);
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
