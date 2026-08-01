// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace SGReactiveUI.SourceGenerators.Test;

/// <summary>Provides a test attribute for generated property metadata.</summary>
/// <seealso cref="System.Attribute" />
[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class TestAttribute : Attribute
{
    /// <summary>Gets a parameter.</summary>
    /// <value>
    /// a parameter.
    /// </value>
    public string? AParameter { get; init; }
}
