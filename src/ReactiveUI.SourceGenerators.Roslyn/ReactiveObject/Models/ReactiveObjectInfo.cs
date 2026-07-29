// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.SourceGenerators.Models;

/// <summary>A model with gathered information about a generated ReactiveObject (view model).</summary>
/// <param name="TargetInfo">The target type that owns the generated ReactiveObject members.</param>
internal sealed record ReactiveObjectInfo(
    TargetInfo TargetInfo);
