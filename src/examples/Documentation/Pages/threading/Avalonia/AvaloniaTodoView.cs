// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Avalonia.Controls;
using ReactiveUI.Binding.Documentation.Todo;
using CheckBox = Avalonia.Controls.CheckBox;

namespace ReactiveUI.Binding.Documentation.Threading;

/// <summary>The to-do screen as an Avalonia view: the filter box, the item list, the checkbox for the selected item and the count of items left.</summary>
[System.Diagnostics.DebuggerDisplay("AvaloniaTodoView: ViewModel = {ViewModel}")]
public sealed class AvaloniaTodoView : UserControl, IViewFor<TodoListViewModel>
{
    /// <summary>Gets or sets the view model the view shows.</summary>
    public TodoListViewModel? ViewModel { get; set; }

    /// <summary>Gets the box where the user narrows the list.</summary>
    public TextBox FilterTextBox { get; } = new();

    /// <summary>Gets the list of items.</summary>
    public ListBox ItemsList { get; } = new();

    /// <summary>Gets the checkbox that ticks the selected item as finished.</summary>
    public CheckBox DoneCheckBox { get; } = new() { Content = "Done" };

    /// <summary>Gets the label that shows how many items are left to do.</summary>
    public TextBlock RemainingLabel { get; } = new();

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (TodoListViewModel?)value;
    }
}
