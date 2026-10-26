// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.Tracing;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
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
    /// <returns>A task representing the run.</returns>
    internal static async Task Main(string[] args)
    {
        if (args is ["--eventpipe", var outputDirectory])
        {
            // Allocation figures: GCAllocationTick events from an EventPipe session over a fixed number of operations.
            _ = await EventPipeAllocations.MeasureAsync(RuntimeScenarios.Create(), outputDirectory, CancellationToken.None).ConfigureAwait(false);
            return;
        }

        _ = await BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).RunAsync(args, CreateConfig(), CancellationToken.None).ConfigureAwait(false);
    }

    /// <summary>Creates the run configuration.</summary>
    /// <returns>An EventPipe trace per benchmark unless profiling is turned off; allocation figures come from its GC events.</returns>
    private static ManualConfig CreateConfig()
    {
        // Built under obj rather than in a .bdn folder beside the project, which git would otherwise pick up.
        var config = ManualConfig.Create(DefaultConfig.Instance).AddJob(Job.Default.WithToolchain(ObjFolderToolchain.Instance).AsDefault());
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
