// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Windows.Forms;
using ReactiveUI.Binding.Documentation.Todo;
using CheckBox = System.Windows.Forms.CheckBox;
using Label = System.Windows.Forms.Label;

namespace ReactiveUI.Binding.Documentation.Threading;

/// <summary>The to-do screen as a Windows Forms form: the filter box, the item list, the priority picker, the checkbox for the selected item and the count of items left.</summary>
[System.Diagnostics.DebuggerDisplay("WinFormsTodoForm: ViewModel = {ViewModel}")]
public sealed class WinFormsTodoForm : Form, IViewFor<TodoListViewModel>
{
    /// <summary>Initializes a new instance of the <see cref="WinFormsTodoForm"/> class and puts its controls on the form.</summary>
    public WinFormsTodoForm() => Controls.AddRange([FilterTextBox, ItemsList, PriorityBox, DoneCheckBox, RemainingLabel]);

    /// <summary>Gets or sets the view model the form shows.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public TodoListViewModel? ViewModel { get; set; }

    /// <summary>Gets the box where the user narrows the list.</summary>
    public TextBox FilterTextBox { get; } = new();

    /// <summary>Gets the list of items.</summary>
    public ListBox ItemsList { get; } = new();

    /// <summary>Gets the picker for the priority of the selected item.</summary>
    public ComboBox PriorityBox { get; } = new() { DropDownStyle = ComboBoxStyle.DropDownList };

    /// <summary>Gets the checkbox that ticks the selected item as finished.</summary>
    public CheckBox DoneCheckBox { get; } = new() { Text = "Done" };

    /// <summary>Gets the label that shows how many items are left to do.</summary>
    public Label RemainingLabel { get; } = new();

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (TodoListViewModel?)value;
    }
}
