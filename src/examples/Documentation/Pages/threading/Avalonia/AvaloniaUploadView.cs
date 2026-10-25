// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Avalonia.Controls;
using ReactiveUI.Binding.Documentation.CloudStorage;
using ProgressBar = Avalonia.Controls.ProgressBar;

namespace ReactiveUI.Binding.Documentation.Threading;

/// <summary>The storage upload screen as an Avalonia view: the progress bar of the running upload.</summary>
[System.Diagnostics.DebuggerDisplay("AvaloniaUploadView: ViewModel = {ViewModel}")]
public sealed class AvaloniaUploadView : UserControl, IViewFor<StorageBrowserViewModel>
{
    /// <summary>Gets or sets the view model the view shows.</summary>
    public StorageBrowserViewModel? ViewModel { get; set; }

    /// <summary>Gets the bar that shows how much of the running upload the service has received, from 0 to 100.</summary>
    public ProgressBar UploadProgressBar { get; } = new();

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (StorageBrowserViewModel?)value;
    }
}
