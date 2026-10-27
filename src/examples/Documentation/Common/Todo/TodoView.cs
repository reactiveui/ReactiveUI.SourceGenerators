// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Maui.Controls;
using ReactiveUI.Binding.Documentation.Infrastructure;

namespace ReactiveUI.Binding.Documentation.Todo;

/// <summary>
/// The to-do screen: a list with a title box to add items and a detail area for the selected item. A real UI
/// framework builds these controls from markup; here the view creates them in code.
/// </summary>
[System.Diagnostics.DebuggerDisplay("TodoView: ViewModel = {ViewModel}")]
public sealed class TodoView : ObservableObject, IViewFor<TodoListViewModel>
{
    /// <summary>Gets or sets the view model the view shows.</summary>
    public TodoListViewModel? ViewModel
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>Gets the entry where the user types the title of a new item.</summary>
    public Entry NewTitleTextBox { get; } = new() { Placeholder = "What needs doing?" };

    /// <summary>Gets the entry where the user narrows the list.</summary>
    public Entry FilterTextBox { get; } = new() { Placeholder = "Filter" };

    /// <summary>Gets the button that adds the new item.</summary>
    public Button AddButton { get; } = new() { Text = "Add" };

    /// <summary>Gets the button that finishes the selected item.</summary>
    public Button CompleteButton { get; } = new() { Text = "Complete" };

    /// <summary>Gets the list of items.</summary>
    public CollectionView ItemsList { get; } = new() { SelectionMode = SelectionMode.Single };

    /// <summary>Gets the label that shows how many items are left to do.</summary>
    public Label RemainingLabel { get; } = new();

    /// <summary>Gets the label that shows the last error.</summary>
    public Label ErrorLabel { get; } = new();

    /// <summary>Gets the entry that edits the title of the selected item.</summary>
    public Entry SelectedTitleTextBox { get; } = new();

    /// <summary>Gets the entry that edits the notes of the selected item.</summary>
    public Entry NotesTextBox { get; } = new();

    /// <summary>Gets the entry that edits the due date of the selected item.</summary>
    public Entry DueDateTextBox { get; } = new() { Placeholder = "yyyy-MM-dd" };

    /// <summary>Gets the check box that ticks the selected item as finished.</summary>
    public CheckBox DoneCheckBox { get; } = new();

    /// <summary>Gets the label that shows the tags of the selected item.</summary>
    public Label TagsLabel { get; } = new();

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (TodoListViewModel?)value;
    }
}
