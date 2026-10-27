// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Plugins.CommandBinding;

namespace ReactiveUI.Binding.SourceGenerators.Plugins;

/// <summary>
/// Static registry of <see cref="ICommandBindingPlugin"/> instances sorted by affinity descending.
/// Returns the highest-affinity plugin that can handle a given <see cref="BindCommandInvocationInfo"/>.
/// </summary>
internal static class CommandBindingPluginRegistry
{
    /// <summary>All command binding plugins sorted by affinity descending (highest priority first).</summary>
    private static readonly ICommandBindingPlugin[] Plugins =
    [
        new UIKitControlCommandBindingPlugin(),
        new UIKitCommandBindingPlugin(),
        new AndroidCommandBindingPlugin(),
        new CommandPropertyBindingPlugin(), // Affinity 5
        new AppKitCommandBindingPlugin(),
        new EventEnabledBindingPlugin(), // Affinity 4
        new DefaultEventBindingPlugin() // Affinity 3
    ];

    /// <summary>Finds the strongest native route while the control's symbols are available.</summary>
    /// <param name="control">The concrete control, or null when unresolved.</param>
    /// <returns>The native member data, or null.</returns>
    internal static NativeCommandInfo? InspectControl(INamedTypeSymbol? control)
    {
        if (control is null)
        {
            return null;
        }

        NativeCommandInfo? best = null;
        var affinity = 0;
        foreach (var plugin in Plugins)
        {
            if (plugin is not IPlatformCommandBindingPlugin native || plugin.Affinity <= affinity || native.InspectControl(control) is not { } candidate)
            {
                continue;
            }

            best = candidate;
            affinity = plugin.Affinity;
        }

        return best;
    }

    /// <summary>Returns the highest-affinity plugin that can handle the given invocation, or <see langword="null"/> if no plugin matches.</summary>
    /// <param name="inv">The BindCommand invocation info.</param>
    /// <returns>The best matching plugin, or null.</returns>
    internal static ICommandBindingPlugin? GetBestPlugin(BindCommandInvocationInfo inv)
    {
        ICommandBindingPlugin? best = null;
        for (var i = 0; i < Plugins.Length; i++)
        {
            var candidate = Plugins[i];
            if (candidate.CanHandle(inv) && (best is null || candidate.Affinity > best.Affinity))
            {
                best = candidate;
            }
        }

        return best;
    }
}
