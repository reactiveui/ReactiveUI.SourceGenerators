// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace System;

/// <summary>A polyfill type that mirrors some methods from <see cref="HashCode"/> on .NET 6.</summary>
internal struct HashCode : IEquatable<HashCode>
{
    /// <summary>The number of values held in the state.</summary>
    private const uint ValuesPerState = 4;

    /// <summary>The position of the first queued value.</summary>
    private const uint FirstQueuePosition = 0;

    /// <summary>The position of the second queued value.</summary>
    private const uint SecondQueuePosition = 1;

    /// <summary>The position of the third queued value.</summary>
    private const uint ThirdQueuePosition = 2;

    /// <summary>The bit-width of the hash value.</summary>
    private const int HashWidthInBits = 32;

    /// <summary>The first prime used by the xxHash algorithm.</summary>
    private const uint Prime1 = 2_654_435_761U;

    /// <summary>The second prime used by the xxHash algorithm.</summary>
    private const uint Prime2 = 2_246_822_519U;

    /// <summary>The third prime used by the xxHash algorithm.</summary>
    private const uint Prime3 = 3_266_489_917U;

    /// <summary>The fourth prime used by the xxHash algorithm.</summary>
    private const uint Prime4 = 668_265_263U;

    /// <summary>The fifth prime used by the xxHash algorithm.</summary>
    private const uint Prime5 = 374_761_393U;

    /// <summary>The rotation offset for a round.</summary>
    private const int RoundRotationOffset = 13;

    /// <summary>The rotation offset for a queued value.</summary>
    private const int QueueRoundRotationOffset = 17;

    /// <summary>The rotation offset for the first state value.</summary>
    private const int FirstStateRotationOffset = 1;

    /// <summary>The rotation offset for the second state value.</summary>
    private const int SecondStateRotationOffset = 7;

    /// <summary>The rotation offset for the third state value.</summary>
    private const int ThirdStateRotationOffset = 12;

    /// <summary>The rotation offset for the fourth state value.</summary>
    private const int FourthStateRotationOffset = 18;

    /// <summary>The process-specific hash seed.</summary>
    private static readonly uint seed = GenerateGlobalSeed();

    /// <summary>The first state value.</summary>
    private uint _v1;

    /// <summary>The second state value.</summary>
    private uint _v2;

    /// <summary>The third state value.</summary>
    private uint _v3;

    /// <summary>The fourth state value.</summary>
    private uint _v4;

    /// <summary>The first queued value.</summary>
    private uint _queue1;

    /// <summary>The second queued value.</summary>
    private uint _queue2;

    /// <summary>The third queued value.</summary>
    private uint _queue3;

    /// <summary>The number of values added to the hash.</summary>
    private uint _length;

    /// <inheritdoc/>
    public readonly bool Equals(HashCode other) =>
        _v1 == other._v1
        && _v2 == other._v2
        && _v3 == other._v3
        && _v4 == other._v4
        && _queue1 == other._queue1
        && _queue2 == other._queue2
        && _queue3 == other._queue3
        && _length == other._length;

    /// <inheritdoc/>
    public override readonly bool Equals(object? obj) => obj is HashCode other && Equals(other);

    /// <inheritdoc/>
    public override readonly int GetHashCode() => ToHashCode();

    /// <summary>Adds a single value to the current hash.</summary>
    /// <typeparam name="T">The type of the value to add into the hash code.</typeparam>
    /// <param name="value">The value to add into the hash code.</param>
    internal void Add<T>(T value) => Add(value?.GetHashCode() ?? 0);

    /// <summary>Gets the resulting hashcode from the current instance.</summary>
    /// <returns>The resulting hashcode from the current instance.</returns>
    internal readonly int ToHashCode()
    {
        var length = _length;
        var position = length % ValuesPerState;
        var hash = length < ValuesPerState ? MixEmptyState() : MixState(_v1, _v2, _v3, _v4);

        hash += length * ValuesPerState;

        if (position != 0)
        {
            hash = QueueRound(hash, _queue1);

            if (position > 1)
            {
                hash = QueueRound(hash, _queue2);

                if (position > ThirdQueuePosition)
                {
                    hash = QueueRound(hash, _queue3);
                }
            }
        }

        hash = MixFinal(hash);

        return (int)hash;
    }

    /// <summary>Rotates the specified value left by the specified number of bits. Similar in behavior to the x86 instruction ROL.</summary>
    /// <param name="value">The value to rotate.</param>
    /// <param name="offset">The number of bits to rotate by.
    /// Any value outside the range [0..31] is treated as congruent mod 32.</param>
    /// <returns>The rotated value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint RotateLeft(uint value, int offset) => (value << offset) | (value >> (HashWidthInBits - offset));

    /// <summary>Initializes the default seed.</summary>
    /// <returns>A random seed.</returns>
    private static uint GenerateGlobalSeed()
    {
        var bytes = new byte[4];

        using (var generator = RandomNumberGenerator.Create())
        {
            generator.GetBytes(bytes);
        }

        return BitConverter.ToUInt32(bytes, 0);
    }

    /// <summary>Initializes the four state values.</summary>
    /// <param name="v1">The first state value.</param>
    /// <param name="v2">The second state value.</param>
    /// <param name="v3">The third state value.</param>
    /// <param name="v4">The fourth state value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Initialize(out uint v1, out uint v2, out uint v3, out uint v4)
    {
        v1 = seed + Prime1 + Prime2;
        v2 = seed + Prime2;
        v3 = seed;
        v4 = seed - Prime1;
    }

    /// <summary>Applies a round of the xxHash algorithm.</summary>
    /// <param name="hash">The hash to update.</param>
    /// <param name="input">The input value.</param>
    /// <returns>The updated hash.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint Round(uint hash, uint input) => RotateLeft(hash + (input * Prime2), RoundRotationOffset) * Prime1;

    /// <summary>Applies a round for a queued value.</summary>
    /// <param name="hash">The hash to update.</param>
    /// <param name="queuedValue">The queued value.</param>
    /// <returns>The updated hash.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint QueueRound(uint hash, uint queuedValue) => RotateLeft(hash + (queuedValue * Prime3), QueueRoundRotationOffset) * Prime4;

    /// <summary>Mixes the four state values into a hash.</summary>
    /// <param name="v1">The first state value.</param>
    /// <param name="v2">The second state value.</param>
    /// <param name="v3">The third state value.</param>
    /// <param name="v4">The fourth state value.</param>
    /// <returns>The mixed hash.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint MixState(uint v1, uint v2, uint v3, uint v4) =>
        RotateLeft(v1, FirstStateRotationOffset)
        + RotateLeft(v2, SecondStateRotationOffset)
        + RotateLeft(v3, ThirdStateRotationOffset)
        + RotateLeft(v4, FourthStateRotationOffset);

    /// <summary>Creates a hash state with no added values.</summary>
    /// <returns>The initialized hash state.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint MixEmptyState() => seed + Prime5;

    /// <summary>Applies the final xxHash mixing operations.</summary>
    /// <param name="hash">The hash to mix.</param>
    /// <returns>The final mixed hash.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint MixFinal(uint hash)
    {
        hash ^= hash >> 15;
        hash *= Prime2;
        hash ^= hash >> 13;
        hash *= Prime3;
        hash ^= hash >> 16;

        return hash;
    }

    /// <summary>Adds a hash code to the current state.</summary>
    /// <param name="value">The hash code to add.</param>
    private void Add(int value)
    {
        var val = (uint)value;
        var previousLength = _length++;
        var position = previousLength % ValuesPerState;

        if (position == FirstQueuePosition)
        {
            _queue1 = val;
        }
        else if (position == SecondQueuePosition)
        {
            _queue2 = val;
        }
        else if (position == ThirdQueuePosition)
        {
            _queue3 = val;
        }
        else
        {
            if (previousLength == ValuesPerState - 1)
            {
                Initialize(out _v1, out _v2, out _v3, out _v4);
            }

            _v1 = Round(_v1, _queue1);
            _v2 = Round(_v2, _queue2);
            _v3 = Round(_v3, _queue3);
            _v4 = Round(_v4, val);
        }
    }
}
