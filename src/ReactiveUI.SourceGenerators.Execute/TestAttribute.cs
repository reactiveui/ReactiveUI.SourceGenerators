﻿// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace SGReactiveUI.SourceGenerators.Test;

/// <summary>
/// TestAttribute.
/// </summary>
/// <seealso cref="System.Attribute" />
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
