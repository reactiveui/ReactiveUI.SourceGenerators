// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using ReactiveUI.Binding.Benchmarks.Mocks;
using ReactiveUI.Binding.Benchmarks.ToProperty;

namespace ReactiveUI.Binding.Benchmarks;

/// <summary>ToPropertyUnsafe benchmarks: creating a property whose raise member is found by reflection, and each value it carries.</summary>
/// <remarks>
/// A protected raise method is bound to its owner as a delegate when the helper is created; a field-like event is
/// raised through its field. Either way a value allocates nothing, which the emission benchmarks confirm.
/// </remarks>
#if NET8_0_OR_GREATER
[RequiresUnreferencedCode("Finds the members that raise a property's change notifications by reflection; they may be trimmed.")]
[RequiresDynamicCode("Closes ReactiveUI's generic raise extension over IReactiveObject at run time.")]
#endif
public class ToPropertyUnsafeBenchmark : IDisposable
{
    /// <summary>The number of values each emission benchmark pushes.</summary>
    private const int ValueCount = 1_000;

    /// <summary>The source the creation benchmarks follow.</summary>
    private BenchmarkSource<int> _source = null!;

    /// <summary>The source only the raise-method view model follows.</summary>
    private BenchmarkSource<int> _raiseMethodSource = null!;

    /// <summary>The source only the event view model follows.</summary>
    private BenchmarkSource<int> _eventSource = null!;

    /// <summary>The raise-method view model the emission benchmark drives.</summary>
    private UnsafeRaiseMethodCountViewModel _raiseMethod = null!;

    /// <summary>The event view model the emission benchmark drives.</summary>
    private UnsafeEventCountViewModel _event = null!;

    /// <summary>The next value to push, kept rising so every value passes the distinct gate.</summary>
    private int _next;

    /// <summary>The number of notifications a subscriber received, read so the handler is not elided.</summary>
    private int _notifications;

    /// <summary>Creates the sources and the long-lived view models, each with one PropertyChanged subscriber.</summary>
    [GlobalSetup]
    public void Setup()
    {
        _source = new();
        _raiseMethodSource = new();
        _eventSource = new();
        _raiseMethod = new(_raiseMethodSource);
        _event = new(_eventSource);
        _raiseMethod.PropertyChanged += OnPropertyChanged;
        _event.PropertyChanged += OnPropertyChanged;
    }

    /// <summary>Releases the long-lived view models.</summary>
    [GlobalCleanup]
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Creates a property on a view model with a protected raise method and disposes it.</summary>
    /// <returns>The property's initial value.</returns>
    [Benchmark(Description = "Unsafe create: protected raise method")]
    public int CreateRaiseMethod()
    {
        using var vm = new UnsafeRaiseMethodCountViewModel(_source);
        return vm.Count;
    }

    /// <summary>Creates a property on a view model with only a field-like event and disposes it.</summary>
    /// <returns>The property's initial value.</returns>
    [Benchmark(Description = "Unsafe create: event field")]
    public int CreateEvent()
    {
        using var vm = new UnsafeEventCountViewModel(_source);
        return vm.Count;
    }

    /// <summary>Pushes values through the raise-method view model's helper to one PropertyChanged subscriber.</summary>
    /// <returns>The notification count.</returns>
    [Benchmark(Description = "Unsafe emit: protected raise method", OperationsPerInvoke = ValueCount)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int EmitRaiseMethod() => Emit(_raiseMethodSource);

    /// <summary>Pushes values through the event view model's helper to one PropertyChanged subscriber.</summary>
    /// <returns>The notification count.</returns>
    [Benchmark(Description = "Unsafe emit: event field", OperationsPerInvoke = ValueCount)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int EmitEvent() => Emit(_eventSource);

    /// <summary>Releases the long-lived view models.</summary>
    /// <param name="disposing">Whether managed resources should be released.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        _raiseMethod.Dispose();
        _event.Dispose();
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
