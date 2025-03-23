// Copyright (c) 2025 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.SourceGenerators.Helpers;
using ReactiveUI.SourceGenerators.Models;

namespace ReactiveUI.SourceGenerators.Input.Models;

/// <summary>
/// A model with gathered info on a given command method.
/// </summary>
internal sealed record IViewForInfo(
    TargetInfo TargetInfo,
    string ViewModelTypeName,
    IViewForBaseType BaseType);
