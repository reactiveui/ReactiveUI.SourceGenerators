// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace ReactiveUI.SourceGenerators.CodeFixers.Helpers;

/// <summary>A helper type to build sequences of values with pooled buffers.</summary>
/// <typeparam name="T">The type of items to create sequences for.</typeparam>
internal ref struct ImmutableArrayBuilder<T>
{
    /// <summary>The rented <see cref="Writer"/> instance to use.</summary>
    private Writer? _writer;

    /// <summary>Initializes a new instance of the <see cref="ImmutableArrayBuilder{T}"/> struct.</summary>
    /// <param name="writer">The target data writer to use.</param>
    private ImmutableArrayBuilder(Writer writer) => _writer = writer;

    /// <summary>Gets the data written to the underlying buffer so far, as a <see cref="ReadOnlySpan{T}"/>.</summary>
    [UnscopedRef]
    internal readonly ReadOnlySpan<T> WrittenSpan
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _writer!.WrittenSpan;
    }

    /// <summary>Gets the count.</summary>
    /// <value>
    /// The count.
    /// </value>
    internal readonly int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _writer!.Count;
    }

    /// <inheritdoc/>
    public override readonly string ToString() => _writer!.WrittenSpan.ToString();

    /// <summary>Creates a <see cref="ImmutableArrayBuilder{T}"/> value with a pooled underlying data writer.</summary>
    /// <returns>A <see cref="ImmutableArrayBuilder{T}"/> instance to write data to.</returns>
    internal static ImmutableArrayBuilder<T> Rent() => new(new Writer());

    /// <summary>Adds an item to the end of the builder.</summary>
    /// <param name="item">The item to add.</param>
    internal readonly void Add(T item) => _writer!.Add(item);

    /// <summary>Adds the specified items to the end of the array.</summary>
    /// <param name="items">The items to add at the end of the array.</param>
    internal readonly void AddRange(scoped in ReadOnlySpan<T> items) => _writer!.AddRange(items);

    /// <summary>Creates an immutable array from the values in the builder.</summary>
    /// <returns>An immutable array containing the builder's values.</returns>
    internal readonly ImmutableArray<T> ToImmutable()
    {
        var array = _writer!.WrittenSpan.ToArray();

        return Unsafe.As<T[], ImmutableArray<T>>(ref array);
    }

    /// <summary>Creates an array from the values in the builder.</summary>
    /// <returns>An array containing the builder's values.</returns>
    internal readonly T[] ToArray() => _writer!.WrittenSpan.ToArray();

    /// <summary>Gets an <see cref="IEnumerable{T}"/> instance for the current builder.</summary>
    /// <returns>An <see cref="IEnumerable{T}"/> instance for the current builder.</returns>
    /// <remarks>
    /// The builder should not be mutated while an enumerator is in use.
    /// </remarks>
    internal readonly IEnumerable<T> AsEnumerable() => _writer!;

    /// <summary>Returns the pooled buffer to its owner.</summary>
    internal void Dispose()
    {
        var writer = _writer;

        _writer = null;

        writer?.Dispose();
    }

    /// <summary>A class handling the actual buffer writing.</summary>
    private sealed class Writer : ICollection<T>, IDisposable
    {
        /// <summary>The initial buffer capacity for character builders.</summary>
        private const int CharacterBufferCapacity = 1_024;

        /// <summary>The initial buffer capacity for non-character builders.</summary>
        private const int DefaultBufferCapacity = 8;

        /// <summary>The underlying <typeparamref name="T"/> array.</summary>
        private T?[]? _array;

        /// <summary>Initializes a new instance of the <see cref="Writer"/> class. Creates a new <see cref="Writer"/> instance with the specified parameters.</summary>
        internal Writer()
        {
            _array = ArrayPool<T?>.Shared.Rent(typeof(T) == typeof(char) ? CharacterBufferCapacity : DefaultBufferCapacity);
            Count = 0;
        }

        /// <summary>Gets or sets gets the number of values in the buffer.</summary>
        internal int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get; set;
        }

        /// <summary>Gets a span over the values in the buffer.</summary>
        internal ReadOnlySpan<T> WrittenSpan
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(_array!, 0, Count);
        }

        /// <summary>Gets a value indicating whether this collection is read-only.</summary>
        bool ICollection<T>.IsReadOnly => true;

        /// <summary>Adds an item to the buffer.</summary>
        /// <param name="item">The item to add.</param>
        internal void Add(T item)
        {
            EnsureCapacity(1);

            _array![Count] = item;
            Count++;
        }

        /// <summary>Adds a range of items to the buffer.</summary>
        /// <param name="items">The items to add.</param>
        internal void AddRange(in ReadOnlySpan<T> items)
        {
            EnsureCapacity(items.Length);

            items.CopyTo(_array.AsSpan(Count)!);

            Count += items.Length;
        }

        /// <summary>Returns the pooled buffer.</summary>
        internal void Dispose()
        {
            var array = _array;

            _array = null;

            if (array is null)
            {
                return;
            }

            ArrayPool<T?>.Shared.Return(array, clearArray: typeof(T) != typeof(char));
        }

        /// <summary>Clears this collection.</summary>
        void ICollection<T>.Clear() => throw new NotSupportedException();

        /// <summary>Determines whether this collection contains an item.</summary>
        /// <param name="item">The item to locate.</param>
        /// <returns><see langword="true"/> when the item is in the collection.</returns>
        bool ICollection<T>.Contains(T item) => throw new NotSupportedException();

        /// <summary>Copies the collection to an array.</summary>
        /// <param name="array">The destination array.</param>
        /// <param name="arrayIndex">The destination index at which copying begins.</param>
        void ICollection<T>.CopyTo(T[] array, int arrayIndex) => Array.Copy(_array!, 0, array, arrayIndex, Count);

        /// <inheritdoc/>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            var array = _array!;
            var length = Count;

            for (var i = 0; i < length; i++)
            {
                yield return array[i]!;
            }
        }

        /// <summary>Gets a non-generic enumerator for this collection.</summary>
        /// <returns>A non-generic enumerator for this collection.</returns>
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<T>)this).GetEnumerator();

        /// <summary>Removes an item from this collection.</summary>
        /// <param name="item">The item to remove.</param>
        /// <returns><see langword="true"/> when the item was removed.</returns>
        bool ICollection<T>.Remove(T item) => throw new NotSupportedException();

        /// <summary>Gets the number of items in this collection.</summary>
        int ICollection<T>.Count => Count;

        /// <summary>Adds an item through the collection interface.</summary>
        /// <param name="item">The item to add.</param>
        void ICollection<T>.Add(T item) => Add(item);

        /// <summary>Disposes this collection through the disposable interface.</summary>
        void IDisposable.Dispose() => Dispose();

        /// <summary>Ensures that <see cref="_array"/> has enough free space to contain a given number of new items.</summary>
        /// <param name="requestedSize">The minimum number of items to ensure space for in <see cref="_array"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureCapacity(int requestedSize)
        {
            if (requestedSize <= _array!.Length - Count)
            {
                return;
            }

            ResizeBuffer(requestedSize);
        }

        /// <summary>Resizes <see cref="_array"/> to ensure it can fit the specified number of new items.</summary>
        /// <param name="sizeHint">The minimum number of items to ensure space for in <see cref="_array"/>.</param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ResizeBuffer(int sizeHint)
        {
            var minimumSize = Count + sizeHint;

            var oldArray = _array!;
            var newArray = ArrayPool<T?>.Shared.Rent(minimumSize);

            Array.Copy(oldArray, newArray, Count);

            _array = newArray;

            ArrayPool<T?>.Shared.Return(oldArray, clearArray: typeof(T) != typeof(char));
        }
    }
}
