// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Maui.Controls;
using ReactiveUI.Binding.Documentation.Infrastructure;
using ReactiveUI.Binding.Documentation.Todo;

namespace ReactiveUI.Binding.Documentation.Views;

/// <summary>A preview of one to-do item. It is left out of the generated lookup, so the application maps it itself.</summary>
[ExcludeFromViewRegistration]
[System.Diagnostics.DebuggerDisplay("TodoItemPreviewView: ViewModel = {ViewModel}")]
public sealed class TodoItemPreviewView : ObservableObject, IViewFor<TodoItem>
{
    /// <summary>Gets or sets the view model the view shows.</summary>
    public TodoItem? ViewModel
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>Gets the label that shows the title of the item.</summary>
    public Label TitleLabel { get; } = new();

    /// <summary>Gets or sets a value indicating whether the preview uses the compact layout.</summary>
    public bool IsCompact
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (TodoItem?)value;
    }
}
