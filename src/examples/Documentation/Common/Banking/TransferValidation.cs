// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.Banking;

/// <summary>What the backend found when it checked a transfer without making it.</summary>
/// <param name="Errors">The rules the transfer breaks; empty when the bank would accept it.</param>
[System.Diagnostics.DebuggerDisplay("TransferValidation: Errors = {Errors.Count}")]
public sealed record TransferValidation(IReadOnlyList<string> Errors)
{
    /// <summary>Gets a value indicating whether the transfer breaks no rule.</summary>
    public bool IsValid => Errors.Count == 0;
}
