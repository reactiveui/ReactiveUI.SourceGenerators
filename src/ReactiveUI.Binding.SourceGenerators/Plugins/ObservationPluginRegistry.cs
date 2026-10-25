// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

namespace ReactiveUI.Binding.SourceGenerators.Plugins;

/// <summary>Selects the strongest eligible observation mechanism, preserving declaration order for ties.</summary>
internal static class ObservationPluginRegistry
{
    /// <summary>The supported mechanisms in deterministic tie order.</summary>
    private static readonly IObservationPlugin[] Plugins =
    [
        new KVOObservationPlugin(), // Affinity 15 - Apple NSObject KVO
        UIKitObservation.Plugin,
        UIKitValueObservation.Plugin,
        AppKitObservation.Plugin,
        new ReactiveObjectObservationPlugin(), // Affinity 10 - IReactiveObject
        WinFormsObservation.Plugin, // Affinity  8 - WinForms Component
        WinUIObservation.Plugin, // Affinity  6 - WinUI DependencyObject
        UnoObservation.Plugin,
        new INPCObservationPlugin(), // Affinity  5 - INotifyPropertyChanged
        AndroidObservation.Plugin, // Affinity  5 - Android View
        new WpfObservationPlugin(), // Affinity  4 - WPF DependencyObject
        new PocoObservationPlugin()
    ];

    /// <summary>Gets the total number of registered plugins.</summary>
    internal static int Count => Plugins.Length;

    /// <summary>Gets the highest-affinity plugin that can handle the given type.</summary>
    /// <param name="classInfo">The type-level binding info.</param>
    /// <returns>The best matching plugin, or <see langword="null"/> if no plugin matches.</returns>
    internal static IObservationPlugin? GetBestPlugin(ClassBindingInfo classInfo)
    {
        IObservationPlugin? best = null;
        for (var i = 0; i < Plugins.Length; i++)
        {
            var candidate = Plugins[i];
            if (candidate.IsAMatch(classInfo) && (best is null || candidate.Affinity > best.Affinity))
            {
                best = candidate;
            }
        }

        return best;
    }

    /// <summary>Gets the highest-affinity plugin whose mechanism reaches one particular property.</summary>
    /// <param name="classInfo">The type-level binding info.</param>
    /// <param name="propertyName">The property being observed.</param>
    /// <param name="isBeforeChange">Whether the mechanism must report before the property changes.</param>
    /// <returns>The best matching plugin, or <see langword="null"/> if none reaches that property.</returns>
    /// <remarks>
    /// A mechanism that outranks another on the type can still be the wrong one for a given property - a
    /// component that also raises PropertyChanged declares properties with no change event - so a plugin that
    /// cannot reach the property is passed over for the next, exactly as a zero affinity would be at runtime.
    /// </remarks>
    internal static IObservationPlugin? GetBestPlugin(ClassBindingInfo classInfo, string propertyName, bool isBeforeChange = false)
    {
        IObservationPlugin? best = null;
        var bestScore = 0;
        for (var i = 0; i < Plugins.Length; i++)
        {
            var candidate = Plugins[i];
            var score = candidate.GetAffinityForProperty(classInfo, propertyName, isBeforeChange);
            if (score <= bestScore)
            {
                continue;
            }

            best = candidate;
            bestScore = score;
        }

        return best;
    }

    /// <summary>Collects eligible platform candidates while property symbols are available.</summary>
    /// <param name="owner">The concrete type exposing the property.</param>
    /// <param name="property">The property to inspect.</param>
    /// <returns>Value-equatable candidates for subsequent affinity voting.</returns>
    internal static EquatableArray<PlatformObservationInfo> InspectProperty(INamedTypeSymbol owner, IPropertySymbol property)
    {
        var candidates = new List<PlatformObservationInfo>();
        for (var i = 0; i < Plugins.Length; i++)
        {
            if (Plugins[i] is IPlatformObservationPlugin platform && platform.InspectProperty(owner, property) is { } candidate)
            {
                candidates.Add(candidate);
            }
        }

        return new([.. candidates]);
    }

    /// <summary>Gets a plugin by its observation kind identifier.</summary>
    /// <param name="observationKind">The observation kind (e.g., "INPC", "WpfDP").</param>
    /// <returns>The matching plugin, or <see langword="null"/> if not found.</returns>
    internal static IObservationPlugin? GetPluginByKind(string observationKind)
    {
        for (var i = 0; i < Plugins.Length; i++)
        {
            if (Plugins[i].ObservationKind == observationKind)
            {
                return Plugins[i];
            }
        }

        return null;
    }

    /// <summary>Gets the plugin at the specified index.</summary>
    /// <param name="index">The zero-based index.</param>
    /// <returns>The plugin at the specified index.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static IObservationPlugin GetPlugin(int index) => Plugins[index];
}
