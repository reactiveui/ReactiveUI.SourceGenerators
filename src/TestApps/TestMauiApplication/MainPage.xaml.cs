// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace MauiApp1;

/// <summary>The main page, whose <c>IViewFor</c> implementation is source generated.</summary>
/// <seealso cref="ContentPage" />
[ReactiveUI.SourceGenerators.IViewFor<MainViewModel>]
public partial class MainPage : ContentPage
{
    /// <summary>Initializes a new instance of the <see cref="MainPage"/> class.</summary>
    public MainPage()
    {
        InitializeComponent();
        ViewModel = new();
    }
}
