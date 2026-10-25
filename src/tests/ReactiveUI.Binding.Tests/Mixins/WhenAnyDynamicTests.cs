// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding.Tests.TestModels;
using ReactiveUI.Binding.Tests.WhenAny;

namespace ReactiveUI.Binding.Tests.Mixins;

/// <summary>
/// Tests for the WhenAnyDynamic overloads, which observe property chains named by an expression the
/// caller built rather than by a lambda the compiler could read.
/// </summary>
public class WhenAnyDynamicTests
{
    /// <summary>The value every observed property starts out holding.</summary>
    private const string InitialValue = "a";

    /// <summary>The value the last observed property is moved to.</summary>
    private const string MovedValue = "b";

    /// <summary>How many chains the one-chain overloads observe.</summary>
    private const int OneChain = 1;

    /// <summary>How many chains the two-chain overloads observe.</summary>
    private const int TwoChains = 2;

    /// <summary>How many chains the three-chain overloads observe.</summary>
    private const int ThreeChains = 3;

    /// <summary>How many chains the four-chain overloads observe.</summary>
    private const int FourChains = 4;

    /// <summary>How many chains the five-chain overloads observe.</summary>
    private const int FiveChains = 5;

    /// <summary>How many chains the six-chain overloads observe.</summary>
    private const int SixChains = 6;

    /// <summary>How many chains the seven-chain overloads observe.</summary>
    private const int SevenChains = 7;

    /// <summary>How many chains the eight-chain overloads observe.</summary>
    private const int EightChains = 8;

    /// <summary>How many chains the nine-chain overloads observe.</summary>
    private const int NineChains = 9;

    /// <summary>How many chains the ten-chain overloads observe.</summary>
    private const int TenChains = 10;

    /// <summary>How many chains the eleven-chain overloads observe.</summary>
    private const int ElevenChains = 11;

    /// <summary>How many chains the twelve-chain overloads observe.</summary>
    private const int TwelveChains = 12;

    /// <summary>What a chain reports with the distinct gate on: the seed, then the move.</summary>
    private static readonly string[] SeedThenMove = [InitialValue, MovedValue];

    /// <summary>What a chain reports with the gate off: the seed, the move, and the notification repeating it.</summary>
    private static readonly string[] SeedThenMoveTwice = [InitialValue, MovedValue, MovedValue];

    /// <summary>Observing 1 chain, reporting the combined value and each change.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Arity1_ReportsTheCombinedValueAndEachChange()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new DynamicChainFixture();
        var seen = new List<string>();

        using var subscription = fixture.WhenAnyDynamic(
            Body(x => x.P1),
            static c1 => string.Concat(c1.Value))
            .Subscribe(seen.Add);

        await Assert.That(seen[0]).IsEqualTo(Initial(OneChain));

        fixture.P1 = MovedValue;

        await Assert.That(seen[^1]).IsEqualTo(LastMoved(OneChain));
    }

    /// <summary>Observing 1 chain with the distinct gate named, reporting the combined value and each change.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Arity1WithDistinctSpecified_ReportsTheCombinedValueAndEachChange()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new DynamicChainFixture();
        var seen = new List<string>();

        using var subscription = fixture.WhenAnyDynamic(
            Body(x => x.P1),
            static c1 => string.Concat(c1.Value),
            true)
            .Subscribe(seen.Add);

        await Assert.That(seen[0]).IsEqualTo(Initial(OneChain));

        fixture.P1 = MovedValue;

        await Assert.That(seen[^1]).IsEqualTo(LastMoved(OneChain));
    }

    /// <summary>Observing 2 chains, reporting the combined value and each change.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Arity2_ReportsTheCombinedValueAndEachChange()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new DynamicChainFixture();
        var seen = new List<string>();

        using var subscription = fixture.WhenAnyDynamic(
            Body(x => x.P1),
            Body(x => x.P2),
            static (c1, c2) => string.Concat(c1.Value, c2.Value))
            .Subscribe(seen.Add);

        await Assert.That(seen[0]).IsEqualTo(Initial(TwoChains));

        fixture.P2 = MovedValue;

        await Assert.That(seen[^1]).IsEqualTo(LastMoved(TwoChains));
    }

    /// <summary>Observing 2 chains with the distinct gate named, reporting the combined value and each change.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Arity2WithDistinctSpecified_ReportsTheCombinedValueAndEachChange()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new DynamicChainFixture();
        var seen = new List<string>();

        using var subscription = fixture.WhenAnyDynamic(
            Body(x => x.P1),
            Body(x => x.P2),
            static (c1, c2) => string.Concat(c1.Value, c2.Value),
            true)
            .Subscribe(seen.Add);

        await Assert.That(seen[0]).IsEqualTo(Initial(TwoChains));

        fixture.P2 = MovedValue;

        await Assert.That(seen[^1]).IsEqualTo(LastMoved(TwoChains));
    }

    /// <summary>Observing 3 chains, reporting the combined value and each change.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Arity3_ReportsTheCombinedValueAndEachChange()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new DynamicChainFixture();
        var seen = new List<string>();

        using var subscription = fixture.WhenAnyDynamic(
            Body(x => x.P1),
            Body(x => x.P2),
            Body(x => x.P3),
            static (c1, c2, c3) => string.Concat(c1.Value, c2.Value, c3.Value))
            .Subscribe(seen.Add);

        await Assert.That(seen[0]).IsEqualTo(Initial(ThreeChains));

        fixture.P3 = MovedValue;

        await Assert.That(seen[^1]).IsEqualTo(LastMoved(ThreeChains));
    }

    /// <summary>Observing 3 chains with the distinct gate named, reporting the combined value and each change.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Arity3WithDistinctSpecified_ReportsTheCombinedValueAndEachChange()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new DynamicChainFixture();
        var seen = new List<string>();

        using var subscription = fixture.WhenAnyDynamic(
            Body(x => x.P1),
            Body(x => x.P2),
            Body(x => x.P3),
            static (c1, c2, c3) => string.Concat(c1.Value, c2.Value, c3.Value),
            true)
            .Subscribe(seen.Add);

        await Assert.That(seen[0]).IsEqualTo(Initial(ThreeChains));

        fixture.P3 = MovedValue;

        await Assert.That(seen[^1]).IsEqualTo(LastMoved(ThreeChains));
    }

    /// <summary>Observing 4 chains, reporting the combined value and each change.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Arity4_ReportsTheCombinedValueAndEachChange()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new DynamicChainFixture();
        var seen = new List<string>();

        using var subscription = fixture.WhenAnyDynamic(
            Body(x => x.P1),
            Body(x => x.P2),
            Body(x => x.P3),
            Body(x => x.P4),
            static (c1, c2, c3, c4) => string.Concat(
                c1.Value,
                c2.Value,
                c3.Value,
                c4.Value))
            .Subscribe(seen.Add);

        await Assert.That(seen[0]).IsEqualTo(Initial(FourChains));

        fixture.P4 = MovedValue;

        await Assert.That(seen[^1]).IsEqualTo(LastMoved(FourChains));
    }

    /// <summary>Observing 4 chains with the distinct gate named, reporting the combined value and each change.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Arity4WithDistinctSpecified_ReportsTheCombinedValueAndEachChange()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new DynamicChainFixture();
        var seen = new List<string>();

        using var subscription = fixture.WhenAnyDynamic(
            Body(x => x.P1),
            Body(x => x.P2),
            Body(x => x.P3),
            Body(x => x.P4),
            static (c1, c2, c3, c4) => string.Concat(
                c1.Value,
                c2.Value,
                c3.Value,
                c4.Value),
            true)
            .Subscribe(seen.Add);

        await Assert.That(seen[0]).IsEqualTo(Initial(FourChains));

        fixture.P4 = MovedValue;

        await Assert.That(seen[^1]).IsEqualTo(LastMoved(FourChains));
    }

    /// <summary>Observing 5 chains, reporting the combined value and each change.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Arity5_ReportsTheCombinedValueAndEachChange()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new DynamicChainFixture();
        var seen = new List<string>();

        using var subscription = fixture.WhenAnyDynamic(
            Body(x => x.P1),
            Body(x => x.P2),
            Body(x => x.P3),
            Body(x => x.P4),
            Body(x => x.P5),
            static (c1, c2, c3, c4, c5) => string.Concat(
                c1.Value,
                c2.Value,
                c3.Value,
                c4.Value,
                c5.Value))
            .Subscribe(seen.Add);

        await Assert.That(seen[0]).IsEqualTo(Initial(FiveChains));

        fixture.P5 = MovedValue;

        await Assert.That(seen[^1]).IsEqualTo(LastMoved(FiveChains));
    }

    /// <summary>Observing 5 chains with the distinct gate named, reporting the combined value and each change.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Arity5WithDistinctSpecified_ReportsTheCombinedValueAndEachChange()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new DynamicChainFixture();
        var seen = new List<string>();

        using var subscription = fixture.WhenAnyDynamic(
            Body(x => x.P1),
            Body(x => x.P2),
            Body(x => x.P3),
            Body(x => x.P4),
            Body(x => x.P5),
            static (c1, c2, c3, c4, c5) => string.Concat(
                c1.Value,
                c2.Value,
                c3.Value,
                c4.Value,
                c5.Value),
            true)
            .Subscribe(seen.Add);

        await Assert.That(seen[0]).IsEqualTo(Initial(FiveChains));

        fixture.P5 = MovedValue;

        await Assert.That(seen[^1]).IsEqualTo(LastMoved(FiveChains));
    }

    /// <summary>Observing 6 chains, reporting the combined value and each change.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Arity6_ReportsTheCombinedValueAndEachChange()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new DynamicChainFixture();
        var seen = new List<string>();

        using var subscription = fixture.WhenAnyDynamic(
            Body(x => x.P1),
            Body(x => x.P2),
            Body(x => x.P3),
            Body(x => x.P4),
            Body(x => x.P5),
            Body(x => x.P6),
            static (c1, c2, c3, c4, c5, c6) => string.Concat(
                c1.Value,
                c2.Value,
                c3.Value,
                c4.Value,
                c5.Value,
                c6.Value))
            .Subscribe(seen.Add);

        await Assert.That(seen[0]).IsEqualTo(Initial(SixChains));

        fixture.P6 = MovedValue;

        await Assert.That(seen[^1]).IsEqualTo(LastMoved(SixChains));
    }

    /// <summary>Observing 6 chains with the distinct gate named, reporting the combined value and each change.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Arity6WithDistinctSpecified_ReportsTheCombinedValueAndEachChange()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new DynamicChainFixture();
        var seen = new List<string>();

        using var subscription = fixture.WhenAnyDynamic(
            Body(x => x.P1),
            Body(x => x.P2),
            Body(x => x.P3),
            Body(x => x.P4),
            Body(x => x.P5),
            Body(x => x.P6),
            static (c1, c2, c3, c4, c5, c6) => string.Concat(
                c1.Value,
                c2.Value,
                c3.Value,
                c4.Value,
                c5.Value,
                c6.Value),
            true)
            .Subscribe(seen.Add);

        await Assert.That(seen[0]).IsEqualTo(Initial(SixChains));

        fixture.P6 = MovedValue;

        await Assert.That(seen[^1]).IsEqualTo(LastMoved(SixChains));
    }

    /// <summary>Observing 7 chains, reporting the combined value and each change.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Arity7_ReportsTheCombinedValueAndEachChange()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new DynamicChainFixture();
        var seen = new List<string>();

        using var subscription = fixture.WhenAnyDynamic(
            Body(x => x.P1),
            Body(x => x.P2),
            Body(x => x.P3),
            Body(x => x.P4),
            Body(x => x.P5),
            Body(x => x.P6),
            Body(x => x.P7),
            static (c1, c2, c3, c4, c5, c6, c7) => string.Concat(
                c1.Value,
                c2.Value,
                c3.Value,
                c4.Value,
                c5.Value,
                c6.Value,
                c7.Value))
            .Subscribe(seen.Add);

        await Assert.That(seen[0]).IsEqualTo(Initial(SevenChains));

        fixture.P7 = MovedValue;

        await Assert.That(seen[^1]).IsEqualTo(LastMoved(SevenChains));
    }

    /// <summary>Observing 7 chains with the distinct gate named, reporting the combined value and each change.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Arity7WithDistinctSpecified_ReportsTheCombinedValueAndEachChange()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new DynamicChainFixture();
        var seen = new List<string>();

        using var subscription = fixture.WhenAnyDynamic(
            Body(x => x.P1),
            Body(x => x.P2),
            Body(x => x.P3),
            Body(x => x.P4),
            Body(x => x.P5),
            Body(x => x.P6),
            Body(x => x.P7),
            static (c1, c2, c3, c4, c5, c6, c7) => string.Concat(
                c1.Value,
                c2.Value,
                c3.Value,
                c4.Value,
                c5.Value,
                c6.Value,
                c7.Value),
            true)
            .Subscribe(seen.Add);

        await Assert.That(seen[0]).IsEqualTo(Initial(SevenChains));

        fixture.P7 = MovedValue;

        await Assert.That(seen[^1]).IsEqualTo(LastMoved(SevenChains));
    }

    /// <summary>Observing 8 chains, reporting the combined value and each change.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Arity8_ReportsTheCombinedValueAndEachChange()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new DynamicChainFixture();
        var seen = new List<string>();

        using var subscription = fixture.WhenAnyDynamic(
            Body(x => x.P1),
            Body(x => x.P2),
            Body(x => x.P3),
            Body(x => x.P4),
            Body(x => x.P5),
            Body(x => x.P6),
            Body(x => x.P7),
            Body(x => x.P8),
            static (c1, c2, c3, c4, c5, c6, c7, c8) => string.Concat(
                c1.Value,
                c2.Value,
                c3.Value,
                c4.Value,
                c5.Value,
                c6.Value,
                c7.Value,
                c8.Value))
            .Subscribe(seen.Add);

        await Assert.That(seen[0]).IsEqualTo(Initial(EightChains));

        fixture.P8 = MovedValue;

        await Assert.That(seen[^1]).IsEqualTo(LastMoved(EightChains));
    }

    /// <summary>Observing 8 chains with the distinct gate named, reporting the combined value and each change.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Arity8WithDistinctSpecified_ReportsTheCombinedValueAndEachChange()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new DynamicChainFixture();
        var seen = new List<string>();

        using var subscription = fixture.WhenAnyDynamic(
            Body(x => x.P1),
            Body(x => x.P2),
            Body(x => x.P3),
            Body(x => x.P4),
            Body(x => x.P5),
            Body(x => x.P6),
            Body(x => x.P7),
            Body(x => x.P8),
            static (c1, c2, c3, c4, c5, c6, c7, c8) => string.Concat(
                c1.Value,
                c2.Value,
                c3.Value,
                c4.Value,
                c5.Value,
                c6.Value,
                c7.Value,
                c8.Value),
            true)
            .Subscribe(seen.Add);

        await Assert.That(seen[0]).IsEqualTo(Initial(EightChains));

        fixture.P8 = MovedValue;

        await Assert.That(seen[^1]).IsEqualTo(LastMoved(EightChains));
    }

    /// <summary>Observing 9 chains, reporting the combined value and each change.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Arity9_ReportsTheCombinedValueAndEachChange()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new DynamicChainFixture();
        var seen = new List<string>();

        using var subscription = fixture.WhenAnyDynamic(
            Body(x => x.P1),
            Body(x => x.P2),
            Body(x => x.P3),
            Body(x => x.P4),
            Body(x => x.P5),
            Body(x => x.P6),
            Body(x => x.P7),
            Body(x => x.P8),
            Body(x => x.P9),
            static (c1, c2, c3, c4, c5, c6, c7, c8, c9) => string.Concat(
                c1.Value,
                c2.Value,
                c3.Value,
                c4.Value,
                c5.Value,
                c6.Value,
                c7.Value,
                c8.Value,
                c9.Value))
            .Subscribe(seen.Add);

        await Assert.That(seen[0]).IsEqualTo(Initial(NineChains));

        fixture.P9 = MovedValue;

        await Assert.That(seen[^1]).IsEqualTo(LastMoved(NineChains));
    }

    /// <summary>Observing 9 chains with the distinct gate named, reporting the combined value and each change.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Arity9WithDistinctSpecified_ReportsTheCombinedValueAndEachChange()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new DynamicChainFixture();
        var seen = new List<string>();

        using var subscription = fixture.WhenAnyDynamic(
            Body(x => x.P1),
            Body(x => x.P2),
            Body(x => x.P3),
            Body(x => x.P4),
            Body(x => x.P5),
            Body(x => x.P6),
            Body(x => x.P7),
            Body(x => x.P8),
            Body(x => x.P9),
            static (c1, c2, c3, c4, c5, c6, c7, c8, c9) => string.Concat(
                c1.Value,
                c2.Value,
                c3.Value,
                c4.Value,
                c5.Value,
                c6.Value,
                c7.Value,
                c8.Value,
                c9.Value),
            true)
            .Subscribe(seen.Add);

        await Assert.That(seen[0]).IsEqualTo(Initial(NineChains));

        fixture.P9 = MovedValue;

        await Assert.That(seen[^1]).IsEqualTo(LastMoved(NineChains));
    }

    /// <summary>Observing 10 chains, reporting the combined value and each change.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Arity10_ReportsTheCombinedValueAndEachChange()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new DynamicChainFixture();
        var seen = new List<string>();

        using var subscription = fixture.WhenAnyDynamic(
            Body(x => x.P1),
            Body(x => x.P2),
            Body(x => x.P3),
            Body(x => x.P4),
            Body(x => x.P5),
            Body(x => x.P6),
            Body(x => x.P7),
            Body(x => x.P8),
            Body(x => x.P9),
            Body(x => x.P10),
            static (c1, c2, c3, c4, c5, c6, c7, c8, c9, c10) => string.Concat(
                c1.Value,
                c2.Value,
                c3.Value,
                c4.Value,
                c5.Value,
                c6.Value,
                c7.Value,
                c8.Value,
                c9.Value,
                c10.Value))
            .Subscribe(seen.Add);

        await Assert.That(seen[0]).IsEqualTo(Initial(TenChains));

        fixture.P10 = MovedValue;

        await Assert.That(seen[^1]).IsEqualTo(LastMoved(TenChains));
    }

    /// <summary>Observing 10 chains with the distinct gate named, reporting the combined value and each change.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Arity10WithDistinctSpecified_ReportsTheCombinedValueAndEachChange()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new DynamicChainFixture();
        var seen = new List<string>();

        using var subscription = fixture.WhenAnyDynamic(
            Body(x => x.P1),
            Body(x => x.P2),
            Body(x => x.P3),
            Body(x => x.P4),
            Body(x => x.P5),
            Body(x => x.P6),
            Body(x => x.P7),
            Body(x => x.P8),
            Body(x => x.P9),
            Body(x => x.P10),
            static (c1, c2, c3, c4, c5, c6, c7, c8, c9, c10) => string.Concat(
                c1.Value,
                c2.Value,
                c3.Value,
                c4.Value,
                c5.Value,
                c6.Value,
                c7.Value,
                c8.Value,
                c9.Value,
                c10.Value),
            true)
            .Subscribe(seen.Add);

        await Assert.That(seen[0]).IsEqualTo(Initial(TenChains));

        fixture.P10 = MovedValue;

        await Assert.That(seen[^1]).IsEqualTo(LastMoved(TenChains));
    }

    /// <summary>Observing 11 chains, reporting the combined value and each change.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Arity11_ReportsTheCombinedValueAndEachChange()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new DynamicChainFixture();
        var seen = new List<string>();

        using var subscription = fixture.WhenAnyDynamic(
            Body(x => x.P1),
            Body(x => x.P2),
            Body(x => x.P3),
            Body(x => x.P4),
            Body(x => x.P5),
            Body(x => x.P6),
            Body(x => x.P7),
            Body(x => x.P8),
            Body(x => x.P9),
            Body(x => x.P10),
            Body(x => x.P11),
            static (c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11) => string.Concat(
                c1.Value,
                c2.Value,
                c3.Value,
                c4.Value,
                c5.Value,
                c6.Value,
                c7.Value,
                c8.Value,
                c9.Value,
                c10.Value,
                c11.Value))
            .Subscribe(seen.Add);

        await Assert.That(seen[0]).IsEqualTo(Initial(ElevenChains));

        fixture.P11 = MovedValue;

        await Assert.That(seen[^1]).IsEqualTo(LastMoved(ElevenChains));
    }

    /// <summary>Observing 11 chains with the distinct gate named, reporting the combined value and each change.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Arity11WithDistinctSpecified_ReportsTheCombinedValueAndEachChange()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new DynamicChainFixture();
        var seen = new List<string>();

        using var subscription = fixture.WhenAnyDynamic(
            Body(x => x.P1),
            Body(x => x.P2),
            Body(x => x.P3),
            Body(x => x.P4),
            Body(x => x.P5),
            Body(x => x.P6),
            Body(x => x.P7),
            Body(x => x.P8),
            Body(x => x.P9),
            Body(x => x.P10),
            Body(x => x.P11),
            static (c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11) => string.Concat(
                c1.Value,
                c2.Value,
                c3.Value,
                c4.Value,
                c5.Value,
                c6.Value,
                c7.Value,
                c8.Value,
                c9.Value,
                c10.Value,
                c11.Value),
            true)
            .Subscribe(seen.Add);

        await Assert.That(seen[0]).IsEqualTo(Initial(ElevenChains));

        fixture.P11 = MovedValue;

        await Assert.That(seen[^1]).IsEqualTo(LastMoved(ElevenChains));
    }

    /// <summary>Observing 12 chains, reporting the combined value and each change.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Arity12_ReportsTheCombinedValueAndEachChange()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new DynamicChainFixture();
        var seen = new List<string>();

        using var subscription = fixture.WhenAnyDynamic(
            Body(x => x.P1),
            Body(x => x.P2),
            Body(x => x.P3),
            Body(x => x.P4),
            Body(x => x.P5),
            Body(x => x.P6),
            Body(x => x.P7),
            Body(x => x.P8),
            Body(x => x.P9),
            Body(x => x.P10),
            Body(x => x.P11),
            Body(x => x.P12),
            static (c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11, c12) => string.Concat(
                c1.Value,
                c2.Value,
                c3.Value,
                c4.Value,
                c5.Value,
                c6.Value,
                c7.Value,
                c8.Value,
                c9.Value,
                c10.Value,
                c11.Value,
                c12.Value))
            .Subscribe(seen.Add);

        await Assert.That(seen[0]).IsEqualTo(Initial(TwelveChains));

        fixture.P12 = MovedValue;

        await Assert.That(seen[^1]).IsEqualTo(LastMoved(TwelveChains));
    }

    /// <summary>Observing 12 chains with the distinct gate named, reporting the combined value and each change.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Arity12WithDistinctSpecified_ReportsTheCombinedValueAndEachChange()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new DynamicChainFixture();
        var seen = new List<string>();

        using var subscription = fixture.WhenAnyDynamic(
            Body(x => x.P1),
            Body(x => x.P2),
            Body(x => x.P3),
            Body(x => x.P4),
            Body(x => x.P5),
            Body(x => x.P6),
            Body(x => x.P7),
            Body(x => x.P8),
            Body(x => x.P9),
            Body(x => x.P10),
            Body(x => x.P11),
            Body(x => x.P12),
            static (c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11, c12) => string.Concat(
                c1.Value,
                c2.Value,
                c3.Value,
                c4.Value,
                c5.Value,
                c6.Value,
                c7.Value,
                c8.Value,
                c9.Value,
                c10.Value,
                c11.Value,
                c12.Value),
            true)
            .Subscribe(seen.Add);

        await Assert.That(seen[0]).IsEqualTo(Initial(TwelveChains));

        fixture.P12 = MovedValue;

        await Assert.That(seen[^1]).IsEqualTo(LastMoved(TwelveChains));
    }

    /// <summary>The distinct gate suppresses a notification that leaves the value where it was.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyDynamic_WithTheDistinctGateOn_SuppressesADuplicate()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new DynamicChainFixture();
        var seen = new List<string>();

        using var subscription = fixture
            .WhenAnyDynamic(Body(x => x.P1), static c1 => string.Concat(c1.Value), true)
            .Subscribe(seen.Add);

        fixture.P1 = MovedValue;
        fixture.P1 = MovedValue;

        await Assert.That(seen).IsEquivalentTo(SeedThenMove);
    }

    /// <summary>With the gate off, a notification is reported even where the value did not move.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyDynamic_WithTheDistinctGateOff_ReportsADuplicate()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new DynamicChainFixture();
        var seen = new List<string>();

        using var subscription = fixture
            .WhenAnyDynamic(Body(x => x.P1), static c1 => string.Concat(c1.Value), false)
            .Subscribe(seen.Add);

        fixture.P1 = MovedValue;
        fixture.P1 = MovedValue;

        await Assert.That(seen).IsEquivalentTo(SeedThenMoveTwice);
    }

    /// <summary>A chain through an intermediate follows the intermediate when it is replaced.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyDynamic_ThroughAnIntermediate_FollowsTheReplacement()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new DynamicChainFixture();
        var seen = new List<string>();

        using var subscription = fixture
            .WhenAnyDynamic(Body(x => x.Child!.Name), static c1 => string.Concat(c1.Value))
            .Subscribe(seen.Add);

        fixture.Child = new DynamicChainChild { Name = MovedValue };

        await Assert.That(seen[^1]).IsEqualTo(MovedValue);
    }

    /// <summary>An object to observe is required.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyDynamic_WithNoSender_ThrowsArgumentNullException()
    {
        DynamicChainFixture? fixture = null;

        await Assert.That(() => fixture!.WhenAnyDynamic(Body(x => x.P1), static c1 => string.Concat(c1.Value)))
            .ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>A selector is required.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyDynamic_WithNoSelector_ThrowsArgumentNullException()
    {
        var fixture = new DynamicChainFixture();

        await Assert.That(() => fixture.WhenAnyDynamic<DynamicChainFixture, string>(Body(x => x.P1), null!))
            .ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>The combined value while every observed chain still holds its initial value.</summary>
    /// <param name="count">How many chains were observed.</param>
    /// <returns>What the selector concatenates.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string Initial(int count) => string.Concat(Enumerable.Repeat(InitialValue, count));

    /// <summary>The combined value once the last of the observed chains has moved.</summary>
    /// <param name="count">How many chains were observed.</param>
    /// <returns>What the selector concatenates.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string LastMoved(int count) => Initial(count - 1) + MovedValue;

    /// <summary>Names a property chain the way a caller builds one at run time.</summary>
    /// <typeparam name="TValue">The type the chain ends at.</typeparam>
    /// <param name="property">The chain to name.</param>
    /// <returns>The expression body, which is what these overloads take.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static System.Linq.Expressions.Expression Body<TValue>(
        Expression<Func<DynamicChainFixture, TValue>> property) =>
        property.Body;
}
