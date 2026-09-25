// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.Tracing;
using System.Globalization;
using System.Text;
using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;

namespace ReactiveUI.SourceGenerators.Benchmarks.Shared;

/// <summary>Measures allocations per operation from an EventPipe trace of the running process.</summary>
/// <remarks>
/// <para>
/// Each scenario runs a fixed number of operations inside an EventPipe session on this process with the runtime
/// provider's GC keyword at verbose level, which raises a <c>GCAllocationTick</c> event roughly every 100 KB allocated,
/// carrying the exact amount, the allocated type and the call stack. The sum of those amounts divided by the operation
/// count is the allocation per operation; its sampling error is one tick (about 100 KB) over the whole session, so
/// sessions run enough operations to allocate hundreds of megabytes.
/// </para>
/// <para>
/// The trace of each scenario is kept, so the allocated types and the frames that allocated them can be broken down
/// afterwards in PerfView. The Jit and Loader keywords are on so the frames resolve to methods.
/// </para>
/// </remarks>
public static class EventPipeAllocations
{
    /// <summary>The runtime keywords the session enables: GC for the allocation ticks, and what resolves their stacks.</summary>
    private const ClrTraceEventParser.Keywords SessionKeywords =
        ClrTraceEventParser.Keywords.GC | ClrTraceEventParser.Keywords.Jit | ClrTraceEventParser.Keywords.Loader
        | ClrTraceEventParser.Keywords.Stack;

    /// <summary>The number of untraced runs before a session, so JIT and first-use caches are out of the figure.</summary>
    private const int WarmupOperations = 5;

    /// <summary>Traces each scenario and writes a markdown table of the allocations per operation.</summary>
    /// <param name="scenarios">The scenarios to measure.</param>
    /// <param name="outputDirectory">The directory the traces and the table are written to.</param>
    /// <returns>The table, also written to <c>allocations.md</c> in <paramref name="outputDirectory"/>.</returns>
    public static string Measure(IReadOnlyList<AllocationScenario> scenarios, string outputDirectory)
    {
        ArgumentNullException.ThrowIfNull(scenarios);
        _ = Directory.CreateDirectory(outputDirectory);

        var table = new StringBuilder()
            .AppendLine("| Scenario | Operations | Sampled bytes per operation | Allocation ticks |")
            .AppendLine("| --- | ---: | ---: | ---: |");

        foreach (var scenario in scenarios)
        {
            var result = Measure(scenario, Path.Combine(outputDirectory, $"{scenario.Name}.nettrace"));
            _ = table.Append("| ").Append(scenario.Name)
                .Append(" | ").Append(scenario.Operations.ToString("N0", CultureInfo.InvariantCulture))
                .Append(" | ").Append((result.Bytes / scenario.Operations).ToString("N0", CultureInfo.InvariantCulture))
                .Append(" | ").Append(result.Ticks.ToString("N0", CultureInfo.InvariantCulture))
                .AppendLine(" |");
            Console.WriteLine($"{scenario.Name}: {result.Bytes / scenario.Operations:N0} bytes/op ({result.Ticks:N0} ticks)");
        }

        var text = table.ToString();
        File.WriteAllText(Path.Combine(outputDirectory, "allocations.md"), text);
        return text;
    }

    /// <summary>Traces one scenario.</summary>
    /// <param name="scenario">The scenario.</param>
    /// <param name="tracePath">The file the trace is written to.</param>
    /// <returns>The sampled bytes and the number of allocation ticks.</returns>
    private static (long Bytes, long Ticks) Measure(AllocationScenario scenario, string tracePath)
    {
        scenario.Setup();
        var sink = 0L;
        for (var i = 0; i < WarmupOperations; i++)
        {
            sink += scenario.Operation();
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var client = new DiagnosticsClient(Environment.ProcessId);
        EventPipeProvider[] providers = [new(ClrTraceEventParser.ProviderName, EventLevel.Verbose, (long)SessionKeywords)];
        using (var session = client.StartEventPipeSession(providers, requestRundown: true, circularBufferMB: 1024))
        {
            var copy = Task.Run(() =>
            {
                using var file = File.Create(tracePath);
                session.EventStream.CopyTo(file);
            });

            for (var i = 0; i < scenario.Operations; i++)
            {
                sink += scenario.Operation();
            }

            session.Stop();
            copy.Wait();
        }

        GC.KeepAlive(sink);
        return SumAllocationTicks(tracePath, Environment.ProcessId);
    }

    /// <summary>Sums the allocation ticks a process raised in a trace.</summary>
    /// <param name="tracePath">The trace.</param>
    /// <param name="processId">The process whose ticks count.</param>
    /// <returns>The sampled bytes and the number of ticks.</returns>
    private static (long Bytes, long Ticks) SumAllocationTicks(string tracePath, int processId)
    {
        long bytes = 0;
        long ticks = 0;
        using var source = new EventPipeEventSource(tracePath);
        source.Clr.GCAllocationTick += allocation =>
        {
            if (allocation.ProcessID != processId)
            {
                return;
            }

            bytes += allocation.AllocationAmount64;
            ticks++;
        };

        _ = source.Process();
        return (bytes, ticks);
    }
}
