// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;

namespace ReactiveUI.SourceGenerators.Android;

/// <summary>Excludes a property from compile-time wiring by <see cref="WireUpControlsAttribute"/>.</summary>
[Conditional(KeepAttributes.Symbol)]
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
[DebuggerDisplay("IgnoreResourceAttribute")]
public sealed class IgnoreResourceAttribute : Attribute;
