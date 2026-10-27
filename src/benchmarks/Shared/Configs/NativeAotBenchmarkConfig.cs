// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;

namespace ReactiveUI.Binding.Benchmarks.Configs;

/// <summary>Adds the NativeAOT 10 and 11 runtimes.</summary>
public class NativeAotBenchmarkConfig : ManualConfig
{
    /// <summary>Initializes a new instance of the <see cref="NativeAotBenchmarkConfig"/> class.</summary>
    public NativeAotBenchmarkConfig()
    {
        _ = AddJob(new Job(nameof(RuntimeMoniker.NativeAot10_0)).WithRuntime(NativeAotRuntime.Net10_0));
        _ = AddJob(new Job(nameof(RuntimeMoniker.NativeAot11_0)).WithRuntime(NativeAotRuntime.Net11_0));
    }
}
