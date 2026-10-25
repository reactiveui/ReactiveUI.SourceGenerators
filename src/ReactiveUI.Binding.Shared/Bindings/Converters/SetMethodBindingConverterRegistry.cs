// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Holds set-method binding converters, which are asked about a type pair at lookup time rather than grouped by it.</summary>
/// <remarks>
/// Reads are lock-free against an immutable snapshot; each registration is serialized under a lock and publishes a
/// new snapshot.
/// </remarks>
[DebuggerDisplay("SetMethodBindingConverterRegistry: {_snapshot.Converters.Count} set-method converters registered")]
public sealed class SetMethodBindingConverterRegistry
{
    /// <summary>Synchronization gate for serializing write operations.</summary>
    private readonly Lock _gate = new();

    /// <summary>The current immutable snapshot of registered set-method converters, read via volatile access.</summary>
    private Snapshot? _snapshot;

    /// <summary>Registers a set-method binding converter.</summary>
    /// <param name="converter">The converter to register.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="converter"/> is null.</exception>
    public void Register(ISetMethodBindingConverter converter)
    {
        ArgumentExceptionHelper.ThrowIfNull(converter);

        const int InitialRegistryCapacity = 8;

        lock (_gate)
        {
            var snap = _snapshot ?? new Snapshot([with(InitialRegistryCapacity)]);

            // Copy-on-write update: clone the list
            var newList = new List<ISetMethodBindingConverter>(snap.Converters) { converter };

            // Publish the new snapshot (atomic via reference assignment)
            _snapshot = new(newList);
        }
    }

    /// <summary>Asks each set-method converter for its affinity to the type pair and returns the highest positive one.</summary>
    /// <param name="fromType">The type of the value being written; may be null.</param>
    /// <param name="toType">The type of the target being written to; may be null.</param>
    /// <returns>
    /// The converter with the highest positive affinity, the earliest registered one on a tie; <see langword="null"/>
    /// when none reports a positive affinity.
    /// </returns>
    public ISetMethodBindingConverter? TryGetConverter(
        Type? fromType,
        Type? toType)
    {
        var snap = Volatile.Read(ref _snapshot);
        if (snap is null)
        {
            return null;
        }

        // Find the converter with the highest affinity
        ISetMethodBindingConverter? best = null;
        var bestScore = -1;

        var converters = snap.Converters;
        for (var i = 0; i < converters.Count; i++)
        {
            var converter = converters[i];
            var score = converter.GetAffinityForObjects(fromType, toType);
            if (score <= bestScore || score <= 0)
            {
                continue;
            }

            bestScore = score;
            best = converter;
        }

        return best;
    }

    /// <summary>Returns a copy of every registered set-method converter, in registration order.</summary>
    /// <returns>The converters registered at the time of the call; empty when none are registered.</returns>
    public IEnumerable<ISetMethodBindingConverter> GetAllConverters()
    {
        var snap = Volatile.Read(ref _snapshot);
        return snap is null ? [] : [.. snap.Converters];
    }

    /// <summary>Immutable snapshot of the registry state for lock-free reads.</summary>
    /// <param name="Converters">The registered set-method converters.</param>
    private sealed record Snapshot(List<ISetMethodBindingConverter> Converters);
}
