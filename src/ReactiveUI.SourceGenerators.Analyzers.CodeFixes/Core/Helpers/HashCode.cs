// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace ReactiveUI.SourceGenerators.CodeFixers.Helpers;

/// <summary>Provides a .NET Standard-compatible incremental hash-code implementation.</summary>
internal struct HashCode : IEquatable<HashCode>
{
    /// <summary>The number of values processed in a complete hash block.</summary>
    private const uint ValuesPerBlock = 4U;

    /// <summary>The queue position following the second entry.</summary>
    private const uint ThirdQueuePosition = 2U;

    /// <summary>The number of bits in a byte.</summary>
    private const int BitsPerByte = 8;

    /// <summary>The first xxHash prime.</summary>
    private const uint Prime1 = 2_654_435_761U;

    /// <summary>The second xxHash prime.</summary>
    private const uint Prime2 = 2_246_822_519U;

    /// <summary>The third xxHash prime.</summary>
    private const uint Prime3 = 3_266_489_917U;

    /// <summary>The fourth xxHash prime.</summary>
    private const uint Prime4 = 668_265_263U;

    /// <summary>The fifth xxHash prime.</summary>
    private const uint Prime5 = 374_761_393U;

    /// <summary>The rotation used by <see cref="Round"/>.</summary>
    private const int RoundRotation = 13;

    /// <summary>The rotation used by <see cref="QueueRound"/>.</summary>
    private const int QueueRotation = 17;

    /// <summary>The first rotation used while mixing state.</summary>
    private const int FirstStateRotation = 1;

    /// <summary>The second rotation used while mixing state.</summary>
    private const int SecondStateRotation = 7;

    /// <summary>The third rotation used while mixing state.</summary>
    private const int ThirdStateRotation = 12;

    /// <summary>The fourth rotation used while mixing state.</summary>
    private const int FourthStateRotation = 18;

    /// <summary>The number of bytes used to create the random seed.</summary>
    private const int SeedByteCount = 4;

    /// <summary>The process-wide random seed.</summary>
    private static readonly uint Seed = GenerateGlobalSeed();

    /// <summary>The first accumulated hash value.</summary>
    private uint _v1;

    /// <summary>The second accumulated hash value.</summary>
    private uint _v2;

    /// <summary>The third accumulated hash value.</summary>
    private uint _v3;

    /// <summary>The fourth accumulated hash value.</summary>
    private uint _v4;

    /// <summary>The first queued value.</summary>
    private uint _queue1;

    /// <summary>The second queued value.</summary>
    private uint _queue2;

    /// <summary>The third queued value.</summary>
    private uint _queue3;

    /// <summary>The number of values added to the hash.</summary>
    private uint _length;

    /// <inheritdoc />
    public override readonly bool Equals(object? obj) => obj is HashCode other && Equals(other);

    /// <inheritdoc />
    public override readonly int GetHashCode() => ToHashCode();

    /// <summary>Adds a value to the current hash.</summary>
    /// <typeparam name="T">The type of the value to add.</typeparam>
    /// <param name="value">The value to add.</param>
    internal void Add<T>(T value) => Add(value?.GetHashCode() ?? 0);

    /// <summary>Gets the resulting hash code from the current instance.</summary>
    /// <returns>The resulting hash code.</returns>
    internal readonly int ToHashCode()
    {
        var position = _length % ValuesPerBlock;
        var hash = _length < ValuesPerBlock ? MixEmptyState() : MixState(_v1, _v2, _v3, _v4);
        hash += _length * ValuesPerBlock;

        if (position != 0U)
        {
            hash = QueueRound(hash, _queue1);
            if (position > 1U)
            {
                hash = QueueRound(hash, _queue2);
                if (position > ThirdQueuePosition)
                {
                    hash = QueueRound(hash, _queue3);
                }
            }
        }

        return (int)MixFinal(hash);
    }

    /// <summary>Determines whether this hash state equals another hash state.</summary>
    /// <param name="other">The hash state to compare.</param>
    /// <returns><see langword="true"/> when both hash states are equal.</returns>
    internal readonly bool Equals(HashCode other) =>
        _v1 == other._v1 && _v2 == other._v2 && _v3 == other._v3 && _v4 == other._v4
        && _queue1 == other._queue1 && _queue2 == other._queue2 && _queue3 == other._queue3
        && _length == other._length;

    /// <summary>Rotates a value left by a number of bits.</summary>
    /// <param name="value">The value to rotate.</param>
    /// <param name="offset">The number of bits to rotate by.</param>
    /// <returns>The rotated value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint RotateLeft(uint value, int offset) => (value << offset) | (value >> ((sizeof(uint) * BitsPerByte) - offset));

    /// <summary>Creates the process-wide random seed.</summary>
    /// <returns>A random seed.</returns>
    private static uint GenerateGlobalSeed()
    {
        var bytes = new byte[SeedByteCount];
        using var generator = RandomNumberGenerator.Create();
        generator.GetBytes(bytes);
        return BitConverter.ToUInt32(bytes, 0);
    }

    /// <summary>Initializes a full hash block.</summary>
    /// <param name="v1">The first initialized value.</param>
    /// <param name="v2">The second initialized value.</param>
    /// <param name="v3">The third initialized value.</param>
    /// <param name="v4">The fourth initialized value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Initialize(out uint v1, out uint v2, out uint v3, out uint v4)
    {
        v1 = Seed + Prime1 + Prime2;
        v2 = Seed + Prime2;
        v3 = Seed;
        v4 = Seed - Prime1;
    }

    /// <summary>Mixes a value into a hash accumulator.</summary>
    /// <param name="hash">The current hash.</param>
    /// <param name="input">The value to mix.</param>
    /// <returns>The mixed hash.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint Round(uint hash, uint input) => RotateLeft(hash + (input * Prime2), RoundRotation) * Prime1;

    /// <summary>Mixes a queued value into a hash accumulator.</summary>
    /// <param name="hash">The current hash.</param>
    /// <param name="queuedValue">The queued value to mix.</param>
    /// <returns>The mixed hash.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint QueueRound(uint hash, uint queuedValue) => RotateLeft(hash + (queuedValue * Prime3), QueueRotation) * Prime4;

    /// <summary>Mixes a complete hash state.</summary>
    /// <param name="v1">The first state value.</param>
    /// <param name="v2">The second state value.</param>
    /// <param name="v3">The third state value.</param>
    /// <param name="v4">The fourth state value.</param>
    /// <returns>The mixed hash.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint MixState(uint v1, uint v2, uint v3, uint v4) =>
        RotateLeft(v1, FirstStateRotation) + RotateLeft(v2, SecondStateRotation)
        + RotateLeft(v3, ThirdStateRotation) + RotateLeft(v4, FourthStateRotation);

    /// <summary>Creates the initial hash state for an empty sequence.</summary>
    /// <returns>The initial hash state.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint MixEmptyState() => Seed + Prime5;

    /// <summary>Applies the final avalanche to a hash.</summary>
    /// <param name="hash">The hash to finalize.</param>
    /// <returns>The finalized hash.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint MixFinal(uint hash)
    {
        hash ^= hash >> 15;
        hash *= Prime2;
        hash ^= hash >> RoundRotation;
        hash *= Prime3;
        hash ^= hash >> 16;
        return hash;
    }

    /// <summary>Adds an integer value to the current hash.</summary>
    /// <param name="value">The value to add.</param>
    private void Add(int value)
    {
        var previousLength = _length;
        _length++;
        switch (previousLength % ValuesPerBlock)
        {
            case 0U:
                {
                    _queue1 = (uint)value;
                    return;
                }

            case 1U:
                {
                    _queue2 = (uint)value;
                    return;
                }

            case ThirdQueuePosition:
                {
                    _queue3 = (uint)value;
                    return;
                }

            default:
                {
                    if (previousLength == ValuesPerBlock - 1U)
                    {
                        Initialize(out _v1, out _v2, out _v3, out _v4);
                    }

                    _v1 = Round(_v1, _queue1);
                    _v2 = Round(_v2, _queue2);
                    _v3 = Round(_v3, _queue3);
                    _v4 = Round(_v4, (uint)value);
                    return;
                }
        }
    }

    /// <inheritdoc />
    bool IEquatable<HashCode>.Equals(HashCode other) => Equals(other);
}
