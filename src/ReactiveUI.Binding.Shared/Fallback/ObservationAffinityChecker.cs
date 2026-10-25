// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.CompilerServices;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Fallback;
#else
namespace ReactiveUI.Binding.Fallback;
#endif

/// <summary>
/// Finds a registered <see cref="ICreatesObservableForProperty"/> whose affinity for a type and property is higher than
/// the affinity of the mechanism the generator selected, so generated code can defer to it at runtime.
/// </summary>
/// <remarks>
/// Registrations are read from the service locator on first use, and their best score for each type, property and
/// notification timing is cached until <see cref="Refresh"/>. The generated mechanism wins ties.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class ObservationAffinityChecker
{
    /// <summary>The registrations and scores belonging to the current refresh generation.</summary>
    private static SelectionCache _cache = new();

    /// <summary>Discards the cached registrations and scores so the next lookup reads the service locator again.</summary>
    /// <remarks>A lookup running concurrently with the refresh may finish using the registrations it already captured.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Refresh() => Interlocked.Exchange(ref _cache, new());

    /// <summary>Returns <see langword="true"/> if a registered <see cref="ICreatesObservableForProperty"/> outranks <paramref name="generatedAffinity"/>.</summary>
    /// <param name="type">The type being observed.</param>
    /// <param name="propertyName">The property being observed on that type; a plugin scores a type together with a property.</param>
    /// <param name="generatedAffinity">The affinity of the source generator's selected plugin.</param>
    /// <param name="beforeChanged">Whether before-change (PropertyChanging) observation is requested.</param>
    /// <returns><see langword="true"/> if a user plugin should override the generated observation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="type"/> or <paramref name="propertyName"/> is null.</exception>
    public static bool HasHigherAffinityPlugin(Type type, string propertyName, int generatedAffinity, bool beforeChanged) =>
        FindHigherAffinityPlugin(type, propertyName, generatedAffinity, beforeChanged) is not null;

    /// <summary>Finds the registered <see cref="ICreatesObservableForProperty"/> that outranks <paramref name="generatedAffinity"/>.</summary>
    /// <param name="type">The type being observed.</param>
    /// <param name="propertyName">The property being observed on that type.</param>
    /// <param name="generatedAffinity">The affinity of the source generator's selected plugin.</param>
    /// <param name="beforeChanged">Whether before-change (PropertyChanging) observation is requested.</param>
    /// <returns>The highest-scoring registration whose score exceeds <paramref name="generatedAffinity"/>, or <see langword="null"/> when none does.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="type"/> or <paramref name="propertyName"/> is null.</exception>
    public static ICreatesObservableForProperty? FindHigherAffinityPlugin(
        Type type,
        string propertyName,
        int generatedAffinity,
        bool beforeChanged)
    {
        ArgumentExceptionHelper.ThrowIfNull(type);
        ArgumentExceptionHelper.ThrowIfNull(propertyName);

        var selection = Volatile.Read(ref _cache).Find(type, propertyName, beforeChanged);
        return selection.Affinity > generatedAffinity ? selection.Plugin : null;
    }

    /// <summary>Identifies the inputs to a custom provider's affinity vote.</summary>
    /// <param name="Type">The type being observed.</param>
    /// <param name="PropertyName">The property being observed.</param>
    /// <param name="BeforeChanged">Whether notification occurs before the change.</param>
    [DebuggerDisplay("ObservationKey: {Type.Name,nq}.{PropertyName,nq}, BeforeChanged = {BeforeChanged}")]
    private readonly record struct ObservationKey(Type Type, string PropertyName, bool BeforeChanged);

    /// <summary>Keeps the strongest custom vote independently of any generated mechanism's affinity.</summary>
    /// <param name="Plugin">The winning registration, or null when no registration wins.</param>
    /// <param name="Affinity">The registration's property-specific score.</param>
    [DebuggerDisplay("PluginSelection: Affinity = {Affinity}")]
    private readonly record struct PluginSelection(ICreatesObservableForProperty? Plugin, int Affinity);

    /// <summary>Owns registrations and scored votes so refresh cannot receive a stale publication.</summary>
    private sealed class SelectionCache
    {
        /// <summary>The strongest vote for each observed property and notification timing.</summary>
        private readonly ConcurrentDictionary<ObservationKey, PluginSelection> _selections = new();

        /// <summary>The cache factory, retained to avoid creating a delegate on cache hits.</summary>
        private readonly Func<ObservationKey, PluginSelection> _select;

        /// <summary>The resolved registrations, or null before resolution.</summary>
        private ICreatesObservableForProperty[]? _plugins;

        /// <summary>Initializes a new instance of the <see cref="SelectionCache"/> class.</summary>
        public SelectionCache() => _select = Select;

        /// <summary>Returns the strongest cached vote without allocating property entries for an empty registry.</summary>
        /// <param name="type">The type being observed.</param>
        /// <param name="propertyName">The property being observed.</param>
        /// <param name="beforeChanged">Whether notification occurs before the change.</param>
        /// <returns>The strongest custom vote, or an empty selection when no registrations exist.</returns>
        public PluginSelection Find(Type type, string propertyName, bool beforeChanged) =>
            Resolve().Length == 0 ? default : _selections.GetOrAdd(new(type, propertyName, beforeChanged), _select);

        /// <summary>Scores registrations for one observation, preserving registration order on ties.</summary>
        /// <param name="key">The inputs to each registration's affinity vote.</param>
        /// <returns>The highest scoring registration and its score.</returns>
        private PluginSelection Select(ObservationKey key)
        {
            var plugins = Resolve();
            var bestScore = int.MinValue;
            ICreatesObservableForProperty? best = null;

            for (var i = 0; i < plugins.Length; i++)
            {
                var score = plugins[i].GetAffinityForObject(key.Type, key.PropertyName, key.BeforeChanged);
                if (score <= bestScore)
                {
                    continue;
                }

                bestScore = score;
                best = plugins[i];
            }

            return new(best, bestScore);
        }

        /// <summary>Publishes registrations only into the cache generation that requested them.</summary>
        /// <returns>The resolved registrations.</returns>
        private ICreatesObservableForProperty[] Resolve()
        {
            var resolved = Volatile.Read(ref _plugins);
            if (resolved is not null)
            {
                return resolved;
            }

            ICreatesObservableForProperty[] built = [.. AppLocator.Current.GetServices<ICreatesObservableForProperty>()];
            return Interlocked.CompareExchange(ref _plugins, built, null) ?? built;
        }
    }
}
