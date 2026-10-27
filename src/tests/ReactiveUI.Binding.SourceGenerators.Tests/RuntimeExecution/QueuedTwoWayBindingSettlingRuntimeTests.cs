// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.RuntimeExecution;

/// <summary>Covers a generated two-way binding whose writes wait on a sequencer that queues them, where the two sides start unequal.</summary>
[NotInParallel]
public class QueuedTwoWayBindingSettlingRuntimeTests
{
    /// <summary>The value the model starts with.</summary>
    private const string ModelValue = "Rent March";

    /// <summary>The value a view edit types.</summary>
    private const string ViewEdit = "typed by the user";

    /// <summary>The value a model edit sets.</summary>
    private const string ModelEdit = "Rent April";

    /// <summary>The value the second of two view edits types.</summary>
    private const string SecondViewEdit = "second typed";

    /// <summary>The value the second of two model edits sets.</summary>
    private const string SecondModelEdit = "Rent May";

    /// <summary>The time limit, in milliseconds, that turns a hang into a failure.</summary>
    private const int TimeoutMilliseconds = 60_000;

    /// <summary>A model and view that start unequal, bound through each generated entry point on a sequencer that queues.</summary>
    private const string QueuedSource = """
                                        using System;
                                        using System.Collections.Generic;
                                        using System.ComponentModel;
                                        using ReactiveUI.Binding;
                                        using ReactiveUI.Primitives.Concurrency;

                                        namespace TestApp
                                        {
                                            public class ManualSequencer : ISequencer
                                            {
                                                private readonly Queue<IWorkItem> _pending = new Queue<IWorkItem>();

                                                public DateTimeOffset Now
                                                {
                                                    get { return ImmediateSequencer.Instance.Now; }
                                                }

                                                public long Timestamp
                                                {
                                                    get { return ImmediateSequencer.Instance.Timestamp; }
                                                }

                                                public void Schedule(IWorkItem item)
                                                {
                                                    _pending.Enqueue(item);
                                                }

                                                public void Schedule(IWorkItem item, long dueTimestamp)
                                                {
                                                    _pending.Enqueue(item);
                                                }

                                                public int RunUntilIdle(int maxPasses)
                                                {
                                                    for (var pass = 0; pass < maxPasses; pass++)
                                                    {
                                                        var count = _pending.Count;
                                                        if (count == 0)
                                                        {
                                                            return pass;
                                                        }

                                                        for (var i = 0; i < count; i++)
                                                        {
                                                            _pending.Dequeue().Execute();
                                                        }
                                                    }

                                                    return -1;
                                                }
                                            }

                                            public class Draft : INotifyPropertyChanged
                                            {
                                                private string _reference = "Rent March";

                                                public event PropertyChangedEventHandler PropertyChanged;

                                                public string Reference
                                                {
                                                    get { return _reference; }
                                                    set
                                                    {
                                                        _reference = value;
                                                        var handler = PropertyChanged;
                                                        if (handler != null)
                                                        {
                                                            handler(this, new PropertyChangedEventArgs("Reference"));
                                                        }
                                                    }
                                                }
                                            }

                                            public class Box : INotifyPropertyChanged, IViewFor
                                            {
                                                private string _text = "";

                                                public event PropertyChangedEventHandler PropertyChanged;

                                                public object ViewModel { get; set; }

                                                public string Text
                                                {
                                                    get { return _text; }
                                                    set
                                                    {
                                                        _text = value;
                                                        var handler = PropertyChanged;
                                                        if (handler != null)
                                                        {
                                                            handler(this, new PropertyChangedEventArgs("Text"));
                                                        }
                                                    }
                                                }
                                            }

                                            public class Inner : INotifyPropertyChanged
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
                                                        if (handler != null)
                                                        {
                                                            handler(this, new PropertyChangedEventArgs("Text"));
                                                        }
                                                    }
                                                }
                                            }

                                            public class NestedBox : INotifyPropertyChanged
                                            {
                                                private Inner _inner;

                                                public event PropertyChangedEventHandler PropertyChanged;

                                                public Inner Inner
                                                {
                                                    get { return _inner; }
                                                    set
                                                    {
                                                        _inner = value;
                                                        var handler = PropertyChanged;
                                                        if (handler != null)
                                                        {
                                                            handler(this, new PropertyChangedEventArgs("Inner"));
                                                        }
                                                    }
                                                }
                                            }

                                            public static class Usage
                                            {
                                                public static string RunThroughAMissingParent()
                                                {
                                                    var draft = new Draft();
                                                    var box = new NestedBox();
                                                    var sequencer = new ManualSequencer();

                                                    IDisposable binding = draft.BindTwoWay(box, x => x.Reference, x => x.Inner.Text, sequencer);
                                                    var settled = sequencer.RunUntilIdle(20);

                                                    var inner = new Inner();
                                                    inner.Text = "typed by the user";
                                                    box.Inner = inner;
                                                    var afterEdit = sequencer.RunUntilIdle(20);

                                                    binding.Dispose();

                                                    return (settled == -1 || afterEdit == -1 ? "livelock" : "idle") + "|" + draft.Reference;
                                                }

                                                public static string Run(string entryPoint, string phase)
                                                {
                                                    var draft = new Draft();
                                                    var box = new Box();
                                                    box.ViewModel = draft;
                                                    var sequencer = new ManualSequencer();

                                                    IDisposable binding = Connect(entryPoint, draft, box, sequencer);
                                                    var passes = sequencer.RunUntilIdle(20);

                                                    if (passes != -1 && phase == "viewEdit")
                                                    {
                                                        box.Text = Show(entryPoint, "typed by the user");
                                                        passes = sequencer.RunUntilIdle(20);
                                                    }
                                                    else if (passes != -1 && phase == "modelEdit")
                                                    {
                                                        draft.Reference = "Rent April";
                                                        passes = sequencer.RunUntilIdle(20);
                                                    }
                                                    else if (passes != -1 && phase == "viewBurst")
                                                    {
                                                        box.Text = Show(entryPoint, "first typed");
                                                        box.Text = Show(entryPoint, "second typed");
                                                        passes = sequencer.RunUntilIdle(20);
                                                    }
                                                    else if (passes != -1 && phase == "modelBurst")
                                                    {
                                                        draft.Reference = "Rent April";
                                                        draft.Reference = "Rent May";
                                                        passes = sequencer.RunUntilIdle(20);
                                                    }

                                                    binding.Dispose();

                                                    return (passes == -1 ? "livelock" : "idle") + "|" + draft.Reference + "|" + box.Text;
                                                }

                                                private static string Show(string entryPoint, string value)
                                                {
                                                    return entryPoint == "BindTwoWay" ? value : value + "!";
                                                }

                                                private static IDisposable Connect(string entryPoint, Draft draft, Box box, ManualSequencer sequencer)
                                                {
                                                    if (entryPoint == "BindTwoWay")
                                                    {
                                                        return draft.BindTwoWay(box, x => x.Reference, x => x.Text, sequencer);
                                                    }

                                                    if (entryPoint == "BindTwoWayConverting")
                                                    {
                                                        return draft.BindTwoWay(box, x => x.Reference, x => x.Text, v => v + "!", v => v.EndsWith("!") ? v.Substring(0, v.Length - 1) : v, sequencer);
                                                    }

                                                    return box.Bind(draft, x => x.Reference, x => x.Text, v => v + "!", v => v.EndsWith("!") ? v.Substring(0, v.Length - 1) : v, sequencer);
                                                }
                                            }
                                        }
                                        """;

    /// <summary>A queued binding with unequal sides goes idle and leaves the model's value in the model.</summary>
    /// <param name="entryPoint">The generated entry point that creates the binding.</param>
    /// <param name="cancellationToken">The token that ends the test when it hangs.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Timeout(TimeoutMilliseconds)]
    [Arguments("BindTwoWay")]
    [Arguments("BindTwoWayConverting")]
    [Arguments("Bind")]
    public async Task Binding_WithUnequalSides_GoesIdleOnTheModelsValue(string entryPoint, CancellationToken cancellationToken)
    {
        _ = cancellationToken;
        var outcome = await RunScenarioAsync("Run", [entryPoint, "settle"]);

        await Assert.That(outcome).IsEqualTo($"idle|{ModelValue}|{Shown(entryPoint, ModelValue)}");
    }

    /// <summary>A view edit after a queued binding settles reaches the model and the binding goes idle again.</summary>
    /// <param name="entryPoint">The generated entry point that creates the binding.</param>
    /// <param name="cancellationToken">The token that ends the test when it hangs.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Timeout(TimeoutMilliseconds)]
    [Arguments("BindTwoWay")]
    [Arguments("BindTwoWayConverting")]
    [Arguments("Bind")]
    public async Task Binding_AfterSettling_CarriesAViewEditToTheModel(string entryPoint, CancellationToken cancellationToken)
    {
        _ = cancellationToken;
        var outcome = await RunScenarioAsync("Run", [entryPoint, "viewEdit"]);

        await Assert.That(outcome).IsEqualTo($"idle|{ViewEdit}|{Shown(entryPoint, ViewEdit)}");
    }

    /// <summary>A model edit after a queued binding settles reaches the view and the binding goes idle again.</summary>
    /// <param name="entryPoint">The generated entry point that creates the binding.</param>
    /// <param name="cancellationToken">The token that ends the test when it hangs.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Timeout(TimeoutMilliseconds)]
    [Arguments("BindTwoWay")]
    [Arguments("BindTwoWayConverting")]
    [Arguments("Bind")]
    public async Task Binding_AfterSettling_CarriesAModelEditToTheView(string entryPoint, CancellationToken cancellationToken)
    {
        _ = cancellationToken;
        var outcome = await RunScenarioAsync("Run", [entryPoint, "modelEdit"]);

        await Assert.That(outcome).IsEqualTo($"idle|{ModelEdit}|{Shown(entryPoint, ModelEdit)}");
    }

    /// <summary>Two view edits before the queue drains leave the model with the second one, and the binding goes idle.</summary>
    /// <param name="entryPoint">The generated entry point that creates the binding.</param>
    /// <param name="cancellationToken">The token that ends the test when it hangs.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Timeout(TimeoutMilliseconds)]
    [Arguments("BindTwoWay")]
    [Arguments("BindTwoWayConverting")]
    [Arguments("Bind")]
    public async Task Binding_WithTwoViewEditsBeforeTheQueueDrains_CarriesTheLatestToTheModel(string entryPoint, CancellationToken cancellationToken)
    {
        _ = cancellationToken;
        var outcome = await RunScenarioAsync("Run", [entryPoint, "viewBurst"]);

        await Assert.That(outcome).IsEqualTo($"idle|{SecondViewEdit}|{Shown(entryPoint, SecondViewEdit)}");
    }

    /// <summary>Two model edits before the queue drains leave the view with the second one, and the binding goes idle.</summary>
    /// <param name="entryPoint">The generated entry point that creates the binding.</param>
    /// <param name="cancellationToken">The token that ends the test when it hangs.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Timeout(TimeoutMilliseconds)]
    [Arguments("BindTwoWay")]
    [Arguments("BindTwoWayConverting")]
    [Arguments("Bind")]
    public async Task Binding_WithTwoModelEditsBeforeTheQueueDrains_CarriesTheLatestToTheView(string entryPoint, CancellationToken cancellationToken)
    {
        _ = cancellationToken;
        var outcome = await RunScenarioAsync("Run", [entryPoint, "modelBurst"]);

        await Assert.That(outcome).IsEqualTo($"idle|{SecondModelEdit}|{Shown(entryPoint, SecondModelEdit)}");
    }

    /// <summary>The first value a target path reports after its missing parent appears reaches the model.</summary>
    /// <param name="cancellationToken">The token that ends the test when it hangs.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Timeout(TimeoutMilliseconds)]
    public async Task BindTwoWay_ThroughAMissingParent_CarriesTheFirstValueTheParentReports(CancellationToken cancellationToken)
    {
        _ = cancellationToken;
        var outcome = await RunScenarioAsync("RunThroughAMissingParent", []);

        await Assert.That(outcome).IsEqualTo($"idle|{ViewEdit}");
    }

    /// <summary>Renders a model value the way the view holds it.</summary>
    /// <param name="entryPoint">The entry point that created the binding.</param>
    /// <param name="value">The model value.</param>
    /// <returns>The value the view holds.</returns>
    private static string Shown(string entryPoint, string value) => entryPoint == "BindTwoWay" ? value : $"{value}!";

    /// <summary>Compiles the scenario and runs it once.</summary>
    /// <param name="method">The static method on the scenario's <c>Usage</c> class to run.</param>
    /// <param name="arguments">The arguments the method takes.</param>
    /// <returns>Whether the binding went idle, then the values the scenario reports.</returns>
    private static async Task<string?> RunScenarioAsync(string method, object[] arguments)
    {
        var result = TestHelper.RunGenerator(QueuedSource, LanguageVersion.CSharp10);
        await result.CompilationSucceeds();

        var (assembly, context) = TestHelper.EmitAndLoad(result);
        var run = assembly.GetType("TestApp.Usage")!.GetMethod(method, BindingFlags.Public | BindingFlags.Static)!;

        var outcome = (string?)run.Invoke(null, arguments);

        context.Unload();

        return outcome;
    }
}
