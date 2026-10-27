// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.RuntimeExecution;

/// <summary>
/// Runs the binding overloads that take an <c>IBindingTypeConverter</c>. The converter and its hint arrive at
/// run time, so the generated binding has to carry them through to the conversion of every value.
/// </summary>
public class ConverterOverloadDispatchRuntimeTests
{
    /// <summary>The root namespace the scenarios build under.</summary>
    private const string RootNamespace = "TestApp";

    /// <summary>The models and scenario entry points every case runs against.</summary>
    private const string ScenarioSource = """
        using System;
        using System.ComponentModel;
        using ReactiveUI.Binding;

        namespace TestApp
        {
            public class IntToText : IBindingTypeConverter<int, string>
            {
                public Type FromType { get { return typeof(int); } }

                public Type ToType { get { return typeof(string); } }

                public int GetAffinityForObjects() { return 1; }

                public bool TryConvert(int value, object hint, out string result)
                {
                    result = "n" + value + ":" + hint;
                    return true;
                }

                public bool TryConvertTyped(object value, object hint, out object result)
                {
                    string converted;
                    var success = TryConvert((int)value, hint, out converted);
                    result = converted;
                    return success;
                }
            }

            public class TextToInt : IBindingTypeConverter<string, int>
            {
                public Type FromType { get { return typeof(string); } }

                public Type ToType { get { return typeof(int); } }

                public int GetAffinityForObjects() { return 1; }

                public bool TryConvert(string value, object hint, out int result)
                {
                    result = int.Parse(value.Substring(1, value.IndexOf(':') - 1));
                    return true;
                }

                public bool TryConvertTyped(object value, object hint, out object result)
                {
                    int converted;
                    var success = TryConvert((string)value, hint, out converted);
                    result = converted;
                    return success;
                }
            }

            public class Person : INotifyPropertyChanged
            {
                private int _count;

                public event PropertyChangedEventHandler PropertyChanged;

                public int Count
                {
                    get { return _count; }
                    set
                    {
                        _count = value;
                        var handler = PropertyChanged;
                        if (handler != null) { handler(this, new PropertyChangedEventArgs("Count")); }
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
                public static string BindOneWayWithHint()
                {
                    var person = new Person { Count = 1 };
                    var label = new Label();
                    using (person.BindOneWay(label, x => x.Count, x => x.Text, new IntToText(), "hint")) { person.Count = 5; }
                    return label.Text;
                }

                public static string BindOneWayWithoutHint()
                {
                    var person = new Person { Count = 1 };
                    var label = new Label();
                    using (person.BindOneWay(label, x => x.Count, x => x.Text, new IntToText())) { person.Count = 5; }
                    return label.Text;
                }

                public static string BindOneWayWithScheduler()
                {
                    var person = new Person { Count = 1 };
                    var label = new Label();
                    var scheduler = ReactiveUI.Primitives.Concurrency.Sequencer.Immediate;
                    using (person.BindOneWay(label, x => x.Count, x => x.Text, new IntToText(), "hint", scheduler)) { person.Count = 5; }
                    return label.Text;
                }

                public static string BindOneWayWithNullScheduler()
                {
                    var person = new Person { Count = 1 };
                    var label = new Label();
                    using (person.BindOneWay(label, x => x.Count, x => x.Text, new IntToText(), "hint", null)) { person.Count = 5; }
                    return label.Text;
                }

                public static string BindOneWayWithNamedArguments()
                {
                    var person = new Person { Count = 1 };
                    var label = new Label();
                    using (person.BindOneWay(label, x => x.Count, x => x.Text, conversionHint: "named", converter: new IntToText())) { person.Count = 5; }
                    return label.Text;
                }

                public static string BindTwoWayWithNamedArguments()
                {
                    var person = new Person { Count = 1 };
                    var label = new Label();
                    using (person.BindTwoWay(
                        label,
                        x => x.Count,
                        x => x.Text,
                        targetToSourceConverter: new TextToInt(),
                        sourceToTargetConverter: new IntToText(),
                        conversionHint: "named"))
                    {
                        person.Count = 5;
                        return label.Text;
                    }
                }

                public static string BindTwoWayWithHint()
                {
                    var person = new Person { Count = 1 };
                    var label = new Label();
                    using (person.BindTwoWay(label, x => x.Count, x => x.Text, new IntToText(), new TextToInt(), "hint"))
                    {
                        person.Count = 5;
                        var forward = label.Text;
                        label.Text = "n9:hint";
                        return forward + "|" + person.Count;
                    }
                }

                public static string BindTwoWayWithScheduler()
                {
                    var person = new Person { Count = 1 };
                    var label = new Label();
                    var scheduler = ReactiveUI.Primitives.Concurrency.Sequencer.Immediate;
                    using (person.BindTwoWay(label, x => x.Count, x => x.Text, new IntToText(), new TextToInt(), "hint", scheduler))
                    {
                        person.Count = 5;
                        var forward = label.Text;
                        label.Text = "n9:hint";
                        return forward + "|" + person.Count;
                    }
                }

                public static string OneWayBindWithHint()
                {
                    var person = new Person { Count = 1 };
                    var view = new PersonView { ViewModel = person };
                    using (view.OneWayBind(person, x => x.Count, v => v.Text, new IntToText(), "hint")) { person.Count = 5; }
                    return view.Text;
                }

                public static string OneWayBindWithScheduler()
                {
                    var person = new Person { Count = 1 };
                    var view = new PersonView { ViewModel = person };
                    var scheduler = ReactiveUI.Primitives.Concurrency.Sequencer.Immediate;
                    using (view.OneWayBind(person, x => x.Count, v => v.Text, new IntToText(), "hint", scheduler)) { person.Count = 5; }
                    return view.Text;
                }

                public static string BindWithHint()
                {
                    var person = new Person { Count = 1 };
                    var view = new PersonView { ViewModel = person };
                    using (view.Bind(person, x => x.Count, v => v.Text, new IntToText(), new TextToInt(), "hint"))
                    {
                        person.Count = 5;
                        var forward = view.Text;
                        view.Text = "n9:hint";
                        return forward + "|" + person.Count;
                    }
                }

                public static string BindWithScheduler()
                {
                    var person = new Person { Count = 1 };
                    var view = new PersonView { ViewModel = person };
                    var scheduler = ReactiveUI.Primitives.Concurrency.Sequencer.Immediate;
                    using (view.Bind(person, x => x.Count, v => v.Text, new IntToText(), new TextToInt(), "hint", scheduler))
                    {
                        person.Count = 5;
                        var forward = view.Text;
                        view.Text = "n9:hint";
                        return forward + "|" + person.Count;
                    }
                }
            }
        }
        """;

    /// <summary>The converter and hint a call site names are what convert its values, whichever way the call site is reached.</summary>
    /// <param name="scenario">The scenario entry point to run.</param>
    /// <param name="expected">What the scenario reports when the supplied converter converted its values.</param>
    /// <param name="languageVersion">The language version the consumer builds at.</param>
    /// <param name="intercepted">Whether the build lists the generated namespace for interception.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [Arguments("BindOneWayWithHint", "n5:hint", LanguageVersion.CSharp10, false)]
    [Arguments("BindOneWayWithoutHint", "n5:", LanguageVersion.CSharp10, false)]
    [Arguments("BindOneWayWithScheduler", "n5:hint", LanguageVersion.CSharp10, false)]
    [Arguments("BindOneWayWithNullScheduler", "n5:hint", LanguageVersion.CSharp10, false)]
    [Arguments("BindOneWayWithNamedArguments", "n5:named", LanguageVersion.CSharp10, false)]
    [Arguments("BindTwoWayWithNamedArguments", "n5:named", LanguageVersion.CSharp10, false)]
    [Arguments("BindTwoWayWithHint", "n5:hint|9", LanguageVersion.CSharp10, false)]
    [Arguments("BindTwoWayWithScheduler", "n5:hint|9", LanguageVersion.CSharp10, false)]
    [Arguments("OneWayBindWithHint", "n5:hint", LanguageVersion.CSharp10, false)]
    [Arguments("OneWayBindWithScheduler", "n5:hint", LanguageVersion.CSharp10, false)]
    [Arguments("BindWithHint", "n5:hint|9", LanguageVersion.CSharp10, false)]
    [Arguments("BindWithScheduler", "n5:hint|9", LanguageVersion.CSharp10, false)]
    [Arguments("BindOneWayWithHint", "n5:hint", LanguageVersion.CSharp7_3, false)]
    [Arguments("BindTwoWayWithHint", "n5:hint|9", LanguageVersion.CSharp7_3, false)]
    [Arguments("OneWayBindWithHint", "n5:hint", LanguageVersion.CSharp7_3, false)]
    [Arguments("BindWithHint", "n5:hint|9", LanguageVersion.CSharp7_3, false)]
    [Arguments("BindOneWayWithHint", "n5:hint", LanguageVersion.CSharp10, true)]
    [Arguments("BindTwoWayWithScheduler", "n5:hint|9", LanguageVersion.CSharp10, true)]
    [Arguments("OneWayBindWithHint", "n5:hint", LanguageVersion.CSharp10, true)]
    [Arguments("BindWithScheduler", "n5:hint|9", LanguageVersion.CSharp10, true)]
    public async Task ExplicitConverter_ConvertsTheValuesTheBindingCarries(
        string scenario,
        string expected,
        LanguageVersion languageVersion,
        bool intercepted)
    {
        var parseOptions = intercepted
            ? TestHelper.InterceptingParseOptionsFor(languageVersion)
            : TestHelper.ParseOptionsFor(languageVersion);
        var compilation = TestHelper.CreateCompilation(ScenarioSource, parseOptions, false, "TestAssembly", []);
        var result = TestHelper.RunGenerator(compilation, parseOptions, RootNamespace, false);
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
