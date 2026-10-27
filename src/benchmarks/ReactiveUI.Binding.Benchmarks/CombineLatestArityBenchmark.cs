// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using ReactiveUI.Binding.Benchmarks.Mocks;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Advanced;

namespace ReactiveUI.Binding.Benchmarks;

/// <summary>Compares complete construction, subscription, and disposal for each supported CombineLatest arity.</summary>
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class CombineLatestArityBenchmark
{
    /// <summary>The two-source overload.</summary>
    private const int TwoSources = 2;

    /// <summary>The three-source overload.</summary>
    private const int ThreeSources = 3;

    /// <summary>The four-source overload.</summary>
    private const int FourSources = 4;

    /// <summary>The five-source overload.</summary>
    private const int FiveSources = 5;

    /// <summary>The six-source overload.</summary>
    private const int SixSources = 6;

    /// <summary>The seven-source overload.</summary>
    private const int SevenSources = 7;

    /// <summary>The eight-source overload.</summary>
    private const int EightSources = 8;

    /// <summary>The nine-source overload.</summary>
    private const int NineSources = 9;

    /// <summary>The ten-source overload.</summary>
    private const int TenSources = 10;

    /// <summary>The eleven-source overload.</summary>
    private const int ElevenSources = 11;

    /// <summary>The twelve-source overload.</summary>
    private const int TwelveSources = 12;

    /// <summary>The thirteen-source overload.</summary>
    private const int ThirteenSources = 13;

    /// <summary>The fourteen-source overload.</summary>
    private const int FourteenSources = 14;

    /// <summary>The fifteen-source overload.</summary>
    private const int FifteenSources = 15;

    /// <summary>The sixteen-source overload.</summary>
    private const int SixteenSources = 16;

    /// <summary>The independently subscribed sources.</summary>
    private BenchmarkSource<int>[] _sources = null!;

    /// <summary>The consumer shared by the two construction routes.</summary>
    private DeliveryObserver _observer = null!;

    /// <summary>The extension pipeline whose subscription is measured independently of construction.</summary>
    private IObservable<int> _extension = null!;

    /// <summary>The constructor pipeline whose subscription creates fresh typed slots.</summary>
    private IObservable<int> _constructor = null!;

    /// <summary>Gets or sets the number of sources combined.</summary>
    [Params(2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16)]
    public int Arity { get; set; }

    /// <summary>Creates sources and verifies that both routes attach every source and project its value.</summary>
    [GlobalSetup]
    public void Setup()
    {
        _sources = new BenchmarkSource<int>[Arity];
        _observer = new();
        for (var i = 0; i < _sources.Length; i++)
        {
            _sources[i] = new();
        }

        _extension = CreateExtension();
        _constructor = CreateConstructor();
        Validate(_extension);
        Validate(_constructor);
    }

    /// <summary>Constructs and subscribes through the extension overload, then detaches every source.</summary>
    [Benchmark(Baseline = true)]
    [BenchmarkCategory("ConstructAndSubscribe")]
    public void Extension()
    {
        using var subscription = CreateExtension().Subscribe(_observer);
    }

    /// <summary>Constructs and subscribes through the public concrete constructor, then detaches every source.</summary>
    [Benchmark]
    [BenchmarkCategory("ConstructAndSubscribe")]
    public void Constructor()
    {
        using var subscription = CreateConstructor().Subscribe(_observer);
    }

    /// <summary>Subscribes to an existing extension pipeline, then detaches every source.</summary>
    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Subscribe")]
    public void SubscribeExtension()
    {
        using var subscription = _extension.Subscribe(_observer);
    }

    /// <summary>Subscribes to an existing concrete pipeline, creating fresh slots and projection closure.</summary>
    [Benchmark]
    [BenchmarkCategory("Subscribe")]
    public void SubscribeConstructor()
    {
        using var subscription = _constructor.Subscribe(_observer);
    }

    /// <summary>Checks that the selected arity performs real subscriptions and computes the sum.</summary>
    /// <param name="observable">The construction route to verify.</param>
    /// <exception cref="InvalidOperationException">The route does not project all inputs.</exception>
    private void Validate(IObservable<int> observable)
    {
        using var subscription = observable.Subscribe(_observer);
        _observer.OnNext(-1);
        var expected = 0;
        for (var i = 0; i < _sources.Length; i++)
        {
            _sources[i].Push(i + 1);
            expected += i + 1;
        }

        if (_observer.Value != expected)
        {
            throw new InvalidOperationException("CombineLatest did not combine every source.");
        }
    }

    /// <summary>Selects the extension overload with the exact number of typed inputs.</summary>
    /// <returns>The observable whose construction remains inside the measured operation.</returns>
    /// <exception cref="InvalidOperationException">The configured arity is unsupported.</exception>
    private IObservable<int> CreateExtension() => Arity switch
    {
        TwoSources => CreateExtension2(),
        ThreeSources => CreateExtension3(),
        FourSources => CreateExtension4(),
        FiveSources => CreateExtension5(),
        SixSources => CreateExtension6(),
        SevenSources => CreateExtension7(),
        EightSources => CreateExtension8(),
        NineSources => CreateExtension9(),
        TenSources => CreateExtension10(),
        ElevenSources => CreateExtension11(),
        TwelveSources => CreateExtension12(),
        ThirteenSources => CreateExtension13(),
        FourteenSources => CreateExtension14(),
        FifteenSources => CreateExtension15(),
        SixteenSources => CreateExtension16(),
        _ => throw new InvalidOperationException("Unsupported arity."),
    };

    /// <summary>Selects a concrete constructor whose projection captures typed slots.</summary>
    /// <returns>The observable whose construction remains inside the measured operation.</returns>
    /// <exception cref="InvalidOperationException">The configured arity is unsupported.</exception>
    private IObservable<int> CreateConstructor() => Arity switch
    {
        TwoSources => new CombineLatestSignal<int, int, int>(_sources[0], _sources[1], static (left, right) => left + right),
        ThreeSources => Create3(),
        FourSources => Create4(),
        FiveSources => Create5(),
        SixSources => Create6(),
        SevenSources => Create7(),
        EightSources => Create8(),
        NineSources => Create9(),
        TenSources => Create10(),
        ElevenSources => Create11(),
        TwelveSources => Create12(),
        ThirteenSources => Create13(),
        FourteenSources => Create14(),
        FifteenSources => Create15(),
        SixteenSources => Create16(),
        _ => throw new InvalidOperationException("Unsupported arity."),
    };

    /// <summary>Attaches 3 typed slots with one projection closure per subscription.</summary>
    /// <returns>The publicly constructed observable.</returns>
    private CombineLatestSignal<int> Create3() => new(coordinator =>
    {
        var slot1 = coordinator.Attach(_sources[0]);
        var slot2 = coordinator.Attach(_sources[1]);
        var slot3 = coordinator.Attach(_sources[2]);

        return () => slot1.Value + slot2.Value + slot3.Value;
    });

    /// <summary>Attaches 4 typed slots with one projection closure per subscription.</summary>
    /// <returns>The publicly constructed observable.</returns>
    private CombineLatestSignal<int> Create4() => new(coordinator =>
    {
        var slot1 = coordinator.Attach(_sources[0]);
        var slot2 = coordinator.Attach(_sources[1]);
        var slot3 = coordinator.Attach(_sources[2]);
        var slot4 = coordinator.Attach(_sources[3]);

        return () => slot1.Value + slot2.Value + slot3.Value + slot4.Value;
    });

    /// <summary>Attaches 5 typed slots with one projection closure per subscription.</summary>
    /// <returns>The publicly constructed observable.</returns>
    private CombineLatestSignal<int> Create5() => new(coordinator =>
    {
        var slot1 = coordinator.Attach(_sources[0]);
        var slot2 = coordinator.Attach(_sources[1]);
        var slot3 = coordinator.Attach(_sources[2]);
        var slot4 = coordinator.Attach(_sources[3]);
        var slot5 = coordinator.Attach(_sources[4]);

        return () => slot1.Value + slot2.Value + slot3.Value + slot4.Value + slot5.Value;
    });

    /// <summary>Attaches 6 typed slots with one projection closure per subscription.</summary>
    /// <returns>The publicly constructed observable.</returns>
    private CombineLatestSignal<int> Create6() => new(coordinator =>
    {
        var slot1 = coordinator.Attach(_sources[0]);
        var slot2 = coordinator.Attach(_sources[1]);
        var slot3 = coordinator.Attach(_sources[2]);
        var slot4 = coordinator.Attach(_sources[3]);
        var slot5 = coordinator.Attach(_sources[4]);
        var slot6 = coordinator.Attach(_sources[5]);

        return () => slot1.Value + slot2.Value + slot3.Value + slot4.Value + slot5.Value + slot6.Value;
    });

    /// <summary>Attaches 7 typed slots with one projection closure per subscription.</summary>
    /// <returns>The publicly constructed observable.</returns>
    private CombineLatestSignal<int> Create7() => new(coordinator =>
    {
        var slot1 = coordinator.Attach(_sources[0]);
        var slot2 = coordinator.Attach(_sources[1]);
        var slot3 = coordinator.Attach(_sources[2]);
        var slot4 = coordinator.Attach(_sources[3]);
        var slot5 = coordinator.Attach(_sources[4]);
        var slot6 = coordinator.Attach(_sources[5]);
        var slot7 = coordinator.Attach(_sources[6]);

        return () => slot1.Value + slot2.Value + slot3.Value + slot4.Value + slot5.Value + slot6.Value + slot7.Value;
    });

    /// <summary>Attaches 8 typed slots with one projection closure per subscription.</summary>
    /// <returns>The publicly constructed observable.</returns>
    private CombineLatestSignal<int> Create8() => new(coordinator =>
    {
        var slot1 = coordinator.Attach(_sources[0]);
        var slot2 = coordinator.Attach(_sources[1]);
        var slot3 = coordinator.Attach(_sources[2]);
        var slot4 = coordinator.Attach(_sources[3]);
        var slot5 = coordinator.Attach(_sources[4]);
        var slot6 = coordinator.Attach(_sources[5]);
        var slot7 = coordinator.Attach(_sources[6]);
        var slot8 = coordinator.Attach(_sources[7]);

        return () => slot1.Value + slot2.Value + slot3.Value + slot4.Value + slot5.Value + slot6.Value + slot7.Value + slot8.Value;
    });

    /// <summary>Attaches 9 typed slots with one projection closure per subscription.</summary>
    /// <returns>The publicly constructed observable.</returns>
    private CombineLatestSignal<int> Create9() => new(coordinator =>
    {
        var slot1 = coordinator.Attach(_sources[0]);
        var slot2 = coordinator.Attach(_sources[1]);
        var slot3 = coordinator.Attach(_sources[2]);
        var slot4 = coordinator.Attach(_sources[3]);
        var slot5 = coordinator.Attach(_sources[4]);
        var slot6 = coordinator.Attach(_sources[5]);
        var slot7 = coordinator.Attach(_sources[6]);
        var slot8 = coordinator.Attach(_sources[7]);
        var slot9 = coordinator.Attach(_sources[8]);

        return () => slot1.Value + slot2.Value + slot3.Value + slot4.Value + slot5.Value + slot6.Value + slot7.Value + slot8.Value + slot9.Value;
    });

    /// <summary>Attaches 10 typed slots with one projection closure per subscription.</summary>
    /// <returns>The publicly constructed observable.</returns>
    private CombineLatestSignal<int> Create10() => new(coordinator =>
    {
        var slot1 = coordinator.Attach(_sources[0]);
        var slot2 = coordinator.Attach(_sources[1]);
        var slot3 = coordinator.Attach(_sources[2]);
        var slot4 = coordinator.Attach(_sources[3]);
        var slot5 = coordinator.Attach(_sources[4]);
        var slot6 = coordinator.Attach(_sources[5]);
        var slot7 = coordinator.Attach(_sources[6]);
        var slot8 = coordinator.Attach(_sources[7]);
        var slot9 = coordinator.Attach(_sources[8]);
        var slot10 = coordinator.Attach(_sources[9]);

        return () => slot1.Value + slot2.Value + slot3.Value + slot4.Value + slot5.Value + slot6.Value + slot7.Value + slot8.Value + slot9.Value + slot10.Value;
    });

    /// <summary>Attaches 11 typed slots with one projection closure per subscription.</summary>
    /// <returns>The publicly constructed observable.</returns>
    private CombineLatestSignal<int> Create11() => new(coordinator =>
    {
        var slot1 = coordinator.Attach(_sources[0]);
        var slot2 = coordinator.Attach(_sources[1]);
        var slot3 = coordinator.Attach(_sources[2]);
        var slot4 = coordinator.Attach(_sources[3]);
        var slot5 = coordinator.Attach(_sources[4]);
        var slot6 = coordinator.Attach(_sources[5]);
        var slot7 = coordinator.Attach(_sources[6]);
        var slot8 = coordinator.Attach(_sources[7]);
        var slot9 = coordinator.Attach(_sources[8]);
        var slot10 = coordinator.Attach(_sources[9]);
        var slot11 = coordinator.Attach(_sources[10]);

        return () => slot1.Value + slot2.Value + slot3.Value + slot4.Value + slot5.Value + slot6.Value + slot7.Value + slot8.Value + slot9.Value + slot10.Value + slot11.Value;
    });

    /// <summary>Attaches 12 typed slots with one projection closure per subscription.</summary>
    /// <returns>The publicly constructed observable.</returns>
    private CombineLatestSignal<int> Create12() => new(coordinator =>
    {
        var slot1 = coordinator.Attach(_sources[0]);
        var slot2 = coordinator.Attach(_sources[1]);
        var slot3 = coordinator.Attach(_sources[2]);
        var slot4 = coordinator.Attach(_sources[3]);
        var slot5 = coordinator.Attach(_sources[4]);
        var slot6 = coordinator.Attach(_sources[5]);
        var slot7 = coordinator.Attach(_sources[6]);
        var slot8 = coordinator.Attach(_sources[7]);
        var slot9 = coordinator.Attach(_sources[8]);
        var slot10 = coordinator.Attach(_sources[9]);
        var slot11 = coordinator.Attach(_sources[10]);
        var slot12 = coordinator.Attach(_sources[11]);

        return () => slot1.Value + slot2.Value + slot3.Value + slot4.Value + slot5.Value + slot6.Value + slot7.Value + slot8.Value + slot9.Value + slot10.Value + slot11.Value + slot12.Value;
    });

    /// <summary>Attaches 13 typed slots with one projection closure per subscription.</summary>
    /// <returns>The publicly constructed observable.</returns>
    private CombineLatestSignal<int> Create13() => new(coordinator =>
    {
        var slot1 = coordinator.Attach(_sources[0]);
        var slot2 = coordinator.Attach(_sources[1]);
        var slot3 = coordinator.Attach(_sources[2]);
        var slot4 = coordinator.Attach(_sources[3]);
        var slot5 = coordinator.Attach(_sources[4]);
        var slot6 = coordinator.Attach(_sources[5]);
        var slot7 = coordinator.Attach(_sources[6]);
        var slot8 = coordinator.Attach(_sources[7]);
        var slot9 = coordinator.Attach(_sources[8]);
        var slot10 = coordinator.Attach(_sources[9]);
        var slot11 = coordinator.Attach(_sources[10]);
        var slot12 = coordinator.Attach(_sources[11]);
        var slot13 = coordinator.Attach(_sources[12]);

        return () => slot1.Value + slot2.Value + slot3.Value + slot4.Value + slot5.Value + slot6.Value + slot7.Value
            + slot8.Value + slot9.Value + slot10.Value + slot11.Value + slot12.Value + slot13.Value;
    });

    /// <summary>Attaches 14 typed slots with one projection closure per subscription.</summary>
    /// <returns>The publicly constructed observable.</returns>
    private CombineLatestSignal<int> Create14() => new(coordinator =>
    {
        var slot1 = coordinator.Attach(_sources[0]);
        var slot2 = coordinator.Attach(_sources[1]);
        var slot3 = coordinator.Attach(_sources[2]);
        var slot4 = coordinator.Attach(_sources[3]);
        var slot5 = coordinator.Attach(_sources[4]);
        var slot6 = coordinator.Attach(_sources[5]);
        var slot7 = coordinator.Attach(_sources[6]);
        var slot8 = coordinator.Attach(_sources[7]);
        var slot9 = coordinator.Attach(_sources[8]);
        var slot10 = coordinator.Attach(_sources[9]);
        var slot11 = coordinator.Attach(_sources[10]);
        var slot12 = coordinator.Attach(_sources[11]);
        var slot13 = coordinator.Attach(_sources[12]);
        var slot14 = coordinator.Attach(_sources[13]);

        return () => slot1.Value + slot2.Value + slot3.Value + slot4.Value + slot5.Value + slot6.Value + slot7.Value
            + slot8.Value + slot9.Value + slot10.Value + slot11.Value + slot12.Value + slot13.Value + slot14.Value;
    });

    /// <summary>Attaches 15 typed slots with one projection closure per subscription.</summary>
    /// <returns>The publicly constructed observable.</returns>
    private CombineLatestSignal<int> Create15() => new(coordinator =>
    {
        var slot1 = coordinator.Attach(_sources[0]);
        var slot2 = coordinator.Attach(_sources[1]);
        var slot3 = coordinator.Attach(_sources[2]);
        var slot4 = coordinator.Attach(_sources[3]);
        var slot5 = coordinator.Attach(_sources[4]);
        var slot6 = coordinator.Attach(_sources[5]);
        var slot7 = coordinator.Attach(_sources[6]);
        var slot8 = coordinator.Attach(_sources[7]);
        var slot9 = coordinator.Attach(_sources[8]);
        var slot10 = coordinator.Attach(_sources[9]);
        var slot11 = coordinator.Attach(_sources[10]);
        var slot12 = coordinator.Attach(_sources[11]);
        var slot13 = coordinator.Attach(_sources[12]);
        var slot14 = coordinator.Attach(_sources[13]);
        var slot15 = coordinator.Attach(_sources[14]);

        return () => slot1.Value + slot2.Value + slot3.Value + slot4.Value + slot5.Value + slot6.Value + slot7.Value
            + slot8.Value + slot9.Value + slot10.Value + slot11.Value + slot12.Value + slot13.Value + slot14.Value + slot15.Value;
    });

    /// <summary>Attaches 16 typed slots with one projection closure per subscription.</summary>
    /// <returns>The publicly constructed observable.</returns>
    private CombineLatestSignal<int> Create16() => new(coordinator =>
    {
        var slot1 = coordinator.Attach(_sources[0]);
        var slot2 = coordinator.Attach(_sources[1]);
        var slot3 = coordinator.Attach(_sources[2]);
        var slot4 = coordinator.Attach(_sources[3]);
        var slot5 = coordinator.Attach(_sources[4]);
        var slot6 = coordinator.Attach(_sources[5]);
        var slot7 = coordinator.Attach(_sources[6]);
        var slot8 = coordinator.Attach(_sources[7]);
        var slot9 = coordinator.Attach(_sources[8]);
        var slot10 = coordinator.Attach(_sources[9]);
        var slot11 = coordinator.Attach(_sources[10]);
        var slot12 = coordinator.Attach(_sources[11]);
        var slot13 = coordinator.Attach(_sources[12]);
        var slot14 = coordinator.Attach(_sources[13]);
        var slot15 = coordinator.Attach(_sources[14]);
        var slot16 = coordinator.Attach(_sources[15]);

        return () => slot1.Value + slot2.Value + slot3.Value + slot4.Value + slot5.Value + slot6.Value + slot7.Value + slot8.Value
            + slot9.Value + slot10.Value + slot11.Value + slot12.Value + slot13.Value + slot14.Value + slot15.Value + slot16.Value;
    });

    /// <summary>Constructs the extension pipeline for 2 typed sources.</summary>
    /// <returns>The unshared pipeline instance.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IObservable<int> CreateExtension2() => LinqExtensions.CombineLatest(
        _sources[0],
        _sources[1],
        static (v1, v2) => v1 + v2);

    /// <summary>Constructs the extension pipeline for 3 typed sources.</summary>
    /// <returns>The unshared pipeline instance.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IObservable<int> CreateExtension3() => LinqExtensions.CombineLatest(
        _sources[0],
        _sources[1],
        _sources[2],
        static (v1, v2, v3) => v1 + v2 + v3);

    /// <summary>Constructs the extension pipeline for 4 typed sources.</summary>
    /// <returns>The unshared pipeline instance.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IObservable<int> CreateExtension4() => LinqExtensions.CombineLatest(
        _sources[0],
        _sources[1],
        _sources[2],
        _sources[3],
        static (v1, v2, v3, v4) => v1 + v2 + v3 + v4);

    /// <summary>Constructs the extension pipeline for 5 typed sources.</summary>
    /// <returns>The unshared pipeline instance.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IObservable<int> CreateExtension5() => LinqExtensions.CombineLatest(
        _sources[0],
        _sources[1],
        _sources[2],
        _sources[3],
        _sources[4],
        static (v1, v2, v3, v4, v5) => v1 + v2 + v3 + v4 + v5);

    /// <summary>Constructs the extension pipeline for 6 typed sources.</summary>
    /// <returns>The unshared pipeline instance.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IObservable<int> CreateExtension6() => LinqExtensions.CombineLatest(
        _sources[0],
        _sources[1],
        _sources[2],
        _sources[3],
        _sources[4],
        _sources[5],
        static (v1, v2, v3, v4, v5, v6) => v1 + v2 + v3 + v4 + v5 + v6);

    /// <summary>Constructs the extension pipeline for 7 typed sources.</summary>
    /// <returns>The unshared pipeline instance.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IObservable<int> CreateExtension7() => LinqExtensions.CombineLatest(
        _sources[0],
        _sources[1],
        _sources[2],
        _sources[3],
        _sources[4],
        _sources[5],
        _sources[6],
        static (v1, v2, v3, v4, v5, v6, v7) => v1 + v2 + v3 + v4 + v5 + v6 + v7);

    /// <summary>Constructs the extension pipeline for 8 typed sources.</summary>
    /// <returns>The unshared pipeline instance.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IObservable<int> CreateExtension8() => LinqExtensions.CombineLatest(
        _sources[0],
        _sources[1],
        _sources[2],
        _sources[3],
        _sources[4],
        _sources[5],
        _sources[6],
        _sources[7],
        static (v1, v2, v3, v4, v5, v6, v7, v8) => v1 + v2 + v3 + v4 + v5 + v6 + v7 + v8);

    /// <summary>Constructs the extension pipeline for 9 typed sources.</summary>
    /// <returns>The unshared pipeline instance.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IObservable<int> CreateExtension9() => LinqExtensions.CombineLatest(
        _sources[0],
        _sources[1],
        _sources[2],
        _sources[3],
        _sources[4],
        _sources[5],
        _sources[6],
        _sources[7],
        _sources[8],
        static (v1, v2, v3, v4, v5, v6, v7, v8, v9) => v1 + v2 + v3 + v4 + v5 + v6 + v7 + v8 + v9);

    /// <summary>Constructs the extension pipeline for 10 typed sources.</summary>
    /// <returns>The unshared pipeline instance.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IObservable<int> CreateExtension10() => LinqExtensions.CombineLatest(
        _sources[0],
        _sources[1],
        _sources[2],
        _sources[3],
        _sources[4],
        _sources[5],
        _sources[6],
        _sources[7],
        _sources[8],
        _sources[9],
        static (v1, v2, v3, v4, v5, v6, v7, v8, v9, v10) => v1 + v2 + v3 + v4 + v5 + v6 + v7 + v8 + v9 + v10);

    /// <summary>Constructs the extension pipeline for 11 typed sources.</summary>
    /// <returns>The unshared pipeline instance.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IObservable<int> CreateExtension11() => LinqExtensions.CombineLatest(
        _sources[0],
        _sources[1],
        _sources[2],
        _sources[3],
        _sources[4],
        _sources[5],
        _sources[6],
        _sources[7],
        _sources[8],
        _sources[9],
        _sources[10],
        static (v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11) => v1 + v2 + v3 + v4 + v5 + v6 + v7 + v8 + v9 + v10 + v11);

    /// <summary>Constructs the extension pipeline for 12 typed sources.</summary>
    /// <returns>The unshared pipeline instance.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IObservable<int> CreateExtension12() => LinqExtensions.CombineLatest(
        _sources[0],
        _sources[1],
        _sources[2],
        _sources[3],
        _sources[4],
        _sources[5],
        _sources[6],
        _sources[7],
        _sources[8],
        _sources[9],
        _sources[10],
        _sources[11],
        static (v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12) => v1 + v2 + v3 + v4 + v5 + v6 + v7 + v8 + v9 + v10 + v11 + v12);

    /// <summary>Constructs the extension pipeline for 13 typed sources.</summary>
    /// <returns>The unshared pipeline instance.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IObservable<int> CreateExtension13() => LinqExtensions.CombineLatest(
        _sources[0],
        _sources[1],
        _sources[2],
        _sources[3],
        _sources[4],
        _sources[5],
        _sources[6],
        _sources[7],
        _sources[8],
        _sources[9],
        _sources[10],
        _sources[11],
        _sources[12],
        static (v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12, v13) => v1 + v2 + v3 + v4 + v5 + v6 + v7 + v8 + v9 + v10 + v11 + v12 + v13);

    /// <summary>Constructs the extension pipeline for 14 typed sources.</summary>
    /// <returns>The unshared pipeline instance.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IObservable<int> CreateExtension14() => LinqExtensions.CombineLatest(
        _sources[0],
        _sources[1],
        _sources[2],
        _sources[3],
        _sources[4],
        _sources[5],
        _sources[6],
        _sources[7],
        _sources[8],
        _sources[9],
        _sources[10],
        _sources[11],
        _sources[12],
        _sources[13],
        static (v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12, v13, v14) => v1 + v2 + v3 + v4 + v5 + v6 + v7 + v8 + v9 + v10 + v11 + v12 + v13 + v14);

    /// <summary>Constructs the extension pipeline for 15 typed sources.</summary>
    /// <returns>The unshared pipeline instance.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IObservable<int> CreateExtension15() => LinqExtensions.CombineLatest(
        _sources[0],
        _sources[1],
        _sources[2],
        _sources[3],
        _sources[4],
        _sources[5],
        _sources[6],
        _sources[7],
        _sources[8],
        _sources[9],
        _sources[10],
        _sources[11],
        _sources[12],
        _sources[13],
        _sources[14],
        static (v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12, v13, v14, v15) => v1 + v2 + v3 + v4 + v5 + v6 + v7 + v8 + v9 + v10 + v11 + v12 + v13 + v14 + v15);

    /// <summary>Constructs the extension pipeline for 16 typed sources.</summary>
    /// <returns>The unshared pipeline instance.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IObservable<int> CreateExtension16() => LinqExtensions.CombineLatest(
        _sources[0],
        _sources[1],
        _sources[2],
        _sources[3],
        _sources[4],
        _sources[5],
        _sources[6],
        _sources[7],
        _sources[8],
        _sources[9],
        _sources[10],
        _sources[11],
        _sources[12],
        _sources[13],
        _sources[14],
        _sources[15],
        static (v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12, v13, v14, v15, v16) => v1 + v2 + v3 + v4 + v5 + v6 + v7 + v8 + v9 + v10 + v11 + v12 + v13 + v14 + v15 + v16);
}
