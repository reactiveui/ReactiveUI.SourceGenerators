// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.View;

/// <summary>Tests for the view registration attribute classes.</summary>
public class ViewAttributeTests
{
    /// <summary>Verifies that ViewContractAttribute stores and returns the contract string.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ViewContractAttribute_StoresContract()
    {
        var attr = new ViewContractAttribute("compact");

        await Assert.That(attr.Contract).IsEqualTo("compact");
    }

    /// <summary>Verifies that ExcludeFromViewRegistrationAttribute can be instantiated.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExcludeFromViewRegistrationAttribute_CanInstantiate()
    {
        var attr = new ExcludeFromViewRegistrationAttribute();

        await Assert.That(attr).IsNotNull();
    }

    /// <summary>Verifies that SingleInstanceViewAttribute can be instantiated.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleInstanceViewAttribute_CanInstantiate()
    {
        var attr = new SingleInstanceViewAttribute();

        await Assert.That(attr).IsNotNull();
    }
}
