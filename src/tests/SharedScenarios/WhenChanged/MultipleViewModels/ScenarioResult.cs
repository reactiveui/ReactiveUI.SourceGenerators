// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using ReactiveUI.Binding;

namespace SharedScenarios.WhenChanged.MultipleViewModels;

/// <summary>What this scenario hands back to the test that runs it.</summary>
/// <param name="NameObs">The scenario's NameObs result.</param>
/// <param name="CountObs">The scenario's CountObs result.</param>
public sealed record ScenarioResult(
    IObservable<string> NameObs,
    IObservable<int> CountObs);
