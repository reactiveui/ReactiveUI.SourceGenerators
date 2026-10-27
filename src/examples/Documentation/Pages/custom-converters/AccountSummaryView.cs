// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Maui.Controls;
using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.Infrastructure;

namespace ReactiveUI.Binding.Documentation.Converters;

/// <summary>A one-line summary of an account, shown as a row of the accounts list.</summary>
[System.Diagnostics.DebuggerDisplay("AccountSummaryView: ViewModel = {ViewModel}")]
public sealed class AccountSummaryView : ObservableObject, IViewFor<Account>
{
    /// <summary>Gets or sets the account the row shows.</summary>
    public Account? ViewModel
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>Gets the label that shows the name of the account.</summary>
    public Label NameLabel { get; } = new();

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (Account?)value;
    }
}
