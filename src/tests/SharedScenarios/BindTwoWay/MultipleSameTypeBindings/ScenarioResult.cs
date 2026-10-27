// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using ReactiveUI.Binding;

namespace SharedScenarios.BindTwoWay.MultipleSameTypeBindings;

/// <summary>What this scenario hands back to the test that runs it.</summary>
/// <param name="FirstBinding">The scenario's FirstBinding result.</param>
/// <param name="LastBinding">The scenario's LastBinding result.</param>
public sealed record ScenarioResult(
    IDisposable FirstBinding,
    IDisposable LastBinding);
