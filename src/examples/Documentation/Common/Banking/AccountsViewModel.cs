// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Maui.Controls;
using ReactiveUI.Binding.Documentation.Infrastructure;

namespace ReactiveUI.Binding.Documentation.Banking;

/// <summary>
/// The view model behind the accounts screen. It lists the accounts and the transactions of the selected account.
/// Each command starts the matching method and returns; await the method itself to wait for its work. A refused
/// request never throws from a method. It sets <see cref="ErrorMessage"/>.
/// </summary>
[System.Diagnostics.DebuggerDisplay("AccountsViewModel: Account = {SelectedAccount}, TotalBalance = {TotalBalance}")]
public sealed class AccountsViewModel : ObservableObject
{
    /// <summary>The backend the view model calls.</summary>
    private readonly IBankingBackend _backend;

    /// <summary>Initializes a new instance of the <see cref="AccountsViewModel"/> class.</summary>
    /// <param name="backend">The backend to call.</param>
    public AccountsViewModel(IBankingBackend backend)
    {
        _backend = backend;
        LoadAccountsCommand = new(() => _ = LoadAccountsAsync());
        LoadTransactionsCommand = new(() => _ = LoadTransactionsAsync(), () => SelectedAccount is not null);
    }

    /// <summary>Gets the command that runs <see cref="LoadAccountsAsync"/>.</summary>
    public Command LoadAccountsCommand { get; }

    /// <summary>Gets the command that runs <see cref="LoadTransactionsAsync"/>; it runs only while an account is selected.</summary>
    public Command LoadTransactionsCommand { get; }

    /// <summary>Gets the accounts of the customer.</summary>
    public IReadOnlyList<Account> Accounts
    {
        get;
        private set => SetProperty(ref field, value);
    } = [];

    /// <summary>Gets or sets the account the customer picked. A path such as <c>SelectedAccount.Balance</c> is empty while no account is picked.</summary>
    public Account? SelectedAccount
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                LoadTransactionsCommand.ChangeCanExecute();
            }
        }
    }

    /// <summary>Gets the transactions of <see cref="SelectedAccount"/>, newest first.</summary>
    public IReadOnlyList<Transaction> Transactions
    {
        get;
        private set => SetProperty(ref field, value);
    } = [];

    /// <summary>Gets the combined balance of <see cref="Accounts"/>.</summary>
    public decimal TotalBalance
    {
        get;
        private set => SetProperty(ref field, value);
    }

    /// <summary>Gets the message from the last refused request, or an empty string.</summary>
    public string ErrorMessage
    {
        get;
        private set => SetProperty(ref field, value);
    } = string.Empty;

    /// <summary>Loads the accounts and keeps the selected account when it still exists.</summary>
    /// <returns>A task that completes when the accounts are loaded or the request was refused.</returns>
    public async Task LoadAccountsAsync()
    {
        try
        {
            IReadOnlyList<Account> accounts = await _backend.GetAccountsAsync().ConfigureAwait(false);
            string? selectedId = SelectedAccount?.Id;

            ErrorMessage = string.Empty;
            Accounts = accounts;
            SelectedAccount = accounts.FirstOrDefault(account => account.Id == selectedId);
            TotalBalance = accounts.Sum(static account => account.Balance);
        }
        catch (BankingException ex)
        {
            ErrorMessage = $"{ex.Failure}: {ex.Message}";
        }
    }

    /// <summary>Loads the transactions of the selected account.</summary>
    /// <returns>A task that completes when the transactions are loaded or the request was refused.</returns>
    public async Task LoadTransactionsAsync()
    {
        if (SelectedAccount is not { } account)
        {
            return;
        }

        try
        {
            Transactions = await _backend.GetTransactionsAsync(account.Id).ConfigureAwait(false);
            ErrorMessage = string.Empty;
        }
        catch (BankingException ex)
        {
            ErrorMessage = $"{ex.Failure}: {ex.Message}";
        }
    }
}
