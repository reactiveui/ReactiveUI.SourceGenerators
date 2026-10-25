// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.View;

/// <summary>Tests for the <see cref="ViewLocatorNotFoundException"/> class.</summary>
public class ViewLocatorNotFoundExceptionTests
{
    /// <summary>Verifies that the default constructor creates an exception with a default message.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DefaultConstructor_HasMessage()
    {
        var ex = new ViewLocatorNotFoundException();

        await Assert.That(ex.Message).IsNotNull();
        await Assert.That(ex.Message).Contains("IViewLocator");
    }

    /// <summary>Verifies that the message constructor preserves the message.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MessageConstructor_PreservesMessage()
    {
        var ex = new ViewLocatorNotFoundException("custom message");

        await Assert.That(ex.Message).IsEqualTo("custom message");
    }

    /// <summary>Verifies that the message+inner constructor preserves both.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MessageAndInnerConstructor_PreservesBoth()
    {
        var inner = new InvalidOperationException("inner");
        var ex = new ViewLocatorNotFoundException("outer", inner);

        await Assert.That(ex.Message).IsEqualTo("outer");
        await Assert.That(ex.InnerException).IsEqualTo(inner);
    }
}
