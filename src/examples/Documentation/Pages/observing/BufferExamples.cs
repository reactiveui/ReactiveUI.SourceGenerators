// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Concurrency;

namespace ReactiveUI.Binding.Documentation.Observing;

/// <summary>Collects property changes into lists with <c>Buffer</c>: the consumer receives each list once it is full or its time is up.</summary>
public static class BufferExamples
{
    /// <summary>The first reference the customer types.</summary>
    private const string FirstReference = "Rent";

    /// <summary>The second reference the customer types.</summary>
    private const string SecondReference = "Rent March";

    /// <summary>The third reference the customer types.</summary>
    private const string ThirdReference = "Rent March 2026";

    /// <summary>The fourth reference the customer types.</summary>
    private const string FourthReference = "Rent April 2026";

    /// <summary>The number of edits in one list.</summary>
    private const int EditsPerList = 3;

    /// <summary>The number of edits between the start of one list and the start of the next.</summary>
    private const int EditsBetweenLists = 1;

    /// <summary>The length of a time buffer, in milliseconds.</summary>
    private const int BufferMilliseconds = 100;

    /// <summary>The text between the values of a list.</summary>
    private const string Separator = " | ";

    /// <summary>How long a time buffer lasts.</summary>
    private static readonly TimeSpan _bufferLength = TimeSpan.FromMilliseconds(BufferMilliseconds);

    /// <summary>Delivers the reference edits in lists of three.</summary>
    public static void BufferEditsByCount()
    {
        TransferDraft draft = new();

        using IDisposable subscription = draft.WhenChanged(x => x.Reference)
            .Skip(1)
            .Buffer(EditsPerList)
            .Subscribe(static edits => Console.WriteLine(string.Join(Separator, edits)));

        draft.Reference = FirstReference;
        draft.Reference = SecondReference;
        draft.Reference = ThirdReference;
        draft.Reference = FourthReference;

        // Output:
        // Rent | Rent March | Rent March 2026
    }

    /// <summary>Starts a new list after every edit, so each list holds the last three edits.</summary>
    public static void BufferTheLastEdits()
    {
        TransferDraft draft = new();

        using IDisposable subscription = draft.WhenChanged(x => x.Reference)
            .Skip(1)
            .Buffer(EditsPerList, EditsBetweenLists)
            .Subscribe(static edits => Console.WriteLine(string.Join(Separator, edits)));

        draft.Reference = FirstReference;
        draft.Reference = SecondReference;
        draft.Reference = ThirdReference;
        draft.Reference = FourthReference;

        // Output:
        // Rent | Rent March | Rent March 2026
        // Rent March | Rent March 2026 | Rent April 2026
    }

    /// <summary>Delivers the reference edits made in each 100 ms period on a virtual clock.</summary>
    public static void BufferEditsByTime()
    {
        VirtualClock clock = new();
        TransferDraft draft = new();

        using IDisposable subscription = draft.WhenChanged(x => x.Reference)
            .Skip(1)
            .Buffer(_bufferLength, clock)
            .Subscribe(static edits => Console.WriteLine($"Batch of {edits.Count}: {string.Join(Separator, edits)}"));

        draft.Reference = FirstReference;
        draft.Reference = SecondReference;
        clock.AdvanceBy(_bufferLength);
        draft.Reference = ThirdReference;
        clock.AdvanceBy(_bufferLength);

        // Output:
        // Batch of 2: Rent | Rent March
        // Batch of 1: Rent March 2026
    }
}
