// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Primitives;

namespace ReactiveUI.Binding.Documentation.Setup.Aot;

/// <summary>
/// Bindings in a program published with Native AOT. The generator writes each binding at build time, so the published
/// program needs no reflection, expression trees or run-time code generation.
/// </summary>
public static class AotExamples
{
    /// <summary>The title an example gives the item.</summary>
    private const string RenamedTitle = "Renew car registration online";

    /// <summary>The text of a finished item.</summary>
    private const string DoneText = "Done";

    /// <summary>The text of an unfinished item.</summary>
    private const string OpenText = "Open";

    /// <summary>Observes the title of an item with <c>WhenChanged</c>.</summary>
    public static void ObserveTitleChange()
    {
        TodoItem item = new TodoItem { Title = "Renew car registration" };

        using (item.WhenChanged(x => x.Title).Subscribe(Console.WriteLine))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration
        // Renew car registration online
    }

    /// <summary>Binds the finished flag of an item to the status text of a row, turning the flag into words.</summary>
    public static void BindDoneFlagOneWay()
    {
        TodoItem item = new TodoItem { Title = "File quarterly tax return" };
        TodoRowView row = new TodoRowView();

        using (item.BindOneWay(row, x => x.IsDone, v => v.StatusText, static done => done ? DoneText : OpenText))
        {
            Console.WriteLine(row.StatusText);

            item.IsDone = true;

            Console.WriteLine(row.StatusText);
        }

        // Output:
        // Open
        // Done
    }

    /// <summary>Carries the title both ways between an item and the title text of a row.</summary>
    public static void BindTitleTwoWay()
    {
        TodoItem item = new TodoItem { Title = "Book dentist appointment" };
        TodoRowView row = new TodoRowView();

        using (item.BindTwoWay(row, x => x.Title, v => v.TitleText))
        {
            Console.WriteLine(row.TitleText);

            row.TitleText = "Book dentist appointment for Friday";

            Console.WriteLine(item.Title);
        }

        // Output:
        // Book dentist appointment
        // Book dentist appointment for Friday
    }

    /// <summary>Shows that the program cannot generate code at run time, so every binding it ran was written at build time.</summary>
    public static void ShowNoRuntimeCodeGeneration()
    {
        Console.WriteLine(RuntimeFeature.IsDynamicCodeSupported);

        // Output:
        // False
    }
}
