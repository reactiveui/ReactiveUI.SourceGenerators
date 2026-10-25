// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Concurrency;
using ReactiveUI.Primitives.Signals;

namespace ReactiveUI.Binding.Documentation.Observing;

/// <summary>Delivers property changes on a sequencer, either all of them or only the newest one that is waiting.</summary>
public static class LatestValueExamples
{
    /// <summary>The first amount the customer types.</summary>
    private const decimal FirstAmount = 10M;

    /// <summary>The second amount the customer types.</summary>
    private const decimal SecondAmount = 20M;

    /// <summary>The third amount the customer types.</summary>
    private const decimal ThirdAmount = 30M;

    /// <summary>Redraws the preview once for every amount that was typed before the sequencer runs.</summary>
    public static void DeliverEveryAmountOnASequencer()
    {
        VirtualClock uiThread = new();
        TransferDraft draft = new();

        using IDisposable subscription = draft.WhenChanged(x => x.Amount)
            .Skip(1)
            .WitnessOn(uiThread)
            .Subscribe(static amount => Console.WriteLine($"Preview {amount}"));

        draft.Amount = FirstAmount;
        draft.Amount = SecondAmount;
        draft.Amount = ThirdAmount;
        Console.WriteLine("Typed three amounts");
        uiThread.Start();

        // Output:
        // Typed three amounts
        // Preview 10
        // Preview 20
        // Preview 30
    }

    /// <summary>Redraws the preview once with the newest amount, because the sequencer drops the amounts that a newer one replaced.</summary>
    public static void DeliverOnlyTheNewestAmountOnASequencer()
    {
        VirtualClock uiThread = new();
        TransferDraft draft = new();

        using IDisposable subscription = draft.WhenChanged(x => x.Amount)
            .Skip(1)
            .WitnessLatestOn(uiThread)
            .Subscribe(static amount => Console.WriteLine($"Preview {amount}"));

        draft.Amount = FirstAmount;
        draft.Amount = SecondAmount;
        draft.Amount = ThirdAmount;
        Console.WriteLine("Typed three amounts");
        uiThread.Start();

        // Output:
        // Typed three amounts
        // Preview 30
    }
}
