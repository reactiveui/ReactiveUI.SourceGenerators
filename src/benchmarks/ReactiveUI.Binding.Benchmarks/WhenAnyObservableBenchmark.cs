// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Attributes;
using ReactiveUI.Binding.Benchmarks.Mocks;

namespace ReactiveUI.Binding.Benchmarks;

/// <summary>Source-generated WhenAnyObservable benchmarks, which switch to whichever stream a property holds.</summary>
public class WhenAnyObservableBenchmark
{
    /// <summary>How many values each benchmark pushes through the observed stream.</summary>
    private const int ValueCount = 1_000;

    /// <summary>The view model holding the observed streams.</summary>
    private BenchmarkViewModel _vm = null!;

    /// <summary>The stream the view model's first property holds.</summary>
    private BenchmarkSource<string> _first = null!;

    /// <summary>The stream the view model's second property holds.</summary>
    private BenchmarkSource<string> _second = null!;

    /// <summary>Builds a fresh view model and streams before each iteration.</summary>
    [IterationSetup]
    public void Setup()
    {
        _first = new();
        _second = new();
        _vm = new() { Name = "Initial", Signal = _first, OtherSignal = _second };
    }

    /// <summary>One stream, driven through the property that holds it.</summary>
    [Benchmark(Description = "Single Stream")]
    public void SingleStream()
    {
        var last = string.Empty;
        using var sub = _vm.WhenAnyObservable(x => x.Signal!)
            .Subscribe(v => last = v);

        for (var i = 0; i < ValueCount; i++)
        {
            _first.Push($"Value_{i}");
        }
    }

    /// <summary>Two streams, merged.</summary>
    [Benchmark(Description = "Two Streams")]
    public void TwoStreams()
    {
        var last = string.Empty;
        using var sub = _vm.WhenAnyObservable(x => x.Signal!, x => x.OtherSignal!)
            .Subscribe(v => last = v);

        for (var i = 0; i < ValueCount; i++)
        {
            _first.Push($"First_{i}");
            _second.Push($"Second_{i}");
        }
    }

    /// <summary>Subscribing to the stream a property holds, with no values pushed.</summary>
    [Benchmark(Description = "First Observation")]
    public void FirstObservation()
    {
        var result = string.Empty;
        using var sub = _vm.WhenAnyObservable(x => x.Signal!)
            .Subscribe(v => result = v);
    }
}
