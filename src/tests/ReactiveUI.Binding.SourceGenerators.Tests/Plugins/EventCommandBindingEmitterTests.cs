// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Plugins.CommandBinding;

namespace ReactiveUI.Binding.SourceGenerators.Tests.Plugins;

/// <summary>Tests for <see cref="EventCommandBindingEmitter"/>.</summary>
public class EventCommandBindingEmitterTests
{
    /// <summary>The event args type the handlers are declared with.</summary>
    private const string EventArgsType = "global::System.EventArgs";

    /// <summary>A handler with a parameter reads it once and both asks and runs the command with it.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AppendHandler_WithParameter_ReadsTheParameterThenRunsTheCommandWithIt()
    {
        const string paramAccess = "viewModel.Selected";
        var expected = new SourceWriter();
        EventCommandBindingEmitter.AppendHandlerDeclaration(expected, EventArgsType, false);
        _ = expected.Var("param", paramAccess);
        EventCommandBindingEmitter.AppendHandlerExecution(expected, "param");
        var actual = new SourceWriter();

        EventCommandBindingEmitter.AppendHandler(actual, EventArgsType, false, paramAccess);

        await Assert.That(actual.ToString()).IsEqualTo(expected.ToString());
    }

    /// <summary>A handler without a parameter asks and runs the command with null and reads nothing.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AppendHandler_WithoutParameter_RunsTheCommandWithNull()
    {
        var expected = new SourceWriter();
        EventCommandBindingEmitter.AppendHandlerDeclaration(expected, EventArgsType, true);
        EventCommandBindingEmitter.AppendHandlerExecution(expected, "null");
        var actual = new SourceWriter();

        EventCommandBindingEmitter.AppendHandler(actual, EventArgsType, true, null);

        await Assert.That(actual.ToString()).IsEqualTo(expected.ToString());
        await Assert.That(actual.ToString()).DoesNotContain("var param");
    }

    /// <summary>The command-only return hands back the serial subscription and closes the member.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AppendCommandOnlyReturn_ReturnsTheSubscriptionAndClosesTheMember()
    {
        var actual = new SourceWriter().Indent();

        EventCommandBindingEmitter.AppendCommandOnlyReturn(actual);

        await Assert.That(actual.ToString()).IsEqualTo(
            "    return new global::ReactiveUI.Primitives.Disposables.MultipleDisposable(__cmdSub, serial);\n}\n");
    }
}
