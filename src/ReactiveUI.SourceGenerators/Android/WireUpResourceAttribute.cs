// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;

namespace ReactiveUI.SourceGenerators.Android;

/// <summary>Marks a property to be wired by the compile-time <see cref="WireUpControlsAttribute"/> generator, optionally overriding the Android resource name.</summary>
/// <param name="resourceNameOverride">The optional <c>Resource.Id</c> member name; defaults to the property name.</param>
[Conditional(KeepAttributes.Symbol)]
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
[DebuggerDisplay("WireUpResourceAttribute: ResourceNameOverride = {ResourceNameOverride}")]
public sealed class WireUpResourceAttribute(string? resourceNameOverride = null) : Attribute
{
    /// <summary>Gets the optional Android <c>Resource.Id</c> member name override.</summary>
    public string? ResourceNameOverride { get; } = resourceNameOverride;
}
