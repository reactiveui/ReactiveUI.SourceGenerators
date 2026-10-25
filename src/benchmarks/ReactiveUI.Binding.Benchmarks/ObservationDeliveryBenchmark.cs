// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using ReactiveUI.Binding.Benchmarks.Mocks;

namespace ReactiveUI.Binding.Benchmarks;

/// <summary>Separates observation construction and subscription from delivery through a live subscription.</summary>
public class ObservationDeliveryBenchmark
{
    /// <summary>The changes delivered by one measured invocation.</summary>
    private const int ChangeCount = 1_000;

    /// <summary>The notification source shared by each invocation.</summary>
    private DeliveryViewModel _source = null!;

    /// <summary>The allocation-free consumer.</summary>
    private DeliveryObserver _observer = null!;

    /// <summary>The live subscription used only for delivery measurements.</summary>
    private IDisposable _subscription = null!;

    /// <summary>Gets or sets the observation contract.</summary>
    [Params(nameof(ObservationKind.Changed), nameof(ObservationKind.Changing), nameof(ObservationKind.Plugin))]
    public string Kind { get; set; } = nameof(ObservationKind.Changed);

    /// <summary>Creates the source and observer outside the measured subscription.</summary>
    [GlobalSetup(Target = nameof(Subscribe))]
    public void Setup()
    {
        _source = new();
        _observer = new();
    }

    /// <summary>Attaches the steady-state observer before delivery measurement begins.</summary>
    [GlobalSetup(Target = nameof(Deliver))]
    public void SetupDelivery()
    {
        Setup();
        _subscription = DeliveryObservation.Create(_source, Kind).Subscribe(_observer);
    }

    /// <summary>Releases the live delivery subscription.</summary>
    [GlobalCleanup(Target = nameof(Deliver))]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CleanupDelivery() => _subscription.Dispose();

    /// <summary>Constructs, subscribes, delivers the initial value, and disposes.</summary>
    [Benchmark]
    public void Subscribe()
    {
        using var subscription = DeliveryObservation.Create(_source, Kind).Subscribe(_observer);
    }

    /// <summary>Delivers integer changes on the producer thread through an established subscription.</summary>
    /// <returns>The last value consumed.</returns>
    [Benchmark(OperationsPerInvoke = ChangeCount)]
    public int Deliver()
    {
        for (var i = 0; i < ChangeCount; i++)
        {
            _source.Value = i;
        }

        return _observer.Value;
    }
}
