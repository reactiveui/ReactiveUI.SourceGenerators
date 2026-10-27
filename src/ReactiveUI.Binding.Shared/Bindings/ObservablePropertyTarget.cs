// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>The object and property an <see cref="ObservableAsPropertyHelper{T}"/> backs.</summary>
/// <param name="owner">The object that owns the property, which the helper passes to its change callbacks.</param>
/// <param name="propertyName">The property's name.</param>
/// <remarks>Generated <c>ToProperty</c> code builds this to create a helper.</remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
[DebuggerDisplay("{Owner}.{PropertyName}")]
public readonly struct ObservablePropertyTarget(object owner, string propertyName) : IEquatable<ObservablePropertyTarget>
{
    /// <summary>Gets the object that owns the property.</summary>
    public object Owner { get; } = owner;

    /// <summary>Gets the property's name.</summary>
    public string PropertyName { get; } = propertyName;

    /// <summary>Determines whether two targets are the same object and property.</summary>
    /// <param name="left">The first target.</param>
    /// <param name="right">The second target.</param>
    /// <returns><see langword="true"/> when they are equal.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(ObservablePropertyTarget left, ObservablePropertyTarget right) => left.Equals(right);

    /// <summary>Determines whether two targets differ.</summary>
    /// <param name="left">The first target.</param>
    /// <param name="right">The second target.</param>
    /// <returns><see langword="true"/> when they are not equal.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(ObservablePropertyTarget left, ObservablePropertyTarget right) => !left.Equals(right);

    /// <inheritdoc/>
    public bool Equals(ObservablePropertyTarget other) =>
        ReferenceEquals(Owner, other.Owner) && string.Equals(PropertyName, other.PropertyName, StringComparison.Ordinal);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ObservablePropertyTarget other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(Owner) ^ StringComparer.Ordinal.GetHashCode(PropertyName);
}
