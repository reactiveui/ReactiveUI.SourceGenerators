// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using ReactiveUI.Binding;

namespace SharedScenarios.BindOneWay.MultipleBindings;

/// <summary>What this scenario hands back to the test that runs it.</summary>
/// <param name="NameBinding">The scenario's NameBinding result.</param>
/// <param name="AgeBinding">The scenario's AgeBinding result.</param>
public sealed record ScenarioResult(
    IDisposable NameBinding,
    IDisposable AgeBinding);
