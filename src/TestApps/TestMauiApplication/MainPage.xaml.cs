// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI;

namespace MauiApp1;

/// <summary>The main page.</summary>
/// <seealso cref="ContentPage" />
public partial class MainPage : ContentPage, IViewFor<MainViewModel>
{
    /// <summary>Initializes a new instance of the <see cref="MainPage"/> class.</summary>
    public MainPage()
    {
        InitializeComponent();
        ViewModel = new();
    }

    /// <inheritdoc/>
    public MainViewModel? ViewModel { get; set; }

    /// <inheritdoc/>
    object? IViewFor.ViewModel { get => ViewModel; set => ViewModel = (MainViewModel?)value; }
}
