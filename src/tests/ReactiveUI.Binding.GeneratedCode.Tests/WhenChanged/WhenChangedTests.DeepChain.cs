// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.GeneratedCode.TestModels.Scenarios;
using ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

namespace ReactiveUI.Binding.GeneratedCode.Tests.WhenChanged;

/// <summary>
/// Observation of a chain deeper than one property: replacing the intermediate object, suppressing a
/// repeated value, and observing several deep chains at once.
/// </summary>
public partial class WhenChangedTests
{
    /// <summary>The city the deep-chain fixtures start out holding.</summary>
    private const string DeepChainInitialCity = "Seattle";

    /// <summary>The city a deep-chain fixture is moved to, so the observation has something to report.</summary>
    private const string DeepChainReplacementCity = "Portland";

    /// <summary>The city a deep-chain fixture is moved to a second time.</summary>
    private const string DeepChainSecondReplacementCity = "Eugene";

    /// <summary>The age an integer-property fixture starts out holding.</summary>
    private const int InitialAge = 25;

    /// <summary>The age an integer-property fixture is moved to.</summary>
    private const int ReplacementAge = 30;

    /// <summary>The name a fixture is moved to, so the observation has something to report.</summary>
    private const string ReplacementName = "Changed";

    /// <summary>The value written after the subscription is disposed, which must not be observed.</summary>
    private const string AfterDisposalValue = "AfterDisposal";

    /// <summary>The initial name used by the fixtures in these tests.</summary>
    private const string InitialChildName = "Alice";

    /// <summary>
    /// Verifies that replacing the intermediate object in a deep chain re-subscribes
    /// and emits the new object's property value.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepChain_IntermediateObjectReplacement_ReSubscribes()
    {
        var vm = new BigViewModel();
        vm.Address.City = DeepChainInitialCity;
        var values = new List<string>();

        using var sub = WhenChangedScenarios.DeepChain_AddressCity(vm)
            .Subscribe(values.Add);

        await Assert.That(values[0]).IsEqualTo(DeepChainInitialCity);

        // Replace the entire Address object
        var newAddress = new Address { City = DeepChainReplacementCity };
        vm.Address = newAddress;

        await Assert.That(values).Contains(DeepChainReplacementCity);

        // Change the new address's City
        newAddress.City = DeepChainSecondReplacementCity;

        await Assert.That(values).Contains(DeepChainSecondReplacementCity);
    }

    /// <summary>Verifies that after replacing the intermediate object, changes to the old object's property are no longer observed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepChain_IntermediateObjectReplacement_UnsubscribesOld()
    {
        var vm = new BigViewModel();
        vm.Address.Street = "First Ave";
        var values = new List<string>();

        using var sub = WhenChangedScenarios.DeepChain_AddressStreet(vm)
            .Subscribe(values.Add);

        var oldAddress = vm.Address;

        // Replace the Address object
        vm.Address = new() { Street = "Second Ave" };

        var countAfterReplacement = values.Count;

        // Change the old address — should NOT emit
        oldAddress.Street = "Third Ave";

        await Assert.That(values.Count).IsEqualTo(countAfterReplacement);
        await Assert.That(values[^1]).IsEqualTo("Second Ave");
    }

    /// <summary>Verifies that DistinctUntilChanged filters out duplicate consecutive values for single-property observation.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_DistinctUntilChanged_FiltersDuplicates()
    {
        var vm = new TestViewModel { Name = "A" };
        var values = new List<string>();

        using var sub = WhenChangedScenarios.SingleProperty_Name(vm)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsEqualTo(1);
        await Assert.That(values[0]).IsEqualTo("A");

        // Setting to the same value — TestViewModel's setter checks inequality,
        // so PropertyChanged won't fire. This verifies no extra emission.
        vm.Name = "A";

        await Assert.That(values.Count).IsEqualTo(1);
    }

    /// <summary>Verifies that DistinctUntilChanged filters out duplicate consecutive values in deep chain observation.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepChain_DistinctUntilChanged_FiltersDuplicates()
    {
        var vm = new BigViewModel();
        vm.Address.City = DeepChainInitialCity;
        var values = new List<string>();

        using var sub = WhenChangedScenarios.DeepChain_AddressCity(vm)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsEqualTo(1);
        await Assert.That(values[0]).IsEqualTo(DeepChainInitialCity);

        // Replace Address with one that has the same City — DistinctUntilChanged should filter
        vm.Address = new() { City = DeepChainInitialCity };

        await Assert.That(values.Count).IsEqualTo(1);
    }

    /// <summary>Verifies that multi-property observation with a deep chain emits when the nested property changes.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MultiProperty_WithDeepChain_EmitsOnNestedChange()
    {
        var vm = new BigViewModel { Prop1 = FirstPropertyValue };
        vm.Address.City = DeepChainInitialCity;
        var values = new List<PropertyValues<string, string>>();

        using var sub = WhenChangedScenarios.MultiProperty_WithDeepChain(vm)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0].Property1).IsEqualTo(DeepChainInitialCity);
        await Assert.That(values[0].Property2).IsEqualTo(FirstPropertyValue);

        // Change the nested property
        vm.Address.City = DeepChainReplacementCity;

        await Assert.That(values[^1].Property1).IsEqualTo(DeepChainReplacementCity);
        await Assert.That(values[^1].Property2).IsEqualTo(FirstPropertyValue);
    }

    /// <summary>Verifies that multi-property observation with a deep chain emits when the simple property changes.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MultiProperty_WithDeepChain_EmitsOnSimpleChange()
    {
        var vm = new BigViewModel { Prop1 = FirstPropertyValue };
        vm.Address.City = DeepChainInitialCity;
        var values = new List<PropertyValues<string, string>>();

        using var sub = WhenChangedScenarios.MultiProperty_WithDeepChain(vm)
            .Subscribe(values.Add);

        // Change the simple property
        vm.Prop1 = "World";

        await Assert.That(values[^1].Property1).IsEqualTo(DeepChainInitialCity);
        await Assert.That(values[^1].Property2).IsEqualTo("World");
    }

    /// <summary>Verifies that multi-property observation with a deep chain re-subscribes when the intermediate object is replaced.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MultiProperty_WithDeepChain_IntermediateReplacement()
    {
        var vm = new BigViewModel { Prop1 = FirstPropertyValue };
        vm.Address.City = DeepChainInitialCity;
        var values = new List<PropertyValues<string, string>>();

        using var sub = WhenChangedScenarios.MultiProperty_WithDeepChain(vm)
            .Subscribe(values.Add);

        // Replace the intermediate object
        vm.Address = new() { City = DeepChainReplacementCity };

        await Assert.That(values[^1].Property1).IsEqualTo(DeepChainReplacementCity);

        // Change the new object's property
        vm.Address.City = DeepChainSecondReplacementCity;

        await Assert.That(values[^1].Property1).IsEqualTo(DeepChainSecondReplacementCity);
    }

    /// <summary>Verifies that multiple subscriptions to the same WhenChanged observable each receive independent emissions.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MultipleSubscriptions_ReceiveIndependentEmissions()
    {
        var vm = new TestViewModel { Name = "Initial" };
        var values1 = new List<string>();
        var values2 = new List<string>();

        var obs = WhenChangedScenarios.SingleProperty_Name(vm);

        using var sub1 = obs.Subscribe(values1.Add);
        using var sub2 = obs.Subscribe(values2.Add);

        vm.Name = ReplacementName;

        await Assert.That(values1.Count).IsGreaterThanOrEqualTo(MinEmissionsAfterChange);
        await Assert.That(values2.Count).IsGreaterThanOrEqualTo(MinEmissionsAfterChange);
        await Assert.That(values1).Contains(ReplacementName);
        await Assert.That(values2).Contains(ReplacementName);
    }

    /// <summary>Verifies that disposing one subscription does not affect another.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MultipleSubscriptions_DisposingOneDoesNotAffectOther()
    {
        var vm = new TestViewModel { Name = "Initial" };
        var values1 = new List<string>();
        var values2 = new List<string>();

        var obs = WhenChangedScenarios.SingleProperty_Name(vm);

        var sub1 = obs.Subscribe(values1.Add);
        using var sub2 = obs.Subscribe(values2.Add);

        sub1.Dispose();

        vm.Name = AfterDisposalValue;

        await Assert.That(values1).DoesNotContain(AfterDisposalValue);
        await Assert.That(values2).Contains(AfterDisposalValue);
    }

    /// <summary>Verifies that the deep chain disposal stops listening for nested property changes.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepChain_Disposal_StopsListening()
    {
        var vm = new BigViewModel();
        vm.Address.City = DeepChainInitialCity;
        var values = new List<string>();

        var sub = WhenChangedScenarios.DeepChain_AddressCity(vm)
            .Subscribe(values.Add);

        sub.Dispose();

        vm.Address.City = DeepChainReplacementCity;

        await Assert.That(values.Count).IsEqualTo(1);
        await Assert.That(values[0]).IsEqualTo(DeepChainInitialCity);
    }

    /// <summary>Verifies that the deep chain disposal also stops listening after intermediate replacement.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepChain_Disposal_AfterIntermediateReplacement()
    {
        var vm = new BigViewModel();
        vm.Address.City = DeepChainInitialCity;
        var values = new List<string>();

        var sub = WhenChangedScenarios.DeepChain_AddressCity(vm)
            .Subscribe(values.Add);

        vm.Address = new() { City = DeepChainReplacementCity };
        sub.Dispose();

        vm.Address.City = DeepChainSecondReplacementCity;

        await Assert.That(values).DoesNotContain(DeepChainSecondReplacementCity);
    }

    /// <summary>Verifies that the int property WhenChanged emits the initial value and changes.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_IntType_EmitsChanges()
    {
        var vm = new TestViewModel { Age = InitialAge };
        var values = new List<int>();

        using var sub = WhenChangedScenarios.SingleProperty_Age(vm)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0]).IsEqualTo(InitialAge);

        vm.Age = ReplacementAge;

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(MinEmissionsAfterChange);
        await Assert.That(values).Contains(ReplacementAge);
    }

    /// <summary>
    /// Verifies that deep chain with null-forgiving operator (x => x.Child!.Name)
    /// correctly observes the nested property when the intermediate is non-null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepChain_NullForgiving_EmitsWhenChildSet()
    {
        var host = new HostTestFixture { Child = new() { Name = InitialChildName } };
        var values = new List<string>();

        using var sub = WhenChangedScenarios.DeepChain_ChildName(host)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0]).IsEqualTo(InitialChildName);

        host.Child!.Name = "Bob";

        await Assert.That(values).Contains("Bob");
    }

    /// <summary>Verifies that deep chain with null-forgiving operator does not emit when the intermediate object is null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepChain_NullForgiving_DoesNotEmitWhenChildNull()
    {
        var host = new HostTestFixture { Child = null };
        var values = new List<string>();

        using var sub = WhenChangedScenarios.DeepChain_ChildName(host)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsEqualTo(0);
    }

    /// <summary>Verifies that deep chain with null-forgiving operator re-subscribes when the intermediate object is replaced.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepChain_NullForgiving_ReSubscribesOnReplacement()
    {
        var host = new HostTestFixture { Child = new() { Name = InitialChildName } };
        var values = new List<string>();

        using var sub = WhenChangedScenarios.DeepChain_ChildName(host)
            .Subscribe(values.Add);

        // Replace the child entirely
        host.Child = new() { Name = "Charlie" };

        await Assert.That(values).Contains("Charlie");

        // Modify the new child
        host.Child.Name = "Dave";

        await Assert.That(values).Contains("Dave");
    }

    /// <summary>Verifies that deep chain with null-forgiving operator transitions from null to non-null and starts observing.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepChain_NullForgiving_NullToNonNull()
    {
        var host = new HostTestFixture { Child = null };
        var values = new List<string>();

        using var sub = WhenChangedScenarios.DeepChain_ChildName(host)
            .Subscribe(values.Add);

        // Set child from null to a value
        host.Child = new() { Name = "Eve" };

        await Assert.That(values).Contains("Eve");

        // Verify further changes are tracked
        host.Child.Name = "Frank";

        await Assert.That(values).Contains("Frank");
    }
}
