// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Immutable;

namespace ReactiveUI.SourceGenerators.Helpers;

/// <summary>Extensions for <see cref="EquatableArray{T}"/>.</summary>
internal static class EquatableArrayExtensions
{
    /// <summary>Provides extension members for an immutable array.</summary>
    /// <typeparam name="T">The type of items in the array.</typeparam>
    /// <param name="array">The immutable array to extend.</param>
    extension<T>(ImmutableArray<T> array)
        where T : IEquatable<T>
    {
        /// <summary>Creates an <see cref="EquatableArray{T}"/> instance from this array.</summary>
        /// <returns>An <see cref="EquatableArray{T}"/> instance.</returns>
        internal EquatableArray<T> AsEquatableArray() => new(array);
    }
}
