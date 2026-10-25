// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>A fluent string builder for short generated fragments, backed by thread-local pooled buffers.</summary>
/// <remarks>
/// <para>
/// The emitters build a great many small, transient fragments - grouping keys, type argument lists, property
/// access chains - one per invocation or per group. Each of those was a fresh builder and its own chunk chain.
/// Accumulating into a pooled <c>char[]</c> instead lets the buffer be reused across fragments, so the steady
/// state for the whole emission is a handful of arrays rather than one allocation per fragment.
/// </para>
/// <para>
/// The free list is thread-local rather than a shared array pool: the emitters nest fragment builders inside
/// one another, which exhausts a shared pool's lock-free tier and would put renting on a contended path for no
/// benefit. Nothing here needs to be visible across threads, because a builder never outlives the call that
/// rented it.
/// </para>
/// <para>
/// Unlike a single-use design, <see cref="ToString"/> leaves the buffer in place so the builder can be cleared
/// and reused across a loop, which is how the grouping keys are built. Call <see cref="Return"/> when finished.
/// Forgetting to costs reuse, never correctness.
/// </para>
/// </remarks>
internal sealed class PooledStringBuilder
{
    /// <summary>The default rented capacity, sized to hold a typical fragment without growing.</summary>
    private const int DefaultCapacity = 256;

    /// <summary>The factor the buffer grows by when exhausted.</summary>
    private const int GrowthFactor = 2;

    /// <summary>The number of buffers cached per thread, covering the emitters' fragment nesting depth.</summary>
    private const int MaxPooledPerThread = 16;

    /// <summary>The base of the decimal rendering used by <see cref="Append(int)"/>.</summary>
    private const int DecimalBase = 10;

    /// <summary>The widest decimal rendering of an <see cref="int"/>, including the sign.</summary>
    private const int MaxIntegerDigits = 11;

    /// <summary>The per-thread free list of reusable buffers.</summary>
    [ThreadStatic]
    private static char[][]? _pool;

    /// <summary>The number of populated slots in <see cref="_pool"/>.</summary>
    [ThreadStatic]
    private static int _pooledCount;

    /// <summary>The pooled array currently backing this builder.</summary>
    private char[] _buffer;

    /// <summary>The write position within <see cref="_buffer"/>.</summary>
    private int _position;

    /// <summary>Initializes a new instance of the <see cref="PooledStringBuilder"/> class.</summary>
    internal PooledStringBuilder()
        : this(DefaultCapacity)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="PooledStringBuilder"/> class.</summary>
    /// <param name="capacity">The capacity to rent up front.</param>
    internal PooledStringBuilder(int capacity) =>
        _buffer = RentBuffer(capacity < DefaultCapacity ? DefaultCapacity : capacity);

    /// <summary>Gets the number of characters accumulated so far.</summary>
    internal int Length => _position;

    /// <summary>Materializes the accumulated content, leaving the builder usable.</summary>
    /// <returns>The accumulated string.</returns>
    public override string ToString() => _position == 0 ? string.Empty : new string(_buffer, 0, _position);

    /// <summary>Appends a string.</summary>
    /// <param name="value">The string to append, which may be null or empty.</param>
    /// <returns>This builder, for chaining.</returns>
    internal PooledStringBuilder Append(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return this;
        }

        EnsureCapacity(_position + value!.Length);
        value.CopyTo(0, _buffer, _position, value.Length);
        _position += value.Length;
        return this;
    }

    /// <summary>Appends a single character.</summary>
    /// <param name="value">The character to append.</param>
    /// <returns>This builder, for chaining.</returns>
    internal PooledStringBuilder Append(char value)
    {
        EnsureCapacity(_position + 1);
        _buffer[_position] = value;
        _position++;
        return this;
    }

    /// <summary>Appends a boolean, rendered the way the framework builder renders it.</summary>
    /// <param name="value">The value to append.</param>
    /// <returns>This builder, for chaining.</returns>
    /// <remarks>
    /// Matches the framework rendering exactly, because these appends land in the grouping keys that decide
    /// which invocations share a generated overload.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal PooledStringBuilder Append(bool value) => Append(value ? "True" : "False");

    /// <summary>Appends the invariant decimal rendering of an integer.</summary>
    /// <param name="value">The value to append.</param>
    /// <returns>This builder, for chaining.</returns>
    /// <remarks>
    /// Formats digits straight into the buffer. These appends sit in per-property loops, so going through
    /// <c>ToString</c> would allocate a string per index for a value that is almost always one or two digits.
    /// </remarks>
    internal PooledStringBuilder Append(int value)
    {
        EnsureCapacity(_position + MaxIntegerDigits);

        if (value < 0)
        {
            _buffer[_position] = '-';
            _position++;
        }

        // Accumulated in the negative range rather than the positive one, so int.MinValue needs no special case.
        var remaining = value < 0 ? value : -value;
        var digitStart = _position;

        do
        {
            _buffer[_position] = (char)('0' - (remaining % DecimalBase));
            _position++;
            remaining /= DecimalBase;
        }
        while (remaining != 0);

        ReverseDigits(digitStart);
        return this;
    }

    /// <summary>Empties the builder, keeping its buffer for the next fragment.</summary>
    /// <returns>This builder, for chaining.</returns>
    internal PooledStringBuilder Clear()
    {
        _position = 0;
        return this;
    }

    /// <summary>Hands the buffer back to the thread's free list.</summary>
    /// <remarks>The builder must not be appended to afterwards.</remarks>
    internal void Return()
    {
        var toReturn = _buffer;
        _buffer = [];
        _position = 0;
        ReturnBuffer(toReturn);
    }

    /// <summary>Materializes the accumulated content and hands the buffer back.</summary>
    /// <returns>The accumulated string.</returns>
    internal string ToStringAndReturn()
    {
        var result = ToString();
        Return();
        return result;
    }

    /// <summary>Takes a buffer of at least the requested length from the thread's free list, or allocates one.</summary>
    /// <param name="minimumLength">The minimum length required.</param>
    /// <returns>A buffer at least <paramref name="minimumLength"/> long.</returns>
    private static char[] RentBuffer(int minimumLength)
    {
        var pool = _pool;
        if (pool is not null)
        {
            for (var i = _pooledCount - 1; i >= 0; i--)
            {
                var candidate = pool[i];
                if (candidate.Length < minimumLength)
                {
                    continue;
                }

                _pooledCount--;
                pool[i] = pool[_pooledCount];
                pool[_pooledCount] = null!;
                return candidate;
            }
        }

        return new char[minimumLength];
    }

    /// <summary>Puts a buffer back on the thread's free list, dropping it when the list is full.</summary>
    /// <param name="buffer">The buffer to return.</param>
    private static void ReturnBuffer(char[] buffer)
    {
        if (buffer.Length == 0)
        {
            return;
        }

        var pool = _pool ??= new char[MaxPooledPerThread][];
        if (_pooledCount >= MaxPooledPerThread)
        {
            return;
        }

        pool[_pooledCount] = buffer;
        _pooledCount++;
    }

    /// <summary>Grows the buffer when the requested length no longer fits.</summary>
    /// <param name="required">The total capacity required.</param>
    private void EnsureCapacity(int required)
    {
        if (required <= _buffer.Length)
        {
            return;
        }

        var grown = _buffer.Length * GrowthFactor;
        var next = RentBuffer(required > grown ? required : grown);
        Array.Copy(_buffer, next, _position);
        var toReturn = _buffer;
        _buffer = next;
        ReturnBuffer(toReturn);
    }

    /// <summary>Reverses the digits written from the given position, which were emitted least significant first.</summary>
    /// <param name="start">The index the digits start at.</param>
    private void ReverseDigits(int start)
    {
        for (var end = _position - 1; start < end; start++, end--)
        {
            (_buffer[end], _buffer[start]) = (_buffer[start], _buffer[end]);
        }
    }
}
