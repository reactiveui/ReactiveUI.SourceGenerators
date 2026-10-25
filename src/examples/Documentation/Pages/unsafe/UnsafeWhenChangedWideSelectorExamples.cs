// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Primitives;

namespace ReactiveUI.Binding.Documentation.UnsafeOverloads;

/// <summary>Demonstrates <c>WhenChangedUnsafe</c> over four to sixteen properties with a selector. Calling an <c>Unsafe</c> method always reads the property paths at run time.</summary>
public static class UnsafeWhenChangedWideSelectorExamples
{
    /// <summary>The title of the item an example observes.</summary>
    private const string OriginalTitle = "Renew car registration";

    /// <summary>The title an example gives the item.</summary>
    private const string RenamedTitle = "Renew car registration online";

    /// <summary>The text placed between two values.</summary>
    private const string Separator = " | ";

    /// <summary>Observes four properties after they change and combines them with a selector.</summary>
    public static void ObserveFourColumnsWithSelector()
    {
        TodoItem item = CreateItem();

        using (item.WhenChangedUnsafe(
            x => x.Title,
            x => x.Id,
            x => x.IsDone,
            x => x.Priority,
            static (c1, c2, c3, c4) =>
            string.Join(Separator, c1, c2, c3, c4)).Subscribe(Console.WriteLine))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | 1 | False | High
        // Renew car registration online | 1 | False | High
    }

    /// <summary>Observes five properties after they change and combines them with a selector.</summary>
    public static void ObserveFiveColumnsWithSelector()
    {
        TodoItem item = CreateItem();

        using (item.WhenChangedUnsafe(
            x => x.Title,
            x => x.Id,
            x => x.IsDone,
            x => x.Priority,
            x => x.Id,
            static (c1, c2, c3, c4, c5) =>
            string.Join(Separator, c1, c2, c3, c4, c5)).Subscribe(Console.WriteLine))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | 1 | False | High | 1
        // Renew car registration online | 1 | False | High | 1
    }

    /// <summary>Observes six properties after they change and combines them with a selector.</summary>
    public static void ObserveSixColumnsWithSelector()
    {
        TodoItem item = CreateItem();

        using (item.WhenChangedUnsafe(
            x => x.Title,
            x => x.Id,
            x => x.IsDone,
            x => x.Priority,
            x => x.Id,
            x => x.IsDone,
            static (c1, c2, c3, c4, c5, c6) =>
            string.Join(Separator, c1, c2, c3, c4, c5, c6)).Subscribe(Console.WriteLine))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | 1 | False | High | 1 | False
        // Renew car registration online | 1 | False | High | 1 | False
    }

    /// <summary>Observes seven properties after they change and combines them with a selector.</summary>
    public static void ObserveSevenColumnsWithSelector()
    {
        TodoItem item = CreateItem();

        using (item.WhenChangedUnsafe(
            x => x.Title,
            x => x.Id,
            x => x.IsDone,
            x => x.Priority,
            x => x.Id,
            x => x.IsDone,
            x => x.Priority,
            static (c1, c2, c3, c4, c5, c6, c7) =>
            string.Join(Separator, c1, c2, c3, c4, c5, c6, c7)).Subscribe(Console.WriteLine))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | 1 | False | High | 1 | False | High
        // Renew car registration online | 1 | False | High | 1 | False | High
    }

    /// <summary>Observes eight properties after they change and combines them with a selector.</summary>
    public static void ObserveEightColumnsWithSelector()
    {
        TodoItem item = CreateItem();

        using (item.WhenChangedUnsafe(
            x => x.Title,
            x => x.Id,
            x => x.IsDone,
            x => x.Priority,
            x => x.Id,
            x => x.IsDone,
            x => x.Priority,
            x => x.Id,
            static (c1, c2, c3, c4, c5, c6, c7, c8) =>
            string.Join(Separator, c1, c2, c3, c4, c5, c6, c7, c8)).Subscribe(Console.WriteLine))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | 1 | False | High | 1 | False | High | 1
        // Renew car registration online | 1 | False | High | 1 | False | High | 1
    }

    /// <summary>Observes nine properties after they change and combines them with a selector.</summary>
    public static void ObserveNineColumnsWithSelector()
    {
        TodoItem item = CreateItem();

        using (item.WhenChangedUnsafe(
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
            string.Join(Separator, c1, c2, c3, c4, c5, c6, c7, c8, c9)).Subscribe(Console.WriteLine))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | 1 | False | High | 1 | False | High | 1 | False
        // Renew car registration online | 1 | False | High | 1 | False | High | 1 | False
    }

    /// <summary>Observes ten properties after they change and combines them with a selector.</summary>
    public static void ObserveTenColumnsWithSelector()
    {
        TodoItem item = CreateItem();

        using (item.WhenChangedUnsafe(
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
            string.Join(Separator, c1, c2, c3, c4, c5, c6, c7, c8, c9, c10)).Subscribe(Console.WriteLine))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | 1 | False | High | 1 | False | High | 1 | False | High
        // Renew car registration online | 1 | False | High | 1 | False | High | 1 | False | High
    }

    /// <summary>Observes eleven properties after they change and combines them with a selector.</summary>
    public static void ObserveElevenColumnsWithSelector()
    {
        TodoItem item = CreateItem();

        using (item.WhenChangedUnsafe(
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
            string.Join(Separator, c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11)).Subscribe(Console.WriteLine))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | 1 | False | High | 1 | False | High | 1 | False | High | 1
        // Renew car registration online | 1 | False | High | 1 | False | High | 1 | False | High | 1
    }

    /// <summary>Observes twelve properties after they change and combines them with a selector.</summary>
    public static void ObserveTwelveColumnsWithSelector()
    {
        TodoItem item = CreateItem();

        using (item.WhenChangedUnsafe(
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
            string.Join(Separator, c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11, c12)).Subscribe(Console.WriteLine))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | 1 | False | High | 1 | False | High | 1 | False | High | 1 | False
        // Renew car registration online | 1 | False | High | 1 | False | High | 1 | False | High | 1 | False
    }

    /// <summary>Observes thirteen properties after they change and combines them with a selector.</summary>
    public static void ObserveThirteenColumnsWithSelector()
    {
        TodoItem item = CreateItem();

        using (item.WhenChangedUnsafe(
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
            x => x.Priority,
            static (c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11, c12, c13) =>
            string.Join(Separator, c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11, c12, c13)).Subscribe(Console.WriteLine))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | 1 | False | High | 1 | False | High | 1 | False | High | 1 | False | High
        // Renew car registration online | 1 | False | High | 1 | False | High | 1 | False | High | 1 | False | High
    }

    /// <summary>Observes fourteen properties after they change and combines them with a selector.</summary>
    public static void ObserveFourteenColumnsWithSelector()
    {
        TodoItem item = CreateItem();

        using (item.WhenChangedUnsafe(
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
            x => x.Priority,
            x => x.Id,
            static (c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11, c12, c13, c14) =>
            string.Join(Separator, c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11, c12, c13, c14)).Subscribe(Console.WriteLine))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | 1 | False | High | 1 | False | High | 1 | False | High | 1 | False | High | 1
        // Renew car registration online | 1 | False | High | 1 | False | High | 1 | False | High | 1 | False | High | 1
    }

    /// <summary>Observes fifteen properties after they change and combines them with a selector.</summary>
    public static void ObserveFifteenColumnsWithSelector()
    {
        TodoItem item = CreateItem();

        using (item.WhenChangedUnsafe(
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
            x => x.Priority,
            x => x.Id,
            x => x.IsDone,
            static (c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11, c12, c13, c14, c15) =>
            string.Join(Separator, c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11, c12, c13, c14, c15)).Subscribe(Console.WriteLine))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | 1 | False | High | 1 | False | High | 1 | False | High | 1 | False | High | 1 | False
        // Renew car registration online | 1 | False | High | 1 | False | High | 1 | False | High | 1 | False | High | 1 | False
    }

    /// <summary>Observes sixteen properties after they change and combines them with a selector.</summary>
    public static void ObserveSixteenColumnsWithSelector()
    {
        TodoItem item = CreateItem();

        using (item.WhenChangedUnsafe(
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
            x => x.Priority,
            x => x.Id,
            x => x.IsDone,
            x => x.Priority,
            static (c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11, c12, c13, c14, c15, c16) =>
            string.Join(Separator, c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11, c12, c13, c14, c15, c16)).Subscribe(Console.WriteLine))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | 1 | False | High | 1 | False | High | 1 | False | High | 1 | False | High | 1 | False | High
        // Renew car registration online | 1 | False | High | 1 | False | High | 1 | False | High | 1 | False | High | 1 | False | High
    }

    /// <summary>Creates the item an example observes.</summary>
    /// <returns>The item.</returns>
    private static TodoItem CreateItem() => new() { Id = 1, Title = OriginalTitle, Priority = TodoPriority.High };
}
