// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.Tracing;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;
using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Tracing.Parsers;
using ReactiveUI.SourceGenerators.Benchmarks.Support;

namespace ReactiveUI.SourceGenerators.Benchmarks;

/// <summary>Entry point that hands the command line to the BenchmarkDotNet switcher.</summary>
internal static class Program
{
    /// <summary>The environment variable that turns the EventPipe profiler off when set to <c>false</c>.</summary>
    private const string ProfilersVariable = "BENCHMARK_PROFILERS";

    /// <summary>Runs the benchmarks the command line selects, or <c>--smoke</c> to run each corpus once.</summary>
    /// <param name="args">The command-line arguments.</param>
    internal static void Main(string[] args)
    {
        if (args is ["--smoke"])
        {
            // Runs each corpus once outside BenchmarkDotNet, to prove the mocks still generate before a long run.
            foreach (var corpus in GeneratorCatalog.Corpora)
            {
                var benchmark = new GenerationBenchmarks { Corpus = corpus };
                benchmark.Setup();
                Console.WriteLine($"{corpus}: {benchmark.Generate():N0} characters, {GeneratorHarness.CountErrors(corpus)} compile errors");
            }

            return;
        }

        _ = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, CreateConfig());
    }

    /// <summary>Creates the run configuration.</summary>
    /// <returns>The memory diagnoser, plus an EventPipe trace per benchmark unless profiling is turned off.</returns>
    /// <remarks>
    /// The trace carries verbose GC events, whose AllocationTick events name the allocated type and its call stack,
    /// and CPU samples. The traces land in BenchmarkDotNet.Artifacts and are summarised with nettrace-analyzer.cs.
    /// </remarks>
    private static ManualConfig CreateConfig()
    {
        var config = ManualConfig.Create(DefaultConfig.Instance).AddDiagnoser(MemoryDiagnoser.Default);
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
