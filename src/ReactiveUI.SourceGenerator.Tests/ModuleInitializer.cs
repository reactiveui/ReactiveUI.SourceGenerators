﻿// Copyright (c) 2025 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
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
