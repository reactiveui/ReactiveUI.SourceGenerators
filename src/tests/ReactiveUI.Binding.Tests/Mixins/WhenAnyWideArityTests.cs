// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using ReactiveUI.Binding.Tests.TestModels;
using ReactiveUI.Binding.Tests.WhenAny;

namespace ReactiveUI.Binding.Tests.Mixins;

/// <summary>Reaches every arity of the runtime WhenAny overloads, which hand the selector an observed change rather than a bare value.</summary>
/// <remarks>
/// Each overload is called on the declaring class rather than as an extension method. Written as an
/// extension call the generated dispatch wins overload resolution, and the runtime overload these
/// assertions are about would never run.
/// </remarks>
public class WhenAnyWideArityTests
{
    /// <summary>The value every observed property starts out holding.</summary>
    private const string InitialValue = "a";

    /// <summary>The first and last observed values read together, which is what each test projects.</summary>
    private const string BothEnds = InitialValue + InitialValue;

    /// <summary>Observing one property hands the selector each observed change.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAny_OnOneProperty_ReportsTheSelectorResult()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityFixture();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenAnyUnsafe(
                fixture,
                x => x.Value1,
                static c1 => c1.Value)
            .Subscribe(seen.Add);

        await Assert.That(seen[0]).IsEqualTo(InitialValue);
    }

    /// <summary>Observing two properties hands the selector each observed change.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAny_OnTwoProperties_ReportsTheSelectorResult()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityFixture();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenAnyUnsafe(
                fixture,
                x => x.Value1,
                x => x.Value2,
                static (c1, c2) => c1.Value + c2.Value)
            .Subscribe(seen.Add);

        await Assert.That(seen[0]).IsEqualTo(BothEnds);
    }

    /// <summary>Observing three properties hands the selector each observed change.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAny_OnThreeProperties_ReportsTheSelectorResult()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityFixture();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenAnyUnsafe(
                fixture,
                x => x.Value1,
                x => x.Value2,
                x => x.Value3,
                static (c1, c2, c3) => c1.Value + c3.Value)
            .Subscribe(seen.Add);

        await Assert.That(seen[0]).IsEqualTo(BothEnds);
    }

    /// <summary>Observing four properties hands the selector each observed change.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAny_OnFourProperties_ReportsTheSelectorResult()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityFixture();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenAnyUnsafe(
                fixture,
                x => x.Value1,
                x => x.Value2,
                x => x.Value3,
                x => x.Value4,
                static (c1, c2, c3, c4) => c1.Value + c4.Value)
            .Subscribe(seen.Add);

        await Assert.That(seen[0]).IsEqualTo(BothEnds);
    }

    /// <summary>Observing five properties hands the selector each observed change.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAny_OnFiveProperties_ReportsTheSelectorResult()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityFixture();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenAnyUnsafe(
                fixture,
                x => x.Value1,
                x => x.Value2,
                x => x.Value3,
                x => x.Value4,
                x => x.Value5,
                static (c1, c2, c3, c4, c5) => c1.Value + c5.Value)
            .Subscribe(seen.Add);

        await Assert.That(seen[0]).IsEqualTo(BothEnds);
    }

    /// <summary>Observing six properties hands the selector each observed change.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAny_OnSixProperties_ReportsTheSelectorResult()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityFixture();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenAnyUnsafe(
                fixture,
                x => x.Value1,
                x => x.Value2,
                x => x.Value3,
                x => x.Value4,
                x => x.Value5,
                x => x.Value6,
                static (c1, c2, c3, c4, c5, c6) => c1.Value + c6.Value)
            .Subscribe(seen.Add);

        await Assert.That(seen[0]).IsEqualTo(BothEnds);
    }

    /// <summary>Observing seven properties hands the selector each observed change.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAny_OnSevenProperties_ReportsTheSelectorResult()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityFixture();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenAnyUnsafe(
                fixture,
                x => x.Value1,
                x => x.Value2,
                x => x.Value3,
                x => x.Value4,
                x => x.Value5,
                x => x.Value6,
                x => x.Value7,
                static (c1, c2, c3, c4, c5, c6, c7) => c1.Value + c7.Value)
            .Subscribe(seen.Add);

        await Assert.That(seen[0]).IsEqualTo(BothEnds);
    }

    /// <summary>Observing eight properties hands the selector each observed change.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAny_OnEightProperties_ReportsTheSelectorResult()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityFixture();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenAnyUnsafe(
                fixture,
                x => x.Value1,
                x => x.Value2,
                x => x.Value3,
                x => x.Value4,
                x => x.Value5,
                x => x.Value6,
                x => x.Value7,
                x => x.Value8,
                static (c1, c2, c3, c4, c5, c6, c7, c8) => c1.Value + c8.Value)
            .Subscribe(seen.Add);

        await Assert.That(seen[0]).IsEqualTo(BothEnds);
    }

    /// <summary>Observing nine properties hands the selector each observed change.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAny_OnNineProperties_ReportsTheSelectorResult()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityFixture();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenAnyUnsafe(
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
                static (c1, c2, c3, c4, c5, c6, c7, c8, c9) => c1.Value + c9.Value)
            .Subscribe(seen.Add);

        await Assert.That(seen[0]).IsEqualTo(BothEnds);
    }

    /// <summary>Observing ten properties hands the selector each observed change.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAny_OnTenProperties_ReportsTheSelectorResult()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityFixture();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenAnyUnsafe(
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
                static (c1, c2, c3, c4, c5, c6, c7, c8, c9, c10) => c1.Value + c10.Value)
            .Subscribe(seen.Add);

        await Assert.That(seen[0]).IsEqualTo(BothEnds);
    }

    /// <summary>Observing eleven properties hands the selector each observed change.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAny_OnElevenProperties_ReportsTheSelectorResult()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityFixture();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenAnyUnsafe(
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
                static (c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11) => c1.Value + c11.Value)
            .Subscribe(seen.Add);

        await Assert.That(seen[0]).IsEqualTo(BothEnds);
    }

    /// <summary>Observing twelve properties hands the selector each observed change.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAny_OnTwelveProperties_ReportsTheSelectorResult()
    {
        WhenAnyTests.EnsureInitialized();

        var fixture = new WideArityFixture();
        var seen = new List<string>();

        using var subscription = ReactiveUIBindingExtensions.WhenAnyUnsafe(
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
                static (c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11, c12) => c1.Value + c12.Value)
            .Subscribe(seen.Add);

        await Assert.That(seen[0]).IsEqualTo(BothEnds);
    }
}
