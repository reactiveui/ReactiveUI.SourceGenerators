// Copyright (c) 2026 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.SourceGenerators.Models;

/// <summary>
/// A model with gathered information about a generated ReactiveObject (view model).
/// </summary>
internal sealed record ReactiveObjectInfo(
    TargetInfo TargetInfo);
