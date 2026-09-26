// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;

namespace ReactiveUI.SourceGenerators;

/// <summary>Generates a property that raises change notifications from the annotated field or partial property.</summary>
/// <param name="alsoNotify">The names of other properties to raise change notifications for.</param>
[Conditional(KeepAttributes.Symbol)]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class ReactiveAttribute(params string[] alsoNotify) : Attribute
{
    /// <summary>Gets or sets the accessibility of the generated set accessor.</summary>
    public AccessModifier SetModifier { get; set; }

    /// <summary>Gets or sets the inheritance modifier of the generated property.</summary>
    public InheritanceModifier Inheritance { get; set; }

    /// <summary>Gets or sets a value indicating whether the generated property is <c>required</c>.</summary>
    public bool UseRequired { get; set; }

    /// <summary>Gets the names of other properties to raise change notifications for.</summary>
    public string[]? AlsoNotify { get; } = alsoNotify;
}
