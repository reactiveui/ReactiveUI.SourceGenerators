// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Benchmarks.Configs;

/// <summary>Reads whether the benchmark configs attach their profilers.</summary>
/// <remarks>
/// An A/B comparison measures timing only. It sets <c>BENCHMARK_PROFILERS</c> to <c>false</c>, which skips the
/// second pass each profiler adds to every benchmark.
/// </remarks>
public static class BenchmarkProfiling
{
    /// <summary>The environment variable that turns the profilers off when set to <c>false</c>.</summary>
    private const string VariableName = "BENCHMARK_PROFILERS";

    /// <summary>Gets a value indicating whether the profilers are attached.</summary>
    public static bool Enabled =>
        !string.Equals(Environment.GetEnvironmentVariable(VariableName), "false", StringComparison.OrdinalIgnoreCase);
}
