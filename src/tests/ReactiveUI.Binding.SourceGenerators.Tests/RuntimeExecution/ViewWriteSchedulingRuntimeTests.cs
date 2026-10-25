// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.RuntimeExecution;

/// <summary>Covers which thread a generated binding writes to the view on.</summary>
[NotInParallel]
public class ViewWriteSchedulingRuntimeTests
{
    /// <summary>What a scenario returns when the write ran on the calling thread.</summary>
    private const string Inline = "inline";

    /// <summary>What a scenario returns when the write went through the host's main thread.</summary>
    private const string Scheduled = "scheduled";

    /// <summary>What a scenario returns when the write went through the platform's own dispatcher.</summary>
    private const string Dispatched = "dispatched";

    /// <summary>What a burst scenario returns when both sides end on the latest value.</summary>
    private const string Settled = "settled";

    /// <summary>Bindings onto stand-ins for WPF, WinForms and MAUI views, and onto a plain object.</summary>
    private const string SchedulingSource = """
                                            using System;
                                            using System.ComponentModel;
                                            using ReactiveUI.Binding;
                                            using ReactiveUI.Primitives.Concurrency;

                                            namespace TestApp
                                            {
                                                public class RecordingSequencer : ISequencer
                                                {
                                                    public bool Used { get; private set; }

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
                                                        Used = true;
                                                        ImmediateSequencer.Instance.Schedule(item);
                                                    }

                                                    public void Schedule(IWorkItem item, long dueTimestamp)
                                                    {
                                                        Used = true;
                                                        ImmediateSequencer.Instance.Schedule(item, dueTimestamp);
                                                    }
                                                }

                                                public class MyViewModel : INotifyPropertyChanged
                                                {
                                                    private string _name = "";

                                                    public event PropertyChangedEventHandler PropertyChanged;

                                                    public string Name
                                                    {
                                                        get { return _name; }
                                                        set
                                                        {
                                                            _name = value;
                                                            var handler = PropertyChanged;
                                                            if (handler != null)
                                                            {
                                                                handler(this, new PropertyChangedEventArgs("Name"));
                                                            }
                                                        }
                                                    }
                                                }

                                                public class PlainView
                                                {
                                                    public string DisplayName { get; set; } = "";
                                                }

                                                public class WpfView : System.Windows.Threading.DispatcherObject
                                                {
                                                    public string DisplayName { get; set; } = "";
                                                }

                                                public class TwoWayWpfView : System.Windows.Threading.DispatcherObject, INotifyPropertyChanged
                                                {
                                                    private string _displayName = "";

                                                    public event PropertyChangedEventHandler PropertyChanged;

                                                    public int Writes { get; private set; }

                                                    public string DisplayName
                                                    {
                                                        get { return _displayName; }
                                                        set
                                                        {
                                                            Writes++;
                                                            if (Writes > 1000)
                                                            {
                                                                throw new InvalidOperationException("The binding kept writing.");
                                                            }

                                                            _displayName = value;
                                                            var handler = PropertyChanged;
                                                            if (handler != null)
                                                            {
                                                                handler(this, new PropertyChangedEventArgs("DisplayName"));
                                                            }
                                                        }
                                                    }
                                                }

                                                public class BoundWpfView : TwoWayWpfView, IViewFor
                                                {
                                                    public object ViewModel { get; set; }
                                                }

                                                public class WinFormsView : System.Windows.Forms.Control
                                                {
                                                    public string DisplayName { get; set; } = "";
                                                }

                                                public class MauiView : Microsoft.Maui.Controls.BindableObject
                                                {
                                                    public string DisplayName { get; set; } = "";
                                                }

                                                public class RecordingDispatcher : Microsoft.Maui.Dispatching.IDispatcher
                                                {
                                                    public int Posts { get; private set; }

                                                    public bool IsDispatchRequired
                                                    {
                                                        get { return true; }
                                                    }

                                                    public bool Dispatch(Action action)
                                                    {
                                                        Posts++;
                                                        action();
                                                        return true;
                                                    }
                                                }

                                                public class ManualStream : IObservable<string>
                                                {
                                                    private IObserver<string> _observer;

                                                    public void Push(string value)
                                                    {
                                                        _observer.OnNext(value);
                                                    }

                                                    public IDisposable Subscribe(IObserver<string> observer)
                                                    {
                                                        _observer = observer;
                                                        return new Subscription();
                                                    }

                                                    private sealed class Subscription : IDisposable
                                                    {
                                                        public void Dispose()
                                                        {
                                                        }
                                                    }
                                                }

                                                public static class Usage
                                                {
                                                    public static string OneWayToAPlainView()
                                                    {
                                                        var sequencer = new RecordingSequencer();
                                                        BindingSchedulers.MainThread = sequencer;

                                                        try
                                                        {
                                                            var viewModel = new MyViewModel();
                                                            var view = new PlainView();
                                                            var binding = viewModel.BindOneWay(view, x => x.Name, x => x.DisplayName);

                                                            viewModel.Name = "changed";

                                                            return Describe(view.DisplayName, sequencer.Used, 0);
                                                        }
                                                        finally
                                                        {
                                                            BindingSchedulers.MainThread = null;
                                                        }
                                                    }

                                                    public static string OneWayToAWpfViewOnItsThread()
                                                    {
                                                        var sequencer = new RecordingSequencer();
                                                        BindingSchedulers.MainThread = sequencer;

                                                        try
                                                        {
                                                            var viewModel = new MyViewModel();
                                                            var view = new WpfView();
                                                            view.HasAccess = true;
                                                            var binding = viewModel.BindOneWay(view, x => x.Name, x => x.DisplayName);

                                                            viewModel.Name = "changed";

                                                            return Describe(view.DisplayName, sequencer.Used, view.Dispatcher.Posts);
                                                        }
                                                        finally
                                                        {
                                                            BindingSchedulers.MainThread = null;
                                                        }
                                                    }

                                                    public static string OneWayToAWpfViewFromAnotherThread()
                                                    {
                                                        var viewModel = new MyViewModel();
                                                        var view = new WpfView();
                                                        var binding = viewModel.BindOneWay(view, x => x.Name, x => x.DisplayName);

                                                        viewModel.Name = "changed";

                                                        return Describe(view.DisplayName, false, view.Dispatcher.Posts);
                                                    }

                                                    public static string OneWayToAWpfViewFromAnotherThreadWithAMainThread()
                                                    {
                                                        var sequencer = new RecordingSequencer();
                                                        BindingSchedulers.MainThread = sequencer;

                                                        try
                                                        {
                                                            var viewModel = new MyViewModel();
                                                            var view = new WpfView();
                                                            var binding = viewModel.BindOneWay(view, x => x.Name, x => x.DisplayName);

                                                            viewModel.Name = "changed";

                                                            return Describe(view.DisplayName, sequencer.Used, view.Dispatcher.Posts);
                                                        }
                                                        finally
                                                        {
                                                            BindingSchedulers.MainThread = null;
                                                        }
                                                    }

                                                    public static string BindToAWpfViewFromAnotherThread()
                                                    {
                                                        var stream = new ManualStream();
                                                        var view = new WpfView();
                                                        var binding = stream.BindTo(view, x => x.DisplayName);

                                                        stream.Push("changed");

                                                        return Describe(view.DisplayName, false, view.Dispatcher.Posts);
                                                    }

                                                    public static string OneWayToAWinFormsViewFromAnotherThread()
                                                    {
                                                        var viewModel = new MyViewModel();
                                                        var view = new WinFormsView();
                                                        view.InvokeRequired = true;
                                                        var binding = viewModel.BindOneWay(view, x => x.Name, x => x.DisplayName);

                                                        viewModel.Name = "changed";

                                                        return Describe(view.DisplayName, false, view.Posts);
                                                    }

                                                    public static string OneWayToAMauiViewFromAnotherThread()
                                                    {
                                                        var viewModel = new MyViewModel();
                                                        var view = new MauiView();
                                                        var dispatcher = new RecordingDispatcher();
                                                        view.UseDispatcher(dispatcher);
                                                        var binding = viewModel.BindOneWay(view, x => x.Name, x => x.DisplayName);

                                                        viewModel.Name = "changed";

                                                        return Describe(view.DisplayName, false, dispatcher.Posts);
                                                    }

                                                    public static string OneWayToAMauiViewWithNoDispatcher()
                                                    {
                                                        var viewModel = new MyViewModel();
                                                        var view = new MauiView();
                                                        var binding = viewModel.BindOneWay(view, x => x.Name, x => x.DisplayName);

                                                        viewModel.Name = "changed";

                                                        return Describe(view.DisplayName, false, 0);
                                                    }

                                                    public static string TwoWayBurstFromAnotherThread()
                                                    {
                                                        var viewModel = new MyViewModel();
                                                        var view = new TwoWayWpfView();
                                                        view.Dispatcher.Holds = true;
                                                        var binding = viewModel.BindTwoWay(view, x => x.Name, x => x.DisplayName);

                                                        return RunBurst(viewModel, view);
                                                    }

                                                    public static string ViewFirstBindBurstFromAnotherThread()
                                                    {
                                                        var viewModel = new MyViewModel();
                                                        var view = new BoundWpfView();
                                                        view.Dispatcher.Holds = true;
                                                        var binding = view.Bind(viewModel, x => x.Name, x => x.DisplayName);

                                                        return RunBurst(viewModel, view);
                                                    }

                                                    private static string RunBurst(MyViewModel viewModel, TwoWayWpfView view)
                                                    {
                                                        try
                                                        {
                                                            view.Dispatcher.Pump(view);
                                                            viewModel.Name = "A";
                                                            viewModel.Name = "B";
                                                            view.Dispatcher.Pump(view);
                                                        }
                                                        catch (Exception)
                                                        {
                                                            return "kept writing";
                                                        }

                                                        return viewModel.Name == "B" && view.DisplayName == "B"
                                                            ? "settled"
                                                            : "stale: " + viewModel.Name + " / " + view.DisplayName;
                                                    }

                                                    private static string Describe(string written, bool scheduled, int dispatched)
                                                    {
                                                        if (written != "changed")
                                                        {
                                                            return "not written";
                                                        }

                                                        if (scheduled)
                                                        {
                                                            return "scheduled";
                                                        }

                                                        return dispatched > 0 ? "dispatched" : "inline";
                                                    }
                                                }
                                            }

                                            namespace System.Windows.Threading
                                            {
                                                public enum DispatcherPriority
                                                {
                                                    Normal = 9,
                                                }

                                                public class Dispatcher
                                                {
                                                    private readonly System.Collections.Generic.Queue<Action> _held = new System.Collections.Generic.Queue<Action>();

                                                    public int Posts { get; private set; }

                                                    public bool Holds { get; set; }

                                                    public object BeginInvoke(DispatcherPriority priority, Delegate method, object arg)
                                                    {
                                                        Posts++;
                                                        if (Holds)
                                                        {
                                                            _held.Enqueue(() => method.DynamicInvoke(arg));
                                                            return null;
                                                        }

                                                        method.DynamicInvoke(arg);
                                                        return null;
                                                    }

                                                    public void Pump(DispatcherObject owner)
                                                    {
                                                        owner.HasAccess = true;
                                                        try
                                                        {
                                                            while (_held.Count > 0)
                                                            {
                                                                _held.Dequeue()();
                                                            }
                                                        }
                                                        finally
                                                        {
                                                            owner.HasAccess = false;
                                                        }
                                                    }
                                                }

                                                public class DispatcherObject
                                                {
                                                    private readonly Dispatcher _dispatcher = new Dispatcher();

                                                    public Dispatcher Dispatcher
                                                    {
                                                        get { return _dispatcher; }
                                                    }

                                                    public bool HasAccess { get; set; }

                                                    public bool CheckAccess()
                                                    {
                                                        return HasAccess;
                                                    }
                                                }
                                            }

                                            namespace System.Windows.Forms
                                            {
                                                public class Control
                                                {
                                                    public int Posts { get; private set; }

                                                    public bool InvokeRequired { get; set; }

                                                    public IAsyncResult BeginInvoke(Delegate method, params object[] args)
                                                    {
                                                        Posts++;
                                                        method.DynamicInvoke(args);
                                                        return null;
                                                    }
                                                }
                                            }

                                            namespace Microsoft.Maui.Dispatching
                                            {
                                                public interface IDispatcher
                                                {
                                                    bool IsDispatchRequired { get; }

                                                    bool Dispatch(Action action);
                                                }
                                            }

                                            namespace Microsoft.Maui.Controls
                                            {
                                                public class BindableObject
                                                {
                                                    private Microsoft.Maui.Dispatching.IDispatcher _dispatcher;

                                                    public Microsoft.Maui.Dispatching.IDispatcher Dispatcher
                                                    {
                                                        get
                                                        {
                                                            if (_dispatcher == null)
                                                            {
                                                                throw new InvalidOperationException("No dispatcher.");
                                                            }

                                                            return _dispatcher;
                                                        }
                                                    }

                                                    public void UseDispatcher(Microsoft.Maui.Dispatching.IDispatcher dispatcher)
                                                    {
                                                        _dispatcher = dispatcher;
                                                    }
                                                }
                                            }
                                            """;

    /// <summary>A write to an object no platform claims runs inline, even when the host set a main thread.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_ToAnObjectNoPlatformClaims_WritesInline() =>
        await Assert.That(await RunScenarioAsync("OneWayToAPlainView")).IsEqualTo(Inline);

    /// <summary>A write on a WPF view's own thread runs inline, even when the host set a main thread.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_ToAWpfViewOnItsOwnThread_WritesInline() =>
        await Assert.That(await RunScenarioAsync("OneWayToAWpfViewOnItsThread")).IsEqualTo(Inline);

    /// <summary>A write from another thread goes through the WPF view's dispatcher.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_ToAWpfViewFromAnotherThread_GoesThroughItsDispatcher() =>
        await Assert.That(await RunScenarioAsync("OneWayToAWpfViewFromAnotherThread")).IsEqualTo(Dispatched);

    /// <summary>A write from another thread goes through the main thread the host set.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_ToAWpfViewFromAnotherThreadWithAMainThread_GoesThroughTheMainThread() =>
        await Assert.That(await RunScenarioAsync("OneWayToAWpfViewFromAnotherThreadWithAMainThread")).IsEqualTo(Scheduled);

    /// <summary>A stream's write from another thread goes through the WPF view's dispatcher.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindTo_ToAWpfViewFromAnotherThread_GoesThroughItsDispatcher() =>
        await Assert.That(await RunScenarioAsync("BindToAWpfViewFromAnotherThread")).IsEqualTo(Dispatched);

    /// <summary>A write from another thread goes through the WinForms control.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_ToAWinFormsControlFromAnotherThread_GoesThroughTheControl() =>
        await Assert.That(await RunScenarioAsync("OneWayToAWinFormsViewFromAnotherThread")).IsEqualTo(Dispatched);

    /// <summary>A write from another thread goes through the MAUI object's dispatcher.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_ToAMauiObjectFromAnotherThread_GoesThroughItsDispatcher() =>
        await Assert.That(await RunScenarioAsync("OneWayToAMauiViewFromAnotherThread")).IsEqualTo(Dispatched);

    /// <summary>A write to a MAUI object with no dispatcher runs inline instead of throwing.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_ToAMauiObjectWithNoDispatcher_WritesInline() =>
        await Assert.That(await RunScenarioAsync("OneWayToAMauiViewWithNoDispatcher")).IsEqualTo(Inline);

    /// <summary>Two changes from another thread before the view's thread runs leave a two-way binding on the latest value.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindTwoWay_ABurstFromAnotherThread_SettlesOnTheLatestValue() =>
        await Assert.That(await RunScenarioAsync("TwoWayBurstFromAnotherThread")).IsEqualTo(Settled);

    /// <summary>Two changes from another thread before the view's thread runs leave a view-first binding on the latest value.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Bind_ABurstFromAnotherThread_SettlesOnTheLatestValue() =>
        await Assert.That(await RunScenarioAsync("ViewFirstBindBurstFromAnotherThread")).IsEqualTo(Settled);

    /// <summary>Compiles the scenario, runs one of its entry points, and reports where the write was delivered.</summary>
    /// <param name="entryPoint">The static method on the scenario's <c>Usage</c> class to run.</param>
    /// <returns>What the entry point reported.</returns>
    private static async Task<string?> RunScenarioAsync(string entryPoint)
    {
        var result = TestHelper.RunGenerator(
            SchedulingSource + RuntimeInvokerStandIns.Wpf + RuntimeInvokerStandIns.WinForms + RuntimeInvokerStandIns.Maui,
            LanguageVersion.CSharp10);
        await result.CompilationSucceeds();

        var (assembly, context) = TestHelper.EmitAndLoad(result);
        var run = assembly.GetType("TestApp.Usage")!.GetMethod(entryPoint, BindingFlags.Public | BindingFlags.Static)!;

        var outcome = (string?)run.Invoke(null, null);

        context.Unload();

        return outcome;
    }
}
