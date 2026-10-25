// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Primitives;

namespace ReactiveUI.Binding.Documentation.UnsafeOverloads;

/// <summary>Demonstrates <c>WhenChangingUnsafe</c> over three to sixteen properties with a selector. Calling an <c>Unsafe</c> method always reads the property paths at run time.</summary>
public static class UnsafeWhenChangingWideSelectorExamples
{
    /// <summary>The title of the item an example observes.</summary>
    private const string OriginalTitle = "Renew car registration";

    /// <summary>The title an example gives the item.</summary>
    private const string RenamedTitle = "Renew car registration online";

    /// <summary>The notes of the item an example observes.</summary>
    private const string SeededNotes = "Bring ID.";

    /// <summary>The text placed between two values.</summary>
    private const string Separator = " | ";

    /// <summary>Observes three properties before they change and combines them with a selector.</summary>
    public static void ObserveThreeColumnsBeforeChangeWithSelector()
    {
        EditableTodoItem item = CreateItem();

        using (item.WhenChangingUnsafe(x => x.Title, x => x.Notes, x => x.IsDone, static (c1, c2, c3) => string.Join(Separator, c1, c2, c3)).Subscribe(Console.WriteLine))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | Bring ID. | False
    }

    /// <summary>Observes four properties before they change and combines them with a selector.</summary>
    public static void ObserveFourColumnsBeforeChangeWithSelector()
    {
        EditableTodoItem item = CreateItem();

        using (item.WhenChangingUnsafe(
            x => x.Title,
            x => x.Notes,
            x => x.IsDone,
            x => x.Notes,
            static (c1, c2, c3, c4) =>
            string.Join(Separator, c1, c2, c3, c4)).Subscribe(Console.WriteLine))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | Bring ID. | False | Bring ID.
    }

    /// <summary>Observes five properties before they change and combines them with a selector.</summary>
    public static void ObserveFiveColumnsBeforeChangeWithSelector()
    {
        EditableTodoItem item = CreateItem();

        using (item.WhenChangingUnsafe(
            x => x.Title,
            x => x.Notes,
            x => x.IsDone,
            x => x.Notes,
            x => x.IsDone,
            static (c1, c2, c3, c4, c5) =>
            string.Join(Separator, c1, c2, c3, c4, c5)).Subscribe(Console.WriteLine))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | Bring ID. | False | Bring ID. | False
    }

    /// <summary>Observes six properties before they change and combines them with a selector.</summary>
    public static void ObserveSixColumnsBeforeChangeWithSelector()
    {
        EditableTodoItem item = CreateItem();

        using (item.WhenChangingUnsafe(
            x => x.Title,
            x => x.Notes,
            x => x.IsDone,
            x => x.Notes,
            x => x.IsDone,
            x => x.Notes,
            static (c1, c2, c3, c4, c5, c6) =>
            string.Join(Separator, c1, c2, c3, c4, c5, c6)).Subscribe(Console.WriteLine))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | Bring ID. | False | Bring ID. | False | Bring ID.
    }

    /// <summary>Observes seven properties before they change and combines them with a selector.</summary>
    public static void ObserveSevenColumnsBeforeChangeWithSelector()
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
            static (c1, c2, c3, c4, c5, c6, c7) =>
            string.Join(Separator, c1, c2, c3, c4, c5, c6, c7)).Subscribe(Console.WriteLine))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | Bring ID. | False | Bring ID. | False | Bring ID. | False
    }

    /// <summary>Observes eight properties before they change and combines them with a selector.</summary>
    public static void ObserveEightColumnsBeforeChangeWithSelector()
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
            static (c1, c2, c3, c4, c5, c6, c7, c8) =>
            string.Join(Separator, c1, c2, c3, c4, c5, c6, c7, c8)).Subscribe(Console.WriteLine))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | Bring ID. | False | Bring ID. | False | Bring ID. | False | Bring ID.
    }

    /// <summary>Observes nine properties before they change and combines them with a selector.</summary>
    public static void ObserveNineColumnsBeforeChangeWithSelector()
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
            static (c1, c2, c3, c4, c5, c6, c7, c8, c9) =>
            string.Join(Separator, c1, c2, c3, c4, c5, c6, c7, c8, c9)).Subscribe(Console.WriteLine))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | Bring ID. | False | Bring ID. | False | Bring ID. | False | Bring ID. | False
    }

    /// <summary>Observes ten properties before they change and combines them with a selector.</summary>
    public static void ObserveTenColumnsBeforeChangeWithSelector()
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
            static (c1, c2, c3, c4, c5, c6, c7, c8, c9, c10) =>
            string.Join(Separator, c1, c2, c3, c4, c5, c6, c7, c8, c9, c10)).Subscribe(Console.WriteLine))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | Bring ID. | False | Bring ID. | False | Bring ID. | False | Bring ID. | False | Bring ID.
    }

    /// <summary>Observes eleven properties before they change and combines them with a selector.</summary>
    public static void ObserveElevenColumnsBeforeChangeWithSelector()
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
            static (c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11) =>
            string.Join(Separator, c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11)).Subscribe(Console.WriteLine))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | Bring ID. | False | Bring ID. | False | Bring ID. | False | Bring ID. | False | Bring ID. | False
    }

    /// <summary>Observes twelve properties before they change and combines them with a selector.</summary>
    public static void ObserveTwelveColumnsBeforeChangeWithSelector()
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
            static (c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11, c12) =>
            string.Join(Separator, c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11, c12)).Subscribe(Console.WriteLine))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | Bring ID. | False | Bring ID. | False | Bring ID. | False | Bring ID. | False | Bring ID. | False | Bring ID.
    }

    /// <summary>Observes thirteen properties before they change and combines them with a selector.</summary>
    public static void ObserveThirteenColumnsBeforeChangeWithSelector()
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
            static (c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11, c12, c13) =>
            string.Join(Separator, c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11, c12, c13)).Subscribe(Console.WriteLine))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | Bring ID. | False | Bring ID. | False | Bring ID. | False | Bring ID. | False | Bring ID. | False | Bring ID. | False
    }

    /// <summary>Observes fourteen properties before they change and combines them with a selector.</summary>
    public static void ObserveFourteenColumnsBeforeChangeWithSelector()
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
            static (c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11, c12, c13, c14) =>
            string.Join(Separator, c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11, c12, c13, c14)).Subscribe(Console.WriteLine))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | Bring ID. | False | Bring ID. | False | Bring ID. | False | Bring ID. | False | Bring ID. | False | Bring ID. | False | Bring ID.
    }

    /// <summary>Observes fifteen properties before they change and combines them with a selector.</summary>
    public static void ObserveFifteenColumnsBeforeChangeWithSelector()
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
            static (c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11, c12, c13, c14, c15) =>
            string.Join(Separator, c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11, c12, c13, c14, c15)).Subscribe(Console.WriteLine))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | Bring ID. | False | Bring ID. | False | Bring ID. | False | Bring ID. | False | Bring ID. | False | Bring ID. | False | Bring ID. | False
    }

    /// <summary>Observes sixteen properties before they change and combines them with a selector.</summary>
    public static void ObserveSixteenColumnsBeforeChangeWithSelector()
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
            x => x.Notes,
            static (c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11, c12, c13, c14, c15, c16) =>
            string.Join(Separator, c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11, c12, c13, c14, c15, c16)).Subscribe(Console.WriteLine))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | Bring ID. | False | Bring ID. | False | Bring ID. | False | Bring ID. | False | Bring ID. | False | Bring ID. | False | Bring ID. | False | Bring ID.
    }

    /// <summary>Creates the item an example observes.</summary>
    /// <returns>The item.</returns>
    private static EditableTodoItem CreateItem() => new() { Title = OriginalTitle, Notes = SeededNotes };
}
