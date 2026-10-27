// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Maui.Controls;
using ReactiveUI.Binding.Documentation.Infrastructure;

namespace ReactiveUI.Binding.Documentation.Observing;

/// <summary>The issue search screen: a search box, a search button, a busy bar and the list of matches.</summary>
[System.Diagnostics.DebuggerDisplay("IssueSearchView: ViewModel = {ViewModel}")]
public sealed class IssueSearchView : ObservableObject, IViewFor<IssueSearchViewModel>
{
    /// <summary>Gets or sets the view model the view shows.</summary>
    public IssueSearchViewModel? ViewModel
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>Gets the box where the user types the search term.</summary>
    public Entry SearchBox { get; } = new() { Placeholder = "Search issues" };

    /// <summary>Gets the button that runs the search. It is disabled while a search is waiting.</summary>
    public Button SearchButton { get; } = new() { Text = "Search" };

    /// <summary>Gets the bar that shows a search is waiting.</summary>
    public ProgressBar BusyBar { get; } = new() { IsVisible = false };

    /// <summary>Gets the list of matching issues.</summary>
    public CollectionView ResultList { get; } = new();

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (IssueSearchViewModel?)value;
    }
}
