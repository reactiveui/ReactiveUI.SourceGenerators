// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.Banking;

/// <summary>A person or business the customer sends money to.</summary>
/// <param name="Id">The number that identifies the payee.</param>
/// <param name="Name">The name of the payee.</param>
/// <param name="AccountNumber">The account that receives the money.</param>
/// <param name="TransferLimit">The most the customer allows in one transfer to this payee, or <see langword="null"/> when there is no limit.</param>
[System.Diagnostics.DebuggerDisplay("Payee: {Name}")]
public sealed record Payee(int Id, string Name, string AccountNumber, decimal? TransferLimit);
