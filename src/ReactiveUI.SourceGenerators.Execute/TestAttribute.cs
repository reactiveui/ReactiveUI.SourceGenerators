// Copyright (c) 2025 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace SGReactiveUI.SourceGenerators.Test;

/// <summary>
/// TestAttribute.
/// </summary>
/// <seealso cref="System.Attribute" />
[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class TestAttribute : Attribute
{
    /// <summary>
    /// Gets a parameter.
    /// </summary>
    /// <value>
    /// a parameter.
    /// </value>
    public string? AParameter { get; init; }
}
