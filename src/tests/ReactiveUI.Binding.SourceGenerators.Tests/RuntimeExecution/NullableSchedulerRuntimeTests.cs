// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.RuntimeExecution;

/// <summary>
/// Passes an explicit null scheduler from a nullable-enabled consumer to every property-binding overload that
/// declares one. Each call compiles without a warning and writes on the thread that owns the target.
/// </summary>
public class NullableSchedulerRuntimeTests
{
    /// <summary>The models and scenario entry points every case runs against.</summary>
    private const string ScenarioSource = """
        using System;
        using System.ComponentModel;
        using ReactiveUI.Binding;
        using ReactiveUI.Primitives.Concurrency;

        namespace TestApp
        {
        #nullable disable
            public class IntToText : IBindingTypeConverter<int, string>
            {
                public Type FromType { get { return typeof(int); } }

                public Type ToType { get { return typeof(string); } }

                public int GetAffinityForObjects() { return 1; }

                public bool TryConvert(int value, object hint, out string result)
                {
                    result = "n" + value;
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
                    result = int.Parse(value.Substring(1));
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
        #nullable enable

            public class Person : INotifyPropertyChanged
            {
                private string _name = "a";

                private int _count;

                public event PropertyChangedEventHandler? PropertyChanged;

                public string Name
                {
                    get { return _name; }
                    set
                    {
                        _name = value;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Name"));
                    }
                }

                public int Count
                {
                    get { return _count; }
                    set
                    {
                        _count = value;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Count"));
                    }
                }
            }

            public class ControlLabel : System.Windows.Forms.Control, INotifyPropertyChanged
            {
                private string _caption = "";

                public event PropertyChangedEventHandler? PropertyChanged;

                public string Caption
                {
                    get { return _caption; }
                    set
                    {
                        _caption = value;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Caption"));
                    }
                }
            }

            public class ControlView : System.Windows.Forms.Control, INotifyPropertyChanged, IViewFor
            {
                private string _caption = "";

                public event PropertyChangedEventHandler? PropertyChanged;

                public object? ViewModel { get; set; }

                public string Caption
                {
                    get { return _caption; }
                    set
                    {
                        _caption = value;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Caption"));
                    }
                }
            }

            public static class Usage
            {
                public static string BindOneWay()
                {
                    var person = new Person();
                    var label = new ControlLabel { InvokeRequired = true };
                    using (person.BindOneWay(label, x => x.Name, x => x.Caption, (ISequencer?)null)) { person.Name = "b"; }
                    return Describe(label.Caption, label.Posts);
                }

                public static string BindOneWayConverted()
                {
                    var person = new Person();
                    var label = new ControlLabel { InvokeRequired = true };
                    Func<string, string> convert = name => "c" + name;
                    using (person.BindOneWay(label, x => x.Name, x => x.Caption, convert, (ISequencer?)null)) { person.Name = "b"; }
                    return Describe(label.Caption, label.Posts);
                }

                public static string BindOneWayConverterObject()
                {
                    var person = new Person();
                    var label = new ControlLabel { InvokeRequired = true };
                    using (person.BindOneWay(label, x => x.Count, x => x.Caption, new IntToText(), null, (ISequencer?)null)) { person.Count = 5; }
                    return Describe(label.Caption, label.Posts);
                }

                public static string BindTwoWay()
                {
                    var person = new Person();
                    var label = new ControlLabel { InvokeRequired = true };
                    using (person.BindTwoWay(label, x => x.Name, x => x.Caption, (ISequencer?)null)) { person.Name = "b"; }
                    return Describe(label.Caption, label.Posts);
                }

                public static string BindTwoWayConverted()
                {
                    var person = new Person();
                    var label = new ControlLabel { InvokeRequired = true };
                    Func<string, string> forward = name => "c" + name;
                    Func<string, string> reverse = text => text.Substring(1);
                    using (person.BindTwoWay(label, x => x.Name, x => x.Caption, forward, reverse, (ISequencer?)null)) { person.Name = "b"; }
                    return Describe(label.Caption, label.Posts);
                }

                public static string BindTwoWayConverterObjects()
                {
                    var person = new Person();
                    var label = new ControlLabel { InvokeRequired = true };
                    using (person.BindTwoWay(label, x => x.Count, x => x.Caption, new IntToText(), new TextToInt(), null, (ISequencer?)null)) { person.Count = 5; }
                    return Describe(label.Caption, label.Posts);
                }

                public static string OneWayBind()
                {
                    var person = new Person();
                    var view = new ControlView { InvokeRequired = true };
                    Func<string, string> select = name => "s" + name;
                    using (view.OneWayBind(person, x => x.Name, v => v.Caption, select, (ISequencer?)null)) { person.Name = "b"; }
                    return Describe(view.Caption, view.Posts);
                }

                public static string OneWayBindConverterObject()
                {
                    var person = new Person();
                    var view = new ControlView { InvokeRequired = true };
                    using (view.OneWayBind(person, x => x.Count, v => v.Caption, new IntToText(), null, (ISequencer?)null)) { person.Count = 5; }
                    return Describe(view.Caption, view.Posts);
                }

                public static string Bind()
                {
                    var person = new Person();
                    var view = new ControlView { InvokeRequired = true };
                    Func<string, string> forward = name => "c" + name;
                    Func<string, string> reverse = text => text.Substring(1);
                    using (view.Bind(person, x => x.Name, v => v.Caption, forward, reverse, (ISequencer?)null)) { person.Name = "b"; }
                    return Describe(view.Caption, view.Posts);
                }

                public static string BindConverterObjects()
                {
                    var person = new Person();
                    var view = new ControlView { InvokeRequired = true };
                    using (view.Bind(person, x => x.Count, v => v.Caption, new IntToText(), new TextToInt(), null, (ISequencer?)null)) { person.Count = 5; }
                    return Describe(view.Caption, view.Posts);
                }

                private static string Describe(string written, int posts)
                {
                    return written + "|" + (posts > 0 ? "dispatched" : "inline");
                }
            }
        }

        namespace System.Windows.Forms
        {
            public class Control
            {
                public int Posts { get; private set; }

                public bool InvokeRequired { get; set; }

                public IAsyncResult? BeginInvoke(Delegate method, params object[] args)
                {
                    Posts++;
                    method.DynamicInvoke(args);
                    return null;
                }
            }
        }
        """;

    /// <summary>An explicit null scheduler compiles without a warning and writes on the owning thread.</summary>
    /// <param name="scenario">The scenario entry point to run.</param>
    /// <param name="expected">What the scenario reports when the binding wrote through the owning thread.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [Arguments("BindOneWay", "b|dispatched")]
    [Arguments("BindOneWayConverted", "cb|dispatched")]
    [Arguments("BindOneWayConverterObject", "n5|dispatched")]
    [Arguments("BindTwoWay", "b|dispatched")]
    [Arguments("BindTwoWayConverted", "cb|dispatched")]
    [Arguments("BindTwoWayConverterObjects", "n5|dispatched")]
    [Arguments("OneWayBind", "sb|dispatched")]
    [Arguments("OneWayBindConverterObject", "n5|dispatched")]
    [Arguments("Bind", "cb|dispatched")]
    [Arguments("BindConverterObjects", "n5|dispatched")]
    public async Task ExplicitNullScheduler_WritesOnTheThreadThatOwnsTheTarget(string scenario, string expected)
    {
        var result = TestHelper.RunGenerator(ScenarioSource + RuntimeInvokerStandIns.WinForms, LanguageVersion.CSharp10);

        await result.CompilationSucceeds();
        await result.HasNoCompilationWarnings();

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
