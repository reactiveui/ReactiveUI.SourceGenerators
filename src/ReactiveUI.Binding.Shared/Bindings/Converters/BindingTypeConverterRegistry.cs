// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Holds typed binding converters grouped by their exact source and target type pair.</summary>
/// <remarks>
/// Reads are lock-free against an immutable snapshot; each registration is serialized under a lock and publishes a
/// new snapshot, so registering is expensive and lookups are cheap.
/// </remarks>
[DebuggerDisplay("BindingTypeConverterRegistry: Typed converters for {_snapshot.ConvertersByTypePair.Count} type pairs")]
public sealed class BindingTypeConverterRegistry
{
    /// <summary>Synchronization gate for serializing write operations.</summary>
    private readonly Lock _gate = new();

    /// <summary>The current immutable snapshot of registered typed converters, read via volatile access.</summary>
    private Snapshot? _snapshot;

    /// <summary>Registers a converter under its <see cref="IBindingTypeConverter.FromType"/> and <see cref="IBindingTypeConverter.ToType"/> pair.</summary>
    /// <param name="converter">The converter to register.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="converter"/> is null.</exception>
    /// <remarks>
    /// Several converters can share a pair; <see cref="TryGetConverter"/> picks the one with the highest affinity.
    /// The pair is read once, at registration.
    /// </remarks>
    public void Register(IBindingTypeConverter converter)
    {
        ArgumentExceptionHelper.ThrowIfNull(converter);

        const int InitialConverterListCapacity = 4;
        const int InitialRegistryCapacity = 16;

        var key = new ConverterTypePair(converter.FromType, converter.ToType);

        lock (_gate)
        {
            var snap = _snapshot ?? new Snapshot([with(InitialRegistryCapacity)]);

            // Copy-on-write update: clone the dictionary shallowly
            var newDict = CloneRegistryShallow(snap.ConvertersByTypePair);

            List<IBindingTypeConverter>? list =
                !newDict.TryGetValue(key, out list)
                    ? [with(InitialConverterListCapacity)]
                    : [.. list];

            list.Add(converter);
            newDict[key] = list;

            // Publish the new snapshot (atomic via reference assignment)
            _snapshot = new(newDict);
        }
    }

    /// <summary>Returns the registered converter with the highest positive affinity for the exact type pair.</summary>
    /// <param name="fromType">The source type to convert from.</param>
    /// <param name="toType">The target type to convert to.</param>
    /// <returns>
    /// The best converter, the earliest registered one on a tie in affinity; <see langword="null"/> when none is
    /// registered for the pair or every one reports an affinity of zero or less.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="fromType"/> or <paramref name="toType"/> is null.
    /// </exception>
    public IBindingTypeConverter? TryGetConverter(Type fromType, Type toType)
    {
        ArgumentExceptionHelper.ThrowIfNull(fromType);
        ArgumentExceptionHelper.ThrowIfNull(toType);

        var snap = Volatile.Read(ref _snapshot);
        if (snap is null)
        {
            return null;
        }

        if (!snap.ConvertersByTypePair.TryGetValue(new(fromType, toType), out var list))
        {
            return null;
        }

        // Find the converter with the highest affinity
        IBindingTypeConverter? best = null;
        var bestScore = -1;

        for (var i = 0; i < list.Count; i++)
        {
            var converter = list[i];
            var score = converter.GetAffinityForObjects();
            if (score <= bestScore || score <= 0)
            {
                continue;
            }

            bestScore = score;
            best = converter;
        }

        return best;
    }

    /// <summary>Returns a copy of every registered converter, in no particular order across type pairs.</summary>
    /// <returns>
    /// The converters registered at the time of the call; empty when none are registered.
    /// Later registrations do not change the returned sequence.
    /// </returns>
    public IEnumerable<IBindingTypeConverter> GetAllConverters()
    {
        var snap = Volatile.Read(ref _snapshot);
        if (snap is null)
        {
            return [];
        }

        var result = new List<IBindingTypeConverter>();
        foreach (var kvp in snap.ConvertersByTypePair)
        {
            result.AddRange(kvp.Value);
        }

        return result;
    }

    /// <summary>Creates a shallow clone of the converter dictionary, preserving list references without deep-copying them.</summary>
    /// <param name="source">The source dictionary to clone.</param>
    /// <returns>A new dictionary with the same entries as <paramref name="source"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Dictionary<ConverterTypePair, List<IBindingTypeConverter>> CloneRegistryShallow(
        Dictionary<ConverterTypePair, List<IBindingTypeConverter>> source)
    {
        ArgumentExceptionHelper.ThrowIfNull(source);

        var clone = new Dictionary<ConverterTypePair, List<IBindingTypeConverter>>(source.Count);
        foreach (var kvp in source)
        {
            clone[kvp.Key] = kvp.Value;
        }

        return clone;
    }

    /// <summary>Immutable snapshot of the registry state for lock-free reads.</summary>
    /// <param name="ConvertersByTypePair">The converters indexed by source and target type pair.</param>
    private sealed record Snapshot(
        Dictionary<ConverterTypePair, List<IBindingTypeConverter>> ConvertersByTypePair);
}
