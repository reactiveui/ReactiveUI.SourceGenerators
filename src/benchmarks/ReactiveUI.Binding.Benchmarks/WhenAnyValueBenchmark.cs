// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Attributes;
using ReactiveUI.Binding.Benchmarks.Mocks;

namespace ReactiveUI.Binding.Benchmarks;

/// <summary>Source-generated WhenAnyValue benchmarks, which read against the ReactiveUI baseline of the same name.</summary>
public class WhenAnyValueBenchmark
{
    /// <summary>How many property changes each benchmark drives through one subscription.</summary>
    private const int PropertyChangeCount = 1_000;

    /// <summary>The view model under observation.</summary>
    private BenchmarkViewModel _vm = null!;

    /// <summary>Builds a fresh view model before each iteration.</summary>
    [IterationSetup]
    public void Setup() =>
        _vm = new() { Name = "Initial", Age = 0, Child = new() { Value = "ChildInitial" } };

    /// <summary>One property.</summary>
    [Benchmark(Description = "Single Property")]
    public void SingleProperty()
    {
        var last = string.Empty;
        using var sub = _vm.WhenAnyValue(x => x.Name)
            .Subscribe(v => last = v);

        for (var i = 0; i < PropertyChangeCount; i++)
        {
            _vm.Name = $"Name_{i}";
        }
    }

    /// <summary>A two-link chain, which re-subscribes when the intermediate object is replaced.</summary>
    [Benchmark(Description = "Deep Chain")]
    public void DeepChain()
    {
        var last = string.Empty;
        using var sub = _vm.WhenAnyValue(x => x.Child.Value)
            .Subscribe(v => last = v);

        for (var i = 0; i < PropertyChangeCount; i++)
        {
            _vm.Child.Value = $"Value_{i}";
        }
    }

    /// <summary>Two properties, combined.</summary>
    [Benchmark(Description = "Two Properties")]
    public void TwoProperties()
    {
        PropertyValues<string, int> last = default;
        using var sub = _vm.WhenAnyValue(x => x.Name, x => x.Age)
            .Subscribe(v => last = v);

        for (var i = 0; i < PropertyChangeCount; i++)
        {
            _vm.Name = $"Name_{i}";
            _vm.Age = i;
        }
    }

    /// <summary>Subscribing and reading the first value, with no changes driven.</summary>
    /// <returns>The observed value.</returns>
    [Benchmark(Description = "First Observation")]
    public string FirstObservation()
    {
        var result = string.Empty;
        using var sub = _vm.WhenAnyValue(x => x.Name)
            .Subscribe(v => result = v);
        return result;
    }
}
