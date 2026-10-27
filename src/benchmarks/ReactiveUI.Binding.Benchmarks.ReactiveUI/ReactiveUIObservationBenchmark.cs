// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using BenchmarkDotNet.Attributes;
using ReactiveUI.Binding.Benchmarks.Mocks;
using ReactiveUI.Builder;
using Splat;

namespace ReactiveUI.Binding.Benchmarks;

/// <summary>ReactiveUI expression-tree WhenAnyValue benchmarks for comparison.</summary>
[DebuggerDisplay("ReactiveUIObservationBenchmark: Expression-tree observation over {PropertyChangeCount} changes")]
public class ReactiveUIObservationBenchmark
{
    /// <summary>The number of property changes to fire during each benchmark iteration.</summary>
    private const int PropertyChangeCount = 1_000;

    /// <summary>The view model instance used for observation benchmarks.</summary>
    private BenchmarkViewModel _vm = null!;

    /// <summary>Configures ReactiveUI for the benchmark process.</summary>
    [GlobalSetup]
    public static void Register()
    {
        ModeDetector.OverrideModeDetector(new BenchmarkModeDetector());
        _ = RxAppBuilder.CreateReactiveUIBuilder()
            .WithCoreServices()
            .BuildApp();
    }

    /// <summary>Sets up a fresh view model before each benchmark iteration.</summary>
    [IterationSetup]
    public void Setup() =>
        _vm = new() { Name = "Initial", Age = 0, Child = new() { Value = "ChildInitial" } };

    /// <summary>Expression-tree single property observation: subscribe, fire N changes, dispose.</summary>
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

    /// <summary>Expression-tree deep chain observation: subscribe, fire N changes, dispose.</summary>
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

    /// <summary>Expression-tree multi-property observation: subscribe, fire N changes, dispose.</summary>
    [Benchmark(Description = "Two Properties")]
    public void TwoProperties()
    {
        (string Name, int Age) last = default;
        using var sub = _vm.WhenAnyValue(x => x.Name, x => x.Age, static (name, age) => (name, age))
            .Subscribe(v => last = v);

        for (var i = 0; i < PropertyChangeCount; i++)
        {
            _vm.Name = $"Name_{i}";
            _vm.Age = i;
        }
    }

    /// <summary>Cold start: subscribe, read initial value, dispose. Includes expression compilation overhead.</summary>
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
