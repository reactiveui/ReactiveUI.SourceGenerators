// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Reports;

namespace ReactiveUI.Binding.Benchmarks.Configs;

/// <summary>Rejects benchmark reports with failed processes or missing workload measurements.</summary>
public static class BenchmarkRunValidation
{
    /// <summary>Validates every scheduled case and returns the number actually measured.</summary>
    /// <param name="summaries">The completed benchmark runs.</param>
    /// <returns>The number of valid reports.</returns>
    /// <exception cref="InvalidOperationException">A run failed or did not measure all scheduled cases.</exception>
    public static int CountVerified(IEnumerable<Summary> summaries)
    {
        var count = 0;
        foreach (var summary in summaries)
        {
            if (summary.HasCriticalValidationErrors || summary.Reports.Length != summary.BenchmarksCases.Length)
            {
                throw new InvalidOperationException($"Benchmark validation failed: {summary.Title}");
            }

            foreach (var report in summary.Reports)
            {
                ValidateReport(report);
                count++;
            }
        }

        BenchmarkDotNet.Loggers.ConsoleLogger.Default.WriteLine(BenchmarkDotNet.Loggers.LogKind.Info, $"Verified benchmark cases: {count}.");
        return count;
    }

    /// <summary>Allows informational CLI commands while rejecting an empty measurement run.</summary>
    /// <param name="count">The number of verified cases.</param>
    /// <param name="args">The CLI arguments.</param>
    /// <exception cref="InvalidOperationException">No benchmark was measured.</exception>
    public static void RequireMeasurements(int count, string[] args)
    {
        if (count != 0 || Array.IndexOf(args, "--help") >= 0 || Array.IndexOf(args, "--list") >= 0 || Array.IndexOf(args, "--info") >= 0)
        {
            return;
        }

        throw new InvalidOperationException("No benchmark cases produced verified measurements.");
    }

    /// <summary>Requires a positive finite workload result rather than only warmup or overhead samples.</summary>
    /// <param name="measurements">The measurements from one benchmark.</param>
    /// <returns>True when actual workload results exist.</returns>
    internal static bool HasWorkloadResult(IReadOnlyList<Measurement> measurements)
    {
        foreach (var measurement in measurements)
        {
            if (measurement.IterationMode == IterationMode.Workload && measurement.IterationStage == IterationStage.Result
                && measurement.Operations > 0 && measurement.Nanoseconds > 0
                && !double.IsNaN(measurement.Nanoseconds) && !double.IsInfinity(measurement.Nanoseconds))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Checks both process outcomes and the data underlying a reported mean.</summary>
    /// <param name="report">One scheduled benchmark case.</param>
    /// <exception cref="InvalidOperationException">The benchmark did not produce valid measurements.</exception>
    private static void ValidateReport(BenchmarkReport report)
    {
        if (!report.Success || report.ResultStatistics is null || report.ExecuteResults.Count == 0)
        {
            throw new InvalidOperationException($"Missing benchmark measurements: {report.BenchmarkCase.DisplayInfo}");
        }

        foreach (var execution in report.ExecuteResults)
        {
            if (!execution.IsSuccess || execution.ExitCode != 0 || execution.Errors.Count != 0)
            {
                throw new InvalidOperationException($"Benchmark process failed: {report.BenchmarkCase.DisplayInfo}");
            }
        }

        if (!HasWorkloadResult(report.AllMeasurements))
        {
            throw new InvalidOperationException($"No workload results: {report.BenchmarkCase.DisplayInfo}");
        }
    }
}
