// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Maui.Controls;
using ReactiveUI.Binding.Documentation.Infrastructure;
using ReactiveUI.Binding.Documentation.Todo;

namespace ReactiveUI.Binding.Documentation.Views;

/// <summary>The detail screen of one to-do item. It is left out of the generated lookup, so the application registers it with the service locator.</summary>
[ExcludeFromViewRegistration]
[System.Diagnostics.DebuggerDisplay("TodoItemDetailView: ViewModel = {ViewModel}")]
public sealed class TodoItemDetailView : ObservableObject, IViewFor<TodoItem>
{
    /// <summary>Gets or sets the view model the view shows.</summary>
    public TodoItem? ViewModel
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>Gets the label that shows the notes of the item.</summary>
    public Label NotesLabel { get; } = new();

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (TodoItem?)value;
    }
}
