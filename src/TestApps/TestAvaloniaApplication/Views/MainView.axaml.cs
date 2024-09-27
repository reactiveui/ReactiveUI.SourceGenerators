// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Avalonia.Controls;

namespace AvaloniaApplication1.Views;

/// <summary>
/// MainView.
/// </summary>
/// <seealso cref="UserControl" />
[ReactiveUI.SourceGenerators.IViewFor<ViewModels.MainViewModel>]
public partial class MainView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainView"/> class.
    /// </summary>
    public MainView()
    {
        InitializeComponent();
        ViewModel = new ViewModels.MainViewModel();
    }
}
