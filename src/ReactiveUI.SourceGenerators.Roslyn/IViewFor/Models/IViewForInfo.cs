// Copyright (c) 2026 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.SourceGenerators.Models;

namespace ReactiveUI.SourceGenerators.Input.Models;

/// <summary>
/// A model with gathered info on a given command method.
/// </summary>
internal sealed record IViewForInfo(
    TargetInfo TargetInfo,
    string ViewModelTypeName,
    IViewForBaseType BaseType,
    string SplatRegistrationType,
    string SplatViewModelRegistrationType);
