// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Concurrency;

namespace ReactiveUI.Binding.Documentation.Observing;

/// <summary>Splits a stream of property changes into one stream per key with <c>GroupBy</c> and <c>GroupByUntil</c>.</summary>
public static class GroupedChangesExamples
{
    /// <summary>The name of the title field.</summary>
    private const string TitleField = "Title";

    /// <summary>The name of the notes field.</summary>
    private const string NotesField = "Notes";

    /// <summary>The first title the user types.</summary>
    private const string FirstTitle = "Renew";

    /// <summary>The second title the user types.</summary>
    private const string SecondTitle = "Renew car registration";

    /// <summary>The first note the user types.</summary>
    private const string FirstNote = "Bring the old plates";

    /// <summary>The second note the user types.</summary>
    private const string SecondNote = "Bring the plates and the receipt";

    /// <summary>The number of distinct tags the groups are sized for.</summary>
    private const int ExpectedTagCount = 8;

    /// <summary>How long a field must stay unchanged before its group ends, in milliseconds.</summary>
    private const int QuietMilliseconds = 300;

    /// <summary>The time between two edits, in milliseconds.</summary>
    private const int EditGapMilliseconds = 100;

    /// <summary>The text between the values of a group.</summary>
    private const string Separator = ", ";

    /// <summary>How long a field must stay unchanged before its group ends.</summary>
    private static readonly TimeSpan _quietPeriod = TimeSpan.FromMilliseconds(QuietMilliseconds);

    /// <summary>How long the user takes between two edits.</summary>
    private static readonly TimeSpan _editGap = TimeSpan.FromMilliseconds(EditGapMilliseconds);

    /// <summary>Counts the edits of each field of a task, one group per field.</summary>
    public static void GroupEditsByField()
    {
        TodoItem item = new();

        using IDisposable subscription = ObserveEdits(item)
            .GroupBy(static edit => edit.Field, static edit => edit.Value)
            .SelectMany(static group => group.Scan(0, static (count, _) => count + 1).Select(count => $"{group.Key} edit {count}"))
            .Subscribe(Console.WriteLine);

        item.Title = FirstTitle;
        item.Notes = FirstNote;
        item.Title = SecondTitle;
        item.Notes = SecondNote;

        // Output:
        // Title edit 1
        // Notes edit 1
        // Title edit 2
        // Notes edit 2
    }

    /// <summary>Counts how often each tag is used, treating "Car" and "car" as one tag.</summary>
    public static void GroupTagsIgnoringCase()
    {
        TodoItem item = new();

        using IDisposable subscription = item.WhenChanged(x => x.Tags)
            .Skip(1)
            .SelectMany(static tags => tags)
            .GroupBy(static tag => tag, ExpectedTagCount, StringComparer.OrdinalIgnoreCase)
            .SelectMany(static group => group.Scan(0, static (count, _) => count + 1).Select(count => $"{group.Key}: {count}"))
            .Subscribe(Console.WriteLine);

        item.Tags = ["Car", "Errands"];
        item.Tags = ["car", "admin"];

        // Output:
        // Car: 1
        // Errands: 1
        // Car: 2
        // admin: 1
    }

    /// <summary>Ends the group of a field when the field has stayed unchanged for 300 ms, and reports what was typed in it.</summary>
    public static void GroupEditsUntilTheFieldIsQuiet()
    {
        VirtualClock clock = new();
        TodoItem item = new();

        using IDisposable subscription = ObserveEdits(item)
            .GroupByUntil(static edit => edit.Field, static edit => edit.Value, group => group.Throttle(_quietPeriod, clock))
            .SelectMany(static group => group.ToList().Select(values => $"{group.Key}: {string.Join(Separator, values)}"))
            .Subscribe(Console.WriteLine);

        item.Title = FirstTitle;
        clock.AdvanceBy(_editGap);
        item.Notes = FirstNote;
        clock.AdvanceBy(_editGap);
        item.Title = SecondTitle;
        clock.AdvanceBy(_quietPeriod);
        item.Notes = SecondNote;
        clock.AdvanceBy(_quietPeriod);

        // Output:
        // Notes: Bring the old plates
        // Title: Renew, Renew car registration
        // Notes: Bring the plates and the receipt
    }

    /// <summary>Merges the edits of the title and of the notes into one stream that names the field.</summary>
    /// <param name="item">The task being edited.</param>
    /// <returns>A stream with the field name and the new text of each edit.</returns>
    private static IObservable<(string Field, string Value)> ObserveEdits(TodoItem item)
    {
        IObservable<(string Field, string Value)> titles = item.WhenChanged(x => x.Title).Skip(1).Select(static title => (Field: TitleField, Value: title));
        IObservable<(string Field, string Value)> notes = item.WhenChanged(x => x.Notes).Skip(1).Select(static note => (Field: NotesField, Value: note));

        return titles.Merge(notes);
    }
}
