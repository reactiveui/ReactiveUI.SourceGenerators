// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.ObservableForProperty;

/// <summary>Tests for <see cref="ObservedChange{TSender, TValue}"/>, the change every observation carries.</summary>
public class ObservedChangeTests
{
    /// <summary>The property value the observed change reports.</summary>
    private const int ObservedValue = 42;

    /// <summary>The object the observed change reports the value from.</summary>
    private const string ObservedSender = "sender";

    /// <summary>A change carries the sender, the value, and the expression it was observed through.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Constructor_ReportsSenderValueAndExpression()
    {
        var change = new ObservedChange<string, int>(ObservedSender, null, ObservedValue);

        await Assert.That(change.Sender).IsEqualTo(ObservedSender);
        await Assert.That(change.Value).IsEqualTo(ObservedValue);
        await Assert.That(change.Expression).IsNull();
    }
}
