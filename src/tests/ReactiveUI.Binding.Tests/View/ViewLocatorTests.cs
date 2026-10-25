// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.Tests.TestExecutors;
using TUnit.Core.Executors;

namespace ReactiveUI.Binding.Tests.View;

/// <summary>Tests for the <see cref="ViewLocator"/> static accessor.</summary>
[NotInParallel]
[TestExecutor<BindingBuilderTestExecutor>]
public class ViewLocatorTests
{
    /// <summary>Verifies that GetCurrent returns a locator when one is registered.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetCurrent_WhenRegistered_ReturnsLocator()
    {
        // WithCoreServices registers DefaultViewLocator as IViewLocator
        var locator = ViewLocator.GetCurrent();

        await Assert.That(locator).IsNotNull();
        await Assert.That(locator).IsTypeOf<DefaultViewLocator>();
    }

    /// <summary>Verifies that GetCurrent throws ViewLocatorNotFoundException when no locator is registered.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetCurrent_NotRegistered_ThrowsViewLocatorNotFoundException()
    {
        // Reset without rebuilding to ensure no IViewLocator is registered
        RxBindingBuilder.ResetForTesting();

        await Assert.That(ViewLocator.GetCurrent).ThrowsException();
    }
}
