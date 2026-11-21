// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;

namespace ReactiveUI.SourceGenerators;

/// <summary>Generates a compile-time <c>ConfigureValidation</c> method that wires DataAnnotations attributes on <c>ReactiveProperty&lt;T&gt;</c> properties without reflection.</summary>
[Conditional(KeepAttributes.Symbol)]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
[DebuggerDisplay("ReactiveValidationAttribute")]
public sealed class ReactiveValidationAttribute : Attribute;
