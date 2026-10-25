// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Primitives;

namespace ReactiveUI.Binding.Documentation.UnsafeOverloads;

/// <summary>Demonstrates <c>WhenAnyValueUnsafe</c> over three to sixteen properties as a group of values. Calling an <c>Unsafe</c> method always reads the property paths at run time.</summary>
public static class UnsafeWhenAnyValueWideExamples
{
    /// <summary>The title of the item an example observes.</summary>
    private const string OriginalTitle = "Renew car registration";

    /// <summary>The title an example gives the item.</summary>
    private const string RenamedTitle = "Renew car registration online";

    /// <summary>The text placed between two values.</summary>
    private const string Separator = " | ";

    /// <summary>Observes three properties; each change delivers all three values as one group, and the example prints the first and the last.</summary>
    public static void ObserveThreeColumns()
    {
        TodoItem item = CreateItem();

        using (item.WhenAnyValueUnsafe(x => x.Title, x => x.Id, x => x.IsDone).Subscribe(static values => Console.WriteLine($"{values.Property1}{Separator}{values.Property3}")))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | False
        // Renew car registration online | False
    }

    /// <summary>Observes four properties; each change delivers all four values as one group, and the example prints the first and the last.</summary>
    public static void ObserveFourColumns()
    {
        TodoItem item = CreateItem();

        using (item.WhenAnyValueUnsafe(
            x => x.Title,
            x => x.Id,
            x => x.IsDone,
            x => x.Priority).Subscribe(static values => Console.WriteLine($"{values.Property1}{Separator}{values.Property4}")))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | High
        // Renew car registration online | High
    }

    /// <summary>Observes five properties; each change delivers all five values as one group, and the example prints the first and the last.</summary>
    public static void ObserveFiveColumns()
    {
        TodoItem item = CreateItem();

        using (item.WhenAnyValueUnsafe(
            x => x.Title,
            x => x.Id,
            x => x.IsDone,
            x => x.Priority,
            x => x.Id).Subscribe(static values => Console.WriteLine($"{values.Property1}{Separator}{values.Property5}")))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | 1
        // Renew car registration online | 1
    }

    /// <summary>Observes six properties; each change delivers all six values as one group, and the example prints the first and the last.</summary>
    public static void ObserveSixColumns()
    {
        TodoItem item = CreateItem();

        using (item.WhenAnyValueUnsafe(
            x => x.Title,
            x => x.Id,
            x => x.IsDone,
            x => x.Priority,
            x => x.Id,
            x => x.IsDone).Subscribe(static values => Console.WriteLine($"{values.Property1}{Separator}{values.Property6}")))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | False
        // Renew car registration online | False
    }

    /// <summary>Observes seven properties; each change delivers all seven values as one group, and the example prints the first and the last.</summary>
    public static void ObserveSevenColumns()
    {
        TodoItem item = CreateItem();

        using (item.WhenAnyValueUnsafe(
            x => x.Title,
            x => x.Id,
            x => x.IsDone,
            x => x.Priority,
            x => x.Id,
            x => x.IsDone,
            x => x.Priority).Subscribe(static values => Console.WriteLine($"{values.Property1}{Separator}{values.Property7}")))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | High
        // Renew car registration online | High
    }

    /// <summary>Observes eight properties; each change delivers all eight values as one group, and the example prints the first and the last.</summary>
    public static void ObserveEightColumns()
    {
        TodoItem item = CreateItem();

        using (item.WhenAnyValueUnsafe(
            x => x.Title,
            x => x.Id,
            x => x.IsDone,
            x => x.Priority,
            x => x.Id,
            x => x.IsDone,
            x => x.Priority,
            x => x.Id).Subscribe(static values => Console.WriteLine($"{values.Property1}{Separator}{values.Property8}")))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | 1
        // Renew car registration online | 1
    }

    /// <summary>Observes nine properties; each change delivers all nine values as one group, and the example prints the first and the last.</summary>
    public static void ObserveNineColumns()
    {
        TodoItem item = CreateItem();

        using (item.WhenAnyValueUnsafe(
            x => x.Title,
            x => x.Id,
            x => x.IsDone,
            x => x.Priority,
            x => x.Id,
            x => x.IsDone,
            x => x.Priority,
            x => x.Id,
            x => x.IsDone).Subscribe(static values => Console.WriteLine($"{values.Property1}{Separator}{values.Property9}")))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | False
        // Renew car registration online | False
    }

    /// <summary>Observes ten properties; each change delivers all ten values as one group, and the example prints the first and the last.</summary>
    public static void ObserveTenColumns()
    {
        TodoItem item = CreateItem();

        using (item.WhenAnyValueUnsafe(
            x => x.Title,
            x => x.Id,
            x => x.IsDone,
            x => x.Priority,
            x => x.Id,
            x => x.IsDone,
            x => x.Priority,
            x => x.Id,
            x => x.IsDone,
            x => x.Priority).Subscribe(static values => Console.WriteLine($"{values.Property1}{Separator}{values.Property10}")))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | High
        // Renew car registration online | High
    }

    /// <summary>Observes eleven properties; each change delivers all eleven values as one group, and the example prints the first and the last.</summary>
    public static void ObserveElevenColumns()
    {
        TodoItem item = CreateItem();

        using (item.WhenAnyValueUnsafe(
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
            x => x.Id).Subscribe(static values => Console.WriteLine($"{values.Property1}{Separator}{values.Property11}")))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | 1
        // Renew car registration online | 1
    }

    /// <summary>Observes twelve properties; each change delivers all twelve values as one group, and the example prints the first and the last.</summary>
    public static void ObserveTwelveColumns()
    {
        TodoItem item = CreateItem();

        using (item.WhenAnyValueUnsafe(
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
            x => x.IsDone).Subscribe(static values => Console.WriteLine($"{values.Property1}{Separator}{values.Property12}")))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | False
        // Renew car registration online | False
    }

    /// <summary>Observes thirteen properties; each change delivers all thirteen values as one group, and the example prints the first and the last.</summary>
    public static void ObserveThirteenColumns()
    {
        TodoItem item = CreateItem();

        using (item.WhenAnyValueUnsafe(
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
            x => x.Priority).Subscribe(static values => Console.WriteLine($"{values.Property1}{Separator}{values.Property13}")))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | High
        // Renew car registration online | High
    }

    /// <summary>Observes fourteen properties; each change delivers all fourteen values as one group, and the example prints the first and the last.</summary>
    public static void ObserveFourteenColumns()
    {
        TodoItem item = CreateItem();

        using (item.WhenAnyValueUnsafe(
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
            x => x.Id).Subscribe(static values => Console.WriteLine($"{values.Property1}{Separator}{values.Property14}")))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | 1
        // Renew car registration online | 1
    }

    /// <summary>Observes fifteen properties; each change delivers all fifteen values as one group, and the example prints the first and the last.</summary>
    public static void ObserveFifteenColumns()
    {
        TodoItem item = CreateItem();

        using (item.WhenAnyValueUnsafe(
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
            x => x.IsDone).Subscribe(static values => Console.WriteLine($"{values.Property1}{Separator}{values.Property15}")))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | False
        // Renew car registration online | False
    }

    /// <summary>Observes sixteen properties; each change delivers all sixteen values as one group, and the example prints the first and the last.</summary>
    public static void ObserveSixteenColumns()
    {
        TodoItem item = CreateItem();

        using (item.WhenAnyValueUnsafe(
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
            x => x.Priority).Subscribe(static values => Console.WriteLine($"{values.Property1}{Separator}{values.Property16}")))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration | High
        // Renew car registration online | High
    }

    /// <summary>Creates the item an example observes.</summary>
    /// <returns>The item.</returns>
    private static TodoItem CreateItem() => new() { Id = 1, Title = OriginalTitle, Priority = TodoPriority.High };
}
