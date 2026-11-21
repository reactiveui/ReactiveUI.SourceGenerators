// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.SourceGenerators.Helpers;

namespace ReactiveUI.SourceGenerators.Models;

/// <summary>Describes compile-time Android control wire-up for a class marked with <c>[WireUpControls]</c>.</summary>
/// <param name="TargetInfo">The target type metadata.</param>
/// <param name="IsActivity">Whether the target class derives from <c>Android.App.Activity</c>.</param>
/// <param name="IsView">Whether the target class derives from <c>Android.Views.View</c>.</param>
/// <param name="Controls">The properties to wire.</param>
internal sealed record WireUpControlsInfo(
    TargetInfo TargetInfo,
    bool IsActivity,
    bool IsView,
    EquatableArray<WiredControlInfo> Controls);
