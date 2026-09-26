// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;

namespace ReactiveUI.SourceGenerators;

/// <summary>Generates a property that exposes the annotated <c>ReadOnlyObservableCollection&lt;T&gt;</c> field.</summary>
[Conditional(KeepAttributes.Symbol)]
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class BindableDerivedListAttribute : Attribute
{
    /// <summary>Gets or sets the accessibility of the generated property.</summary>
    public PropertyAccessModifier AccessModifier { get; set; }
}
