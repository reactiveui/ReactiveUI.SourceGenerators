// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.CloudStorage;
using ProgressBar = System.Windows.Controls.ProgressBar;
using Window = System.Windows.Window;

namespace ReactiveUI.Binding.Documentation.Threading;

/// <summary>The storage upload screen as a WPF window: the progress bar of the running upload.</summary>
[System.Diagnostics.DebuggerDisplay("WpfUploadWindow: ViewModel = {ViewModel}")]
public sealed class WpfUploadWindow : Window, IViewFor<StorageBrowserViewModel>
{
    /// <summary>The value of a full progress bar.</summary>
    private const double FullPercent = 100;

    /// <summary>Gets or sets the view model the window shows.</summary>
    public StorageBrowserViewModel? ViewModel { get; set; }

    /// <summary>Gets the bar that shows how much of the running upload the service has received.</summary>
    public ProgressBar UploadProgressBar { get; } = new() { Minimum = 0, Maximum = FullPercent };

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (StorageBrowserViewModel?)value;
    }
}
