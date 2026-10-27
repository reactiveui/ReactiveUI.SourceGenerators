// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Maui.Controls;
using ReactiveUI.Binding.Documentation.Infrastructure;

namespace ReactiveUI.Binding.Documentation.Views;

/// <summary>The screen for any list of accounts. It is registered for the interface, so every view model that implements it gets this screen.</summary>
[System.Diagnostics.DebuggerDisplay("AccountListView: ViewModel = {ViewModel}")]
public sealed class AccountListView : ObservableObject, IViewFor<IAccountList>
{
    /// <summary>Gets or sets the view model the view shows.</summary>
    public IAccountList? ViewModel
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>Gets the label that shows how many accounts are listed.</summary>
    public Label CountLabel { get; } = new();

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (IAccountList?)value;
    }
}
