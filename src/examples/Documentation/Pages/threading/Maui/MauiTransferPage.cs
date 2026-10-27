// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using ReactiveUI.Binding.Documentation.Banking;

namespace ReactiveUI.Binding.Documentation.Threading;

/// <summary>The transfer screen as a MAUI page: the amount entry, the send button and the labels for validation and the receipt.</summary>
[System.Diagnostics.DebuggerDisplay("MauiTransferPage: ViewModel = {ViewModel}")]
public sealed class MauiTransferPage : ContentPage, IViewFor<TransferViewModel>
{
    /// <summary>The bindable property behind <see cref="ValidationVisibility"/>.</summary>
    public static readonly BindableProperty ValidationVisibilityProperty = BindableProperty.Create(
        nameof(ValidationVisibility),
        typeof(Visibility),
        typeof(MauiTransferPage),
        Visibility.Collapsed,
        propertyChanged: OnValidationVisibilityChanged);

    /// <summary>Gets or sets the view model the page shows.</summary>
    public TransferViewModel? ViewModel { get; set; }

    /// <summary>Gets the entry where the customer types the amount.</summary>
    public Entry AmountEntry { get; } = new();

    /// <summary>Gets the button that sends the transfer.</summary>
    public Button TransferButton { get; } = new() { Text = "Send" };

    /// <summary>Gets the label that shows what is wrong with the draft.</summary>
    public Label ValidationLabel { get; } = new();

    /// <summary>Gets or sets whether the validation label shows. A label exposes <c>IsVisible</c> as a boolean, so the page exposes the <see cref="Visibility"/> a binding writes.</summary>
    public Visibility ValidationVisibility
    {
        get => (Visibility)GetValue(ValidationVisibilityProperty);
        set => SetValue(ValidationVisibilityProperty, value);
    }

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (TransferViewModel?)value;
    }

    /// <summary>Shows or hides the validation label to match the new visibility.</summary>
    /// <param name="bindable">The page that changed.</param>
    /// <param name="oldValue">The previous visibility.</param>
    /// <param name="newValue">The new visibility.</param>
    private static void OnValidationVisibilityChanged(BindableObject bindable, object oldValue, object newValue) =>
        ((MauiTransferPage)bindable).ValidationLabel.IsVisible = (Visibility)newValue == Visibility.Visible;
}
