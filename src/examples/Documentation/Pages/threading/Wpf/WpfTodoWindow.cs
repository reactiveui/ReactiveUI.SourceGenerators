// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows;
using System.Windows.Controls;
using ReactiveUI.Binding.Documentation.Todo;
using CheckBox = System.Windows.Controls.CheckBox;
using Window = System.Windows.Window;

namespace ReactiveUI.Binding.Documentation.Threading;

/// <summary>The to-do screen as a WPF window: the filter box, the checkbox and the tick mark for the selected item, and the count of items left.</summary>
[System.Diagnostics.DebuggerDisplay("WpfTodoWindow: ViewModel = {ViewModel}")]
public sealed class WpfTodoWindow : Window, IViewFor<TodoListViewModel>
{
    /// <summary>The dependency property behind <see cref="ViewModel"/>.</summary>
    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
        nameof(ViewModel),
        typeof(TodoListViewModel),
        typeof(WpfTodoWindow));

    /// <summary>Gets or sets the view model the window shows.</summary>
    public TodoListViewModel? ViewModel
    {
        get => (TodoListViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    /// <summary>Gets the box where the user narrows the list.</summary>
    public TextBox FilterTextBox { get; } = new();

    /// <summary>Gets the checkbox that ticks the selected item as finished.</summary>
    public CheckBox DoneCheckBox { get; } = new() { Content = "Done" };

    /// <summary>Gets the tick mark that is visible while the selected item is finished.</summary>
    public TextBlock DoneMark { get; } = new() { Text = "Done", Visibility = Visibility.Collapsed };

    /// <summary>Gets the label that shows how many items are left to do.</summary>
    public TextBlock RemainingLabel { get; } = new();

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (TodoListViewModel?)value;
    }
}
