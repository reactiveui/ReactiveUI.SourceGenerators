// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if NET8_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif
using BenchmarkDotNet.Attributes;
using ReactiveUI.Binding.Benchmarks.Mocks;
using ReactiveUI.Binding.Builder;

namespace ReactiveUI.Binding.Benchmarks;

/// <summary>The Unsafe overloads, which resolve the property path by reflection.</summary>
/// <remarks>
/// No NativeAOT job is declared. These overloads carry <c>RequiresUnreferencedCode</c> precisely because they
/// walk the path at run time, so an ahead-of-time publish cannot be relied on to keep the members they reach.
/// Read these against the generated benchmark of the same operator to see what the fallback costs.
/// </remarks>
#if NET8_0_OR_GREATER
[RequiresUnreferencedCode("Evaluates expression-based member chains via reflection; members may be trimmed.")]
#endif
public class UnsafeFallbackBenchmark
{
    /// <summary>How many property changes each benchmark drives through one subscription.</summary>
    private const int PropertyChangeCount = 1_000;

    /// <summary>The view model under observation.</summary>
    private BenchmarkViewModel _vm = null!;

    /// <summary>The binding's target.</summary>
    private BenchmarkView _view = null!;

    /// <summary>Registers the runtime services the reflection path resolves through.</summary>
    [GlobalSetup]
    public static void Register()
    {
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        _ = builder.WithCoreServices();
        _ = builder.BuildApp();
    }

    /// <summary>Builds a fresh source and target before each iteration.</summary>
    [IterationSetup]
    public void Setup()
    {
        _vm = new() { Name = "Initial", Age = 0, Child = new() { Value = "ChildInitial" } };
        _view = new() { ViewModel = _vm };
    }

    /// <summary>One property, resolved by reflection.</summary>
    [Benchmark(Description = "WhenChangedUnsafe")]
    public void WhenChangedUnsafe()
    {
        var last = string.Empty;
        using var sub = ReactiveUIBindingExtensions.WhenChangedUnsafe(_vm, x => x.Name)
            .Subscribe(v => last = v);

        for (var i = 0; i < PropertyChangeCount; i++)
        {
            _vm.Name = $"Name_{i}";
        }
    }

    /// <summary>A two-link chain, resolved by reflection.</summary>
    [Benchmark(Description = "WhenChangedUnsafe deep chain")]
    public void WhenChangedUnsafeDeepChain()
    {
        var last = string.Empty;
        using var sub = ReactiveUIBindingExtensions.WhenChangedUnsafe(_vm, x => x.Child.Value)
            .Subscribe(v => last = v);

        for (var i = 0; i < PropertyChangeCount; i++)
        {
            _vm.Child.Value = $"Value_{i}";
        }
    }

    /// <summary>One property under the ReactiveUI-compatible name, resolved by reflection.</summary>
    [Benchmark(Description = "WhenAnyValueUnsafe")]
    public void WhenAnyValueUnsafe()
    {
        var last = string.Empty;
        using var sub = ReactiveUIBindingExtensions.WhenAnyValueUnsafe(_vm, x => x.Name)
            .Subscribe(v => last = v);

        for (var i = 0; i < PropertyChangeCount; i++)
        {
            _vm.Name = $"Name_{i}";
        }
    }

    /// <summary>A one-way binding, resolved by reflection.</summary>
    [Benchmark(Description = "BindOneWayUnsafe")]
    public void BindOneWayUnsafe()
    {
        using var binding = ReactiveUIBindingExtensions.BindOneWayUnsafe(_vm, _view, x => x.Name, v => v.DisplayName);

        for (var i = 0; i < PropertyChangeCount; i++)
        {
            _vm.Name = $"Name_{i}";
        }
    }

    /// <summary>A two-way binding, resolved by reflection.</summary>
    [Benchmark(Description = "BindUnsafe")]
    public void BindUnsafe()
    {
        using var binding = ReactiveUIBindingExtensions.BindUnsafe(_view, _vm, x => x.Name, v => v.DisplayName);

        for (var i = 0; i < PropertyChangeCount; i++)
        {
            _vm.Name = $"Name_{i}";
        }
    }
}
