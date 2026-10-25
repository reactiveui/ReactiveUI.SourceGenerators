// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.Interactions;

/// <summary>Tests for <see cref="InteractionContext{TInput, TOutput}"/>.</summary>
public class InteractionContextTests
{
    /// <summary>A sample input value used across the interaction-context tests.</summary>
    private const string SampleInput = "input";

    /// <summary>A sample output value set on the interaction context.</summary>
    private const int SampleOutput = 42;

    /// <summary>A secondary output value used to verify SetOutput cannot be called twice.</summary>
    private const int SecondOutput = 2;

    /// <summary>Verifies that Input returns the value passed at construction.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Input_ReturnsConstructionValue()
    {
        var context = CreateContext("hello");
        await Assert.That(context.Input).IsEqualTo("hello");
    }

    /// <summary>Verifies that IsHandled is false before SetOutput is called.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task IsHandled_FalseBeforeSetOutput()
    {
        var context = CreateContext(SampleInput);
        await Assert.That(context.IsHandled).IsFalse();
    }

    /// <summary>Verifies that SetOutput sets IsHandled to true.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task SetOutput_SetsIsHandledTrue()
    {
        var context = CreateContext(SampleInput);
        context.SetOutput(SampleOutput);
        await Assert.That(context.IsHandled).IsTrue();
    }

    /// <summary>Verifies that GetOutput returns the value set by SetOutput.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task GetOutput_ReturnsSetValue()
    {
        var context = CreateContext(SampleInput);
        context.SetOutput(SampleOutput);
        await Assert.That(context.GetOutput()).IsEqualTo(SampleOutput);
    }

    /// <summary>Verifies that calling SetOutput twice throws InvalidOperationException.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task SetOutput_CalledTwice_Throws()
    {
        var context = CreateContext(SampleInput);
        context.SetOutput(1);
        await Assert.That(() => context.SetOutput(SecondOutput)).Throws<InvalidOperationException>();
    }

    /// <summary>Verifies that GetOutput throws when output has not been set.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task GetOutput_BeforeSetOutput_Throws()
    {
        var context = CreateContext(SampleInput);
        await Assert.That(context.GetOutput).Throws<InvalidOperationException>();
    }

    /// <summary>Creates an <see cref="InteractionContext{TInput, TOutput}"/>. The internal constructor is directly accessible to this assembly via <c>InternalsVisibleTo</c>.</summary>
    /// <param name="input">The input value for the interaction context.</param>
    /// <returns>A new <see cref="InteractionContext{TInput, TOutput}"/> instance.</returns>
    private static InteractionContext<string, int> CreateContext(string input) =>
        new(input);
}
