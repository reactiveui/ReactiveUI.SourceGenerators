// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;

namespace ReactiveUI.SourceGenerators.Android;

/// <summary>Generates a compile-time <c>WireUpControls</c> method that assigns Android views using <c>FindViewById</c> without reflection.</summary>
[Conditional(KeepAttributes.Symbol)]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
[DebuggerDisplay("WireUpControlsAttribute: ExplicitOptIn = {ExplicitOptIn}")]
public sealed class WireUpControlsAttribute : Attribute
{
    /// <summary>Gets or sets a value indicating whether only properties marked with <see cref="WireUpResourceAttribute"/> are wired.</summary>
    public bool ExplicitOptIn { get; set; }
}
