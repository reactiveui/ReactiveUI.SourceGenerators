// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Maui.Controls;
using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.Infrastructure;

namespace ReactiveUI.Binding.Documentation.Views;

/// <summary>The accounts screen for a narrow window: it shows only the balance of the selected account.</summary>
[ViewContract(AccountViewContracts.Compact)]
[System.Diagnostics.DebuggerDisplay("CompactAccountsView: ViewModel = {ViewModel}")]
public sealed class CompactAccountsView : ObservableObject, IViewFor<AccountsViewModel>
{
    /// <summary>Gets or sets the view model the view shows.</summary>
    public AccountsViewModel? ViewModel
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>Gets the label that shows the balance of the selected account.</summary>
    public Label BalanceLabel { get; } = new();

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (AccountsViewModel?)value;
    }
}
