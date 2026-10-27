// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using ReactiveUI.Binding.Tests.TestModels;
using ReactiveUI.Binding.Tests.WhenAny;

namespace ReactiveUI.Binding.Tests.Mixins;

/// <summary>Reaches every arity of the runtime WhenChanged overloads, which observe after a value changes.</summary>
/// <remarks>
/// Each overload is called on the declaring class rather than as an extension method. Written as an
/// extension call the generated dispatch wins overload resolution, and the runtime overload these
/// assertions are about would never run.
/// </remarks>
public class WhenChangedWideArityTests
{
    /// <summary>The value every observed property starts out holding.</summary>
    private const string InitialValue = "a";

    /// <summary>The first and last observed values read together, which is what each test projects.</summary>
    private const string BothEnds = InitialValue + InitialValue;

    /// <summary>Observing one property reports the first and last observed value.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanged_OnOneProperty_ReportsTheObservedValues()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityFixture();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenChangedUnsafe(
                fixture,
                x => x.Value1)
            .Subscribe(seen.Add);

        await Assert.That(seen[0]).IsEqualTo(InitialValue);
    }

    /// <summary>Observing two properties reports the first and last observed value.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanged_OnTwoProperties_ReportsTheObservedValues()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityFixture();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenChangedUnsafe(
                fixture,
                x => x.Value1,
                x => x.Value2)
            .Subscribe(v => seen.Add(v.Property1 + v.Property2));

        await Assert.That(seen[0]).IsEqualTo(BothEnds);
    }

    /// <summary>Observing three properties reports the first and last observed value.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanged_OnThreeProperties_ReportsTheObservedValues()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityFixture();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenChangedUnsafe(
                fixture,
                x => x.Value1,
                x => x.Value2,
                x => x.Value3)
            .Subscribe(v => seen.Add(v.Property1 + v.Property3));

        await Assert.That(seen[0]).IsEqualTo(BothEnds);
    }

    /// <summary>Observing four properties reports the first and last observed value.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanged_OnFourProperties_ReportsTheObservedValues()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityFixture();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenChangedUnsafe(
                fixture,
                x => x.Value1,
                x => x.Value2,
                x => x.Value3,
                x => x.Value4)
            .Subscribe(v => seen.Add(v.Property1 + v.Property4));

        await Assert.That(seen[0]).IsEqualTo(BothEnds);
    }

    /// <summary>Observing five properties reports the first and last observed value.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanged_OnFiveProperties_ReportsTheObservedValues()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityFixture();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenChangedUnsafe(
                fixture,
                x => x.Value1,
                x => x.Value2,
                x => x.Value3,
                x => x.Value4,
                x => x.Value5)
            .Subscribe(v => seen.Add(v.Property1 + v.Property5));

        await Assert.That(seen[0]).IsEqualTo(BothEnds);
    }

    /// <summary>Observing six properties reports the first and last observed value.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanged_OnSixProperties_ReportsTheObservedValues()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityFixture();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenChangedUnsafe(
                fixture,
                x => x.Value1,
                x => x.Value2,
                x => x.Value3,
                x => x.Value4,
                x => x.Value5,
                x => x.Value6)
            .Subscribe(v => seen.Add(v.Property1 + v.Property6));

        await Assert.That(seen[0]).IsEqualTo(BothEnds);
    }

    /// <summary>Observing seven properties reports the first and last observed value.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanged_OnSevenProperties_ReportsTheObservedValues()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityFixture();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenChangedUnsafe(
                fixture,
                x => x.Value1,
                x => x.Value2,
                x => x.Value3,
                x => x.Value4,
                x => x.Value5,
                x => x.Value6,
                x => x.Value7)
            .Subscribe(v => seen.Add(v.Property1 + v.Property7));

        await Assert.That(seen[0]).IsEqualTo(BothEnds);
    }

    /// <summary>Observing eight properties reports the first and last observed value.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanged_OnEightProperties_ReportsTheObservedValues()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityFixture();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenChangedUnsafe(
                fixture,
                x => x.Value1,
                x => x.Value2,
                x => x.Value3,
                x => x.Value4,
                x => x.Value5,
                x => x.Value6,
                x => x.Value7,
                x => x.Value8)
            .Subscribe(v => seen.Add(v.Property1 + v.Property8));

        await Assert.That(seen[0]).IsEqualTo(BothEnds);
    }

    /// <summary>Observing nine properties reports the first and last observed value.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanged_OnNineProperties_ReportsTheObservedValues()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityFixture();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenChangedUnsafe(
                fixture,
                x => x.Value1,
                x => x.Value2,
                x => x.Value3,
                x => x.Value4,
                x => x.Value5,
                x => x.Value6,
                x => x.Value7,
                x => x.Value8,
                x => x.Value9)
            .Subscribe(v => seen.Add(v.Property1 + v.Property9));

        await Assert.That(seen[0]).IsEqualTo(BothEnds);
    }

    /// <summary>Observing ten properties reports the first and last observed value.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanged_OnTenProperties_ReportsTheObservedValues()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityFixture();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenChangedUnsafe(
                fixture,
                x => x.Value1,
                x => x.Value2,
                x => x.Value3,
                x => x.Value4,
                x => x.Value5,
                x => x.Value6,
                x => x.Value7,
                x => x.Value8,
                x => x.Value9,
                x => x.Value10)
            .Subscribe(v => seen.Add(v.Property1 + v.Property10));

        await Assert.That(seen[0]).IsEqualTo(BothEnds);
    }

    /// <summary>Observing eleven properties reports the first and last observed value.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanged_OnElevenProperties_ReportsTheObservedValues()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityFixture();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenChangedUnsafe(
                fixture,
                x => x.Value1,
                x => x.Value2,
                x => x.Value3,
                x => x.Value4,
                x => x.Value5,
                x => x.Value6,
                x => x.Value7,
                x => x.Value8,
                x => x.Value9,
                x => x.Value10,
                x => x.Value11)
            .Subscribe(v => seen.Add(v.Property1 + v.Property11));

        await Assert.That(seen[0]).IsEqualTo(BothEnds);
    }

    /// <summary>Observing twelve properties reports the first and last observed value.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanged_OnTwelveProperties_ReportsTheObservedValues()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityFixture();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenChangedUnsafe(
                fixture,
                x => x.Value1,
                x => x.Value2,
                x => x.Value3,
                x => x.Value4,
                x => x.Value5,
                x => x.Value6,
                x => x.Value7,
                x => x.Value8,
                x => x.Value9,
                x => x.Value10,
                x => x.Value11,
                x => x.Value12)
            .Subscribe(v => seen.Add(v.Property1 + v.Property12));

        await Assert.That(seen[0]).IsEqualTo(BothEnds);
    }

    /// <summary>Observing thirteen properties reports the first and last observed value.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanged_OnThirteenProperties_ReportsTheObservedValues()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityFixture();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenChangedUnsafe(
                fixture,
                x => x.Value1,
                x => x.Value2,
                x => x.Value3,
                x => x.Value4,
                x => x.Value5,
                x => x.Value6,
                x => x.Value7,
                x => x.Value8,
                x => x.Value9,
                x => x.Value10,
                x => x.Value11,
                x => x.Value12,
                x => x.Value13)
            .Subscribe(v => seen.Add(v.Property1 + v.Property13));

        await Assert.That(seen[0]).IsEqualTo(BothEnds);
    }

    /// <summary>Observing fourteen properties reports the first and last observed value.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanged_OnFourteenProperties_ReportsTheObservedValues()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityFixture();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenChangedUnsafe(
                fixture,
                x => x.Value1,
                x => x.Value2,
                x => x.Value3,
                x => x.Value4,
                x => x.Value5,
                x => x.Value6,
                x => x.Value7,
                x => x.Value8,
                x => x.Value9,
                x => x.Value10,
                x => x.Value11,
                x => x.Value12,
                x => x.Value13,
                x => x.Value14)
            .Subscribe(v => seen.Add(v.Property1 + v.Property14));

        await Assert.That(seen[0]).IsEqualTo(BothEnds);
    }

    /// <summary>Observing fifteen properties reports the first and last observed value.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanged_OnFifteenProperties_ReportsTheObservedValues()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityFixture();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenChangedUnsafe(
                fixture,
                x => x.Value1,
                x => x.Value2,
                x => x.Value3,
                x => x.Value4,
                x => x.Value5,
                x => x.Value6,
                x => x.Value7,
                x => x.Value8,
                x => x.Value9,
                x => x.Value10,
                x => x.Value11,
                x => x.Value12,
                x => x.Value13,
                x => x.Value14,
                x => x.Value15)
            .Subscribe(v => seen.Add(v.Property1 + v.Property15));

        await Assert.That(seen[0]).IsEqualTo(BothEnds);
    }

    /// <summary>Observing sixteen properties reports the first and last observed value.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanged_OnSixteenProperties_ReportsTheObservedValues()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityFixture();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenChangedUnsafe(
                fixture,
                x => x.Value1,
                x => x.Value2,
                x => x.Value3,
                x => x.Value4,
                x => x.Value5,
                x => x.Value6,
                x => x.Value7,
                x => x.Value8,
                x => x.Value9,
                x => x.Value10,
                x => x.Value11,
                x => x.Value12,
                x => x.Value13,
                x => x.Value14,
                x => x.Value15,
                x => x.Value16)
            .Subscribe(v => seen.Add(v.Property1 + v.Property16));

        await Assert.That(seen[0]).IsEqualTo(BothEnds);
    }

    /// <summary>Projecting two observed properties reports what the selector returns.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanged_ProjectingTwoProperties_ReportsTheSelectorResult()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityFixture();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenChangedUnsafe(
                fixture,
                x => x.Value1,
                x => x.Value2,
                static (v1, v2) => v1 + v2)
            .Subscribe(seen.Add);

        await Assert.That(seen[0]).IsEqualTo(BothEnds);
    }

    /// <summary>Projecting three observed properties reports what the selector returns.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanged_ProjectingThreeProperties_ReportsTheSelectorResult()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityFixture();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenChangedUnsafe(
                fixture,
                x => x.Value1,
                x => x.Value2,
                x => x.Value3,
                static (v1, v2, v3) => v1 + v3)
            .Subscribe(seen.Add);

        await Assert.That(seen[0]).IsEqualTo(BothEnds);
    }

    /// <summary>Projecting four observed properties reports what the selector returns.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanged_ProjectingFourProperties_ReportsTheSelectorResult()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityFixture();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenChangedUnsafe(
                fixture,
                x => x.Value1,
                x => x.Value2,
                x => x.Value3,
                x => x.Value4,
                static (v1, v2, v3, v4) => v1 + v4)
            .Subscribe(seen.Add);

        await Assert.That(seen[0]).IsEqualTo(BothEnds);
    }

    /// <summary>Projecting five observed properties reports what the selector returns.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanged_ProjectingFiveProperties_ReportsTheSelectorResult()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityFixture();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenChangedUnsafe(
                fixture,
                x => x.Value1,
                x => x.Value2,
                x => x.Value3,
                x => x.Value4,
                x => x.Value5,
                static (v1, v2, v3, v4, v5) => v1 + v5)
            .Subscribe(seen.Add);

        await Assert.That(seen[0]).IsEqualTo(BothEnds);
    }

    /// <summary>Projecting six observed properties reports what the selector returns.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanged_ProjectingSixProperties_ReportsTheSelectorResult()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityFixture();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenChangedUnsafe(
                fixture,
                x => x.Value1,
                x => x.Value2,
                x => x.Value3,
                x => x.Value4,
                x => x.Value5,
                x => x.Value6,
                static (v1, v2, v3, v4, v5, v6) => v1 + v6)
            .Subscribe(seen.Add);

        await Assert.That(seen[0]).IsEqualTo(BothEnds);
    }

    /// <summary>Projecting seven observed properties reports what the selector returns.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanged_ProjectingSevenProperties_ReportsTheSelectorResult()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityFixture();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenChangedUnsafe(
                fixture,
                x => x.Value1,
                x => x.Value2,
                x => x.Value3,
                x => x.Value4,
                x => x.Value5,
                x => x.Value6,
                x => x.Value7,
                static (v1, v2, v3, v4, v5, v6, v7) => v1 + v7)
            .Subscribe(seen.Add);

        await Assert.That(seen[0]).IsEqualTo(BothEnds);
    }

    /// <summary>Projecting eight observed properties reports what the selector returns.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanged_ProjectingEightProperties_ReportsTheSelectorResult()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityFixture();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenChangedUnsafe(
                fixture,
                x => x.Value1,
                x => x.Value2,
                x => x.Value3,
                x => x.Value4,
                x => x.Value5,
                x => x.Value6,
                x => x.Value7,
                x => x.Value8,
                static (v1, v2, v3, v4, v5, v6, v7, v8) => v1 + v8)
            .Subscribe(seen.Add);

        await Assert.That(seen[0]).IsEqualTo(BothEnds);
    }

    /// <summary>Projecting nine observed properties reports what the selector returns.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanged_ProjectingNineProperties_ReportsTheSelectorResult()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityFixture();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenChangedUnsafe(
                fixture,
                x => x.Value1,
                x => x.Value2,
                x => x.Value3,
                x => x.Value4,
                x => x.Value5,
                x => x.Value6,
                x => x.Value7,
                x => x.Value8,
                x => x.Value9,
                static (v1, v2, v3, v4, v5, v6, v7, v8, v9) => v1 + v9)
            .Subscribe(seen.Add);

        await Assert.That(seen[0]).IsEqualTo(BothEnds);
    }

    /// <summary>Projecting ten observed properties reports what the selector returns.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanged_ProjectingTenProperties_ReportsTheSelectorResult()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityFixture();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenChangedUnsafe(
                fixture,
                x => x.Value1,
                x => x.Value2,
                x => x.Value3,
                x => x.Value4,
                x => x.Value5,
                x => x.Value6,
                x => x.Value7,
                x => x.Value8,
                x => x.Value9,
                x => x.Value10,
                static (v1, v2, v3, v4, v5, v6, v7, v8, v9, v10) => v1 + v10)
            .Subscribe(seen.Add);

        await Assert.That(seen[0]).IsEqualTo(BothEnds);
    }

    /// <summary>Projecting eleven observed properties reports what the selector returns.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanged_ProjectingElevenProperties_ReportsTheSelectorResult()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityFixture();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenChangedUnsafe(
                fixture,
                x => x.Value1,
                x => x.Value2,
                x => x.Value3,
                x => x.Value4,
                x => x.Value5,
                x => x.Value6,
                x => x.Value7,
                x => x.Value8,
                x => x.Value9,
                x => x.Value10,
                x => x.Value11,
                static (v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11) => v1 + v11)
            .Subscribe(seen.Add);

        await Assert.That(seen[0]).IsEqualTo(BothEnds);
    }

    /// <summary>Projecting twelve observed properties reports what the selector returns.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanged_ProjectingTwelveProperties_ReportsTheSelectorResult()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityFixture();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenChangedUnsafe(
                fixture,
                x => x.Value1,
                x => x.Value2,
                x => x.Value3,
                x => x.Value4,
                x => x.Value5,
                x => x.Value6,
                x => x.Value7,
                x => x.Value8,
                x => x.Value9,
                x => x.Value10,
                x => x.Value11,
                x => x.Value12,
                static (v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12) => v1 + v12)
            .Subscribe(seen.Add);

        await Assert.That(seen[0]).IsEqualTo(BothEnds);
    }

    /// <summary>Projecting thirteen observed properties reports what the selector returns.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanged_ProjectingThirteenProperties_ReportsTheSelectorResult()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityFixture();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenChangedUnsafe(
                fixture,
                x => x.Value1,
                x => x.Value2,
                x => x.Value3,
                x => x.Value4,
                x => x.Value5,
                x => x.Value6,
                x => x.Value7,
                x => x.Value8,
                x => x.Value9,
                x => x.Value10,
                x => x.Value11,
                x => x.Value12,
                x => x.Value13,
                static (v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12, v13) => v1 + v13)
            .Subscribe(seen.Add);

        await Assert.That(seen[0]).IsEqualTo(BothEnds);
    }

    /// <summary>Projecting fourteen observed properties reports what the selector returns.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanged_ProjectingFourteenProperties_ReportsTheSelectorResult()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityFixture();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenChangedUnsafe(
                fixture,
                x => x.Value1,
                x => x.Value2,
                x => x.Value3,
                x => x.Value4,
                x => x.Value5,
                x => x.Value6,
                x => x.Value7,
                x => x.Value8,
                x => x.Value9,
                x => x.Value10,
                x => x.Value11,
                x => x.Value12,
                x => x.Value13,
                x => x.Value14,
                static (v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12, v13, v14) => v1 + v14)
            .Subscribe(seen.Add);

        await Assert.That(seen[0]).IsEqualTo(BothEnds);
    }

    /// <summary>Projecting fifteen observed properties reports what the selector returns.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanged_ProjectingFifteenProperties_ReportsTheSelectorResult()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityFixture();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenChangedUnsafe(
                fixture,
                x => x.Value1,
                x => x.Value2,
                x => x.Value3,
                x => x.Value4,
                x => x.Value5,
                x => x.Value6,
                x => x.Value7,
                x => x.Value8,
                x => x.Value9,
                x => x.Value10,
                x => x.Value11,
                x => x.Value12,
                x => x.Value13,
                x => x.Value14,
                x => x.Value15,
                static (v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12, v13, v14, v15) => v1 + v15)
            .Subscribe(seen.Add);

        await Assert.That(seen[0]).IsEqualTo(BothEnds);
    }

    /// <summary>Projecting sixteen observed properties reports what the selector returns.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanged_ProjectingSixteenProperties_ReportsTheSelectorResult()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityFixture();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenChangedUnsafe(
                fixture,
                x => x.Value1,
                x => x.Value2,
                x => x.Value3,
                x => x.Value4,
                x => x.Value5,
                x => x.Value6,
                x => x.Value7,
                x => x.Value8,
                x => x.Value9,
                x => x.Value10,
                x => x.Value11,
                x => x.Value12,
                x => x.Value13,
                x => x.Value14,
                x => x.Value15,
                x => x.Value16,
                static (v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12, v13, v14, v15, v16) => v1 + v16)
            .Subscribe(seen.Add);

        await Assert.That(seen[0]).IsEqualTo(BothEnds);
    }
}
