// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using ReactiveUI.Binding.Benchmarks.Mocks;
using ReactiveUI.Binding.Benchmarks.ToProperty;

namespace ReactiveUI.Binding.Benchmarks;

/// <summary>Source-generated ToProperty benchmarks: creating a helper-backed property, and each value it carries.</summary>
/// <remarks>
/// Each raise mechanism the generator can choose has its own pair, so the cost of the mechanism is visible apart
/// from the helper's own. The profiled config records a GcVerbose trace, which attributes every allocation.
/// </remarks>
public class ToPropertyBenchmark : IDisposable
{
    /// <summary>The number of values each emission benchmark pushes.</summary>
    private const int ValueCount = 1_000;

    /// <summary>The source the creation benchmarks follow.</summary>
    private BenchmarkSource<int> _source = null!;

    /// <summary>The source only the partial view model follows.</summary>
    private BenchmarkSource<int> _partialSource = null!;

    /// <summary>The source only the raise-method view model follows.</summary>
    private BenchmarkSource<int> _raiseMethodSource = null!;

    /// <summary>The source only the ReactiveUI view model follows.</summary>
    private BenchmarkSource<int> _reactiveSource = null!;

    /// <summary>The partial view model the emission benchmark drives.</summary>
    private PartialCountViewModel _partial = null!;

    /// <summary>The raise-method view model the emission benchmark drives.</summary>
    private RaiseMethodCountViewModel _raiseMethod = null!;

    /// <summary>The ReactiveUI view model the emission benchmark drives.</summary>
    private ReactiveCountViewModel _reactive = null!;

    /// <summary>The next value to push, kept rising so every value passes the distinct gate.</summary>
    private int _next;

    /// <summary>The number of notifications a subscriber received, read so the handler is not elided.</summary>
    private int _notifications;

    /// <summary>Creates the sources and the long-lived view models, each with one PropertyChanged subscriber.</summary>
    [GlobalSetup]
    public void Setup()
    {
        _source = new();
        _partialSource = new();
        _raiseMethodSource = new();
        _reactiveSource = new();
        _partial = new(_partialSource);
        _raiseMethod = new(_raiseMethodSource);
        _reactive = new(_reactiveSource);
        _partial.PropertyChanged += OnPropertyChanged;
        _raiseMethod.PropertyChanged += OnPropertyChanged;
        _reactive.PropertyChanged += OnPropertyChanged;
    }

    /// <summary>Releases the long-lived view models.</summary>
    [GlobalCleanup]
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Creates a helper-backed property on a partial view model and disposes it.</summary>
    /// <returns>The property's initial value.</returns>
    [Benchmark(Description = "Create: partial event")]
    public int CreatePartial()
    {
        using var vm = new PartialCountViewModel(_source);
        return vm.Count;
    }

    /// <summary>Creates a helper-backed property on a view model with a public raise method and disposes it.</summary>
    /// <returns>The property's initial value.</returns>
    [Benchmark(Description = "Create: raise method")]
    public int CreateRaiseMethod()
    {
        using var vm = new RaiseMethodCountViewModel(_source);
        return vm.Count;
    }

    /// <summary>Creates a helper-backed property on a ReactiveUI view model and disposes it.</summary>
    /// <returns>The property's initial value.</returns>
    [Benchmark(Description = "Create: ReactiveObject")]
    public int CreateReactiveObject()
    {
        using var vm = new ReactiveCountViewModel(_source);
        return vm.Count;
    }

    /// <summary>Creates a deferred helper, reads its value once to subscribe, and disposes it.</summary>
    /// <returns>The property's initial value.</returns>
    [Benchmark(Description = "Create: deferred, first read")]
    public int CreateDeferred()
    {
        using var vm = new DeferredCountViewModel(_source);
        return vm.Count;
    }

    /// <summary>Creates the helper alone with static callbacks, isolating its cost from any view model.</summary>
    /// <returns>The helper's initial value.</returns>
    [Benchmark(Description = "Create: helper only")]
    public int CreateHelperOnly()
    {
        using var helper = ObservableAsPropertyHelper<int>.Create(_source, this, static (_, _) => { }, null, 0, false, null);
        return helper.Value;
    }

    /// <summary>Pushes values through a partial view model's helper to one PropertyChanged subscriber.</summary>
    /// <returns>The notification count.</returns>
    [Benchmark(Description = "Emit: partial event", OperationsPerInvoke = ValueCount)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int EmitPartial() => Emit(_partialSource);

    /// <summary>Pushes values through a raise-method view model's helper to one PropertyChanged subscriber.</summary>
    /// <returns>The notification count.</returns>
    [Benchmark(Description = "Emit: raise method", OperationsPerInvoke = ValueCount)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int EmitRaiseMethod() => Emit(_raiseMethodSource);

    /// <summary>Pushes values through a ReactiveUI view model's helper to one PropertyChanged subscriber.</summary>
    /// <returns>The notification count.</returns>
    [Benchmark(Description = "Emit: ReactiveObject", OperationsPerInvoke = ValueCount)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int EmitReactiveObject() => Emit(_reactiveSource);

    /// <summary>Releases the long-lived view models.</summary>
    /// <param name="disposing">Whether managed resources should be released.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        _partial.Dispose();
        _raiseMethod.Dispose();
        _reactive.Dispose();
    }

    /// <summary>Counts a notification.</summary>
    /// <param name="sender">The view model.</param>
    /// <param name="args">The event arguments.</param>
    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs args) => _notifications++;

    /// <summary>Pushes the value count through one view model's source.</summary>
    /// <param name="source">The source only that view model follows.</param>
    /// <returns>The notification count.</returns>
    private int Emit(BenchmarkSource<int> source)
    {
        for (var i = 0; i < ValueCount; i++)
        {
            _next++;
            source.Push(_next);
        }

        return _notifications;
    }
}
