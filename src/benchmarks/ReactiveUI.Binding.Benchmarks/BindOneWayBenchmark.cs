// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Attributes;
using ReactiveUI.Binding.Benchmarks.Mocks;
using ReactiveUI.Primitives.Concurrency;

namespace ReactiveUI.Binding.Benchmarks;

/// <summary>Source-generated BindOneWay benchmarks with and without scheduler.</summary>
public class BindOneWayBenchmark
{
    /// <summary>Represents the number of property change events to be triggered during the benchmark tests.</summary>
    private const int PropertyChangeCount = 1_000;

    /// <summary>The number of consecutive bindings created in the setup/teardown benchmark.</summary>
    private const int BindingCount = 10;

    /// <summary>The source view model of the benchmark.</summary>
    private BenchmarkViewModel _source = null!;

    /// <summary>The target view of the benchmark.</summary>
    private BenchmarkView _target = null!;

    /// <summary>Sets up fresh source and target objects before each benchmark iteration.</summary>
    [IterationSetup]
    public void Setup()
    {
        _source = new() { Name = "Initial", Age = 0 };
        _target = new();
    }

    /// <summary>One-way binding without scheduler: setup, fire N changes, dispose.</summary>
    [Benchmark(Description = "BindOneWay")]
    public void Standard()
    {
        using var binding = _source.BindOneWay(_target, x => x.Name, x => x.DisplayName);

        for (var i = 0; i < PropertyChangeCount; i++)
        {
            _source.Name = $"Name_{i}";
        }
    }

    /// <summary>One-way binding with ImmediateSequencer: measures the scheduler-carrying overload's overhead.</summary>
    [Benchmark(Description = "BindOneWay + Scheduler")]
    public void WithScheduler()
    {
        using var binding = _source.BindOneWay(_target, x => x.Name, x => x.DisplayName, ImmediateSequencer.Instance);

        for (var i = 0; i < PropertyChangeCount; i++)
        {
            _source.Name = $"Name_{i}";
        }
    }

    /// <summary>Cold start: create binding, dispose immediately. No property changes.</summary>
    [Benchmark(Description = "First Binding")]
    public void FirstBinding()
    {
        using var binding = _source.BindOneWay(_target, x => x.Name, x => x.DisplayName);
    }

    /// <summary>Setup and teardown cost of 10 consecutive bindings.</summary>
    [Benchmark(Description = "10x Setup/Teardown")]
    public void SetupTeardown()
    {
        var bindings = new IDisposable[BindingCount];

        for (var i = 0; i < BindingCount; i++)
        {
            _source.Name = $"VM_{i}";
            bindings[i] = _source.BindOneWay(_target, x => x.Name, x => x.DisplayName);
        }

        for (var i = 0; i < BindingCount; i++)
        {
            bindings[i].Dispose();
        }
    }
}
