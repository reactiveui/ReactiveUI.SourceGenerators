// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Maui.Controls;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Primitives.Signals;

namespace ReactiveUI.Binding.Documentation.Bindings;

/// <summary>Shows <see cref="ICreatesCommandBinding"/>, the plug-in that attaches a command to a control, by asking a binder directly.</summary>
public static class CreatesCommandBindingExamples
{
    /// <summary>The name of the event a MAUI button raises when the user presses it.</summary>
    private const string ClickedEventName = "Clicked";

    /// <summary>The name of an event the button does not have.</summary>
    private const string PressedEventName = "Pressed";

    /// <summary>The text of the filter the customer types.</summary>
    private const string CarFilter = "car";

    /// <summary>Asks a binder how well it fits two controls: the score is positive for a button and zero for an entry box.</summary>
    public static void AskBinderForAffinity()
    {
        ICreatesCommandBinding binder = ChooseBinder();

        Console.WriteLine(binder.GetAffinityForObject<Button>(hasEventTarget: false));
        Console.WriteLine(binder.GetAffinityForObject<Entry>(hasEventTarget: false));

        // Output:
        // 10
        // 0
    }

    /// <summary>Attaches the export command to the export button through the default event; disposing the binding detaches it.</summary>
    public static void BindExportButtonToDefaultEvent()
    {
        StatementExportViewModel viewModel = new() { HasStatement = true };
        StatementExportView view = new();
        ICreatesCommandBinding binder = ChooseBinder();

        using (binder.BindCommandToObject(viewModel.ExportCommand, view.ExportButton, Signal.Never<object?>()))
        {
            ((IButtonController)view.ExportButton).SendClicked();

            Console.WriteLine(viewModel.ExportCount);
        }

        ((IButtonController)view.ExportButton).SendClicked();

        Console.WriteLine(viewModel.ExportCount);

        // Output:
        // 1
        // 1
    }

    /// <summary>Attaches the export command to an event chosen by name; an event the binder does not know gives no binding.</summary>
    public static void BindExportButtonToNamedEvent()
    {
        StatementExportViewModel viewModel = new() { HasStatement = true };
        StatementExportView view = new();
        ICreatesCommandBinding binder = ChooseBinder();

        using IDisposable? clicked = binder.BindCommandToObject<Button, EventArgs>(viewModel.ExportCommand, view.ExportButton, Signal.Never<object?>(), ClickedEventName);
        IDisposable? pressed = binder.BindCommandToObject<Button, EventArgs>(viewModel.ExportCommand, view.ExportButton, Signal.Never<object?>(), PressedEventName);

        ((IButtonController)view.ExportButton).SendClicked();

        Console.WriteLine(viewModel.ExportCount);
        Console.WriteLine(pressed is null);

        // Output:
        // The binder is asked for the Clicked event, which carries EventArgs
        // The binder is asked for the Pressed event, which carries EventArgs
        // 1
        // True
    }

    /// <summary>Attaches a search command to the filter box with the add and remove handlers the caller supplies, so no reflection finds the event.</summary>
    public static void BindSearchToTextChangedEvent()
    {
        TodoView view = new();
        ICreatesCommandBinding binder = ChooseBinder();
        Command search = new(static () => Console.WriteLine("search"));
        Entry entry = view.FilterTextBox;

        using (binder.BindCommandToObject<Entry, TextChangedEventArgs>(
            search,
            entry,
            Signal.Never<object?>(),
            handler => entry.TextChanged += handler,
            handler => entry.TextChanged -= handler))
        {
            entry.Text = CarFilter;
        }

        entry.Text = string.Empty;

        // Output:
        // search
    }

    /// <summary>Picks the binder with the highest affinity for a button, the way the runtime picks among the binders it knows.</summary>
    /// <returns>The binder that fits a button best.</returns>
    private static ICreatesCommandBinding ChooseBinder()
    {
        ICreatesCommandBinding[] binders = [new ClickedCommandBinder()];

        return binders.MaxBy(static candidate => candidate.GetAffinityForObject<Button>(hasEventTarget: false))!;
    }
}
