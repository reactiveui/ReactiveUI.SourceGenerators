// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.Infrastructure;

namespace ReactiveUI.Binding.Documentation.Views;

/// <summary>The accounts the customer can use.</summary>
[System.Diagnostics.DebuggerDisplay("AccountListViewModel: Accounts = {Accounts.Count}")]
public sealed class AccountListViewModel : ObservableObject, IAccountList
{
    /// <summary>Initializes a new instance of the <see cref="AccountListViewModel"/> class.</summary>
    /// <param name="accounts">The accounts to list.</param>
    public AccountListViewModel(IReadOnlyList<Account> accounts) => Accounts = accounts;

    /// <inheritdoc/>
    public IReadOnlyList<Account> Accounts { get; }
}
