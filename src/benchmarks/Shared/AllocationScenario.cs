// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.SourceGenerators.Benchmarks.Shared;

/// <summary>One measured operation and how often to run it.</summary>
/// <param name="Name">The scenario name, also the trace's file name.</param>
/// <param name="Operations">How many operations the traced session runs.</param>
/// <param name="Setup">Prepares the scenario before its warmup.</param>
/// <param name="Operation">One operation; its result is kept so the work cannot be optimized away.</param>
public sealed record AllocationScenario(string Name, int Operations, Action Setup, Func<long> Operation);
