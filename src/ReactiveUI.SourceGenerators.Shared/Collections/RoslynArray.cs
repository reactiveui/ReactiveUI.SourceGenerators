// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace ReactiveUI.SourceGenerators.Helpers;

/// <summary>Reads the arrays Roslyn returns as plain arrays.</summary>
/// <remarks>Roslyn hands out <see cref="ImmutableArray{T}"/> from <c>Collect()</c> and its symbol APIs. Everything past that boundary uses arrays.</remarks>
internal static class RoslynArray
{
    /// <summary>Reads the array a Roslyn API returned as a plain array, without copying it.</summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="array">The Roslyn-owned array. Callers must not write to the result.</param>
    /// <returns>The array behind it, or an empty array for a default value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T[] Unwrap<T>(ImmutableArray<T> array) =>
        Unsafe.As<ImmutableArray<T>, T[]?>(ref array) ?? [];
}
