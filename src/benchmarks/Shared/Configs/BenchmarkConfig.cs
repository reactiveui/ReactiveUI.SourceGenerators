// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;

namespace ReactiveUI.Binding.Benchmarks.Configs;

/// <summary>Runs benchmarks on .NET 10 and 11, with the memory diagnoser and an EventPipe trace.</summary>
public class BenchmarkConfig : ProfilerConfig
{
    /// <summary>Initializes a new instance of the <see cref="BenchmarkConfig"/> class.</summary>
    public BenchmarkConfig()
    {
        _ = AddJob(new Job().WithRuntime(CoreRuntime.Core10_0));
        _ = AddJob(new Job().WithRuntime(CoreRuntime.Core11_0));
        _ = AddExporter(MarkdownExporter.GitHub);
    }
}
