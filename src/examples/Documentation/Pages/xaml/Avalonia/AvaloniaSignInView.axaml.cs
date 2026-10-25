// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Avalonia.Controls;

namespace ReactiveUI.Binding.Documentation.Xaml;

/// <summary>
/// An Avalonia view whose controls are named in XAML. Avalonia's name generator declares a field for each, and the
/// binding generator reads the view to bind them.
/// </summary>
[System.Diagnostics.DebuggerDisplay("AvaloniaSignInView: ViewModel = {ViewModel}")]
public partial class AvaloniaSignInView : UserControl, IViewFor<SignInViewModel>
{
    /// <summary>Initializes a new instance of the <see cref="AvaloniaSignInView"/> class.</summary>
    public AvaloniaSignInView() => InitializeComponent();

    /// <inheritdoc/>
    public SignInViewModel? ViewModel { get; set; }

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (SignInViewModel?)value;
    }
}
