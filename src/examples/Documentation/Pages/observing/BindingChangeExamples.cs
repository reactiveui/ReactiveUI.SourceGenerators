// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.Observing;

/// <summary>
/// Shows <see cref="BindingChange"/>, the value a binding reports each time it writes. It holds the value that was
/// written and whether the write went from the view model to the view. It is a read-only record struct, so two changes
/// are equal when both members are equal.
/// </summary>
public static class BindingChangeExamples
{
    /// <summary>The title a to-do item is renamed to.</summary>
    private const string RenamedTitle = "Renew car registration online";

    /// <summary>Compares changes; equal members make equal changes.</summary>
    public static void CompareBindingChanges()
    {
        BindingChange fromViewModel = new(RenamedTitle, true);
        BindingChange sameChange = new(RenamedTitle, true);
        BindingChange fromView = new(RenamedTitle, false);

        Console.WriteLine(fromViewModel == sameChange);
        Console.WriteLine(fromViewModel != fromView);
        Console.WriteLine(fromViewModel.Equals(sameChange));
        Console.WriteLine(fromViewModel.Equals((object)fromView));
        Console.WriteLine(fromViewModel.GetHashCode() == sameChange.GetHashCode());

        // Output:
        // True
        // True
        // True
        // False
        // True
    }

    /// <summary>Prints a change; the text names each member.</summary>
    public static void PrintBindingChange()
    {
        BindingChange change = new(RenamedTitle, true);

        string text = change.ToString();

        Console.WriteLine($"Change: {text}.");

        // Output:
        // Change: BindingChange { Value = Renew car registration online, FromViewModel = True }.
    }
}
