// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.Bindings;

/// <summary>Tests for <see cref="BindingDirection"/>, which every binding reports itself by.</summary>
public class BindingDirectionTests
{
    /// <summary>Both directions a binding can run in are defined.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Values_DefineBothBindingDirections()
    {
        await Assert.That(Enum.IsDefined(BindingDirection.OneWay)).IsTrue();
        await Assert.That(Enum.IsDefined(BindingDirection.TwoWay)).IsTrue();
    }
}
