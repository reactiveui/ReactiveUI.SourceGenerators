// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.Tracing;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;
using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Tracing.Parsers;
using ReactiveUI.SourceGenerators.Benchmarks.Shared;

namespace ReactiveUI.SourceGenerators.Runtime.Benchmarks;

/// <summary>Entry point that hands the command line to the BenchmarkDotNet switcher.</summary>
internal static class Program
{
    /// <summary>The environment variable that turns the EventPipe profiler off when set to <c>false</c>.</summary>
    private const string ProfilersVariable = "BENCHMARK_PROFILERS";

    /// <summary>Runs the benchmarks the command line selects, or <c>--eventpipe dir</c> to trace allocations.</summary>
    /// <param name="args">The command-line arguments.</param>
    internal static void Main(string[] args)
    {
        if (args is ["--eventpipe", var outputDirectory])
        {
            // Allocation figures: GCAllocationTick events from an EventPipe session over a fixed number of operations.
            _ = EventPipeAllocations.Measure(RuntimeScenarios.Create(), outputDirectory);
            return;
        }

        _ = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, CreateConfig());
    }

    /// <summary>Creates the run configuration.</summary>
    /// <returns>An EventPipe trace per benchmark unless profiling is turned off; allocation figures come from its GC events.</returns>
    private static ManualConfig CreateConfig()
    {
        var config = ManualConfig.Create(DefaultConfig.Instance);
        if (string.Equals(Environment.GetEnvironmentVariable(ProfilersVariable), "false", StringComparison.OrdinalIgnoreCase))
        {
            return config;
        }

        EventPipeProvider[] providers =
        [
            new("Microsoft-DotNETCore-SampleProfiler", EventLevel.Informational),
            new(ClrTraceEventParser.ProviderName, EventLevel.Verbose, (long)(ClrTraceEventParser.Keywords.Default | ClrTraceEventParser.Keywords.GCHandle)),
        ];

        return config.AddDiagnoser(new EventPipeProfiler(EventPipeProfile.GcVerbose, providers));
    }
}
