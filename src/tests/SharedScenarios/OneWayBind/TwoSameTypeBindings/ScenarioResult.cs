// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding;

namespace SharedScenarios.OneWayBind.TwoSameTypeBindings;

/// <summary>What this scenario hands back to the test that runs it.</summary>
/// <param name="First">The scenario's First result.</param>
/// <param name="Last">The scenario's Last result.</param>
public sealed record ScenarioResult(
    IReactiveBinding<MyView, string> First,
    IReactiveBinding<MyView, string> Last);
