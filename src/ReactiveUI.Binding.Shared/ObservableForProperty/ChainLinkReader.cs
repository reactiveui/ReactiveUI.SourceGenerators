// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.ObservableForProperty;
#else
namespace ReactiveUI.Binding.ObservableForProperty;
#endif

/// <summary>
/// Reads the value of one link in an expression member chain: compiling the link's fetcher once, then
/// applying it to whatever parent the link is currently hanging off.
/// </summary>
/// <remarks>
/// Kept apart from the chain sink because a link that names no member - an array index, whose expression
/// carries no indexer - can only be reached through a subscription that then fails, so the behaviour is
/// otherwise impossible to pin down on its own.
/// </remarks>
internal static class ChainLinkReader
{
    /// <summary>Compiles the value fetcher for a chain link.</summary>
    /// <param name="link">The link to read.</param>
    /// <returns>
    /// The fetcher, or <see langword="null"/> when the link names no member and therefore has nothing to
    /// compile against - an array index is the case that reaches this.
    /// </returns>
    [RequiresUnreferencedCode("Evaluates expression-based member chains via reflection; members may be trimmed.")]
    internal static Func<object?, object?[]?, object?>? CreateGetter(Expression link)
    {
        var member = link.GetMemberInfo();
        return member is null ? null : Reflection.GetValueFetcherForProperty(member);
    }

    /// <summary>Reads the current value of a link from a parent.</summary>
    /// <param name="parent">The object the link is read from.</param>
    /// <param name="getter">The link's compiled fetcher, or <see langword="null"/> when it has none.</param>
    /// <param name="arguments">The link's index arguments, non-null only for indexer links.</param>
    /// <param name="link">The link, used to read the value when there is no fetcher.</param>
    /// <returns>The link's current value, or the default when the parent is absent.</returns>
    [RequiresUnreferencedCode("Evaluates expression-based member chains via reflection; members may be trimmed.")]
    internal static object? ReadValue(
        object? parent,
        Func<object?, object?[]?, object?>? getter,
        object?[]? arguments,
        Expression link)
    {
        if (parent is null)
        {
            return null;
        }

        return getter is not null
            ? getter(parent, arguments)
            : new ObservedChange<object?, object?>(parent, link, null).GetValueOrDefault();
    }
}
