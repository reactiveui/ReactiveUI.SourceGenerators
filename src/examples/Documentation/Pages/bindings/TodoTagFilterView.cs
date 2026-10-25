// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Maui.Controls;
using ReactiveUI.Binding.Documentation.Infrastructure;
using ReactiveUI.Binding.Documentation.Todo;

namespace ReactiveUI.Binding.Documentation.Bindings;

/// <summary>A picker that narrows the to-do list to one tag. The picker holds its selection as an object.</summary>
[System.Diagnostics.DebuggerDisplay("TodoTagFilterView: ViewModel = {ViewModel}")]
public sealed class TodoTagFilterView : ObservableObject, IViewFor<TodoListViewModel>
{
    /// <summary>Gets or sets the view model the view shows.</summary>
    public TodoListViewModel? ViewModel
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>Gets the picker that lists the tags.</summary>
    public Picker TagPicker { get; } = new() { ItemsSource = new[] { "car", "health", "family" } };

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (TodoListViewModel?)value;
    }
}
