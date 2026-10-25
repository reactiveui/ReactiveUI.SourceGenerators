// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Infrastructure;

namespace ReactiveUI.Binding.Documentation.Banking;

/// <summary>A bank account. Each property raises <c>PropertyChanged</c>, and <see cref="AvailableBalance"/> changes with <see cref="Balance"/>.</summary>
[System.Diagnostics.DebuggerDisplay("Account: {Id} {Name}: {Balance} {Currency}")]
public sealed class Account : ObservableObject
{
    /// <summary>Gets or sets the number that identifies the account, such as <c>ACC-1001</c>.</summary>
    public string Id
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    /// <summary>Gets or sets the name the customer gave the account.</summary>
    public string Name
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    /// <summary>Gets or sets the kind of the account.</summary>
    public AccountKind Kind
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>Gets or sets the code of the currency the account holds, such as <c>AUD</c>.</summary>
    public string Currency
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    /// <summary>Gets or sets the money in the account. It is negative while the account is overdrawn.</summary>
    public decimal Balance
    {
        get;
        set
        {
            if (!SetProperty(ref field, value))
            {
                return;
            }

            RaisePropertyChanged(nameof(AvailableBalance));
        }
    }

    /// <summary>Gets or sets how far below zero the balance may go, or <see langword="null"/> when the account cannot be overdrawn.</summary>
    public decimal? OverdraftLimit
    {
        get;
        set
        {
            if (!SetProperty(ref field, value))
            {
                return;
            }

            RaisePropertyChanged(nameof(AvailableBalance));
        }
    }

    /// <summary>Gets the money the customer can spend: the balance plus the overdraft limit.</summary>
    public decimal AvailableBalance => Balance + (OverdraftLimit ?? 0);

    /// <summary>Creates an independent copy, as the backend returns a new object for each response.</summary>
    /// <returns>A copy with the same values.</returns>
    public Account Clone() => new() { Id = Id, Name = Name, Kind = Kind, Currency = Currency, Balance = Balance, OverdraftLimit = OverdraftLimit };
}
