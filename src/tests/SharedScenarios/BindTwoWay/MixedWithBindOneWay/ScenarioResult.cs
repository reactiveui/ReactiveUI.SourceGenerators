// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using ReactiveUI.Binding;

namespace SharedScenarios.BindTwoWay.MixedWithBindOneWay;

/// <summary>What this scenario hands back to the test that runs it.</summary>
/// <param name="TwoWay">The scenario's TwoWay result.</param>
/// <param name="OneWay">The scenario's OneWay result.</param>
public sealed record ScenarioResult(
    IDisposable TwoWay,
    IDisposable OneWay);
