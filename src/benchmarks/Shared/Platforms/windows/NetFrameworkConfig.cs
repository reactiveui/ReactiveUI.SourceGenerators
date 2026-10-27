// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Diagnostics.Windows;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;

namespace ReactiveUI.Binding.Benchmarks.Configs;

/// <summary>Runs benchmarks on .NET Framework 4.6.2 with the memory diagnoser, traced with ETW.</summary>
/// <remarks>EventPipe cannot attach to .NET Framework, and a run keeps one set of profilers, so this runs apart from <see cref="BenchmarkConfig"/>.</remarks>
public class NetFrameworkConfig : ManualConfig
{
    /// <summary>Initializes a new instance of the <see cref="NetFrameworkConfig"/> class.</summary>
    public NetFrameworkConfig()
    {
        Add(DefaultConfig.Instance);
        _ = AddJob(new Job(nameof(RuntimeMoniker.Net462)).WithRuntime(ClrRuntime.Net462));
        _ = AddDiagnoser(MemoryDiagnoser.Default);
        _ = AddColumn(CategoriesColumn.Default);
        _ = AddExporter(MarkdownExporter.GitHub);
        _ = WithOption(ConfigOptions.DontOverwriteResults, true);

        if (BenchmarkProfiling.Enabled)
        {
            _ = AddDiagnoser(new EtwProfiler(new EtwProfilerConfig()));
        }
    }
}
