﻿// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

/// <summary>
/// Initializes the source generator verifiers.
/// </summary>
public static class ModuleInitializer
{
    /// <summary>
    /// Initializes the source generators.
    /// </summary>
    [ModuleInitializer]
    public static void Init() => VerifySourceGenerators.Initialize();
}
