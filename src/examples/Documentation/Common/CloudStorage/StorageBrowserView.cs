// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Infrastructure;

namespace ReactiveUI.Binding.Documentation.CloudStorage;

/// <summary>
/// The storage browser screen: a bucket list, a path box, an object list and an upload progress bar. A real UI
/// framework builds these controls from markup; here the view creates them in code.
/// </summary>
[System.Diagnostics.DebuggerDisplay("StorageBrowserView: ViewModel = {ViewModel}")]
public sealed class StorageBrowserView : ObservableObject, IViewFor<StorageBrowserViewModel>
{
    /// <summary>Gets or sets the view model the view shows.</summary>
    public StorageBrowserViewModel? ViewModel
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>Gets the list of buckets.</summary>
    public CollectionView BucketList { get; } = new() { SelectionMode = SelectionMode.Single };

    /// <summary>Gets the box that holds the folder being listed.</summary>
    public Entry PrefixTextBox { get; } = new() { Placeholder = "Folder" };

    /// <summary>Gets the button that lists the objects again.</summary>
    public Button RefreshButton { get; } = new() { Text = "Refresh" };

    /// <summary>Gets the list of objects.</summary>
    public CollectionView ObjectList { get; } = new() { SelectionMode = SelectionMode.Single };

    /// <summary>Gets the button that uploads a file.</summary>
    public Button UploadButton { get; } = new() { Text = "Upload" };

    /// <summary>Gets the bar that shows how much of the running upload the service has received, from 0 to 1.</summary>
    public ProgressBar UploadProgressBar { get; } = new();

    /// <summary>Gets the label that shows the state of the link.</summary>
    public Label ConnectionLabel { get; } = new();

    /// <summary>Gets the button that reopens the link.</summary>
    public Button ConnectButton { get; } = new() { Text = "Reconnect" };

    /// <summary>Gets the label that shows the combined size of the listed objects.</summary>
    public Label TotalSizeLabel { get; } = new();

    /// <summary>Gets the label that shows the size of the selected object.</summary>
    public Label SelectedSizeLabel { get; } = new();

    /// <summary>Gets the label that shows the media type of the selected object.</summary>
    public Label SelectedTypeLabel { get; } = new();

    /// <summary>Gets the label that shows the last error.</summary>
    public Label ErrorLabel { get; } = new();

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (StorageBrowserViewModel?)value;
    }
}
