// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.Banking;

/// <summary>A transfer as the backend receives it.</summary>
/// <param name="SourceAccountId">The account the money leaves.</param>
/// <param name="PayeeId">The payee the money goes to.</param>
/// <param name="Amount">The amount to send.</param>
/// <param name="Reference">The text that appears on the payee's statement.</param>
[System.Diagnostics.DebuggerDisplay("TransferRequest: {Amount} from {SourceAccountId} to payee {PayeeId}")]
public sealed record TransferRequest(string SourceAccountId, int PayeeId, decimal Amount, string Reference);
