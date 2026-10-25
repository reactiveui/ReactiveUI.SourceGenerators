// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Avalonia.Controls;
using ReactiveUI.Binding.Documentation.Banking;
using Button = Avalonia.Controls.Button;

namespace ReactiveUI.Binding.Documentation.Threading;

/// <summary>The transfer screen as an Avalonia view: the amount box, the send button and the label for validation.</summary>
[System.Diagnostics.DebuggerDisplay("AvaloniaTransferView: ViewModel = {ViewModel}")]
public sealed class AvaloniaTransferView : UserControl, IViewFor<TransferViewModel>
{
    /// <summary>Gets or sets the view model the view shows.</summary>
    public TransferViewModel? ViewModel { get; set; }

    /// <summary>Gets the box where the customer types the amount.</summary>
    public TextBox AmountTextBox { get; } = new();

    /// <summary>Gets the button that sends the transfer.</summary>
    public Button TransferButton { get; } = new() { Content = "Send" };

    /// <summary>Gets the label that shows what is wrong with the draft.</summary>
    public TextBlock ValidationLabel { get; } = new();

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (TransferViewModel?)value;
    }
}
