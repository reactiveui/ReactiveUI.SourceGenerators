// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.GeneratedCode.TestModels.Scenarios;
using ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

namespace ReactiveUI.Binding.GeneratedCode.Tests.WhenAnyValue;

/// <summary>Tests that the source-generator-generated WhenAnyValue code works correctly at runtime.</summary>
public partial class WhenAnyValueTests
{
    /// <summary>The expected emission count after an initial value plus one change.</summary>
    private const int ExpectedEmissionCount = 2;

    /// <summary>The second emission index.</summary>
    private const int SecondIndex = 2;

    /// <summary>The expected emission count after an initial value plus three changes.</summary>
    private const int FourEmissionCount = 4;

    /// <summary>The third emission index.</summary>
    private const int ThirdIndex = 3;

    /// <summary>Verifies that single-property WhenAnyValue emits initial value and subsequent changes.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_EmitsInitialAndChanges()
    {
        var fixture = new WhenAnyTestFixture { Value1 = "A" };
        var values = new List<string>();

        using var sub = WhenAnyValueScenarios.SingleProperty(fixture)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0]).IsEqualTo("A");

        fixture.Value1 = "B";

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(ExpectedEmissionCount);
        await Assert.That(values).Contains("B");
    }

    /// <summary>Verifies that two-property WhenAnyValue emits tuples.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TwoProperties_EmitsTuples()
    {
        var fixture = new WhenAnyTestFixture { Value1 = "A", Value2 = "B" };
        var values = new List<PropertyValues<string, string>>();

        using var sub = WhenAnyValueScenarios.TwoProperties(fixture)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0].Property1).IsEqualTo("A");
        await Assert.That(values[0].Property2).IsEqualTo("B");
    }

    /// <summary>Verifies that three-property WhenAnyValue emits tuples.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ThreeProperties_EmitsTuples()
    {
        var fixture = new WhenAnyTestFixture { Value1 = "A", Value2 = "B", Value3 = "C" };
        var values = new List<PropertyValues<string, string, string>>();

        using var sub = WhenAnyValueScenarios.ThreeProperties(fixture)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0].Property1).IsEqualTo("A");
        await Assert.That(values[0].Property2).IsEqualTo("B");
        await Assert.That(values[0].Property3).IsEqualTo("C");
    }

    /// <summary>Verifies that WhenAnyValue with a selector projects values correctly.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithSelector_ProjectsValues()
    {
        var fixture = new WhenAnyTestFixture { Value1 = "Hello", Value2 = "World" };
        var values = new List<string>();

        using var sub = WhenAnyValueScenarios.WithSelector_TwoProperties(fixture)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0]).IsEqualTo("Hello_World");
    }

    /// <summary>Verifies that WhenAnyValue emits all sequential changes.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SequentialChanges_EmitsAll()
    {
        var fixture = new WhenAnyTestFixture { Value1 = "A" };
        var values = new List<string>();

        using var sub = WhenAnyValueScenarios.SingleProperty(fixture)
            .Subscribe(values.Add);

        fixture.Value1 = "B";
        fixture.Value1 = "C";
        fixture.Value1 = "D";

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(FourEmissionCount);
        await Assert.That(values[0]).IsEqualTo("A");
        await Assert.That(values[1]).IsEqualTo("B");
        await Assert.That(values[SecondIndex]).IsEqualTo("C");
        await Assert.That(values[ThirdIndex]).IsEqualTo("D");
    }
}
