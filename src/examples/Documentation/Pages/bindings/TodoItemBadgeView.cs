// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Maui.Controls;
using ReactiveUI.Binding.Documentation.Infrastructure;
using ReactiveUI.Binding.Documentation.Todo;

namespace ReactiveUI.Binding.Documentation.Bindings;

/// <summary>The row of one to-do item in a list: a badge that shows how urgent the item is.</summary>
[System.Diagnostics.DebuggerDisplay("TodoItemBadgeView: ViewModel = {ViewModel}")]
public sealed class TodoItemBadgeView : ObservableObject, IViewFor<TodoItem>
{
    /// <summary>Gets or sets the item the row shows.</summary>
    public TodoItem? ViewModel
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>Gets the badge coloured by the priority of the item.</summary>
    public BoxView PriorityBadge { get; } = new();

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (TodoItem?)value;
    }
}
