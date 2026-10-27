// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Filters;

namespace ReactiveUI.Binding.Benchmarks.Configs;

/// <summary>Runs benchmarks on NativeAOT 10 and 11 with the memory diagnoser.</summary>
/// <remarks>
/// EventPipe cannot attach to a NativeAOT process built with the default settings, and a run keeps one set of
/// diagnosers, so these jobs run apart from <see cref="BenchmarkConfig"/>. A benchmark type marked
/// <c>[RequiresUnreferencedCode]</c> or <c>[RequiresDynamicCode]</c> measures a reflection path that an ahead-of-time
/// publish cannot keep, so the filter leaves it out. The attributes are matched by name, because .NET Framework
/// builds see only the polyfill copies.
/// </remarks>
public class NativeAotMemoryConfig : ManualConfig
{
    /// <summary>Initializes a new instance of the <see cref="NativeAotMemoryConfig"/> class.</summary>
    public NativeAotMemoryConfig()
    {
        Add(DefaultConfig.Instance);
        Add(new NativeAotBenchmarkConfig());
        _ = AddDiagnoser(MemoryDiagnoser.Default);
        _ = AddColumn(CategoriesColumn.Default);
        _ = AddExporter(MarkdownExporter.GitHub);
        _ = AddFilter(new SimpleFilter(static benchmark => PublishesAheadOfTime(benchmark.Descriptor.Type)));
        _ = WithOption(ConfigOptions.DontOverwriteResults, true);
    }

    /// <summary>Gets a value indicating whether a benchmark type's code can publish ahead of time.</summary>
    /// <param name="type">The benchmark type.</param>
    /// <returns><see langword="true"/> unless the type is marked as needing unreferenced or dynamic code.</returns>
    private static bool PublishesAheadOfTime(Type type)
    {
        foreach (var attribute in type.GetCustomAttributesData())
        {
            if (attribute.AttributeType.Name is "RequiresUnreferencedCodeAttribute" or "RequiresDynamicCodeAttribute")
            {
                return false;
            }
        }

        return true;
    }
}
