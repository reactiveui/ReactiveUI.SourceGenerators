// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Text;

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>Hands out reusable string builders so a generation pass reuses buffers instead of allocating per file.</summary>
/// <remarks>
/// <para>
/// Each generated file was built in its own fresh builder, so a pass allocated a builder and its grown chunk
/// chain per file and threw them away. Renting from a per-thread free list lets one pass reuse the same
/// buffers, and a builder that has already grown to fit one file starts large enough for the next.
/// </para>
/// <para>
/// The free list is a list rather than a single slot because the emitters nest: a grouping key is built while
/// the file it belongs to is still open. A single slot would hand the same builder to both.
/// </para>
/// <para>
/// Thread-static because source-output callbacks can run concurrently, and because renting must not need a
/// lock to be worth doing. A builder is only reused after <see cref="ToStringAndReturn"/> hands it back, so two
/// live rents never share one.
/// </para>
/// </remarks>
internal static class PooledBuilder
{
    /// <summary>How many builders to keep per thread, covering the emitters' nesting depth.</summary>
    private const int MaxPooled = 8;

    /// <summary>
    /// The largest builder worth keeping. One outsized file would otherwise pin its whole chunk chain for the
    /// life of the thread, which in a build host outlives the compilation that needed it.
    /// </summary>
    private const int MaxRetainedCapacity = 256 * 1024;

    /// <summary>The per-thread free list.</summary>
    [ThreadStatic]
    private static StringBuilder?[]? _pool;

    /// <summary>The number of populated slots in <see cref="_pool"/>.</summary>
    [ThreadStatic]
    private static int _pooledCount;

    /// <summary>Rents a builder, empty and ready to write.</summary>
    /// <param name="capacity">The capacity the caller expects to need.</param>
    /// <returns>An empty builder.</returns>
    internal static StringBuilder Rent(int capacity)
    {
        var pool = _pool;
        if (pool is not null && _pooledCount > 0)
        {
            _pooledCount--;
            var pooled = pool[_pooledCount]!;
            pool[_pooledCount] = null;
            _ = pooled.EnsureCapacity(capacity);
            return pooled;
        }

        return new(capacity);
    }

    /// <summary>Materializes a rented builder's content and hands the builder back for reuse.</summary>
    /// <param name="builder">The rented builder, which the caller must not touch afterwards.</param>
    /// <returns>The accumulated string.</returns>
    internal static string ToStringAndReturn(StringBuilder builder)
    {
        var result = builder.ToString();
        Return(builder);
        return result;
    }

    /// <summary>Hands a rented builder back without materializing it.</summary>
    /// <param name="builder">The rented builder, which the caller must not touch afterwards.</param>
    internal static void Return(StringBuilder builder)
    {
        if (builder.Capacity > MaxRetainedCapacity)
        {
            return;
        }

        var pool = _pool ??= new StringBuilder?[MaxPooled];
        if (_pooledCount >= MaxPooled)
        {
            return;
        }

        _ = builder.Clear();
        pool[_pooledCount] = builder;
        _pooledCount++;
    }
}
