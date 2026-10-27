// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Binding.Fallback;
using ReactiveUI.Primitives;

namespace ReactiveUI.Binding.Documentation.UnsafeOverloads;

/// <summary>
/// Demonstrates <see cref="RuntimeObservationFallback"/>, the observation the <c>Unsafe</c> methods run on. It reads a
/// property path from an expression while the app runs.
/// </summary>
public static class FallbackObservationExamples
{
    /// <summary>The title of the first seeded item.</summary>
    private const string OriginalTitle = "Renew car registration";

    /// <summary>The title an example gives an item.</summary>
    private const string RenamedTitle = "Renew car registration online";

    /// <summary>The notes of the first seeded item.</summary>
    private const string OriginalNotes = "Bring the insurance certificate.";

    /// <summary>The notes an example gives an item.</summary>
    private const string ReplacementNotes = "Bring the insurance certificate and a photo.";

    /// <summary>Observes one property after it changes.</summary>
    public static void ObserveOnePropertyAfterChange()
    {
        TodoItem item = new TodoItem { Title = OriginalTitle };
        List<string> titles = [];

        using (RuntimeObservationFallback.WhenChanged(item, x => x.Title).Subscribe(titles.Add))
        {
            item.Title = RenamedTitle;
        }

        Console.WriteLine(string.Join(", ", titles));

        // Output:
        // Renew car registration, Renew car registration online
    }

    /// <summary>Observes two properties after they change; each change delivers both values.</summary>
    public static void ObserveTwoPropertiesAfterChange()
    {
        TodoItem item = new TodoItem { Title = OriginalTitle };

        using (RuntimeObservationFallback.WhenChanged(item, x => x.Title, x => x.IsDone).Subscribe(static values => Console.WriteLine($"{values.Property1} {values.Property2}")))
        {
            item.IsDone = true;
        }

        // Output:
        // Renew car registration False
        // Renew car registration True
    }

    /// <summary>Observes three properties after they change; each change delivers all three values.</summary>
    public static void ObserveThreePropertiesAfterChange()
    {
        TodoItem item = new TodoItem { Title = OriginalTitle, Notes = OriginalNotes };

        using (RuntimeObservationFallback
            .WhenChanged(item, x => x.Title, x => x.Notes, x => x.IsDone)
            .Subscribe(static values => Console.WriteLine($"{values.Property1} {values.Property2} {values.Property3}")))
        {
            item.Notes = ReplacementNotes;
        }

        // Output:
        // Renew car registration Bring the insurance certificate. False
        // Renew car registration Bring the insurance certificate and a photo. False
    }

    /// <summary>Observes the value a property held before it changed.</summary>
    public static void ObserveOnePropertyBeforeChange()
    {
        EditableTodoItem item = new EditableTodoItem { Title = OriginalTitle };
        List<string> titles = [];

        using (RuntimeObservationFallback.WhenChanging(item, x => x.Title).Subscribe(titles.Add))
        {
            item.Title = RenamedTitle;
        }

        Console.WriteLine(string.Join(", ", titles));

        // Output:
        // Renew car registration
    }

    /// <summary>Observes two properties before they change.</summary>
    public static void ObserveTwoPropertiesBeforeChange()
    {
        EditableTodoItem item = new EditableTodoItem { Title = OriginalTitle, Notes = OriginalNotes };

        using (RuntimeObservationFallback.WhenChanging(item, x => x.Title, x => x.Notes).Subscribe(static values => Console.WriteLine($"{values.Property1} {values.Property2}")))
        {
            item.Notes = ReplacementNotes;
        }

        // Output:
        // Renew car registration Bring the insurance certificate.
    }

    /// <summary>Observes three properties before they change.</summary>
    public static void ObserveThreePropertiesBeforeChange()
    {
        EditableTodoItem item = new EditableTodoItem { Title = OriginalTitle, Notes = OriginalNotes };

        using (RuntimeObservationFallback
            .WhenChanging(item, x => x.Title, x => x.Notes, x => x.IsDone)
            .Subscribe(static values => Console.WriteLine($"{values.Property1} {values.Property2} {values.Property3}")))
        {
            item.IsDone = true;
        }

        // Output:
        // Renew car registration Bring the insurance certificate. False
    }

    /// <summary>Observes one property with <c>WhenAnyValue</c>.</summary>
    public static void ObserveOneAnyValue()
    {
        TodoItem item = new TodoItem { Title = OriginalTitle };
        List<string> titles = [];

        using (RuntimeObservationFallback.WhenAnyValue(item, x => x.Title).Subscribe(titles.Add))
        {
            item.Title = RenamedTitle;
        }

        Console.WriteLine(string.Join(", ", titles));

        // Output:
        // Renew car registration, Renew car registration online
    }

    /// <summary>Observes two properties with <c>WhenAnyValue</c>.</summary>
    public static void ObserveTwoAnyValues()
    {
        TodoItem item = new TodoItem { Title = OriginalTitle };

        using (RuntimeObservationFallback.WhenAnyValue(item, x => x.Title, x => x.Priority).Subscribe(static values => Console.WriteLine($"{values.Property1} {values.Property2}")))
        {
            item.Priority = TodoPriority.High;
        }

        // Output:
        // Renew car registration Normal
        // Renew car registration High
    }

    /// <summary>Observes three properties with <c>WhenAnyValue</c>.</summary>
    public static void ObserveThreeAnyValues()
    {
        TodoItem item = new TodoItem { Title = OriginalTitle, Notes = OriginalNotes };

        using (RuntimeObservationFallback
            .WhenAnyValue(item, x => x.Title, x => x.Notes, x => x.Priority)
            .Subscribe(static values => Console.WriteLine($"{values.Property1} {values.Property2} {values.Property3}")))
        {
            item.Priority = TodoPriority.High;
        }

        // Output:
        // Renew car registration Bring the insurance certificate. Normal
        // Renew car registration Bring the insurance certificate. High
    }
}
