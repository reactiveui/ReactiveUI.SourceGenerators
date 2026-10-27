// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Attributes;
using ReactiveUI.Binding.Benchmarks.Mocks;

namespace ReactiveUI.Binding.Benchmarks;

/// <summary>Source-generated BindTwoWay benchmarks through a sequencer that queues every write, from sides that start equal.</summary>
public class BindTwoWayQueuedBenchmark
{
    /// <summary>The value both sides start with.</summary>
    private const string StartValue = "Initial";

    /// <summary>Represents the number of property change events to be triggered during the benchmark tests.</summary>
    private const int PropertyChangeCount = 1_000;

    /// <summary>The source view model.</summary>
    private BenchmarkViewModel _source = null!;

    /// <summary>The target view.</summary>
    private BenchmarkView _target = null!;

    /// <summary>The sequencer that queues every write.</summary>
    private QueuingSequencer _sequencer = null!;

    /// <summary>Sets up fresh source, target and sequencer objects before each benchmark iteration.</summary>
    [IterationSetup]
    public void Setup()
    {
        _source = new() { Name = StartValue, Age = 0 };
        _target = new() { DisplayName = StartValue };
        _sequencer = new();
    }

    /// <summary>The initial delivery: create the binding and run every write it queues.</summary>
    [Benchmark(Description = "Queued BindTwoWay initial delivery")]
    public void InitialDelivery()
    {
        using var binding = _source.BindTwoWay(_target, x => x.Name, x => x.DisplayName, _sequencer);

        _sequencer.Drain();
    }

    /// <summary>Model to view: change the source a thousand times, running the queue after each change.</summary>
    [Benchmark(Description = "Queued BindTwoWay model to view")]
    public void ModelToView()
    {
        using var binding = _source.BindTwoWay(_target, x => x.Name, x => x.DisplayName, _sequencer);
        _sequencer.Drain();

        for (var i = 0; i < PropertyChangeCount; i++)
        {
            _source.Name = $"Name_{i}";
            _sequencer.Drain();
        }
    }

    /// <summary>Model to view in bursts: change the source twice, then run the queue, a thousand times.</summary>
    [Benchmark(Description = "Queued BindTwoWay model to view burst")]
    public void ModelToViewBurst()
    {
        using var binding = _source.BindTwoWay(_target, x => x.Name, x => x.DisplayName, _sequencer);
        _sequencer.Drain();

        for (var i = 0; i < PropertyChangeCount; i++)
        {
            _source.Name = $"First_{i}";
            _source.Name = $"Second_{i}";
            _sequencer.Drain();
        }
    }

    /// <summary>View to model: change the target a thousand times, running the queue after each change.</summary>
    [Benchmark(Description = "Queued BindTwoWay view to model")]
    public void ViewToModel()
    {
        using var binding = _source.BindTwoWay(_target, x => x.Name, x => x.DisplayName, _sequencer);
        _sequencer.Drain();

        for (var i = 0; i < PropertyChangeCount; i++)
        {
            _target.DisplayName = $"Text_{i}";
            _sequencer.Drain();
        }
    }
}
