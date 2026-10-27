// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using ReactiveUI.Binding.Benchmarks.Mocks;
using ReactiveUI.Primitives.Concurrency;

namespace ReactiveUI.Binding.Benchmarks;

/// <summary>Source-generated BindTwoWay benchmarks with and without scheduler.</summary>
public class BindTwoWayBenchmark
{
    /// <summary>Represents the number of property change events to be triggered during the benchmark tests.</summary>
    private const int PropertyChangeCount = 1_000;

    /// <summary>The modulus used to alternate updates between the source and the target each iteration.</summary>
    private const int AlternationModulus = 2;

    /// <summary>The source and target view models used for binding benchmarks.</summary>
    private BenchmarkViewModel _source = null!;

    /// <summary>The target view used for binding benchmarks.</summary>
    private BenchmarkView _target = null!;

    /// <summary>Sets up fresh source and target objects before each benchmark iteration.</summary>
    [IterationSetup]
    public void Setup()
    {
        _source = new() { Name = "Initial", Age = 0 };
        _target = new();
    }

    /// <summary>Two-way binding without scheduler: setup, fire N source changes, dispose.</summary>
    [Benchmark(Description = "BindTwoWay")]
    public void Standard()
    {
        using var binding = _source.BindTwoWay(_target, x => x.Name, x => x.DisplayName);

        for (var i = 0; i < PropertyChangeCount; i++)
        {
            _source.Name = $"Name_{i}";
        }
    }

    /// <summary>Two-way binding with ImmediateSequencer: measures the scheduler-carrying overload's overhead on both directions.</summary>
    [Benchmark(Description = "BindTwoWay + Scheduler")]
    public void WithScheduler()
    {
        using var binding = _source.BindTwoWay(_target, x => x.Name, x => x.DisplayName, ImmediateSequencer.Instance);

        for (var i = 0; i < PropertyChangeCount; i++)
        {
            _source.Name = $"Name_{i}";
        }
    }

    /// <summary>Two-way binding with alternating source and target changes.</summary>
    [Benchmark(Description = "Bidirectional")]
    public void Bidirectional()
    {
        using var binding = _source.BindTwoWay(_target, x => x.Name, x => x.DisplayName);

        for (var i = 0; i < PropertyChangeCount; i++)
        {
            if (i % AlternationModulus == 0)
            {
                _source.Name = $"FromSource_{i}";
            }
            else
            {
                _target.DisplayName = $"FromTarget_{i}";
            }
        }
    }

    /// <summary>
    /// Observes changes to the <see cref="BenchmarkView.DisplayName"/> property using ReactiveUI's
    /// expression-tree-based binding APIs. This method is essential for setting up the reverse
    /// observation path required in bi-directional bindings when benchmarking BindTwoWay.
    /// </summary>
    /// <param name="view">The instance of <see cref="BenchmarkView"/> whose <see cref="BenchmarkView.DisplayName"/> property changes are observed.</param>
    /// <returns>An observable sequence of <see cref="string"/> values that represent the changes to the <see cref="BenchmarkView.DisplayName"/> property.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static IObservable<string> TriggerViewGeneration(BenchmarkView view) =>
        view.WhenChanged(x => x.DisplayName);
}
