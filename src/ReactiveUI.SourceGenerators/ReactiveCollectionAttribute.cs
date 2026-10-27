// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;

namespace ReactiveUI.SourceGenerators;

/// <summary>Generates a property that raises change notifications when the annotated <c>ObservableCollection&lt;T&gt;</c> changes.</summary>
[Conditional(KeepAttributes.Symbol)]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class ReactiveCollectionAttribute : Attribute;
