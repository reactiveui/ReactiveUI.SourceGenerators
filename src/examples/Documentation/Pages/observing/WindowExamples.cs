// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Concurrency;

namespace ReactiveUI.Binding.Documentation.Observing;

/// <summary>Opens a window on a stream of property changes and counts what happens inside it.</summary>
public static class WindowExamples
{
    /// <summary>The first note the user types.</summary>
    private const string FirstNote = "Bring the old plates";

    /// <summary>The second note the user types.</summary>
    private const string SecondNote = "Bring the old plates and the receipt";

    /// <summary>The third note the user types.</summary>
    private const string ThirdNote = "Bring the plates, the receipt and a pen";

    /// <summary>The length of a window, in milliseconds.</summary>
    private const int WindowMilliseconds = 100;

    /// <summary>How long a window lasts.</summary>
    private static readonly TimeSpan _windowLength = TimeSpan.FromMilliseconds(WindowMilliseconds);

    /// <summary>Counts the edits of the notes in each 100 ms window, including a window in which the user typed nothing.</summary>
    public static void CountEditsPerWindow()
    {
        VirtualClock clock = new();
        TodoItem item = new();

        using IDisposable subscription = item.WhenChanged(x => x.Notes)
            .Skip(1)
            .Window(_windowLength, clock)
            .SelectMany(static window => window.Count())
            .Subscribe(static count => Console.WriteLine($"Edits in the window: {count}"));

        item.Notes = FirstNote;
        item.Notes = SecondNote;
        item.Notes = ThirdNote;
        clock.AdvanceBy(_windowLength);
        item.Notes = FirstNote;
        clock.AdvanceBy(_windowLength);
        clock.AdvanceBy(_windowLength);

        // Output:
        // Edits in the window: 3
        // Edits in the window: 1
        // Edits in the window: 0
    }

    /// <summary>Counts the edits of the notes made between one tick of the checkbox and the next.</summary>
    public static void CountEditsBetweenTicks()
    {
        TodoItem item = new();

        using IDisposable subscription = item.WhenChanged(x => x.Notes)
            .Skip(1)
            .Window(() => item.WhenChanged(x => x.IsDone).Skip(1))
            .SelectMany(static window => window.Count())
            .Subscribe(static count => Console.WriteLine($"Edits before the checkbox changed: {count}"));

        item.Notes = FirstNote;
        item.Notes = SecondNote;
        item.IsDone = true;
        item.Notes = ThirdNote;
        item.IsDone = false;

        // Output:
        // Edits before the checkbox changed: 2
        // Edits before the checkbox changed: 1
    }
}
