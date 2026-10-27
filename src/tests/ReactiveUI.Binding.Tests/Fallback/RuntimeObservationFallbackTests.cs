// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.Fallback;
using ReactiveUI.Binding.Mixins;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.Fallback;

/// <summary>Tests for the <see cref="RuntimeObservationFallback"/> class.</summary>
public class RuntimeObservationFallbackTests
{
    /// <summary>The value the view model starts with.</summary>
    private const string InitialValue = "Initial";

    /// <summary>The value assigned to trigger a notification.</summary>
    private const string NotifyingValue = "Changed";

    /// <summary>A sample name value used across the multi-property tests.</summary>
    private const string SampleName = "Alice";

    /// <summary>A sample age value used across the multi-property tests.</summary>
    private const int SampleAge = 30;

    /// <summary>The sample city value used across the fallback tests.</summary>
    private const string SampleCity = "Seattle";

    /// <summary>The expected number of emissions when an initial value plus one change are observed.</summary>
    private const int ExpectedTwoEmissions = 2;

    /// <summary>Verifies that WhenChanged emits the initial value and subsequent changes.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanged_SingleProperty_EmitsInitialAndChanges()
    {
        EnsureInitialized();

        var vm = new TestViewModel { Name = InitialValue };
        var values = new List<string>();

        using var sub = RuntimeObservationFallback.WhenChanged(
                vm,
                x => x.Name)
            .Subscribe(values.Add);

        vm.Name = NotifyingValue;

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(ExpectedTwoEmissions);
        await Assert.That(values[0]).IsEqualTo(InitialValue);
        await Assert.That(values[1]).IsEqualTo(NotifyingValue);
    }

    /// <summary>Verifies that WhenChanging emits before-change notifications.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanging_SingleProperty_EmitsBeforeChange()
    {
        EnsureInitialized();

        var vm = new TestViewModel { Name = InitialValue };
        var values = new List<string>();

        using var sub = RuntimeObservationFallback.WhenChanging(
                vm,
                x => x.Name)
            .Subscribe(values.Add);

        vm.Name = NotifyingValue;

        // Before-change should emit the old value at the time of notification
        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
    }

    /// <summary>Verifies that WhenAnyValue emits the initial and subsequent values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyValue_SingleProperty_EmitsValues()
    {
        EnsureInitialized();

        var vm = new TestViewModel { Name = "Start" };
        var values = new List<string>();

        using var sub = RuntimeObservationFallback.WhenAnyValue(
                vm,
                x => x.Name)
            .Subscribe(values.Add);

        vm.Name = "End";

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(ExpectedTwoEmissions);
        await Assert.That(values[0]).IsEqualTo("Start");
    }

    /// <summary>Verifies that WhenChanged with two properties emits tuples.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanged_TwoProperties_EmitsTuples()
    {
        EnsureInitialized();

        var vm = new TestViewModel { Name = SampleName, Age = SampleAge };
        var values = new List<PropertyValues<string, int>>();

        using var sub = RuntimeObservationFallback.WhenChanged(
                vm,
                x => x.Name,
                x => x.Age)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0].Property1).IsEqualTo(SampleName);
        await Assert.That(values[0].Property2).IsEqualTo(SampleAge);
    }

    /// <summary>Verifies that WhenChanged with three properties emits tuples.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanged_ThreeProperties_EmitsTuples()
    {
        EnsureInitialized();

        var vm = new TestViewModel { Name = SampleName, Age = SampleAge, Address = new() { City = SampleCity } };
        var values = new List<PropertyValues<string, int, string>>();

        using var sub = RuntimeObservationFallback.WhenChanged(
                vm,
                x => x.Name,
                x => x.Age,
                x => x.Address!.City)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0].Property1).IsEqualTo(SampleName);
        await Assert.That(values[0].Property2).IsEqualTo(SampleAge);
        await Assert.That(values[0].Property3).IsEqualTo(SampleCity);
    }

    /// <summary>Verifies that WhenChanged emits after property changes with three properties.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanged_ThreeProperties_UpdatesOnChange()
    {
        EnsureInitialized();

        var vm = new TestViewModel { Name = SampleName, Age = SampleAge, Address = new() { City = SampleCity } };
        var values = new List<PropertyValues<string, int, string>>();

        using var sub = RuntimeObservationFallback.WhenChanged(
                vm,
                x => x.Name,
                x => x.Age,
                x => x.Address!.City)
            .Subscribe(values.Add);

        vm.Name = "Bob";

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(ExpectedTwoEmissions);
        await Assert.That(values[^1].Property1).IsEqualTo("Bob");
    }

    /// <summary>Verifies that WhenChanging with two properties emits tuples.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanging_TwoProperties_EmitsTuples()
    {
        EnsureInitialized();

        var vm = new TestViewModel { Name = SampleName, Age = SampleAge };
        var values = new List<PropertyValues<string, int>>();

        using var sub = RuntimeObservationFallback.WhenChanging(
                vm,
                x => x.Name,
                x => x.Age)
            .Subscribe(values.Add);

        vm.Name = "Bob";

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
    }

    /// <summary>Verifies that WhenChanging with three properties emits tuples.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanging_ThreeProperties_EmitsTuples()
    {
        EnsureInitialized();

        var vm = new TestViewModel { Name = SampleName, Age = SampleAge, Address = new() { City = SampleCity } };
        var values = new List<PropertyValues<string, int, string>>();

        using var sub = RuntimeObservationFallback.WhenChanging(
                vm,
                x => x.Name,
                x => x.Age,
                x => x.Address!.City)
            .Subscribe(values.Add);

        vm.Name = "Bob";

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
    }

    /// <summary>Verifies that WhenAnyValue with two properties emits tuples.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyValue_TwoProperties_EmitsTuples()
    {
        EnsureInitialized();

        var vm = new TestViewModel { Name = SampleName, Age = SampleAge };
        var values = new List<PropertyValues<string, int>>();

        using var sub = RuntimeObservationFallback.WhenAnyValue(
                vm,
                x => x.Name,
                x => x.Age)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0].Property1).IsEqualTo(SampleName);
        await Assert.That(values[0].Property2).IsEqualTo(SampleAge);
    }

    /// <summary>Verifies that WhenAnyValue with three properties emits tuples.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyValue_ThreeProperties_EmitsTuples()
    {
        EnsureInitialized();

        var vm = new TestViewModel { Name = SampleName, Age = SampleAge, Address = new() { City = SampleCity } };
        var values = new List<PropertyValues<string, int, string>>();

        using var sub = RuntimeObservationFallback.WhenAnyValue(
                vm,
                x => x.Name,
                x => x.Age,
                x => x.Address!.City)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0].Property1).IsEqualTo(SampleName);
        await Assert.That(values[0].Property2).IsEqualTo(SampleAge);
        await Assert.That(values[0].Property3).IsEqualTo(SampleCity);
    }

    /// <summary>Verifies that WhenAnyValue with two properties updates on change.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyValue_TwoProperties_UpdatesOnChange()
    {
        EnsureInitialized();

        var vm = new TestViewModel { Name = SampleName, Age = SampleAge };
        var values = new List<PropertyValues<string, int>>();

        using var sub = RuntimeObservationFallback.WhenAnyValue(
                vm,
                x => x.Name,
                x => x.Age)
            .Subscribe(values.Add);

        vm.Name = "Bob";

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(ExpectedTwoEmissions);
        await Assert.That(values[^1].Property1).IsEqualTo("Bob");
    }

    /// <summary>Resets and initializes the ReactiveUI binding infrastructure for testing.</summary>
    internal static void EnsureInitialized()
    {
        RxBindingBuilder.ResetForTesting();
        _ = RxBindingBuilder.CreateReactiveUIBindingBuilder()
            .WithCoreServices()
            .BuildApp();
    }
}
