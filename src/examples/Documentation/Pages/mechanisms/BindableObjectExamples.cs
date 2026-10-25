// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Maui.Controls;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Binding.ObservableForProperty;
using ReactiveUI.Primitives;

namespace ReactiveUI.Binding.Documentation.Mechanisms;

/// <summary>
/// Shows that a MAUI control is a <c>BindableObject</c>, which implements <c>INotifyPropertyChanged</c>, so the
/// <c>PropertyChanged</c> mechanism observes it.
/// </summary>
public static class BindableObjectExamples
{
    /// <summary>The title the user types into the entry.</summary>
    private const string TypedTitle = "Call the plumber";

    /// <summary>The name of the observed text property.</summary>
    private const string TextPropertyName = nameof(Entry.Text);

    /// <summary>Observes the text of the new-title entry with <c>WhenChanged</c>.</summary>
    public static void ObserveEntryTextWithWhenChanged()
    {
        TodoView view = new();

        using (view.NewTitleTextBox.WhenChanged(x => x.Text).Subscribe(static text => Console.WriteLine(text ?? "(none)")))
        {
            view.NewTitleTextBox.Text = TypedTitle;
        }

        // Output:
        // (none)
        // Call the plumber
    }

    /// <summary>Asks both built-in providers about a MAUI entry; <c>PropertyChanged</c> outranks the fallback.</summary>
    public static void AskProvidersAboutEntry()
    {
        INPCObservableForProperty propertyChanged = new();
        POCOObservableForProperty fallback = new();

        Console.WriteLine($"PropertyChanged provider: {propertyChanged.GetAffinityForObject(typeof(Entry), TextPropertyName)}");
        Console.WriteLine($"Fallback provider: {fallback.GetAffinityForObject(typeof(Entry), TextPropertyName)}");

        // Output:
        // PropertyChanged provider: 5
        // Fallback provider: 1
    }
}
