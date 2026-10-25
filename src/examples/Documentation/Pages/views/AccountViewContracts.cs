// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.Views;

/// <summary>The contracts under which the banking screens are registered.</summary>
public static class AccountViewContracts
{
    /// <summary>The contract of the account screen that prints a statement.</summary>
    internal const string Statement = "statement";

    /// <summary>The contract of the accounts screen that fits a narrow window.</summary>
    internal const string Compact = "compact";
}
