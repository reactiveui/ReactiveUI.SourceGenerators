// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Models;

/// <summary>Where a call site sits, in the form the compiler accepts on an interceptor.</summary>
/// <param name="Version">The encoding the compiler asked for, which travels with the data it produced.</param>
/// <param name="Data">The opaque call-site description the compiler produced, empty when it described none.</param>
/// <remarks>
/// Two strings and an integer rather than the compiler's own type: a pipeline model has to compare by value
/// and carry no symbol or syntax, and this is the whole of what an interceptor needs to name its call site.
/// </remarks>
internal readonly record struct InterceptorLocation(int Version, string? Data)
{
    /// <summary>Gets a value indicating whether the compiler described this call site.</summary>
    /// <remarks>
    /// A call site is undescribable when it is not an invocation the compiler can intercept - one written
    /// through a type parameter, for instance - and when the host compiler predates interception entirely.
    /// </remarks>
    internal bool IsAvailable => !string.IsNullOrEmpty(Data);
}
