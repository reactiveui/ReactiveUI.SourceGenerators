// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Primitives;

namespace ReactiveUI.Binding.Documentation.Observing;

/// <summary>Folds a stream of property changes into a running value with <c>Scan</c>.</summary>
public static class RunningTotalExamples
{
    /// <summary>The first note the user types.</summary>
    private const string FirstNote = "Bring the old plates";

    /// <summary>The second note the user types.</summary>
    private const string SecondNote = "Bring the old plates and the receipt";

    /// <summary>The third note the user types.</summary>
    private const string ThirdNote = "Bring the plates, the receipt and a pen";

    /// <summary>The first amount the customer types.</summary>
    private const decimal FirstAmount = 250M;

    /// <summary>The second amount the customer types.</summary>
    private const decimal SecondAmount = 90M;

    /// <summary>The third amount the customer types.</summary>
    private const decimal ThirdAmount = 400M;

    /// <summary>Counts the edits of the notes since the item was opened.</summary>
    public static void CountEditsOfTheNotes()
    {
        TodoItem item = new();

        using IDisposable subscription = item.WhenChanged(x => x.Notes)
            .Skip(1)
            .Scan(0, static (count, _) => count + 1)
            .Subscribe(static count => Console.WriteLine($"Edits: {count}"));

        item.Notes = FirstNote;
        item.Notes = SecondNote;
        item.Notes = ThirdNote;

        // Output:
        // Edits: 1
        // Edits: 2
        // Edits: 3
    }

    /// <summary>Keeps the highest amount the customer has typed so far.</summary>
    public static void TrackTheHighestAmount()
    {
        TransferDraft draft = new();

        using IDisposable subscription = draft.WhenChanged(x => x.Amount)
            .Scan(0M, Math.Max)
            .Subscribe(static highest => Console.WriteLine($"Highest: {highest}"));

        draft.Amount = FirstAmount;
        draft.Amount = SecondAmount;
        draft.Amount = ThirdAmount;

        // Output:
        // Highest: 0
        // Highest: 250
        // Highest: 250
        // Highest: 400
    }
}
