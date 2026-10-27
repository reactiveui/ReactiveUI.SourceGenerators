// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using BenchmarkDotNet.Running;

namespace ReactiveUI.Binding.Benchmarks.Configs;

/// <summary>Runs the selected .NET 10 and 11 benchmarks with EventPipe.</summary>
public static class BenchmarkHost
{
    /// <summary>Runs the benchmarks the command line selects.</summary>
    /// <param name="assembly">The assembly holding the benchmarks.</param>
    /// <param name="args">The command line arguments passed to the benchmark switcher.</param>
    public static void Run(Assembly assembly, string[] args)
    {
        var switcher = BenchmarkSwitcher.FromAssembly(assembly);
        var count = BenchmarkRunValidation.CountVerified(switcher.Run(args, new BenchmarkConfig()));
        count += BenchmarkRunValidation.CountVerified(switcher.Run(args, new NativeAotMemoryConfig()));
        BenchmarkRunValidation.RequireMeasurements(count, args);
    }
}
