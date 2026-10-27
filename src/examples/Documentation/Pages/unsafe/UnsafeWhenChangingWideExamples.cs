// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Primitives;

namespace ReactiveUI.Binding.Documentation.UnsafeOverloads;

/// <summary>Demonstrates <c>WhenChangingUnsafe</c> over three to sixteen properties as a group of values. Calling an <c>Unsafe</c> method always reads the property paths at run time.</summary>
public static class UnsafeWhenChangingWideExamples
{
    /// <summary>The title of the item an example observes.</summary>
    private const string OriginalTitle = "Renew car registration";

    /// <summary>The title an example gives the item.</summary>
    private const string RenamedTitle = "Renew car registration online";

    /// <summary>The notes of the item an example observes.</summary>
    private const string SeededNotes = "Bring ID.";

    /// <summary>The text placed between two values.</summary>
    private const string Separator = " | ";

    /// <summary>Observes three properties before they change; each change delivers all three values as one group, and the example prints the first and the last.</summary>
    public static void ObserveThreeColumnsBeforeChange()
    {
        EditableTodoItem item = CreateItem();

        using (item.WhenChangingUnsafe(x => x.Title, x => x.Notes, x => x.IsDone).Subscribe(static values => Console.WriteLine($"{values.Property1}{Separator}{values.Property3}")))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | False
    }

    /// <summary>Observes four properties before they change; each change delivers all four values as one group, and the example prints the first and the last.</summary>
    public static void ObserveFourColumnsBeforeChange()
    {
        EditableTodoItem item = CreateItem();

        using (item.WhenChangingUnsafe(
            x => x.Title,
            x => x.Notes,
            x => x.IsDone,
            x => x.Notes).Subscribe(static values => Console.WriteLine($"{values.Property1}{Separator}{values.Property4}")))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | Bring ID.
    }

    /// <summary>Observes five properties before they change; each change delivers all five values as one group, and the example prints the first and the last.</summary>
    public static void ObserveFiveColumnsBeforeChange()
    {
        EditableTodoItem item = CreateItem();

        using (item.WhenChangingUnsafe(
            x => x.Title,
            x => x.Notes,
            x => x.IsDone,
            x => x.Notes,
            x => x.IsDone).Subscribe(static values => Console.WriteLine($"{values.Property1}{Separator}{values.Property5}")))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | False
    }

    /// <summary>Observes six properties before they change; each change delivers all six values as one group, and the example prints the first and the last.</summary>
    public static void ObserveSixColumnsBeforeChange()
    {
        EditableTodoItem item = CreateItem();

        using (item.WhenChangingUnsafe(
            x => x.Title,
            x => x.Notes,
            x => x.IsDone,
            x => x.Notes,
            x => x.IsDone,
            x => x.Notes).Subscribe(static values => Console.WriteLine($"{values.Property1}{Separator}{values.Property6}")))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | Bring ID.
    }

    /// <summary>Observes seven properties before they change; each change delivers all seven values as one group, and the example prints the first and the last.</summary>
    public static void ObserveSevenColumnsBeforeChange()
    {
        EditableTodoItem item = CreateItem();

        using (item.WhenChangingUnsafe(
            x => x.Title,
            x => x.Notes,
            x => x.IsDone,
            x => x.Notes,
            x => x.IsDone,
            x => x.Notes,
            x => x.IsDone).Subscribe(static values => Console.WriteLine($"{values.Property1}{Separator}{values.Property7}")))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | False
    }

    /// <summary>Observes eight properties before they change; each change delivers all eight values as one group, and the example prints the first and the last.</summary>
    public static void ObserveEightColumnsBeforeChange()
    {
        EditableTodoItem item = CreateItem();

        using (item.WhenChangingUnsafe(
            x => x.Title,
            x => x.Notes,
            x => x.IsDone,
            x => x.Notes,
            x => x.IsDone,
            x => x.Notes,
            x => x.IsDone,
            x => x.Notes).Subscribe(static values => Console.WriteLine($"{values.Property1}{Separator}{values.Property8}")))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | Bring ID.
    }

    /// <summary>Observes nine properties before they change; each change delivers all nine values as one group, and the example prints the first and the last.</summary>
    public static void ObserveNineColumnsBeforeChange()
    {
        EditableTodoItem item = CreateItem();

        using (item.WhenChangingUnsafe(
            x => x.Title,
            x => x.Notes,
            x => x.IsDone,
            x => x.Notes,
            x => x.IsDone,
            x => x.Notes,
            x => x.IsDone,
            x => x.Notes,
            x => x.IsDone).Subscribe(static values => Console.WriteLine($"{values.Property1}{Separator}{values.Property9}")))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | False
    }

    /// <summary>Observes ten properties before they change; each change delivers all ten values as one group, and the example prints the first and the last.</summary>
    public static void ObserveTenColumnsBeforeChange()
    {
        EditableTodoItem item = CreateItem();

        using (item.WhenChangingUnsafe(
            x => x.Title,
            x => x.Notes,
            x => x.IsDone,
            x => x.Notes,
            x => x.IsDone,
            x => x.Notes,
            x => x.IsDone,
            x => x.Notes,
            x => x.IsDone,
            x => x.Notes).Subscribe(static values => Console.WriteLine($"{values.Property1}{Separator}{values.Property10}")))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | Bring ID.
    }

    /// <summary>Observes eleven properties before they change; each change delivers all eleven values as one group, and the example prints the first and the last.</summary>
    public static void ObserveElevenColumnsBeforeChange()
    {
        EditableTodoItem item = CreateItem();

        using (item.WhenChangingUnsafe(
            x => x.Title,
            x => x.Notes,
            x => x.IsDone,
            x => x.Notes,
            x => x.IsDone,
            x => x.Notes,
            x => x.IsDone,
            x => x.Notes,
            x => x.IsDone,
            x => x.Notes,
            x => x.IsDone).Subscribe(static values => Console.WriteLine($"{values.Property1}{Separator}{values.Property11}")))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | False
    }

    /// <summary>Observes twelve properties before they change; each change delivers all twelve values as one group, and the example prints the first and the last.</summary>
    public static void ObserveTwelveColumnsBeforeChange()
    {
        EditableTodoItem item = CreateItem();

        using (item.WhenChangingUnsafe(
            x => x.Title,
            x => x.Notes,
            x => x.IsDone,
            x => x.Notes,
            x => x.IsDone,
            x => x.Notes,
            x => x.IsDone,
            x => x.Notes,
            x => x.IsDone,
            x => x.Notes,
            x => x.IsDone,
            x => x.Notes).Subscribe(static values => Console.WriteLine($"{values.Property1}{Separator}{values.Property12}")))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | Bring ID.
    }

    /// <summary>Observes thirteen properties before they change; each change delivers all thirteen values as one group, and the example prints the first and the last.</summary>
    public static void ObserveThirteenColumnsBeforeChange()
    {
        EditableTodoItem item = CreateItem();

        using (item.WhenChangingUnsafe(
            x => x.Title,
            x => x.Notes,
            x => x.IsDone,
            x => x.Notes,
            x => x.IsDone,
            x => x.Notes,
            x => x.IsDone,
            x => x.Notes,
            x => x.IsDone,
            x => x.Notes,
            x => x.IsDone,
            x => x.Notes,
            x => x.IsDone).Subscribe(static values => Console.WriteLine($"{values.Property1}{Separator}{values.Property13}")))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | False
    }

    /// <summary>Observes fourteen properties before they change; each change delivers all fourteen values as one group, and the example prints the first and the last.</summary>
    public static void ObserveFourteenColumnsBeforeChange()
    {
        EditableTodoItem item = CreateItem();

        using (item.WhenChangingUnsafe(
            x => x.Title,
            x => x.Notes,
            x => x.IsDone,
            x => x.Notes,
            x => x.IsDone,
            x => x.Notes,
            x => x.IsDone,
            x => x.Notes,
            x => x.IsDone,
            x => x.Notes,
            x => x.IsDone,
            x => x.Notes,
            x => x.IsDone,
            x => x.Notes).Subscribe(static values => Console.WriteLine($"{values.Property1}{Separator}{values.Property14}")))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | Bring ID.
    }

    /// <summary>Observes fifteen properties before they change; each change delivers all fifteen values as one group, and the example prints the first and the last.</summary>
    public static void ObserveFifteenColumnsBeforeChange()
    {
        EditableTodoItem item = CreateItem();

        using (item.WhenChangingUnsafe(
            x => x.Title,
            x => x.Notes,
            x => x.IsDone,
            x => x.Notes,
            x => x.IsDone,
            x => x.Notes,
            x => x.IsDone,
            x => x.Notes,
            x => x.IsDone,
            x => x.Notes,
            x => x.IsDone,
            x => x.Notes,
            x => x.IsDone,
            x => x.Notes,
            x => x.IsDone).Subscribe(static values => Console.WriteLine($"{values.Property1}{Separator}{values.Property15}")))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | False
    }

    /// <summary>Observes sixteen properties before they change; each change delivers all sixteen values as one group, and the example prints the first and the last.</summary>
    public static void ObserveSixteenColumnsBeforeChange()
    {
        EditableTodoItem item = CreateItem();

        using (item.WhenChangingUnsafe(
            x => x.Title,
            x => x.Notes,
            x => x.IsDone,
            x => x.Notes,
            x => x.IsDone,
            x => x.Notes,
            x => x.IsDone,
            x => x.Notes,
            x => x.IsDone,
            x => x.Notes,
            x => x.IsDone,
            x => x.Notes,
            x => x.IsDone,
            x => x.Notes,
            x => x.IsDone,
            x => x.Notes).Subscribe(static values => Console.WriteLine($"{values.Property1}{Separator}{values.Property16}")))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | Bring ID.
    }

    /// <summary>Creates the item an example observes.</summary>
    /// <returns>The item.</returns>
    private static EditableTodoItem CreateItem() => new() { Title = OriginalTitle, Notes = SeededNotes };
}
