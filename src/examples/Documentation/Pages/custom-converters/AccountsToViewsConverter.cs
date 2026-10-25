// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using ReactiveUI.Binding.Documentation.Banking;

namespace ReactiveUI.Binding.Documentation.Converters;

/// <summary>Turns a list of accounts into a list of views, one for each account, by asking a view locator.</summary>
/// <param name="locator">The locator that finds the view of an account.</param>
[System.Diagnostics.DebuggerDisplay("AccountsToViewsConverter: accounts -> views")]
public sealed class AccountsToViewsConverter(IViewLocator locator) : BindingTypeConverter<IReadOnlyList<Account>, IEnumerable>
{
    /// <summary>The affinity of the converter for its type pair.</summary>
    private const int ViewListAffinity = 10;

    /// <inheritdoc/>
    public override int GetAffinityForObjects() => ViewListAffinity;

    /// <inheritdoc/>
    public override bool TryConvert(IReadOnlyList<Account>? from, object? conversionHint, [MaybeNullWhen(true)] out IEnumerable? result)
    {
        if (from is null)
        {
            result = null;
            return false;
        }

        result = from.Select(locator.ResolveView).OfType<IViewFor>().ToList();
        return true;
    }
}
