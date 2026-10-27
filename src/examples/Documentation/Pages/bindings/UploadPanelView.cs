// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Maui.Controls;
using ReactiveUI.Binding.Documentation.Infrastructure;

namespace ReactiveUI.Binding.Documentation.Bindings;

/// <summary>The upload dialog: a single button that uploads the file the user picked.</summary>
[System.Diagnostics.DebuggerDisplay("UploadPanelView: ViewModel = {ViewModel}")]
public sealed class UploadPanelView : ObservableObject, IViewFor<UploadPanelViewModel>
{
    /// <summary>Gets or sets the view model the view shows.</summary>
    public UploadPanelViewModel? ViewModel
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>Gets the button that uploads the picked file.</summary>
    public Button UploadButton { get; } = new() { Text = "Upload" };

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (UploadPanelViewModel?)value;
    }
}
