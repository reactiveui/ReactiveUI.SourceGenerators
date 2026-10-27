// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Avalonia.Controls;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Concurrency;
using Dispatcher = Avalonia.Threading.Dispatcher;
using MauiLabel = Microsoft.Maui.Controls.Label;

namespace ReactiveUI.Binding.Documentation.Threading;

/// <summary>Shows where a binding delivers its writes: <c>BindingSchedulers</c>, on Avalonia's UI thread.</summary>
public static class BindingSchedulersExamples
{
    /// <summary>The title of the item before the sync.</summary>
    private const string OriginalTitle = "Renew car registration";

    /// <summary>The title the user types on the UI thread.</summary>
    private const string RenamedTitle = "Renew car registration online";

    /// <summary>The first title a background sync delivers.</summary>
    private const string DraftTitle = "Renew car registration (draft)";

    /// <summary>The title a background sync delivers.</summary>
    private const string SyncedTitle = "Renew car registration (reviewed)";

    /// <summary>Delivers writes from another thread on the host's main-thread sequencer, when the control is one the UI thread owns.</summary>
    public static void SetMainThread()
    {
        TextBlock titleLabel = new();
        TodoItem item = new() { Title = OriginalTitle };
        AvaloniaScheduler sequencer = AvaloniaScheduler.Instance;
        BindingSchedulers.MainThread = sequencer;

        try
        {
            using (item.BindOneWay(titleLabel, x => x.Title, x => x.Text))
            {
                Thread worker = new Thread(() => item.Title = SyncedTitle);
                worker.Start();
                worker.Join();

                Console.WriteLine(ReferenceEquals(BindingSchedulers.MainThread, sequencer));
                Console.WriteLine(titleLabel.Text);

                Dispatcher.UIThread.RunJobs();

                Console.WriteLine(titleLabel.Text);
            }
        }
        finally
        {
            BindingSchedulers.MainThread = null;
        }

        // Output:
        // True
        // Renew car registration
        // Renew car registration (reviewed)
    }

    /// <summary>Writes on the owning thread: the sequencer never sees the write, because there is nothing to carry.</summary>
    public static void WriteOnOwningThreadSkipsMainThread()
    {
        TextBlock titleLabel = new();
        TodoItem item = new() { Title = OriginalTitle };
        BindingSchedulers.MainThread = AvaloniaScheduler.Instance;

        try
        {
            using (item.BindOneWay(titleLabel, x => x.Title, x => x.Text))
            {
                item.Title = RenamedTitle;

                Console.WriteLine(titleLabel.Text);
            }
        }
        finally
        {
            BindingSchedulers.MainThread = null;
        }

        // Output:
        // Renew car registration online
    }

    /// <summary>Writes to a control no invoker claims: the write runs on the thread that raised it, and the sequencer never sees it.</summary>
    public static void WriteToUnclaimedControlSkipsMainThread()
    {
        MauiLabel titleLabel = new();
        TodoItem item = new() { Title = OriginalTitle };
        BindingSchedulers.MainThread = AvaloniaScheduler.Instance;

        try
        {
            using (item.BindOneWay(titleLabel, x => x.Title, x => x.Text))
            {
                Thread worker = new Thread(() => item.Title = SyncedTitle);
                worker.Start();
                worker.Join();

                Console.WriteLine(titleLabel.Text);
            }
        }
        finally
        {
            BindingSchedulers.MainThread = null;
        }

        // Output:
        // Renew car registration (reviewed)
    }

    /// <summary>Delivers writes from another thread through the synchronization context of the UI thread.</summary>
    public static void UseSynchronizationContext()
    {
        TextBlock titleLabel = new();
        TodoItem item = new() { Title = OriginalTitle };
        BindingSchedulers.UseSynchronizationContext(SynchronizationContext.Current);

        try
        {
            Console.WriteLine(BindingSchedulers.MainThread is SynchronizationContextSequencer);

            using (item.BindOneWay(titleLabel, x => x.Title, x => x.Text))
            {
                Thread worker = new Thread(() => item.Title = SyncedTitle);
                worker.Start();
                worker.Join();

                Console.WriteLine(titleLabel.Text);

                Dispatcher.UIThread.RunJobs();

                Console.WriteLine(titleLabel.Text);
            }
        }
        finally
        {
            BindingSchedulers.UseSynchronizationContext(null);
        }

        Console.WriteLine(BindingSchedulers.MainThread is null);

        // Output:
        // True
        // Renew car registration
        // Renew car registration (reviewed)
        // True
    }

    /// <summary>Routes a stream onto the thread that owns a control, with the registered invokers.</summary>
    public static void ObserveOnViewThread()
    {
        TextBlock titleLabel = new();
        TodoItem item = new() { Title = OriginalTitle };
        List<string> delivered = [];

        IObservable<string> routed = BindingSchedulers.ObserveOnViewThread(item.WhenChanged(x => x.Title), titleLabel);

        using (routed.Subscribe(delivered.Add))
        {
            Thread worker = new Thread(() => item.Title = SyncedTitle);
            worker.Start();
            worker.Join();

            Console.WriteLine(string.Join(", ", delivered));

            Dispatcher.UIThread.RunJobs();

            Console.WriteLine(string.Join(", ", delivered));
        }

        // Output:
        // Renew car registration
        // Renew car registration, Renew car registration (reviewed)
    }

    /// <summary>Routes a stream with a fallback invoker, for a control that no registered invoker claims.</summary>
    public static void ObserveOnViewThreadWithFallback()
    {
        TextBlock titleLabel = new();
        TodoItem item = new() { Title = OriginalTitle };
        AvaloniaViewThreadInvoker fallback = new();
        List<string> delivered = [];

        IObservable<string> routed = BindingSchedulers.ObserveOnViewThread(item.WhenChanged(x => x.Title), titleLabel, fallback);

        using (routed.Subscribe(delivered.Add))
        {
            Thread worker = new Thread(() => item.Title = SyncedTitle);
            worker.Start();
            worker.Join();

            Console.WriteLine(string.Join(", ", delivered));

            Dispatcher.UIThread.RunJobs();

            Console.WriteLine(string.Join(", ", delivered));
        }

        // Output:
        // Renew car registration
        // Renew car registration, Renew car registration (reviewed)
    }

    /// <summary>Routes a stream onto a sequencer: every delivery waits for the sequencer, and a newer value replaces one that is still waiting.</summary>
    public static void ObserveOnSequencer()
    {
        TodoItem item = new() { Title = OriginalTitle };
        AvaloniaScheduler sequencer = AvaloniaScheduler.Instance;
        List<string> delivered = [];

        IObservable<string> routed = BindingSchedulers.ObserveOnSequencer(item.WhenChanged(x => x.Title), sequencer);

        using (routed.Subscribe(delivered.Add))
        {
            item.Title = DraftTitle;
            item.Title = SyncedTitle;

            Console.WriteLine(delivered.Count);

            Dispatcher.UIThread.RunJobs();

            Console.WriteLine(string.Join(", ", delivered));
        }

        // Output:
        // 0
        // Renew car registration (reviewed)
    }
}
