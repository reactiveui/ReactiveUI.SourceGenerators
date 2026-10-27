// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using ReactiveUI.Binding;

namespace SharedScenarios.WhenAny.MultipleInvocationsSameType;

/// <summary>What this scenario hands back to the test that runs it.</summary>
/// <param name="FirstObs">The scenario's FirstObs result.</param>
/// <param name="LastObs">The scenario's LastObs result.</param>
public sealed record ScenarioResult(
    IObservable<string> FirstObs,
    IObservable<string> LastObs);
