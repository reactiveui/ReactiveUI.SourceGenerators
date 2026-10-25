// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows;
using ReactiveUI;

namespace WpfApp1;

/// <summary>Provides the interaction logic for MainWindow.xaml.</summary>
public partial class MainWindow : Window, IViewFor<MainViewModel>
{
    /// <summary>Initializes a new instance of the <see cref="MainWindow"/> class.</summary>
    public MainWindow()
    {
        InitializeComponent();
        ViewModel = new();
        DataContext = ViewModel;
    }

    /// <inheritdoc/>
    public MainViewModel? ViewModel { get; set; }

    /// <inheritdoc/>
    object? IViewFor.ViewModel { get => ViewModel; set => ViewModel = (MainViewModel?)value; }
}
