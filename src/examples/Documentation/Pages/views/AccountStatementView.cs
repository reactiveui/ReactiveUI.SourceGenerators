// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Maui.Controls;
using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.Infrastructure;

namespace ReactiveUI.Binding.Documentation.Views;

/// <summary>The account screen for printing a statement, registered under a contract.</summary>
[ViewContract(AccountViewContracts.Statement)]
[System.Diagnostics.DebuggerDisplay("AccountStatementView: ViewModel = {ViewModel}")]
public sealed class AccountStatementView : ObservableObject, IViewFor<Account>
{
    /// <summary>Gets or sets the view model the view shows.</summary>
    public Account? ViewModel
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>Gets the label that shows the statement heading.</summary>
    public Label HeadingLabel { get; } = new();

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (Account?)value;
    }
}
