// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.GeneratedCode.TestModels.Scenarios;
using ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

namespace ReactiveUI.Binding.GeneratedCode.Tests.WhenChanged;

/// <summary>
/// Tests that the source-generator-generated WhenChanging code works correctly at runtime
/// for extended multi-property overloads (5 through 16 properties).
/// WhenChanging emits the property value before the change is applied.
/// </summary>
public partial class WhenChangingTests
{
    /// <summary>Verifies that five-property WhenChanging emits initial values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FiveProperties_EmitsInitialValues()
    {
        var vm = BigViewModel.CreatePopulated();
        var received = false;

        using var sub = WhenChangingExtendedScenarios.FiveProperties(vm)
            .Subscribe(_ => received = true);

        await Assert.That(received).IsTrue();
    }

    /// <summary>Verifies that six-property WhenChanging emits initial values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SixProperties_EmitsInitialValues()
    {
        var vm = BigViewModel.CreatePopulated();
        var received = false;

        using var sub = WhenChangingExtendedScenarios.SixProperties(vm)
            .Subscribe(_ => received = true);

        await Assert.That(received).IsTrue();
    }

    /// <summary>Verifies that seven-property WhenChanging emits initial values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SevenProperties_EmitsInitialValues()
    {
        var vm = BigViewModel.CreatePopulated();
        var received = false;

        using var sub = WhenChangingExtendedScenarios.SevenProperties(vm)
            .Subscribe(_ => received = true);

        await Assert.That(received).IsTrue();
    }

    /// <summary>Verifies that eight-property WhenChanging emits initial values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EightProperties_EmitsInitialValues()
    {
        var vm = BigViewModel.CreatePopulated();
        var received = false;

        using var sub = WhenChangingExtendedScenarios.EightProperties(vm)
            .Subscribe(_ => received = true);

        await Assert.That(received).IsTrue();
    }

    /// <summary>Verifies that nine-property WhenChanging emits initial values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NineProperties_EmitsInitialValues()
    {
        var vm = BigViewModel.CreatePopulated();
        var received = false;

        using var sub = WhenChangingExtendedScenarios.NineProperties(vm)
            .Subscribe(_ => received = true);

        await Assert.That(received).IsTrue();
    }

    /// <summary>Verifies that ten-property WhenChanging emits initial values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TenProperties_EmitsInitialValues()
    {
        var vm = BigViewModel.CreatePopulated();
        var received = false;

        using var sub = WhenChangingExtendedScenarios.TenProperties(vm)
            .Subscribe(_ => received = true);

        await Assert.That(received).IsTrue();
    }

    /// <summary>Verifies that eleven-property WhenChanging emits initial values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ElevenProperties_EmitsInitialValues()
    {
        var vm = BigViewModel.CreatePopulated();
        var received = false;

        using var sub = WhenChangingExtendedScenarios.ElevenProperties(vm)
            .Subscribe(_ => received = true);

        await Assert.That(received).IsTrue();
    }

    /// <summary>Verifies that twelve-property WhenChanging emits initial values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TwelveProperties_EmitsInitialValues()
    {
        var vm = BigViewModel.CreatePopulated();
        var received = false;

        using var sub = WhenChangingExtendedScenarios.TwelveProperties(vm)
            .Subscribe(_ => received = true);

        await Assert.That(received).IsTrue();
    }

    /// <summary>Verifies that thirteen-property WhenChanging emits initial values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ThirteenProperties_EmitsInitialValues()
    {
        var vm = BigViewModel.CreatePopulated();
        var received = false;

        using var sub = WhenChangingExtendedScenarios.ThirteenProperties(vm)
            .Subscribe(_ => received = true);

        await Assert.That(received).IsTrue();
    }

    /// <summary>Verifies that fourteen-property WhenChanging emits initial values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FourteenProperties_EmitsInitialValues()
    {
        var vm = BigViewModel.CreatePopulated();
        var received = false;

        using var sub = WhenChangingExtendedScenarios.FourteenProperties(vm)
            .Subscribe(_ => received = true);

        await Assert.That(received).IsTrue();
    }

    /// <summary>Verifies that fifteen-property WhenChanging emits initial values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FifteenProperties_EmitsInitialValues()
    {
        var vm = BigViewModel.CreatePopulated();
        var received = false;

        using var sub = WhenChangingExtendedScenarios.FifteenProperties(vm)
            .Subscribe(_ => received = true);

        await Assert.That(received).IsTrue();
    }

    /// <summary>Verifies that sixteen-property WhenChanging emits initial values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SixteenProperties_EmitsInitialValues()
    {
        var vm = BigViewModel.CreatePopulated();
        var received = false;

        using var sub = WhenChangingExtendedScenarios.SixteenProperties(vm)
            .Subscribe(_ => received = true);

        await Assert.That(received).IsTrue();
    }

    /// <summary>Verifies that deep chain WhenChanging emits the nested property value.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepChain_AddressCity()
    {
        var vm = new BigViewModel();
        vm.Address.City = "Seattle";
        var received = false;

        using var sub = WhenChangingExtendedScenarios.DeepChain_AddressCity(vm)
            .Subscribe(_ => received = true);

        await Assert.That(received).IsTrue();
    }
}
