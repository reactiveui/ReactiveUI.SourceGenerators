// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using BenchmarkDotNet.Attributes;
using ReactiveUI.Binding.Benchmarks.Mocks;
using ReactiveUI.Builder;
using Splat;

namespace ReactiveUI.Binding.Benchmarks;

/// <summary>ReactiveUI expression-tree binding benchmarks for comparison.</summary>
[DebuggerDisplay("ReactiveUIBindingBenchmark: Expression-tree binding over {PropertyChangeCount} changes")]
public class ReactiveUIBindingBenchmark
{
    /// <summary>The number of property changes to fire during each benchmark iteration.</summary>
    private const int PropertyChangeCount = 1_000;

    /// <summary>The source view model instance used for binding benchmarks.</summary>
    private BenchmarkViewModel _source = null!;

    /// <summary>The target view instance used for binding benchmarks.</summary>
    private BenchmarkView _target = null!;

    /// <summary>Configures ReactiveUI for the benchmark process.</summary>
    [GlobalSetup]
    public static void Register()
    {
        ModeDetector.OverrideModeDetector(new BenchmarkModeDetector());
        _ = RxAppBuilder.CreateReactiveUIBuilder()
            .WithCoreServices()
            .BuildApp();
    }

    /// <summary>Sets up fresh source and target objects before each benchmark iteration.</summary>
    [IterationSetup]
    public void Setup()
    {
        _source = new() { Name = "Initial", Age = 0 };
        _target = new() { ViewModel = _source };
    }

    /// <summary>Expression-tree one-way binding: setup, fire N changes, dispose.</summary>
    [Benchmark(Description = "OneWayBind")]
    public void OneWayBind()
    {
        using var binding = _target.OneWayBind(_source, x => x.Name, x => x.DisplayName);

        for (var i = 0; i < PropertyChangeCount; i++)
        {
            _source.Name = $"Name_{i}";
        }
    }

    /// <summary>Expression-tree two-way binding: setup, fire N source changes, dispose.</summary>
    [Benchmark(Description = "Bind")]
    public void Bind()
    {
        using var binding = _target.Bind(_source, x => x.Name, x => x.DisplayName);

        for (var i = 0; i < PropertyChangeCount; i++)
        {
            _source.Name = $"Name_{i}";
        }
    }

    /// <summary>Cold start: create one-way binding, dispose immediately.</summary>
    [Benchmark(Description = "First OneWayBind")]
    public void FirstOneWayBind()
    {
        using var binding = _target.OneWayBind(_source, x => x.Name, x => x.DisplayName);
    }

    /// <summary>Cold start: create two-way binding, dispose immediately.</summary>
    [Benchmark(Description = "First Bind")]
    public void FirstBind()
    {
        using var binding = _target.Bind(_source, x => x.Name, x => x.DisplayName);
    }
}
