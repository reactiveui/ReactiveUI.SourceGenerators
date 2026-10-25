// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Linq.Expressions;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Signals;

namespace ReactiveUI.Binding.Documentation.UnsafeOverloads;

/// <summary>
/// Demonstrates the <c>Unsafe</c> twins of the observation methods. Each property path here is held in a variable, as it
/// is when a user picks the columns of a grid, so the generator cannot read it and only the reflection-based overload
/// can serve the call.
/// </summary>
public static class UnsafeObservationExamples
{
    /// <summary>The title of the first seeded item.</summary>
    private const string OriginalTitle = "Renew car registration";

    /// <summary>The title an example gives an item.</summary>
    private const string RenamedTitle = "Renew car registration online";

    /// <summary>The last title an example gives an item.</summary>
    private const string FinalTitle = "Renew car registration by post";

    /// <summary>The last notes an example gives an item.</summary>
    private const string FinalNotes = "Bring nothing.";

    /// <summary>The notes of the first seeded item.</summary>
    private const string SeededNotes = "Bring the insurance certificate.";

    /// <summary>The notes an example writes.</summary>
    private const string ReplacementNotes = "Bring the insurance certificate and a photo.";

    /// <summary>The announcement an example emits.</summary>
    private const string SyncComplete = "Sync complete";

    /// <summary>The urgent announcement an example emits.</summary>
    private const string DiskFull = "Disk full";

    /// <summary>Observes one property whose path is built from a name that is only known while the app runs.</summary>
    public static void ObserveTitleChosenAtRunTime()
    {
        TodoItem item = new TodoItem { Title = OriginalTitle };
        ParameterExpression row = Expression.Parameter(typeof(TodoItem), "row");
        Expression<Func<TodoItem, string>> titleColumn = Expression.Lambda<Func<TodoItem, string>>(Expression.Property(row, nameof(TodoItem.Title)), row);
        List<string> titles = [];

        using (item.WhenChangedUnsafe(titleColumn).Subscribe(titles.Add))
        {
            item.Title = RenamedTitle;
        }

        Console.WriteLine(string.Join(", ", titles));

        // Output:
        // Renew car registration, Renew car registration online
    }

    /// <summary>Observes two properties; each change delivers both values.</summary>
    public static void ObserveTwoColumnsChosenAtRunTime()
    {
        TodoItem item = new TodoItem { Title = OriginalTitle };
        Expression<Func<TodoItem, string>> titleColumn = x => x.Title;
        Expression<Func<TodoItem, bool>> doneColumn = x => x.IsDone;

        using (item.WhenChangedUnsafe(titleColumn, doneColumn).Subscribe(static values => Console.WriteLine($"{values.Property1} {values.Property2}")))
        {
            item.IsDone = true;
        }

        // Output:
        // Renew car registration False
        // Renew car registration True
    }

    /// <summary>Observes two properties and combines them with a selector.</summary>
    public static void ObserveTwoColumnsWithSelector()
    {
        TodoItem item = new TodoItem { Title = OriginalTitle };
        Expression<Func<TodoItem, string>> titleColumn = x => x.Title;
        Expression<Func<TodoItem, bool>> doneColumn = x => x.IsDone;
        List<string> lines = [];

        using (item.WhenChangedUnsafe(titleColumn, doneColumn, static (title, done) => done ? $"[x] {title}" : $"[ ] {title}").Subscribe(lines.Add))
        {
            item.IsDone = true;
        }

        Console.WriteLine(string.Join(", ", lines));

        // Output:
        // [ ] Renew car registration, [x] Renew car registration
    }

    /// <summary>Observes three properties; each change delivers all three values.</summary>
    public static void ObserveThreeColumnsChosenAtRunTime()
    {
        TodoItem item = new TodoItem { Title = OriginalTitle, Notes = SeededNotes };
        Expression<Func<TodoItem, string>> titleColumn = x => x.Title;
        Expression<Func<TodoItem, string>> notesColumn = x => x.Notes;
        Expression<Func<TodoItem, bool>> doneColumn = x => x.IsDone;

        using (item.WhenChangedUnsafe(titleColumn, notesColumn, doneColumn).Subscribe(static values => Console.WriteLine($"{values.Property1} {values.Property2} {values.Property3}")))
        {
            item.IsDone = true;
        }

        // Output:
        // Renew car registration Bring the insurance certificate. False
        // Renew car registration Bring the insurance certificate. True
    }

    /// <summary>Observes three properties and combines them with a selector.</summary>
    public static void ObserveThreeColumnsWithSelector()
    {
        TodoItem item = new TodoItem { Title = OriginalTitle, Notes = SeededNotes };
        Expression<Func<TodoItem, string>> titleColumn = x => x.Title;
        Expression<Func<TodoItem, string>> notesColumn = x => x.Notes;
        Expression<Func<TodoItem, bool>> doneColumn = x => x.IsDone;
        List<string> lines = [];

        using (item.WhenChangedUnsafe(titleColumn, notesColumn, doneColumn, static (title, notes, done) => $"{title} | {notes} | {done}").Subscribe(lines.Add))
        {
            item.Notes = ReplacementNotes;
        }

        Console.WriteLine(string.Join(", ", lines));

        // Output:
        // Renew car registration | Bring the insurance certificate. | False, Renew car registration | Bring the insurance certificate and a photo. | False
    }

    /// <summary>Observes the value a property held before it changed.</summary>
    public static void ObserveTitleBeforeChange()
    {
        EditableTodoItem item = new EditableTodoItem { Title = OriginalTitle };
        Expression<Func<EditableTodoItem, string>> titleColumn = x => x.Title;
        List<string> titles = [];

        using (item.WhenChangingUnsafe(titleColumn).Subscribe(titles.Add))
        {
            item.Title = RenamedTitle;
            item.Title = FinalTitle;
        }

        Console.WriteLine(string.Join(", ", titles));

        // Output:
        // Renew car registration, Renew car registration online
    }

    /// <summary>Observes two properties before they change.</summary>
    public static void ObserveTwoColumnsBeforeChange()
    {
        EditableTodoItem item = new EditableTodoItem { Title = OriginalTitle, Notes = SeededNotes };
        Expression<Func<EditableTodoItem, string>> titleColumn = x => x.Title;
        Expression<Func<EditableTodoItem, string>> notesColumn = x => x.Notes;

        using (item.WhenChangingUnsafe(titleColumn, notesColumn).Subscribe(static values => Console.WriteLine($"{values.Property1} {values.Property2}")))
        {
            item.Notes = ReplacementNotes;
            item.Notes = FinalNotes;
        }

        // Output:
        // Renew car registration Bring the insurance certificate.
        // Renew car registration Bring the insurance certificate and a photo.
    }

    /// <summary>Observes two properties before they change and combines them with a selector.</summary>
    public static void ObserveTwoColumnsBeforeChangeWithSelector()
    {
        EditableTodoItem item = new EditableTodoItem { Title = OriginalTitle, Notes = SeededNotes };
        Expression<Func<EditableTodoItem, string>> titleColumn = x => x.Title;
        Expression<Func<EditableTodoItem, string>> notesColumn = x => x.Notes;
        List<string> lines = [];

        using (item.WhenChangingUnsafe(titleColumn, notesColumn, static (title, notes) => $"{title}: {notes}").Subscribe(lines.Add))
        {
            item.Title = RenamedTitle;
            item.Title = FinalTitle;
        }

        Console.WriteLine(string.Join(", ", lines));

        // Output:
        // Renew car registration: Bring the insurance certificate., Renew car registration online: Bring the insurance certificate.
    }

    /// <summary>Observes one property with <c>WhenAnyValueUnsafe</c>.</summary>
    public static void ObserveAnyValueChosenAtRunTime()
    {
        TodoItem item = new TodoItem { Title = OriginalTitle };
        Expression<Func<TodoItem, string>> titleColumn = x => x.Title;
        List<string> titles = [];

        using (item.WhenAnyValueUnsafe(titleColumn).Subscribe(titles.Add))
        {
            item.Title = RenamedTitle;
        }

        Console.WriteLine(string.Join(", ", titles));

        // Output:
        // Renew car registration, Renew car registration online
    }

    /// <summary>Observes one property with <c>WhenAnyValueUnsafe</c> and maps each value with a selector.</summary>
    public static void ObserveAnyValueWithSelector()
    {
        TodoItem item = new TodoItem { Title = OriginalTitle };
        Expression<Func<TodoItem, string>> titleColumn = x => x.Title;
        List<int> lengths = [];

        using (item.WhenAnyValueUnsafe(titleColumn, static title => title.Length).Subscribe(lengths.Add))
        {
            item.Title = RenamedTitle;
        }

        Console.WriteLine(string.Join(", ", lengths));

        // Output:
        // 22, 29
    }

    /// <summary>Observes two properties with <c>WhenAnyValueUnsafe</c>.</summary>
    public static void ObserveTwoAnyValuesChosenAtRunTime()
    {
        TodoItem item = new TodoItem { Title = OriginalTitle };
        Expression<Func<TodoItem, string>> titleColumn = x => x.Title;
        Expression<Func<TodoItem, TodoPriority>> priorityColumn = x => x.Priority;

        using (item.WhenAnyValueUnsafe(titleColumn, priorityColumn).Subscribe(static values => Console.WriteLine($"{values.Property1} {values.Property2}")))
        {
            item.Priority = TodoPriority.High;
        }

        // Output:
        // Renew car registration Normal
        // Renew car registration High
    }

    /// <summary>Observes two properties with <c>WhenAnyValueUnsafe</c> and combines them with a selector.</summary>
    public static void ObserveTwoAnyValuesWithSelector()
    {
        TodoItem item = new TodoItem { Title = OriginalTitle };
        Expression<Func<TodoItem, string>> titleColumn = x => x.Title;
        Expression<Func<TodoItem, TodoPriority>> priorityColumn = x => x.Priority;
        List<string> lines = [];

        using (item.WhenAnyValueUnsafe(titleColumn, priorityColumn, static (title, priority) => $"{title} [{priority}]").Subscribe(lines.Add))
        {
            item.Priority = TodoPriority.High;
        }

        Console.WriteLine(string.Join(", ", lines));

        // Output:
        // Renew car registration [Normal], Renew car registration [High]
    }

    /// <summary>Observes one property with <c>WhenAnyUnsafe</c>; the selector receives the change, not only its value.</summary>
    public static void ObserveAnyChosenAtRunTime()
    {
        TodoItem item = new TodoItem { Title = OriginalTitle };
        Expression<Func<TodoItem, string>> titleColumn = x => x.Title;
        List<string> lines = [];

        using (item.WhenAnyUnsafe(titleColumn, static change => $"{change.Sender.Id}: {change.Value}").Subscribe(lines.Add))
        {
            item.Title = RenamedTitle;
        }

        Console.WriteLine(string.Join(", ", lines));

        // Output:
        // 0: Renew car registration, 0: Renew car registration online
    }

    /// <summary>Observes two properties with <c>WhenAnyUnsafe</c>.</summary>
    public static void ObserveTwoAnyChosenAtRunTime()
    {
        TodoItem item = new TodoItem { Title = OriginalTitle };
        Expression<Func<TodoItem, string>> titleColumn = x => x.Title;
        Expression<Func<TodoItem, bool>> doneColumn = x => x.IsDone;
        List<string> lines = [];

        using (item.WhenAnyUnsafe(titleColumn, doneColumn, static (title, done) => $"{title.Value} {done.Value}").Subscribe(lines.Add))
        {
            item.IsDone = true;
        }

        Console.WriteLine(string.Join(", ", lines));

        // Output:
        // Renew car registration False, Renew car registration True
    }

    /// <summary>Follows the announcement stream a property holds, and switches when the property is replaced.</summary>
    public static void ObserveAnnouncementStreamChosenAtRunTime()
    {
        TodoAnnouncer announcer = new();
        Expression<Func<TodoAnnouncer, IObservable<string>?>> latestColumn = x => x.Latest;
        List<string> messages = [];

        using (announcer.WhenAnyObservableUnsafe(latestColumn).Subscribe(messages.Add))
        {
            announcer.Latest = Signal.Return(SyncComplete);
        }

        Console.WriteLine(string.Join(", ", messages));

        // Output:
        // Sync complete
    }

    /// <summary>Merges the announcement streams of two properties.</summary>
    public static void MergeAnnouncementStreams()
    {
        TodoAnnouncer announcer = new();
        Expression<Func<TodoAnnouncer, IObservable<string>?>> latestColumn = x => x.Latest;
        Expression<Func<TodoAnnouncer, IObservable<string>?>> urgentColumn = x => x.Urgent;
        List<string> messages = [];

        using (announcer.WhenAnyObservableUnsafe(latestColumn, urgentColumn).Subscribe(messages.Add))
        {
            announcer.Latest = Signal.Return(SyncComplete);
            announcer.Urgent = Signal.Return(DiskFull);
        }

        Console.WriteLine(string.Join(", ", messages));

        // Output:
        // Sync complete, Disk full
    }

    /// <summary>Combines the latest announcement of two properties with a selector.</summary>
    public static void CombineAnnouncementStreams()
    {
        TodoAnnouncer announcer = new();
        Expression<Func<TodoAnnouncer, IObservable<string>?>> latestColumn = x => x.Latest;
        Expression<Func<TodoAnnouncer, IObservable<string>?>> urgentColumn = x => x.Urgent;
        List<string> lines = [];

        using (announcer.WhenAnyObservableUnsafe(latestColumn, urgentColumn, static (latest, urgent) => $"{latest} / {urgent}").Subscribe(lines.Add))
        {
            announcer.Latest = Signal.Return(SyncComplete);
            announcer.Urgent = Signal.Return(DiskFull);
        }

        Console.WriteLine(string.Join(", ", lines));

        // Output:
        // Sync complete / Disk full
    }
}
