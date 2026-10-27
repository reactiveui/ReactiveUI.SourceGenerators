// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.GeneratedCode.TestModels.Scenarios;
using ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

namespace ReactiveUI.Binding.GeneratedCode.Tests.WhenAnyValue;

/// <summary>
/// Tests that the source-generator-generated WhenAnyValue code works correctly at runtime
/// for extended multi-property overloads (4 through 16 properties).
/// </summary>
public partial class WhenAnyValueTests
{
    /// <summary>Verifies that four-property WhenAnyValue emits initial values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FourProperties_EmitsInitialValues()
    {
        var vm = BigViewModel.CreatePopulated();
        var received = false;

        using var sub = WhenAnyValueExtendedScenarios.FourProperties(vm)
            .Subscribe(_ => received = true);

        await Assert.That(received).IsTrue();
    }

    /// <summary>Verifies that five-property WhenAnyValue emits initial values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FiveProperties_EmitsInitialValues()
    {
        var vm = BigViewModel.CreatePopulated();
        var received = false;

        using var sub = WhenAnyValueExtendedScenarios.FiveProperties(vm)
            .Subscribe(_ => received = true);

        await Assert.That(received).IsTrue();
    }

    /// <summary>Verifies that six-property WhenAnyValue emits initial values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SixProperties_EmitsInitialValues()
    {
        var vm = BigViewModel.CreatePopulated();
        var received = false;

        using var sub = WhenAnyValueExtendedScenarios.SixProperties(vm)
            .Subscribe(_ => received = true);

        await Assert.That(received).IsTrue();
    }

    /// <summary>Verifies that seven-property WhenAnyValue emits initial values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SevenProperties_EmitsInitialValues()
    {
        var vm = BigViewModel.CreatePopulated();
        var received = false;

        using var sub = WhenAnyValueExtendedScenarios.SevenProperties(vm)
            .Subscribe(_ => received = true);

        await Assert.That(received).IsTrue();
    }

    /// <summary>Verifies that eight-property WhenAnyValue emits initial values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EightProperties_EmitsInitialValues()
    {
        var vm = BigViewModel.CreatePopulated();
        var received = false;

        using var sub = WhenAnyValueExtendedScenarios.EightProperties(vm)
            .Subscribe(_ => received = true);

        await Assert.That(received).IsTrue();
    }

    /// <summary>Verifies that nine-property WhenAnyValue emits initial values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NineProperties_EmitsInitialValues()
    {
        var vm = BigViewModel.CreatePopulated();
        var received = false;

        using var sub = WhenAnyValueExtendedScenarios.NineProperties(vm)
            .Subscribe(_ => received = true);

        await Assert.That(received).IsTrue();
    }

    /// <summary>Verifies that ten-property WhenAnyValue emits initial values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TenProperties_EmitsInitialValues()
    {
        var vm = BigViewModel.CreatePopulated();
        var received = false;

        using var sub = WhenAnyValueExtendedScenarios.TenProperties(vm)
            .Subscribe(_ => received = true);

        await Assert.That(received).IsTrue();
    }

    /// <summary>Verifies that eleven-property WhenAnyValue emits initial values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ElevenProperties_EmitsInitialValues()
    {
        var vm = BigViewModel.CreatePopulated();
        var received = false;

        using var sub = WhenAnyValueExtendedScenarios.ElevenProperties(vm)
            .Subscribe(_ => received = true);

        await Assert.That(received).IsTrue();
    }

    /// <summary>Verifies that twelve-property WhenAnyValue emits initial values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TwelveProperties_EmitsInitialValues()
    {
        var vm = BigViewModel.CreatePopulated();
        var received = false;

        using var sub = WhenAnyValueExtendedScenarios.TwelveProperties(vm)
            .Subscribe(_ => received = true);

        await Assert.That(received).IsTrue();
    }

    /// <summary>Verifies that thirteen-property WhenAnyValue emits initial values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ThirteenProperties_EmitsInitialValues()
    {
        var vm = BigViewModel.CreatePopulated();
        var received = false;

        using var sub = WhenAnyValueExtendedScenarios.ThirteenProperties(vm)
            .Subscribe(_ => received = true);

        await Assert.That(received).IsTrue();
    }

    /// <summary>Verifies that fourteen-property WhenAnyValue emits initial values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FourteenProperties_EmitsInitialValues()
    {
        var vm = BigViewModel.CreatePopulated();
        var received = false;

        using var sub = WhenAnyValueExtendedScenarios.FourteenProperties(vm)
            .Subscribe(_ => received = true);

        await Assert.That(received).IsTrue();
    }

    /// <summary>Verifies that fifteen-property WhenAnyValue emits initial values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FifteenProperties_EmitsInitialValues()
    {
        var vm = BigViewModel.CreatePopulated();
        var received = false;

        using var sub = WhenAnyValueExtendedScenarios.FifteenProperties(vm)
            .Subscribe(_ => received = true);

        await Assert.That(received).IsTrue();
    }

    /// <summary>Verifies that sixteen-property WhenAnyValue emits initial values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SixteenProperties_EmitsInitialValues()
    {
        var vm = BigViewModel.CreatePopulated();
        var received = false;

        using var sub = WhenAnyValueExtendedScenarios.SixteenProperties(vm)
            .Subscribe(_ => received = true);

        await Assert.That(received).IsTrue();
    }

    /// <summary>Verifies that sixteen-property WhenAnyValue with a selector correctly combines all property values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithSelector_SixteenProperties()
    {
        var vm = BigViewModel.CreatePopulated();
        var values = new List<string>();

        using var sub = WhenAnyValueExtendedScenarios.WithSelector_SixteenProperties(vm)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0]).IsNotNull();
    }

    /// <summary>Verifies that deep chain WhenAnyValue emits the nested property value.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepChain_AddressCity()
    {
        var vm = new BigViewModel();
        vm.Address.City = "Seattle";
        var received = false;

        using var sub = WhenAnyValueExtendedScenarios.DeepChain_AddressCity(vm)
            .Subscribe(_ => received = true);

        await Assert.That(received).IsTrue();
    }
}
