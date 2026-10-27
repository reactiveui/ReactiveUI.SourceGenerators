// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace ReactiveUI.SourceGenerators.Helpers;

/// <summary>
/// A value-equatable wrapper around an array for use in incremental generator pipelines.
/// This enables structural equality comparisons of arrays in record types.
/// </summary>
/// <typeparam name="T">The type of elements in the array, must be equatable.</typeparam>
internal readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IEnumerable<T>
    where T : IEquatable<T>
{
    /// <summary>The multiplier used in the deterministic hash-combine loop.</summary>
    private const int HashMultiplier = 31;

    /// <summary>The seed of the hash-combine loop.</summary>
    private const int HashSeed = 17;

    /// <summary>The underlying array, or null if default-constructed.</summary>
    private readonly T[]? _array;

    /// <summary>Cached hash code computed once at construction.</summary>
    private readonly int _cachedHashCode;

    /// <summary>Initializes a new instance of the <see cref="EquatableArray{T}"/> struct.</summary>
    /// <param name="array">The array to wrap. The caller keeps no other reference that mutates it.</param>
    public EquatableArray(T[] array)
    {
        _array = array;
        _cachedHashCode = ComputeHashCode(array);
    }

    /// <summary>Gets the number of elements.</summary>
    internal int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _array?.Length ?? 0;
    }

    /// <summary>Gets a value indicating whether the array holds no elements.</summary>
    internal bool IsEmpty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (_array?.Length ?? 0) == 0;
    }

    /// <summary>Gets a reference to the element at the specified index.</summary>
    /// <param name="index">The zero-based index of the element to get.</param>
    internal ref readonly T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref _array![index];
    }

    /// <summary>Determines whether two arrays are equal.</summary>
    /// <param name="left">The first array to compare.</param>
    /// <param name="right">The second array to compare.</param>
    /// <returns>true if the arrays are equal; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(EquatableArray<T> left, EquatableArray<T> right) => left.Equals(right);

    /// <summary>Determines whether two arrays are not equal.</summary>
    /// <param name="left">The first array to compare.</param>
    /// <param name="right">The second array to compare.</param>
    /// <returns>true if the arrays are not equal; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(EquatableArray<T> left, EquatableArray<T> right) => !left.Equals(right);

    /// <summary>Indicates whether the current array is equal to another array.</summary>
    /// <param name="other">An array to compare with this array.</param>
    /// <returns>true if the arrays are equal; otherwise, false.</returns>
    public bool Equals(EquatableArray<T> other)
    {
        var left = _array ?? [];
        var right = other._array ?? [];
        if (left.Length != right.Length)
        {
            return false;
        }

        for (var i = 0; i < left.Length; i++)
        {
            if (!left[i].Equals(right[i]))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>Determines whether the specified object is equal to the current array.</summary>
    /// <param name="obj">The object to compare with the current array.</param>
    /// <returns>true if the specified object is equal to the current array; otherwise, false.</returns>
    public override bool Equals(object? obj) => obj is EquatableArray<T> other && Equals(other);

    /// <summary>Returns the cached hash code for this array.</summary>
    /// <returns>A hash code for the current array.</returns>
    public override int GetHashCode() => _cachedHashCode;

    /// <summary>Wraps the array a Roslyn API returned without copying it.</summary>
    /// <param name="array">The Roslyn-owned array. Roslyn never mutates it.</param>
    /// <returns>The wrapped array.</returns>
    /// <remarks>This is the one place an <see cref="ImmutableArray{T}"/> enters the pipeline: Roslyn hands them out.</remarks>
    internal static EquatableArray<T> FromRoslyn(ImmutableArray<T> array) =>
        new(Unsafe.As<ImmutableArray<T>, T[]?>(ref array) ?? []);

    /// <summary>Gets the elements as a read-only span.</summary>
    /// <returns>A span over the elements.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ReadOnlySpan<T> AsSpan() => _array;

    /// <summary>Copies the elements to a new array.</summary>
    /// <returns>A new array holding the elements.</returns>
    internal T[] ToArray() => _array is null || _array.Length == 0 ? [] : (T[])_array.Clone();

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IEnumerable<T>)(_array ?? [])).GetEnumerator();

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<T>)this).GetEnumerator();

    /// <summary>Computes a deterministic hash code for the given array.</summary>
    /// <param name="array">The array to hash.</param>
    /// <returns>A hash code for the array.</returns>
    private static int ComputeHashCode(T[]? array)
    {
        if (array is null)
        {
            return 0;
        }

        unchecked
        {
            var hash = HashSeed;
            for (var i = 0; i < array.Length; i++)
            {
                hash = (hash * HashMultiplier) + (array[i]?.GetHashCode() ?? 0);
            }

            return hash;
        }
    }
}
