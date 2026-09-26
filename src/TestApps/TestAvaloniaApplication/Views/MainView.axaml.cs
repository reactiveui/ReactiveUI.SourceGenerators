// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Avalonia.Controls;
using ReactiveUI;

namespace AvaloniaApplication1.Views;

/// <summary>The main view.</summary>
/// <seealso cref="UserControl" />
public partial class MainView : UserControl, IViewFor<ViewModels.MainViewModel>
{
    /// <summary>Initializes a new instance of the <see cref="MainView"/> class.</summary>
    public MainView()
    {
        InitializeComponent();
        ViewModel = new();
    }

    /// <inheritdoc/>
    public ViewModels.MainViewModel? ViewModel { get; set; }

    /// <inheritdoc/>
    object? IViewFor.ViewModel { get => ViewModel; set => ViewModel = (ViewModels.MainViewModel?)value; }
}
