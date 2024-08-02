// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

namespace ReactiveUI.SourceGenerators;

/// <summary>
/// ReactiveObjectAttribute.
/// </summary>
/// <seealso cref="System.Attribute" />
/// <remarks>
/// Initializes a new instance of the <see cref="IViewForAttribute"/> class.
/// </remarks>
/// <param name="viewModelType">Type of the view model.</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
#pragma warning disable CS9113 // Parameter is unread.
public sealed class IViewForAttribute(string? viewModelType) : Attribute;
#pragma warning restore CS9113 // Parameter is unread.
