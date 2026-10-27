// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.Banking;

/// <summary>A line on an account statement.</summary>
/// <param name="Id">The number that identifies the transaction.</param>
/// <param name="AccountId">The account the transaction belongs to.</param>
/// <param name="PostedAt">When the bank posted the transaction.</param>
/// <param name="Description">What the transaction was for.</param>
/// <param name="Amount">The money moved. It is negative when money leaves the account.</param>
/// <param name="BalanceAfter">The balance of the account after the transaction.</param>
[System.Diagnostics.DebuggerDisplay("Transaction: {Description}: {Amount}")]
public sealed record Transaction(int Id, string AccountId, DateTimeOffset PostedAt, string Description, decimal Amount, decimal BalanceAfter);
