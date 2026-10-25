// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Maui.Controls;
using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.Infrastructure;

namespace ReactiveUI.Binding.Documentation.Bindings;

/// <summary>The account detail strip: the name of the selected account in a box the customer could type into.</summary>
[System.Diagnostics.DebuggerDisplay("AccountNameView: ViewModel = {ViewModel}")]
public sealed class AccountNameView : ObservableObject, IViewFor<AccountsViewModel>
{
    /// <summary>Gets or sets the view model the view shows.</summary>
    public AccountsViewModel? ViewModel
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>Gets the box that shows the name of the selected account.</summary>
    public Entry NameTextBox { get; } = new() { Placeholder = "Account name" };

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (AccountsViewModel?)value;
    }
}
