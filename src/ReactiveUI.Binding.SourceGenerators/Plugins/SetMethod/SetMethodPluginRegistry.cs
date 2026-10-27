// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.SetMethod;

/// <summary>Selects the strongest eligible typed setter, preserving declaration order on ties.</summary>
internal static class SetMethodPluginRegistry
{
    /// <summary>The known native setter mechanisms.</summary>
    private static readonly ISetMethodPlugin[] Plugins = [new PanelSetMethodPlugin(), new TableLayoutSetMethodPlugin()];

    /// <summary>Compares the native candidates for the actual source and destination types.</summary>
    /// <param name="source">The incoming value type.</param>
    /// <param name="target">The collection type.</param>
    /// <returns>The highest-affinity eligible mechanism.</returns>
    internal static SetMethodInfo? Select(ITypeSymbol? source, ITypeSymbol? target)
    {
        if (source is null || target is null)
        {
            return null;
        }

        SetMethodInfo? winner = null;
        foreach (var plugin in Plugins)
        {
            var candidate = plugin.Select(source, target);
            if (candidate is not null && (winner is null || candidate.Affinity > winner.Affinity))
            {
                winner = candidate;
            }
        }

        return winner;
    }
}
