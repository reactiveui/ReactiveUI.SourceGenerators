// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Banking;

namespace ReactiveUI.Binding.Documentation.Views;

/// <summary>What a screen needs from any view model that lists accounts.</summary>
public interface IAccountList
{
    /// <summary>Gets the accounts to list.</summary>
    IReadOnlyList<Account> Accounts { get; }
}
