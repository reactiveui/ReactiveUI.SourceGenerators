// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using ReactiveUI.Binding.Documentation.Todo;

namespace ReactiveUI.Binding.Documentation.Threading;

/// <summary>The to-do screen as a MAUI page: the filter entry, the checkbox and the tick mark for the selected item, and the count of items left.</summary>
[System.Diagnostics.DebuggerDisplay("MauiTodoPage: ViewModel = {ViewModel}")]
public sealed class MauiTodoPage : ContentPage, IViewFor<TodoListViewModel>
{
    /// <summary>The bindable property behind <see cref="DoneMarkVisibility"/>.</summary>
    public static readonly BindableProperty DoneMarkVisibilityProperty = BindableProperty.Create(
        nameof(DoneMarkVisibility),
        typeof(Visibility),
        typeof(MauiTodoPage),
        Visibility.Collapsed,
        propertyChanged: OnDoneMarkVisibilityChanged);

    /// <summary>Gets or sets the view model the page shows.</summary>
    public TodoListViewModel? ViewModel { get; set; }

    /// <summary>Gets the entry where the user narrows the list.</summary>
    public Entry FilterEntry { get; } = new();

    /// <summary>Gets the checkbox that ticks the selected item as finished.</summary>
    public CheckBox DoneCheckBox { get; } = new();

    /// <summary>Gets the tick mark that shows while the selected item is finished.</summary>
    public Label DoneMark { get; } = new() { Text = "Done", IsVisible = false };

    /// <summary>Gets the label that shows how many items are left to do.</summary>
    public Label RemainingLabel { get; } = new();

    /// <summary>Gets or sets whether the tick mark shows. A label exposes <c>IsVisible</c> as a boolean, so the page exposes the <see cref="Visibility"/> a binding writes.</summary>
    public Visibility DoneMarkVisibility
    {
        get => (Visibility)GetValue(DoneMarkVisibilityProperty);
        set => SetValue(DoneMarkVisibilityProperty, value);
    }

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (TodoListViewModel?)value;
    }

    /// <summary>Shows or hides the tick mark to match the new visibility.</summary>
    /// <param name="bindable">The page that changed.</param>
    /// <param name="oldValue">The previous visibility.</param>
    /// <param name="newValue">The new visibility.</param>
    private static void OnDoneMarkVisibilityChanged(BindableObject bindable, object oldValue, object newValue) =>
        ((MauiTodoPage)bindable).DoneMark.IsVisible = (Visibility)newValue == Visibility.Visible;
}
