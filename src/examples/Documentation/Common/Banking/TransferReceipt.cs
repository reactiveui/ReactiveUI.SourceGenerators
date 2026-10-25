// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.Banking;

/// <summary>The proof that the bank made a transfer.</summary>
/// <param name="ReceiptNumber">The number that identifies the transfer.</param>
/// <param name="Amount">The amount sent.</param>
/// <param name="NewBalance">The balance of the source account after the transfer.</param>
/// <param name="CompletedAt">When the bank made the transfer.</param>
[System.Diagnostics.DebuggerDisplay("TransferReceipt: {ReceiptNumber}: {Amount}")]
public sealed record TransferReceipt(string ReceiptNumber, decimal Amount, decimal NewBalance, DateTimeOffset CompletedAt);
