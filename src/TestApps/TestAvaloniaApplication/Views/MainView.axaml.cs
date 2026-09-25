// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Avalonia.Controls;

namespace AvaloniaApplication1.Views;

/// <summary>The main view, whose <c>IViewFor</c> implementation is source generated.</summary>
/// <seealso cref="UserControl" />
[ReactiveUI.SourceGenerators.IViewFor<ViewModels.MainViewModel>]
public partial class MainView : UserControl
{
    /// <summary>Initializes a new instance of the <see cref="MainView"/> class.</summary>
    public MainView()
    {
        InitializeComponent();
        ViewModel = new();
    }
}
