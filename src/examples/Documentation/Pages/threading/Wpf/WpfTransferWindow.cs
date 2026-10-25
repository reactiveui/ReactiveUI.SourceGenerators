// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows.Controls;
using ReactiveUI.Binding.Documentation.Banking;
using Button = System.Windows.Controls.Button;
using Window = System.Windows.Window;

namespace ReactiveUI.Binding.Documentation.Threading;

/// <summary>The transfer screen as a WPF window: the amount box, the reference box, the send button and the labels for validation and the receipt.</summary>
[System.Diagnostics.DebuggerDisplay("WpfTransferWindow: ViewModel = {ViewModel}")]
public sealed class WpfTransferWindow : Window, IViewFor<TransferViewModel>
{
    /// <summary>Gets or sets the view model the window shows.</summary>
    public TransferViewModel? ViewModel { get; set; }

    /// <summary>Gets the box where the customer types the amount.</summary>
    public TextBox AmountTextBox { get; } = new();

    /// <summary>Gets the box where the customer types the reference.</summary>
    public TextBox ReferenceTextBox { get; } = new();

    /// <summary>Gets the button that sends the transfer.</summary>
    public Button TransferButton { get; } = new() { Content = "Send" };

    /// <summary>Gets the label that shows what is wrong with the draft.</summary>
    public TextBlock ValidationLabel { get; } = new();

    /// <summary>Gets the label that shows the last error.</summary>
    public TextBlock ErrorLabel { get; } = new();

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (TransferViewModel?)value;
    }
}
