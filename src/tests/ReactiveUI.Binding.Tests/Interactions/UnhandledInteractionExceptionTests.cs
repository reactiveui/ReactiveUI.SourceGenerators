// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.Interactions;

/// <summary>Tests for <see cref="UnhandledInteractionException{TInput, TOutput}"/>.</summary>
public class UnhandledInteractionExceptionTests
{
    /// <summary>Verifies that the parameterless constructor creates a valid instance.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task ParameterlessConstructor_CreatesInstance()
    {
        var ex = new UnhandledInteractionException<string, bool>();
        await Assert.That(ex.Interaction).IsNull();
        await Assert.That(ex.Message).IsNotNull();
    }

    /// <summary>Verifies that the message constructor sets the message.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task MessageConstructor_SetsMessage()
    {
        var ex = new UnhandledInteractionException<string, bool>("test message");
        await Assert.That(ex.Message).IsEqualTo("test message");
        await Assert.That(ex.Interaction).IsNull();
    }

    /// <summary>Verifies that the message+innerException constructor sets both properties.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task MessageAndInnerExceptionConstructor_SetsBoth()
    {
        var inner = new InvalidOperationException("inner");
        var ex = new UnhandledInteractionException<string, bool>("outer", inner);
        await Assert.That(ex.Message).IsEqualTo("outer");
        await Assert.That(ex.InnerException).IsSameReferenceAs(inner);
    }
}
