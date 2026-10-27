// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Maui.Controls;
using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.Infrastructure;

namespace ReactiveUI.Binding.Documentation.Views;

/// <summary>The screen for a transfer receipt. It needs a heading to be built, so it has no parameterless constructor.</summary>
[System.Diagnostics.DebuggerDisplay("ReceiptView: ViewModel = {ViewModel}")]
public sealed class ReceiptView : ObservableObject, IViewFor<TransferReceipt>
{
    /// <summary>Initializes a new instance of the <see cref="ReceiptView"/> class.</summary>
    /// <param name="heading">The text above the receipt.</param>
    public ReceiptView(string heading) => HeadingLabel = new() { Text = heading };

    /// <summary>Gets or sets the view model the view shows.</summary>
    public TransferReceipt? ViewModel
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>Gets the label that shows the heading of the receipt.</summary>
    public Label HeadingLabel { get; }

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (TransferReceipt?)value;
    }
}
