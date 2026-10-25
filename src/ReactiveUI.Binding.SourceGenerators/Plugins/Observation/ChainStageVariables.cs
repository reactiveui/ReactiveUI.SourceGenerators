// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

/// <summary>The locals one stage of a generated observation chain reads from and writes to.</summary>
/// <param name="PreviousObservable">The local holding the stage before this one.</param>
/// <param name="CurrentObservable">The local this stage is assigned to.</param>
/// <param name="ParentParameter">The name the switch lambda gives the parent value.</param>
/// <remarks>
/// The three travel together through every mechanism's stage emission and mean nothing apart, so they are
/// named once here rather than widening each emitter's parameter list by three.
/// </remarks>
internal readonly record struct ChainStageVariables(
    string PreviousObservable,
    string CurrentObservable,
    string ParentParameter);
