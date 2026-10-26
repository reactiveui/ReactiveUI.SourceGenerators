// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.SourceGenerators.Helpers;

/// <summary>Extensions for <see cref="ArrayBuilder{T}"/>.</summary>
internal static class ArrayBuilderExtensions
{
    /// <summary>Provides extension members for a builder of equatable items.</summary>
    /// <typeparam name="T">The type of items.</typeparam>
    /// <param name="builder">The builder to extend.</param>
    extension<T>(ArrayBuilder<T> builder)
        where T : IEquatable<T>
    {
        /// <summary>Creates a value-equatable array from the written items.</summary>
        /// <returns>The items, ready to store in a pipeline model.</returns>
        internal EquatableArray<T> ToEquatableArray() => new(builder.ToArray());
    }
}
