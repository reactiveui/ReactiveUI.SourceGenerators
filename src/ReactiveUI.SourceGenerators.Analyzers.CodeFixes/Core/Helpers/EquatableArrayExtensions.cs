// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Immutable;

namespace ReactiveUI.SourceGenerators.CodeFixers.Helpers;

/// <summary>Extensions for <see cref="EquatableArray{T}"/>.</summary>
internal static class EquatableArrayExtensions
{
    /// <summary>Extension methods for <see cref="ImmutableArray{T}"/> instances.</summary>
    /// <typeparam name="T">The immutable-array element type.</typeparam>
    /// <param name="array">The immutable array to extend.</param>
    extension<T>(ImmutableArray<T> array)
        where T : IEquatable<T>
    {
        /// <summary>Creates an <see cref="EquatableArray{T}"/> from the current immutable array.</summary>
        /// <returns>An <see cref="EquatableArray{T}"/> instance.</returns>
        internal EquatableArray<T> AsEquatableArray() => new(array);
    }
}
