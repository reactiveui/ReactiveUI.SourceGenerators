// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.Mixins;

/// <summary>The assertion every overload's refusal is checked with.</summary>
/// <remarks>
/// A refusal is only useful if it tells the caller what to do instead, so the message is checked as well as
/// the exception. Shared because each API declares one refusal per arity and they all carry the same contract.
/// </remarks>
internal static class RefusalAssertions
{
    /// <summary>Asserts that a call refuses and names the overload that resolves the expression.</summary>
    /// <param name="call">The call expected to refuse.</param>
    /// <param name="twin">The name the message has to offer.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    internal static async Task AssertRefused(Func<object?> call, string twin)
    {
        var error = await Assert.That(call).Throws<InvalidOperationException>();

        await Assert.That(error!.Message).Contains(twin);
    }
}
