// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Primitives;

namespace ReactiveUI.Binding.Documentation.UnsafeOverloads;

/// <summary>Demonstrates <c>WhenAnyUnsafe</c> over three to twelve properties. Calling an <c>Unsafe</c> method always reads the property paths at run time.</summary>
public static class UnsafeWhenAnyWideExamples
{
    /// <summary>The title of the item an example observes.</summary>
    private const string OriginalTitle = "Renew car registration";

    /// <summary>The title an example gives the item.</summary>
    private const string RenamedTitle = "Renew car registration online";

    /// <summary>The text placed between two values.</summary>
    private const string Separator = " | ";

    /// <summary>Observes three properties; the selector receives each change, not only its value.</summary>
    public static void ObserveThreeColumnsWithChanges()
    {
        TodoItem item = CreateItem();

        using (item.WhenAnyUnsafe(x => x.Title, x => x.Id, x => x.IsDone, static (c1, c2, c3) => string.Join(Separator, c1.Value, c2.Value, c3.Value)).Subscribe(Console.WriteLine))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | 1 | False
        // Renew car registration online | 1 | False
    }

    /// <summary>Observes four properties; the selector receives each change, not only its value.</summary>
    public static void ObserveFourColumnsWithChanges()
    {
        TodoItem item = CreateItem();

        using (item.WhenAnyUnsafe(
            x => x.Title,
            x => x.Id,
            x => x.IsDone,
            x => x.Priority,
            static (c1, c2, c3, c4) =>
            string.Join(Separator, c1.Value, c2.Value, c3.Value, c4.Value)).Subscribe(Console.WriteLine))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | 1 | False | High
        // Renew car registration online | 1 | False | High
    }

    /// <summary>Observes five properties; the selector receives each change, not only its value.</summary>
    public static void ObserveFiveColumnsWithChanges()
    {
        TodoItem item = CreateItem();

        using (item.WhenAnyUnsafe(
            x => x.Title,
            x => x.Id,
            x => x.IsDone,
            x => x.Priority,
            x => x.Id,
            static (c1, c2, c3, c4, c5) =>
            string.Join(Separator, c1.Value, c2.Value, c3.Value, c4.Value, c5.Value)).Subscribe(Console.WriteLine))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | 1 | False | High | 1
        // Renew car registration online | 1 | False | High | 1
    }

    /// <summary>Observes six properties; the selector receives each change, not only its value.</summary>
    public static void ObserveSixColumnsWithChanges()
    {
        TodoItem item = CreateItem();

        using (item.WhenAnyUnsafe(
            x => x.Title,
            x => x.Id,
            x => x.IsDone,
            x => x.Priority,
            x => x.Id,
            x => x.IsDone,
            static (c1, c2, c3, c4, c5, c6) =>
            string.Join(Separator, c1.Value, c2.Value, c3.Value, c4.Value, c5.Value, c6.Value)).Subscribe(Console.WriteLine))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | 1 | False | High | 1 | False
        // Renew car registration online | 1 | False | High | 1 | False
    }

    /// <summary>Observes seven properties; the selector receives each change, not only its value.</summary>
    public static void ObserveSevenColumnsWithChanges()
    {
        TodoItem item = CreateItem();

        using (item.WhenAnyUnsafe(
            x => x.Title,
            x => x.Id,
            x => x.IsDone,
            x => x.Priority,
            x => x.Id,
            x => x.IsDone,
            x => x.Priority,
            static (c1, c2, c3, c4, c5, c6, c7) =>
            string.Join(Separator, c1.Value, c2.Value, c3.Value, c4.Value, c5.Value, c6.Value, c7.Value)).Subscribe(Console.WriteLine))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | 1 | False | High | 1 | False | High
        // Renew car registration online | 1 | False | High | 1 | False | High
    }

    /// <summary>Observes eight properties; the selector receives each change, not only its value.</summary>
    public static void ObserveEightColumnsWithChanges()
    {
        TodoItem item = CreateItem();

        using (item.WhenAnyUnsafe(
            x => x.Title,
            x => x.Id,
            x => x.IsDone,
            x => x.Priority,
            x => x.Id,
            x => x.IsDone,
            x => x.Priority,
            x => x.Id,
            static (c1, c2, c3, c4, c5, c6, c7, c8) =>
            string.Join(Separator, c1.Value, c2.Value, c3.Value, c4.Value, c5.Value, c6.Value, c7.Value, c8.Value)).Subscribe(Console.WriteLine))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | 1 | False | High | 1 | False | High | 1
        // Renew car registration online | 1 | False | High | 1 | False | High | 1
    }

    /// <summary>Observes nine properties; the selector receives each change, not only its value.</summary>
    public static void ObserveNineColumnsWithChanges()
    {
        TodoItem item = CreateItem();

        using (item.WhenAnyUnsafe(
            x => x.Title,
            x => x.Id,
            x => x.IsDone,
            x => x.Priority,
            x => x.Id,
            x => x.IsDone,
            x => x.Priority,
            x => x.Id,
            x => x.IsDone,
            static (c1, c2, c3, c4, c5, c6, c7, c8, c9) =>
            string.Join(Separator, c1.Value, c2.Value, c3.Value, c4.Value, c5.Value, c6.Value, c7.Value, c8.Value, c9.Value)).Subscribe(Console.WriteLine))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | 1 | False | High | 1 | False | High | 1 | False
        // Renew car registration online | 1 | False | High | 1 | False | High | 1 | False
    }

    /// <summary>Observes ten properties; the selector receives each change, not only its value.</summary>
    public static void ObserveTenColumnsWithChanges()
    {
        TodoItem item = CreateItem();

        using (item.WhenAnyUnsafe(
            x => x.Title,
            x => x.Id,
            x => x.IsDone,
            x => x.Priority,
            x => x.Id,
            x => x.IsDone,
            x => x.Priority,
            x => x.Id,
            x => x.IsDone,
            x => x.Priority,
            static (c1, c2, c3, c4, c5, c6, c7, c8, c9, c10) =>
            string.Join(Separator, c1.Value, c2.Value, c3.Value, c4.Value, c5.Value, c6.Value, c7.Value, c8.Value, c9.Value, c10.Value)).Subscribe(Console.WriteLine))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | 1 | False | High | 1 | False | High | 1 | False | High
        // Renew car registration online | 1 | False | High | 1 | False | High | 1 | False | High
    }

    /// <summary>Observes eleven properties; the selector receives each change, not only its value.</summary>
    public static void ObserveElevenColumnsWithChanges()
    {
        TodoItem item = CreateItem();

        using (item.WhenAnyUnsafe(
            x => x.Title,
            x => x.Id,
            x => x.IsDone,
            x => x.Priority,
            x => x.Id,
            x => x.IsDone,
            x => x.Priority,
            x => x.Id,
            x => x.IsDone,
            x => x.Priority,
            x => x.Id,
            static (c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11) =>
            string.Join(Separator, c1.Value, c2.Value, c3.Value, c4.Value, c5.Value, c6.Value, c7.Value, c8.Value, c9.Value, c10.Value, c11.Value)).Subscribe(Console.WriteLine))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | 1 | False | High | 1 | False | High | 1 | False | High | 1
        // Renew car registration online | 1 | False | High | 1 | False | High | 1 | False | High | 1
    }

    /// <summary>Observes twelve properties; the selector receives each change, not only its value.</summary>
    public static void ObserveTwelveColumnsWithChanges()
    {
        TodoItem item = CreateItem();

        using (item.WhenAnyUnsafe(
            x => x.Title,
            x => x.Id,
            x => x.IsDone,
            x => x.Priority,
            x => x.Id,
            x => x.IsDone,
            x => x.Priority,
            x => x.Id,
            x => x.IsDone,
            x => x.Priority,
            x => x.Id,
            x => x.IsDone,
            static (c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11, c12) =>
            string.Join(Separator, c1.Value, c2.Value, c3.Value, c4.Value, c5.Value, c6.Value, c7.Value, c8.Value, c9.Value, c10.Value, c11.Value, c12.Value)).Subscribe(Console.WriteLine))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | 1 | False | High | 1 | False | High | 1 | False | High | 1 | False
        // Renew car registration online | 1 | False | High | 1 | False | High | 1 | False | High | 1 | False
    }

    /// <summary>Creates the item an example observes.</summary>
    /// <returns>The item.</returns>
    private static TodoItem CreateItem() => new() { Id = 1, Title = OriginalTitle, Priority = TodoPriority.High };
}
