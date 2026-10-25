// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Concurrency;
using ReactiveUI.Primitives.Signals;

namespace ReactiveUI.Binding.Documentation.Observing;

/// <summary>Cuts a stream of property changes into slices, so a burst of edits reaches the consumer as one batch.</summary>
public static class SliceExamples
{
    /// <summary>The title a new task starts with.</summary>
    private const string DraftTitle = "Draft";

    /// <summary>The first title the user types.</summary>
    private const string FirstTitle = "Renew";

    /// <summary>The second title the user types.</summary>
    private const string SecondTitle = "Renew car";

    /// <summary>The third title the user types.</summary>
    private const string ThirdTitle = "Renew car registration";

    /// <summary>The fourth title the user types.</summary>
    private const string FourthTitle = "Renew car registration online";

    /// <summary>The number of edits in one slice.</summary>
    private const int EditsPerSlice = 3;

    /// <summary>The number of edits in a pair.</summary>
    private const int EditsPerPair = 2;

    /// <summary>The number of edits between the start of one slice and the start of the next.</summary>
    private const int EditsBetweenSlices = 1;

    /// <summary>The length of a time slice, in milliseconds.</summary>
    private const int SliceMilliseconds = 100;

    /// <summary>The time between two edits, in milliseconds.</summary>
    private const int EditGapMilliseconds = 40;

    /// <summary>The time between two slices that start every half slice, in milliseconds.</summary>
    private const int SliceShiftMilliseconds = 50;

    /// <summary>The time between two autosaves, in milliseconds.</summary>
    private const int AutosaveMilliseconds = 500;

    /// <summary>The first amount the customer types.</summary>
    private const decimal FirstAmount = 10M;

    /// <summary>The second amount the customer types.</summary>
    private const decimal SecondAmount = 20M;

    /// <summary>The third amount the customer types.</summary>
    private const decimal ThirdAmount = 30M;

    /// <summary>The text between the values of a batch.</summary>
    private const string Separator = ", ";

    /// <summary>How long a time slice lasts.</summary>
    private static readonly TimeSpan _sliceLength = TimeSpan.FromMilliseconds(SliceMilliseconds);

    /// <summary>How long the user takes between two edits.</summary>
    private static readonly TimeSpan _editGap = TimeSpan.FromMilliseconds(EditGapMilliseconds);

    /// <summary>How far apart overlapping time slices start.</summary>
    private static readonly TimeSpan _sliceShift = TimeSpan.FromMilliseconds(SliceShiftMilliseconds);

    /// <summary>How often the draft is saved.</summary>
    private static readonly TimeSpan _autosaveInterval = TimeSpan.FromMilliseconds(AutosaveMilliseconds);

    /// <summary>Hands the title edits on in slices of three, so the consumer receives one list for every three edits.</summary>
    public static void SliceEditsByCount()
    {
        TodoItem item = new() { Title = DraftTitle };

        using IDisposable subscription = item.WhenChanged(x => x.Title)
            .Skip(1)
            .Slice(EditsPerSlice)
            .SelectMany(static slice => slice.ToList())
            .Subscribe(static edits => Console.WriteLine(string.Join(Separator, edits)));

        item.Title = FirstTitle;
        item.Title = SecondTitle;
        item.Title = ThirdTitle;
        item.Title = FourthTitle;

        // Output:
        // Renew, Renew car, Renew car registration
    }

    /// <summary>Starts a new slice after every edit, so each edit is reported together with the one before it.</summary>
    public static void SlicePairsOfEdits()
    {
        TodoItem item = new() { Title = DraftTitle };

        using IDisposable subscription = item.WhenChanged(x => x.Title)
            .Slice(EditsPerPair, EditsBetweenSlices)
            .SelectMany(static slice => slice.ToList())
            .Subscribe(static pair => Console.WriteLine(string.Join(" -> ", pair)));

        item.Title = FirstTitle;
        item.Title = SecondTitle;
        item.Title = ThirdTitle;

        // Output:
        // Draft -> Renew
        // Renew -> Renew car
        // Renew car -> Renew car registration
    }

    /// <summary>Collects the amounts typed during each 100 ms slice on a virtual clock.</summary>
    public static void SliceEditsByTime()
    {
        VirtualClock clock = new();
        TransferDraft draft = new();

        using IDisposable subscription = draft.WhenChanged(x => x.Amount)
            .Skip(1)
            .Slice(_sliceLength, clock)
            .SelectMany(static slice => slice.ToList())
            .Subscribe(static amounts => Console.WriteLine(string.Join(Separator, amounts)));

        draft.Amount = FirstAmount;
        clock.AdvanceBy(_editGap);
        draft.Amount = SecondAmount;
        clock.AdvanceBy(_sliceLength);
        draft.Amount = ThirdAmount;
        clock.AdvanceBy(_sliceLength);

        // Output:
        // 10, 20
        // 30
    }

    /// <summary>Ends a slice after three edits or after 100 ms, whichever comes first.</summary>
    public static void SliceEditsByTimeOrCount()
    {
        VirtualClock clock = new();
        TransferDraft draft = new();

        using IDisposable subscription = draft.WhenChanged(x => x.Amount)
            .Skip(1)
            .Slice(_sliceLength, EditsPerSlice, clock)
            .SelectMany(static slice => slice.ToList())
            .Subscribe(static amounts => Console.WriteLine(string.Join(Separator, amounts)));

        draft.Amount = FirstAmount;
        draft.Amount = SecondAmount;
        draft.Amount = ThirdAmount;
        draft.Amount = FirstAmount + ThirdAmount;
        clock.AdvanceBy(_sliceLength);

        // Output:
        // 10, 20, 30
        // 40
    }

    /// <summary>Starts a slice every 50 ms that lasts 100 ms, so each edit lands in two slices.</summary>
    public static void SliceEditsWithOverlappingTime()
    {
        VirtualClock clock = new();
        TransferDraft draft = new();

        using IDisposable subscription = draft.WhenChanged(x => x.Amount)
            .Skip(1)
            .Slice(_sliceLength, _sliceShift, clock)
            .SelectMany(static slice => slice.ToList())
            .Subscribe(static amounts => Console.WriteLine(string.Join(Separator, amounts)));

        clock.AdvanceBy(_editGap);
        draft.Amount = FirstAmount;
        clock.AdvanceBy(_editGap);
        draft.Amount = SecondAmount;
        clock.AdvanceBy(_sliceLength);

        // Output:
        // 10, 20
        // 20
    }

    /// <summary>Ends a slice whenever an autosave fires and starts the next one.</summary>
    public static void SliceEditsBetweenAutosaves()
    {
        VirtualClock clock = new();
        TodoItem item = new();

        using IDisposable subscription = item.WhenChanged(x => x.Notes)
            .Skip(1)
            .Slice(Signal.Every(_autosaveInterval, clock))
            .SelectMany(static slice => slice.ToList())
            .Subscribe(static notes => Console.WriteLine(string.Join(Separator, notes)));

        item.Notes = "Bring the old plates";
        item.Notes = "Bring the old plates and the receipt";
        clock.AdvanceBy(_autosaveInterval);
        item.Notes = "Bring the plates, the receipt and a pen";
        clock.AdvanceBy(_autosaveInterval);

        // Output:
        // Bring the old plates, Bring the old plates and the receipt
        // Bring the plates, the receipt and a pen
    }

    /// <summary>Ends a slice when the task is ticked or unticked, and starts the next one.</summary>
    public static void SliceEditsUntilTheTaskIsTicked()
    {
        TodoItem item = new() { Title = DraftTitle };

        using IDisposable subscription = item.WhenChanged(x => x.Title)
            .Skip(1)
            .Slice(() => item.WhenChanged(x => x.IsDone).Skip(1))
            .SelectMany(static slice => slice.ToList())
            .Subscribe(static titles => Console.WriteLine(string.Join(Separator, titles)));

        item.Title = FirstTitle;
        item.Title = SecondTitle;
        item.IsDone = true;
        item.Title = ThirdTitle;
        item.IsDone = false;

        // Output:
        // Renew, Renew car
        // Renew car registration
    }

    /// <summary>Collects the title edits made while the task is urgent: an opening signal starts a slice and a closing signal ends it.</summary>
    public static void SliceEditsWhileTheTaskIsUrgent()
    {
        TodoItem item = new() { Title = DraftTitle };
        IObservable<TodoPriority> becameUrgent = item.WhenChanged(x => x.Priority).Where(static priority => priority == TodoPriority.High);

        using IDisposable subscription = item.WhenChanged(x => x.Title)
            .Skip(1)
            .Slice(becameUrgent, _ => item.WhenChanged(x => x.Priority).Where(static priority => priority != TodoPriority.High))
            .SelectMany(static slice => slice.ToList())
            .Subscribe(static titles => Console.WriteLine(string.Join(Separator, titles)));

        item.Title = FirstTitle;
        item.Priority = TodoPriority.High;
        item.Title = SecondTitle;
        item.Title = ThirdTitle;
        item.Priority = TodoPriority.Normal;
        item.Title = FourthTitle;

        // Output:
        // Renew car, Renew car registration
    }
}
