// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.WhenAny;

/// <summary>Tests for WhenAnyValue with multi-property overloads (1 through 12 properties).</summary>
public class WhenAnyValueMultiPropertyTests
{
    /// <summary>The expected number of emissions after three successive value changes.</summary>
    private const int ExpectedThreeEmissions = 3;

    /// <summary>The index of the third emitted value.</summary>
    private const int ThirdValueIndex = 2;

    /// <summary>The expected number of emissions after a deep-chain value change.</summary>
    private const int ExpectedTwoEmissions = 2;

    /// <summary>Verifies that WhenAnyValue with 1 property emits values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyValue_1Property()
    {
        EnsureInitialized();

        var fixture = new WhenAnyTestFixture { Value1 = "A" };
        var values = new List<string>();

        using var sub = fixture.WhenAnyValueUnsafe(x => x.Value1)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0]).IsEqualTo("A");
    }

    /// <summary>Verifies that WhenAnyValue with 2 properties emits tuples.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyValue_2Properties()
    {
        EnsureInitialized();

        var fixture = new WhenAnyTestFixture { Value1 = "A", Value2 = "B" };
        var values = new List<PropertyValues<string, string>>();

        using var sub = fixture.WhenAnyValueUnsafe(x => x.Value1, x => x.Value2)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0].Property1).IsEqualTo("A");
        await Assert.That(values[0].Property2).IsEqualTo("B");
    }

    /// <summary>Verifies that WhenAnyValue with 3 properties emits tuples.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyValue_3Properties()
    {
        EnsureInitialized();

        var fixture = new WhenAnyTestFixture { Value1 = "A", Value2 = "B", Value3 = "C" };
        var values = new List<PropertyValues<string, string, string>>();

        using var sub = fixture.WhenAnyValueUnsafe(x => x.Value1, x => x.Value2, x => x.Value3)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0].Property1).IsEqualTo("A");
        await Assert.That(values[0].Property2).IsEqualTo("B");
        await Assert.That(values[0].Property3).IsEqualTo("C");
    }

    /// <summary>Verifies that WhenAnyValue with 4 properties emits tuples.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyValue_4Properties()
    {
        EnsureInitialized();

        var fixture = new WhenAnyTestFixture { Value1 = "A", Value2 = "B", Value3 = "C", Value4 = "D" };
        var values = new List<PropertyValues<string, string, string, string>>();

        using var sub = fixture.WhenAnyValueUnsafe(
                x => x.Value1,
                x => x.Value2,
                x => x.Value3,
                x => x.Value4)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0].Property1).IsEqualTo("A");
        await Assert.That(values[0].Property4).IsEqualTo("D");
    }

    /// <summary>Verifies that WhenAnyValue with 5 properties emits tuples.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyValue_5Properties()
    {
        EnsureInitialized();

        var fixture = new WhenAnyTestFixture { Value1 = "A", Value2 = "B", Value3 = "C", Value4 = "D", Value5 = "E" };
        var values =
            new List<PropertyValues<string, string, string, string, string>>();

        using var sub = fixture.WhenAnyValueUnsafe(
                x => x.Value1,
                x => x.Value2,
                x => x.Value3,
                x => x.Value4,
                x => x.Value5)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0].Property1).IsEqualTo("A");
        await Assert.That(values[0].Property5).IsEqualTo("E");
    }

    /// <summary>Verifies that WhenAnyValue with 6 properties emits tuples.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyValue_6Properties()
    {
        EnsureInitialized();

        var fixture = new WhenAnyTestFixture { Value1 = "A", Value2 = "B", Value3 = "C", Value4 = "D", Value5 = "E", Value6 = "F" };
        var values =
            new List<PropertyValues<string, string, string, string, string, string>>();

        using var sub = fixture.WhenAnyValueUnsafe(
                x => x.Value1,
                x => x.Value2,
                x => x.Value3,
                x => x.Value4,
                x => x.Value5,
                x => x.Value6)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0].Property1).IsEqualTo("A");
        await Assert.That(values[0].Property6).IsEqualTo("F");
    }

    /// <summary>Verifies that WhenAnyValue with 7 properties emits tuples.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyValue_7Properties()
    {
        EnsureInitialized();

        var fixture = new WhenAnyTestFixture { Value1 = "A", Value2 = "B", Value3 = "C", Value4 = "D", Value5 = "E", Value6 = "F", Value7 = "G" };
        var values =
            new List<PropertyValues<string, string, string, string, string, string, string>>();

        using var sub = fixture.WhenAnyValueUnsafe(
                x => x.Value1,
                x => x.Value2,
                x => x.Value3,
                x => x.Value4,
                x => x.Value5,
                x => x.Value6,
                x => x.Value7)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0].Property7).IsEqualTo("G");
    }

    /// <summary>Verifies that WhenAnyValue with 8 properties emits tuples.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyValue_8Properties()
    {
        EnsureInitialized();

        var fixture = new WhenAnyTestFixture { Value1 = "A", Value2 = "B", Value3 = "C", Value4 = "D", Value5 = "E", Value6 = "F", Value7 = "G", Value8 = "H" };

        string? lastItem8 = null;

        using var sub = fixture.WhenAnyValueUnsafe(
                x => x.Value1,
                x => x.Value2,
                x => x.Value3,
                x => x.Value4,
                x => x.Value5,
                x => x.Value6,
                x => x.Value7,
                x => x.Value8)
            .Subscribe(v => lastItem8 = v.Property8);

        await Assert.That(lastItem8).IsEqualTo("H");
    }

    /// <summary>Verifies that WhenAnyValue with 9 properties emits tuples.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyValue_9Properties()
    {
        EnsureInitialized();

        var fixture = new WhenAnyTestFixture { Value1 = "A", Value2 = "B", Value3 = "C", Value4 = "D", Value5 = "E", Value6 = "F", Value7 = "G", Value8 = "H", Value9 = "I" };

        string? lastItem9 = null;

        using var sub = fixture.WhenAnyValueUnsafe(
                x => x.Value1,
                x => x.Value2,
                x => x.Value3,
                x => x.Value4,
                x => x.Value5,
                x => x.Value6,
                x => x.Value7,
                x => x.Value8,
                x => x.Value9)
            .Subscribe(v => lastItem9 = v.Property9);

        await Assert.That(lastItem9).IsEqualTo("I");
    }

    /// <summary>Verifies that WhenAnyValue with 10 properties emits tuples.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyValue_10Properties()
    {
        EnsureInitialized();

        var fixture = new WhenAnyTestFixture { Value1 = "A", Value2 = "B", Value3 = "C", Value4 = "D", Value5 = "E", Value6 = "F", Value7 = "G", Value8 = "H", Value9 = "I", Value10 = "J" };

        string? lastItem10 = null;

        using var sub = fixture.WhenAnyValueUnsafe(
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
            .Subscribe(v => lastItem10 = v.Property10);

        await Assert.That(lastItem10).IsEqualTo("J");
    }

    /// <summary>Verifies that WhenAnyValue with 11 properties emits tuples.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyValue_11Properties()
    {
        EnsureInitialized();

        var fixture = new WhenAnyTestFixture
        {
            Value1 = "A",
            Value2 = "B",
            Value3 = "C",
            Value4 = "D",
            Value5 = "E",
            Value6 = "F",
            Value7 = "G",
            Value8 = "H",
            Value9 = "I",
            Value10 = "J",
            Value11 = "K",
        };

        string? lastItem11 = null;

        using var sub = fixture.WhenAnyValueUnsafe(
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
            .Subscribe(v => lastItem11 = v.Property11);

        await Assert.That(lastItem11).IsEqualTo("K");
    }

    /// <summary>Verifies that WhenAnyValue with 12 properties emits tuples.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyValue_12Properties()
    {
        EnsureInitialized();

        var fixture = new WhenAnyTestFixture
        {
            Value1 = "A",
            Value2 = "B",
            Value3 = "C",
            Value4 = "D",
            Value5 = "E",
            Value6 = "F",
            Value7 = "G",
            Value8 = "H",
            Value9 = "I",
            Value10 = "J",
            Value11 = "K",
            Value12 = "L",
        };

        string? lastItem12 = null;

        using var sub = fixture.WhenAnyValueUnsafe(
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
            .Subscribe(v => lastItem12 = v.Property12);

        await Assert.That(lastItem12).IsEqualTo("L");
    }

    /// <summary>Verifies that WhenAnyValue with 1 property emits sequential changes.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyValue_1Property_SequentialChanges()
    {
        EnsureInitialized();

        var fixture = new WhenAnyTestFixture { Value1 = "A" };
        var values = new List<string>();

        using var sub = fixture.WhenAnyValueUnsafe(x => x.Value1)
            .Subscribe(values.Add);

        fixture.Value1 = "B";
        fixture.Value1 = "C";

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(ExpectedThreeEmissions);
        await Assert.That(values[0]).IsEqualTo("A");
        await Assert.That(values[1]).IsEqualTo("B");
        await Assert.That(values[ThirdValueIndex]).IsEqualTo("C");
    }

    /// <summary>Verifies that WhenAnyValue with 2 properties and a selector works.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyValue_2Properties_WithSelector()
    {
        EnsureInitialized();

        var fixture = new WhenAnyTestFixture { Value1 = "Hello", Value2 = "World" };
        var values = new List<string>();

        using var sub = fixture.WhenAnyValueUnsafe(
                x => x.Value1,
                x => x.Value2,
                static (v1, v2) => $"{v1} {v2}")
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0]).IsEqualTo("Hello World");
    }

    /// <summary>Verifies that WhenAnyValue works with deep property chains.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyValue_DeepChain()
    {
        EnsureInitialized();

        var fixture = new HostTestFixture { Child = new() { IsNotNullString = "Deep" } };
        var values = new List<string>();

        using var sub = fixture.WhenAnyValueUnsafe(x => x.Child!.IsNotNullString)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0]).IsEqualTo("Deep");

        fixture.Child!.IsNotNullString = "Deeper";

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(ExpectedTwoEmissions);
        await Assert.That(values[1]).IsEqualTo("Deeper");
    }

    /// <summary>Resets and initializes the ReactiveUI binding infrastructure for testing.</summary>
    internal static void EnsureInitialized()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        _ = builder.WithCoreServices();
        _ = builder.BuildApp();
    }
}
