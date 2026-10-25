// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections;
using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.SourceGenerators.Models;

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

    /// <summary>The underlying array, or null if default-constructed.</summary>
    private readonly T[]? _array;

    /// <summary>Cached hash code computed once at construction.</summary>
    private readonly int _cachedHashCode;

    /// <summary>Initializes a new instance of the <see cref="EquatableArray{T}"/> struct.</summary>
    /// <param name="array">The array to wrap.</param>
    public EquatableArray(T[] array)
    {
        _array = array;
        _cachedHashCode = ComputeHashCode(array);
    }

    /// <summary>Gets the length of the array.</summary>
    internal int Length => _array?.Length ?? 0;

    /// <summary>Gets the element at the specified index.</summary>
    /// <param name="index">The zero-based index of the element to get.</param>
    internal T this[int index] => _array![index];

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
    public static bool operator !=(EquatableArray<T> left, EquatableArray<T> right) => !left.Equals(right);

    /// <summary>Indicates whether the current array is equal to another array.</summary>
    /// <param name="other">An array to compare with this array.</param>
    /// <returns>true if the arrays are equal; otherwise, false.</returns>
    public bool Equals(EquatableArray<T> other)
    {
        if (_array is null && other._array is null)
        {
            return true;
        }

        if (_array is null || other._array is null)
        {
            return false;
        }

        if (_array.Length != other._array.Length)
        {
            return false;
        }

        for (var i = 0; i < _array.Length; i++)
        {
            if (!_array[i].Equals(other._array[i]))
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

    /// <summary>Returns an enumerator that iterates through the array.</summary>
    /// <returns>An enumerator for the array.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)(_array ?? [])).GetEnumerator();

    /// <summary>Returns an enumerator that iterates through the array.</summary>
    /// <returns>An enumerator for the array.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>Computes a deterministic hash code for the given array.</summary>
    /// <param name="array">The array to hash.</param>
    /// <returns>A hash code for the array.</returns>
    internal static int ComputeHashCode(T[]? array)
    {
        if (array is null)
        {
            return 0;
        }

        unchecked
        {
            var hash = 17;
            for (var i = 0; i < array.Length; i++)
            {
                hash = (hash * HashMultiplier) + (array[i]?.GetHashCode() ?? 0);
            }

            return hash;
        }
    }
}
