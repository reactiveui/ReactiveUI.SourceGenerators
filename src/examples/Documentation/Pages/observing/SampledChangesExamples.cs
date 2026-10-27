// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Concurrency;

namespace ReactiveUI.Binding.Documentation.Observing;

/// <summary>Slows a fast stream of property changes down with <c>Sample</c> and <c>Throttle</c>, on a virtual clock.</summary>
public static class SampledChangesExamples
{
    /// <summary>The first note the user types.</summary>
    private const string FirstNote = "Bring the old plates";

    /// <summary>The second note the user types.</summary>
    private const string SecondNote = "Bring the old plates and the receipt";

    /// <summary>The third note the user types.</summary>
    private const string ThirdNote = "Bring the plates, the receipt and a pen";

    /// <summary>The amount the customer drags the slider to first.</summary>
    private const decimal FirstAmount = 10M;

    /// <summary>The amount the customer drags the slider to second.</summary>
    private const decimal SecondAmount = 20M;

    /// <summary>The amount the customer drags the slider to third.</summary>
    private const decimal ThirdAmount = 30M;

    /// <summary>The amount the customer drags the slider to last.</summary>
    private const decimal LastAmount = 40M;

    /// <summary>How often the preview is drawn, in milliseconds.</summary>
    private const int SampleMilliseconds = 100;

    /// <summary>How long the user must stop typing before the notes are saved, in milliseconds.</summary>
    private const int QuietMilliseconds = 300;

    /// <summary>The time between two key presses, in milliseconds.</summary>
    private const int KeyGapMilliseconds = 100;

    /// <summary>How often the preview is drawn.</summary>
    private static readonly TimeSpan _sampleInterval = TimeSpan.FromMilliseconds(SampleMilliseconds);

    /// <summary>How long the user must stop typing before the notes are saved.</summary>
    private static readonly TimeSpan _quietPeriod = TimeSpan.FromMilliseconds(QuietMilliseconds);

    /// <summary>How long the user takes between two key presses.</summary>
    private static readonly TimeSpan _keyGap = TimeSpan.FromMilliseconds(KeyGapMilliseconds);

    /// <summary>Draws the amount preview once every 100 ms with the newest amount, while the customer drags the slider.</summary>
    public static void SampleAmountOnAnInterval()
    {
        VirtualClock clock = new();
        TransferDraft draft = new();

        using IDisposable subscription = draft.WhenChanged(x => x.Amount)
            .Skip(1)
            .Sample(_sampleInterval, clock)
            .Subscribe(static amount => Console.WriteLine($"Preview {amount}"));

        draft.Amount = FirstAmount;
        draft.Amount = SecondAmount;
        clock.AdvanceBy(_sampleInterval);
        draft.Amount = ThirdAmount;
        draft.Amount = LastAmount;
        clock.AdvanceBy(_sampleInterval);
        clock.AdvanceBy(_sampleInterval);

        // Output:
        // Preview 20
        // Preview 40
    }

    /// <summary>Saves the notes only once the user has stopped typing for 300 ms.</summary>
    public static void ThrottleNotesUntilTheUserPauses()
    {
        VirtualClock clock = new();
        TodoItem item = new();

        using IDisposable subscription = item.WhenChanged(x => x.Notes)
            .Skip(1)
            .Throttle(_quietPeriod, clock)
            .Subscribe(static notes => Console.WriteLine($"Saved: {notes}"));

        item.Notes = FirstNote;
        clock.AdvanceBy(_keyGap);
        item.Notes = SecondNote;
        clock.AdvanceBy(_keyGap);
        item.Notes = ThirdNote;
        clock.AdvanceBy(_quietPeriod);

        // Output:
        // Saved: Bring the plates, the receipt and a pen
    }
}
