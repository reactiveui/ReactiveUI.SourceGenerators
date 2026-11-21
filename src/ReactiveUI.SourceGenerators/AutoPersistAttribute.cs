// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;

namespace ReactiveUI.SourceGenerators;

/// <summary>Generates a compile-time <c>AutoPersistMetadata</c> property for a <c>[DataContract]</c> reactive object without runtime reflection.</summary>
[Conditional(KeepAttributes.Symbol)]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
[DebuggerDisplay("AutoPersistAttribute")]
public sealed class AutoPersistAttribute : Attribute;
