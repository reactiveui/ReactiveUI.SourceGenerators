// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.GeneratedCode.TestModels.Scenarios;
using ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

namespace ReactiveUI.Binding.GeneratedCode.Tests.WhenChanged;

/// <summary>
/// Tests that the source-generator-generated WhenChanged code works correctly at runtime
/// for extended multi-property overloads (5 through 16 properties).
/// </summary>
public partial class WhenChangedTests
{
    /// <summary>Verifies that five-property WhenChanged emits initial values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FiveProperties_EmitsInitialValues()
    {
        var vm = BigViewModel.CreatePopulated();
        var received = false;

        using var sub = WhenChangedExtendedScenarios.FiveProperties(vm)
            .Subscribe(_ => received = true);

        await Assert.That(received).IsTrue();
    }

    /// <summary>Verifies that six-property WhenChanged emits initial values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SixProperties_EmitsInitialValues()
    {
        var vm = BigViewModel.CreatePopulated();
        var received = false;

        using var sub = WhenChangedExtendedScenarios.SixProperties(vm)
            .Subscribe(_ => received = true);

        await Assert.That(received).IsTrue();
    }

    /// <summary>Verifies that seven-property WhenChanged emits initial values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SevenProperties_EmitsInitialValues()
    {
        var vm = BigViewModel.CreatePopulated();
        var received = false;

        using var sub = WhenChangedExtendedScenarios.SevenProperties(vm)
            .Subscribe(_ => received = true);

        await Assert.That(received).IsTrue();
    }

    /// <summary>Verifies that eight-property WhenChanged emits initial values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EightProperties_EmitsInitialValues()
    {
        var vm = BigViewModel.CreatePopulated();
        var received = false;

        using var sub = WhenChangedExtendedScenarios.EightProperties(vm)
            .Subscribe(_ => received = true);

        await Assert.That(received).IsTrue();
    }

    /// <summary>Verifies that nine-property WhenChanged emits initial values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NineProperties_EmitsInitialValues()
    {
        var vm = BigViewModel.CreatePopulated();
        var received = false;

        using var sub = WhenChangedExtendedScenarios.NineProperties(vm)
            .Subscribe(_ => received = true);

        await Assert.That(received).IsTrue();
    }

    /// <summary>Verifies that ten-property WhenChanged emits initial values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TenProperties_EmitsInitialValues()
    {
        var vm = BigViewModel.CreatePopulated();
        var received = false;

        using var sub = WhenChangedExtendedScenarios.TenProperties(vm)
            .Subscribe(_ => received = true);

        await Assert.That(received).IsTrue();
    }

    /// <summary>Verifies that eleven-property WhenChanged emits initial values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ElevenProperties_EmitsInitialValues()
    {
        var vm = BigViewModel.CreatePopulated();
        var received = false;

        using var sub = WhenChangedExtendedScenarios.ElevenProperties(vm)
            .Subscribe(_ => received = true);

        await Assert.That(received).IsTrue();
    }

    /// <summary>Verifies that twelve-property WhenChanged emits initial values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TwelveProperties_EmitsInitialValues()
    {
        var vm = BigViewModel.CreatePopulated();
        var received = false;

        using var sub = WhenChangedExtendedScenarios.TwelveProperties(vm)
            .Subscribe(_ => received = true);

        await Assert.That(received).IsTrue();
    }

    /// <summary>Verifies that thirteen-property WhenChanged emits initial values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ThirteenProperties_EmitsInitialValues()
    {
        var vm = BigViewModel.CreatePopulated();
        var received = false;

        using var sub = WhenChangedExtendedScenarios.ThirteenProperties(vm)
            .Subscribe(_ => received = true);

        await Assert.That(received).IsTrue();
    }

    /// <summary>Verifies that fourteen-property WhenChanged emits initial values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FourteenProperties_EmitsInitialValues()
    {
        var vm = BigViewModel.CreatePopulated();
        var received = false;

        using var sub = WhenChangedExtendedScenarios.FourteenProperties(vm)
            .Subscribe(_ => received = true);

        await Assert.That(received).IsTrue();
    }

    /// <summary>Verifies that fifteen-property WhenChanged emits initial values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FifteenProperties_EmitsInitialValues()
    {
        var vm = BigViewModel.CreatePopulated();
        var received = false;

        using var sub = WhenChangedExtendedScenarios.FifteenProperties(vm)
            .Subscribe(_ => received = true);

        await Assert.That(received).IsTrue();
    }

    /// <summary>Verifies that sixteen-property WhenChanged emits initial values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SixteenProperties_EmitsInitialValues()
    {
        var vm = BigViewModel.CreatePopulated();
        var received = false;

        using var sub = WhenChangedExtendedScenarios.SixteenProperties(vm)
            .Subscribe(_ => received = true);

        await Assert.That(received).IsTrue();
    }

    /// <summary>Verifies that sixteen-property WhenChanged with a selector correctly combines all property values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithSelector_SixteenProperties()
    {
        var vm = BigViewModel.CreatePopulated();
        var values = new List<string>();

        using var sub = WhenChangedExtendedScenarios.WithSelector_SixteenProperties(vm)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0]).IsNotNull();
    }
}
