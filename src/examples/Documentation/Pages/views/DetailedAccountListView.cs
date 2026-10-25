// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Maui.Controls;
using ReactiveUI.Binding.Documentation.Infrastructure;

namespace ReactiveUI.Binding.Documentation.Views;

/// <summary>A richer screen for the current accounts. It is registered for the concrete view model, next to the interface screen.</summary>
[System.Diagnostics.DebuggerDisplay("DetailedAccountListView: ViewModel = {ViewModel}")]
public sealed class DetailedAccountListView : ObservableObject, IViewFor<AccountListViewModel>
{
    /// <summary>Gets or sets the view model the view shows.</summary>
    public AccountListViewModel? ViewModel
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>Gets the label that shows the balance of every listed account.</summary>
    public Label BalancesLabel { get; } = new();

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (AccountListViewModel?)value;
    }
}
