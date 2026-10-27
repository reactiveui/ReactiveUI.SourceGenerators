// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Maui.Controls;
using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.Infrastructure;

namespace ReactiveUI.Binding.Documentation.Views;

/// <summary>The account screen: one line with the balance. The locator builds it once and hands out the same screen every time.</summary>
[SingleInstanceView]
[System.Diagnostics.DebuggerDisplay("AccountSummaryView: ViewModel = {ViewModel}")]
public sealed class AccountSummaryView : ObservableObject, IViewFor<Account>
{
    /// <summary>Gets or sets the view model the view shows.</summary>
    public Account? ViewModel
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>Gets the label that shows the balance of the account.</summary>
    public Label BalanceLabel { get; } = new();

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (Account?)value;
    }
}
