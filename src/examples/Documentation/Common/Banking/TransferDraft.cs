// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Infrastructure;

namespace ReactiveUI.Binding.Documentation.Banking;

/// <summary>The transfer the customer is filling in. Each property raises <c>PropertyChanged</c>.</summary>
[System.Diagnostics.DebuggerDisplay("TransferDraft: {Amount} to {Payee}")]
public sealed class TransferDraft : ObservableObject
{
    /// <summary>Gets or sets the account the money leaves.</summary>
    public Account? Source
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>Gets or sets the payee the money goes to.</summary>
    public Payee? Payee
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>Gets or sets the amount to send, in the currency of the source account.</summary>
    public decimal Amount
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>Gets or sets the text that appears on the payee's statement.</summary>
    public string Reference
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;
}
