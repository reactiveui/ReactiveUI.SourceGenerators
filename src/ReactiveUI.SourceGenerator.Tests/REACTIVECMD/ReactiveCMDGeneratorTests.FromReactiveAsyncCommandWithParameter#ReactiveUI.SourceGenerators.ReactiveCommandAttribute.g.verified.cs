﻿//HintName: ReactiveUI.SourceGenerators.ReactiveCommandAttribute.g.cs
// Copyright (c) 2025 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

// <auto-generated/>
#pragma warning disable
#nullable enable
namespace ReactiveUI.SourceGenerators;

/// <summary>
/// ReativeCommandAttribute.
/// </summary>
/// <seealso cref="Attribute" />
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
internal sealed class ReactiveCommandAttribute : Attribute
{
    /// <summary>
    /// Gets the can execute method or property.
    /// </summary>
    /// <value>
    /// The name of the CanExecute Observable of bool.
    /// </value>
    public string? CanExecute { get; init; }
    
    /// <summary>
    /// Gets the output scheduler.
    /// </summary>
    /// <value>
    /// The output scheduler.
    /// </value>
    public string? OutputScheduler { get; init; }
}
#nullable restore
#pragma warning restore