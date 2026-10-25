// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Maui.Controls;

namespace ReactiveUI.Binding.Documentation.Xaml;

/// <summary>
/// A MAUI page whose controls are named in XAML. MAUI's source generator declares a field for each, and the binding
/// generator reads the page to bind them.
/// </summary>
[System.Diagnostics.DebuggerDisplay("MauiSignInPage: ViewModel = {ViewModel}")]
public partial class MauiSignInPage : ContentPage, IViewFor<SignInViewModel>
{
    /// <summary>Initializes a new instance of the <see cref="MauiSignInPage"/> class.</summary>
    public MauiSignInPage() => InitializeComponent();

    /// <inheritdoc/>
    public SignInViewModel? ViewModel { get; set; }

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (SignInViewModel?)value;
    }
}
