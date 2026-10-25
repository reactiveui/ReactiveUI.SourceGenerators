// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Windows.Forms;
using ReactiveUI.Binding.Documentation.CloudStorage;
using ProgressBar = System.Windows.Forms.ProgressBar;

namespace ReactiveUI.Binding.Documentation.Threading;

/// <summary>The storage upload screen as a Windows Forms form: the progress bar of the running upload.</summary>
[System.Diagnostics.DebuggerDisplay("WinFormsUploadForm: ViewModel = {ViewModel}")]
public sealed class WinFormsUploadForm : Form, IViewFor<StorageBrowserViewModel>
{
    /// <summary>Gets or sets the view model the form shows.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
