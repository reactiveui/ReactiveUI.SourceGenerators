// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using ReactiveUI.Binding.Benchmarks.Mocks;
using ReactiveUI.Builder;
using Splat;

namespace ReactiveUI.Binding.Benchmarks;

/// <summary>ReactiveUI's own ToProperty, over the same scenarios as the generated one, for comparison.</summary>
public class ReactiveUIToPropertyBenchmark : IDisposable
{
    /// <summary>The number of values each emission benchmark pushes.</summary>
    private const int ValueCount = 1_000;

    /// <summary>The source the creation benchmark follows.</summary>
    private BenchmarkSource<int> _source = null!;

    /// <summary>The source only the long-lived view model follows.</summary>
    private BenchmarkSource<int> _emitSource = null!;

    /// <summary>The long-lived view model the emission benchmark drives.</summary>
    private CountViewModel _viewModel = null!;

    /// <summary>The next value to push, kept rising so every value passes the distinct gate.</summary>
    private int _next;

    /// <summary>The number of notifications a subscriber received, read so the handler is not elided.</summary>
    private int _notifications;

    /// <summary>Configures ReactiveUI and creates the long-lived view model with one PropertyChanged subscriber.</summary>
    [GlobalSetup]
    public void Setup()
    {
        ModeDetector.OverrideModeDetector(new BenchmarkModeDetector());
        _ = RxAppBuilder.CreateReactiveUIBuilder()
            .WithCoreServices()
            .BuildApp();
        _source = new();
        _emitSource = new();
        _viewModel = new(_emitSource);
        _viewModel.PropertyChanged += OnPropertyChanged;
    }

    /// <summary>Releases the long-lived view model.</summary>
    [GlobalCleanup]
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Creates a helper-backed property on a ReactiveObject and disposes it.</summary>
    /// <returns>The property's initial value.</returns>
    [Benchmark(Description = "Create: ReactiveObject")]
    public int CreateReactiveObject()
    {
        using var vm = new CountViewModel(_source);
        return vm.Count;
    }

    /// <summary>Creates the helper alone with a no-op callback, isolating its cost from any view model.</summary>
    /// <returns>The helper's initial value.</returns>
    [Benchmark(Description = "Create: helper only")]
    public int CreateHelperOnly()
    {
        using var helper = new ObservableAsPropertyHelper<int>(_source, static _ => { }, 0);
        return helper.Value;
    }

    /// <summary>Pushes values through a ReactiveObject's helper to one PropertyChanged subscriber.</summary>
    /// <returns>The notification count.</returns>
    [Benchmark(Description = "Emit: ReactiveObject", OperationsPerInvoke = ValueCount)]
    public int EmitReactiveObject()
    {
        for (var i = 0; i < ValueCount; i++)
        {
            _next++;
            _emitSource.Push(_next);
        }

        return _notifications;
    }

    /// <summary>Releases the long-lived view model.</summary>
    /// <param name="disposing">Whether managed resources should be released.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _viewModel.Dispose();
        }
    }

    /// <summary>Counts a notification.</summary>
    /// <param name="sender">The view model.</param>
    /// <param name="args">The event arguments.</param>
    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs args) => _notifications++;

    /// <summary>A ReactiveObject whose derived property is backed by ReactiveUI's helper.</summary>
    internal sealed class CountViewModel : ReactiveObject, IDisposable
    {
        /// <summary>Backs <see cref="Count"/>.</summary>
        private readonly ObservableAsPropertyHelper<int> _count;

        /// <summary>Initializes a new instance of the <see cref="CountViewModel"/> class.</summary>
        /// <param name="counts">The values <see cref="Count"/> takes.</param>
        public CountViewModel(IObservable<int> counts) => _count = counts.ToProperty(this, static x => x.Count);

        /// <summary>Gets the latest count.</summary>
        public int Count => _count.Value;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => _count.Dispose();
    }
}
