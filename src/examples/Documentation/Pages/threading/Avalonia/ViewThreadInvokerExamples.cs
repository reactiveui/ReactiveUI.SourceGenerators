// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Avalonia.Controls;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Primitives;
using Splat;
using Dispatcher = Avalonia.Threading.Dispatcher;
using MauiLabel = Microsoft.Maui.Controls.Label;

namespace ReactiveUI.Binding.Documentation.Threading;

/// <summary>Shows how a binding finds the thread that owns a control: <c>IViewThreadInvoker</c> and <c>ViewThreadInvokers</c>, on Avalonia's UI thread.</summary>
public static class ViewThreadInvokerExamples
{
    /// <summary>The title of the item before the sync.</summary>
    private const string OriginalTitle = "Renew car registration";

    /// <summary>The title the user types on the UI thread.</summary>
    private const string RenamedTitle = "Renew car registration online";

    /// <summary>The first title a background sync delivers.</summary>
    private const string FirstSyncedTitle = "Renew car registration (draft)";

    /// <summary>The second title a background sync delivers.</summary>
    private const string SecondSyncedTitle = "Renew car registration (reviewed)";

    /// <summary>The last title a background sync delivers.</summary>
    private const string LastSyncedTitle = "Renew car registration (final)";

    /// <summary>What the examples print for a label that has no text.</summary>
    private const string NoText = "(no text)";

    /// <summary>The text a posted callback records.</summary>
    private const string PostedText = "posted";

    /// <summary>Registers the invoker for Avalonia objects. The binding engine asks each registered invoker whether it claims the control a binding writes to.</summary>
    public static void RegisterInvoker()
    {
        TextBlock titleLabel = new();
        MauiLabel unclaimed = new();
        AvaloniaViewThreadInvoker invoker = new();

        AppLocator.CurrentMutable.RegisterConstant<IViewThreadInvoker>(invoker);
        ViewThreadInvokers.Refresh();

        Console.WriteLine(ReferenceEquals(ViewThreadInvokers.ForTarget(titleLabel), invoker));
        Console.WriteLine(ViewThreadInvokers.ForTarget(unclaimed) is null);
        Console.WriteLine(ViewThreadInvokers.ForTarget(null) is null);

        // Output:
        // True
        // True
        // True
    }

    /// <summary>Calls the three members of the invoker directly: who it claims, who may write, and how to queue work.</summary>
    public static void CallInvokerMembers()
    {
        TextBlock titleLabel = new();
        MauiLabel unclaimed = new();
        AvaloniaViewThreadInvoker invoker = new();
        List<string> log = [];
        bool accessFromWorker = true;

        Thread worker = new Thread(() => accessFromWorker = invoker.CheckAccess(titleLabel));
        worker.Start();
        worker.Join();
        invoker.Post(titleLabel, static state => ((List<string>)state!).Add(PostedText), log);

        Console.WriteLine(invoker.Claims(titleLabel));
        Console.WriteLine(invoker.Claims(unclaimed));
        Console.WriteLine(invoker.CheckAccess(titleLabel));
        Console.WriteLine(accessFromWorker);
        Console.WriteLine(log.Count);

        Dispatcher.UIThread.RunJobs();

        Console.WriteLine(string.Join(", ", log));

        // Output:
        // True
        // False
        // True
        // False
        // 0
        // posted
    }

    /// <summary>Shows that a control refuses a write from another thread, which is what the invoker keeps a binding from doing.</summary>
    public static void RefuseWriteFromWorkerThread()
    {
        TextBlock titleLabel = new();
        bool refused = false;

        Thread worker = new Thread(() =>
        {
            try
            {
                titleLabel.Text = RenamedTitle;
            }
            catch (InvalidOperationException)
            {
                refused = true;
            }
        });
        worker.Start();
        worker.Join();

        Console.WriteLine(refused);
        Console.WriteLine(titleLabel.Text ?? NoText);

        // Output:
        // True
        // (no text)
    }

    /// <summary>Writes a bound value on the owning thread: the write runs inline and nothing waits.</summary>
    public static void WriteOnOwningThreadRunsInline()
    {
        TextBlock titleLabel = new();
        TodoItem item = new() { Title = OriginalTitle };

        using (item.BindOneWay(titleLabel, x => x.Title, x => x.Text))
        {
            Console.WriteLine(titleLabel.Text);

            item.Title = RenamedTitle;

            Console.WriteLine(titleLabel.Text);
        }

        // Output:
        // Renew car registration
        // Renew car registration online
    }

    /// <summary>Writes a bound value from another thread: the write waits until the owning thread runs its queue.</summary>
    public static void WriteFromWorkerThreadWaitsForOwningThread()
    {
        TextBlock titleLabel = new();
        TodoItem item = new() { Title = OriginalTitle };

        using (item.BindOneWay(titleLabel, x => x.Title, x => x.Text))
        {
            Thread worker = new Thread(() => item.Title = FirstSyncedTitle);
            worker.Start();
            worker.Join();

            Console.WriteLine(titleLabel.Text);

            Dispatcher.UIThread.RunJobs();

            Console.WriteLine(titleLabel.Text);
        }

        // Output:
        // Renew car registration
        // Renew car registration (draft)
    }

    /// <summary>Changes a bound value three times before the owning thread runs: only the latest value is written.</summary>
    public static void LatestValueWins()
    {
        TextBlock titleLabel = new();
        TodoItem item = new() { Title = OriginalTitle };
        List<string?> writes = [];

        using (titleLabel.WhenChanged(x => x.Text).Subscribe(writes.Add))
        using (item.BindOneWay(titleLabel, x => x.Title, x => x.Text))
        {
            Thread worker = new Thread(() =>
            {
                item.Title = FirstSyncedTitle;
                item.Title = SecondSyncedTitle;
                item.Title = LastSyncedTitle;
            });
            worker.Start();
            worker.Join();

            Dispatcher.UIThread.RunJobs();

            Console.WriteLine(string.Join(" | ", writes.Select(static text => text ?? NoText)));
        }

        // Output:
        // (no text) | Renew car registration | Renew car registration (final)
    }
}
