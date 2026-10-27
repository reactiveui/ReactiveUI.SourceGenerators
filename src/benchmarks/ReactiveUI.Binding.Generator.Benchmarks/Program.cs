// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Running;
using ReactiveUI.Binding.Benchmarks.Configs;

namespace ReactiveUI.Binding.Generator.Benchmarks;

/// <summary>Entry point that hands the command line to the BenchmarkDotNet switcher.</summary>
internal static class Program
{
    /// <summary>Runs the benchmark selected by the command line.</summary>
    /// <param name="args">The command-line arguments passed to the switcher.</param>
    [STAThread]
    internal static void Main(string[] args)
    {
        var count = BenchmarkRunValidation.CountVerified(BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, new ProfilerConfig()));
        BenchmarkRunValidation.RequireMeasurements(count, args);
    }
}
