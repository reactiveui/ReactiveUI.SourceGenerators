// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Maui.Controls;
using ReactiveUI.Binding.Documentation.Infrastructure;

namespace ReactiveUI.Binding.Documentation.Banking;

/// <summary>
/// The accounts screen: the accounts, the balance of the selected account and its transactions. A real UI framework
/// builds these controls from markup; here the view creates them in code.
/// </summary>
[System.Diagnostics.DebuggerDisplay("AccountsView: ViewModel = {ViewModel}")]
public sealed class AccountsView : ObservableObject, IViewFor<AccountsViewModel>
{
    /// <summary>Gets or sets the view model the view shows.</summary>
    public AccountsViewModel? ViewModel
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>Gets the list of accounts.</summary>
    public CollectionView AccountList { get; } = new();

    /// <summary>Gets the list of transactions of the selected account.</summary>
    public CollectionView TransactionList { get; } = new();

    /// <summary>Gets the button that loads the transactions of the selected account.</summary>
    public Button LoadTransactionsButton { get; } = new() { Text = "Show transactions" };

    /// <summary>Gets the button that loads the accounts again.</summary>
    public Button RefreshButton { get; } = new() { Text = "Refresh" };

    /// <summary>Gets the label that shows the balance of the selected account.</summary>
    public Label BalanceLabel { get; } = new();

    /// <summary>Gets the label that shows the money the customer can spend from the selected account.</summary>
    public Label AvailableBalanceLabel { get; } = new();

    /// <summary>Gets the label that shows the combined balance of all accounts.</summary>
    public Label TotalBalanceLabel { get; } = new();

    /// <summary>Gets the label that shows the last error.</summary>
    public Label ErrorLabel { get; } = new();

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (AccountsViewModel?)value;
    }
}
