// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Fallback;
using ReactiveUI.Binding.GeneratedCode.TestModels.Scenarios;
using ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

namespace ReactiveUI.Binding.GeneratedCode.Tests.WhenAnyValue;

/// <summary>Edge case tests for WhenAnyValue covering disposal, deep chains, and multi-property change emission scenarios.</summary>
public partial class WhenAnyValueTests
{
    /// <summary>The initial fixture value used in disposal and subscription tests.</summary>
    private const string InitialValue = "Initial";

    /// <summary>The initial nested city value used in deep-chain tests.</summary>
    private const string InitialCity = "Seattle";

    /// <summary>The replacement nested city value used in deep-chain tests.</summary>
    private const string ReplacementCity = "Portland";

    /// <summary>The number of property changes applied in the rapid-change test.</summary>
    private const int ChangeCount = 100;

    /// <summary>The expected emission count after the initial value plus 100 changes.</summary>
    private const int ExpectedRapidEmissionCount = 101;

    /// <summary>The expected emission count after the initial value plus 12 property changes.</summary>
    private const int ExpectedTwelvePropertyCount = 13;

    /// <summary>The value written to a property to trigger a change notification.</summary>
    private const string ReplacementName = "Changed";

    /// <summary>The initial child name used in nullable deep-chain tests.</summary>
    private const string InitialChildName = "Alice";

    /// <summary>The replacement child name used in nullable deep-chain tests.</summary>
    private const string Charlie = "Charlie";

    /// <summary>Verifies that disposing the WhenAnyValue subscription stops listening for changes.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Disposal_StopsListening()
    {
        var fixture = new WhenAnyTestFixture { Value1 = InitialValue };
        var values = new List<string>();

        var sub = WhenAnyValueScenarios.SingleProperty(fixture)
            .Subscribe(values.Add);

        sub.Dispose();

        fixture.Value1 = "AfterDisposal";

        await Assert.That(values.Count).IsEqualTo(1);
        await Assert.That(values[0]).IsEqualTo(InitialValue);
    }

    /// <summary>Verifies that multi-property WhenAnyValue emits when either property changes.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TwoProperties_EmitsOnEitherChange()
    {
        var fixture = new WhenAnyTestFixture { Value1 = "A", Value2 = "B" };
        var values = new List<PropertyValues<string, string>>();

        using var sub = WhenAnyValueScenarios.TwoProperties(fixture)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);

        fixture.Value1 = "C";

        await Assert.That(values[^1].Property1).IsEqualTo("C");
        await Assert.That(values[^1].Property2).IsEqualTo("B");

        fixture.Value2 = "D";

        await Assert.That(values[^1].Property1).IsEqualTo("C");
        await Assert.That(values[^1].Property2).IsEqualTo("D");
    }

    /// <summary>Verifies that deep chain WhenAnyValue emits the nested property value.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepChain_EmitsNestedPropertyValue()
    {
        var vm = new BigViewModel();
        vm.Address.City = InitialCity;
        var values = new List<string>();

        using var sub = WhenAnyValueScenarios.DeepChain_AddressCity(vm)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0]).IsEqualTo(InitialCity);
    }

    /// <summary>Verifies that deep chain WhenAnyValue emits on nested property change.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepChain_EmitsOnNestedPropertyChange()
    {
        var vm = new BigViewModel();
        vm.Address.City = InitialCity;
        var values = new List<string>();

        using var sub = WhenAnyValueScenarios.DeepChain_AddressCity(vm)
            .Subscribe(values.Add);

        vm.Address.City = ReplacementCity;

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(ExpectedEmissionCount);
        await Assert.That(values).Contains(ReplacementCity);
    }

    /// <summary>Verifies that deep chain WhenAnyValue re-subscribes on intermediate object replacement.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepChain_IntermediateObjectReplacement()
    {
        var vm = new BigViewModel();
        vm.Address.City = InitialCity;
        var values = new List<string>();

        using var sub = WhenAnyValueScenarios.DeepChain_AddressCity(vm)
            .Subscribe(values.Add);

        var newAddress = new Address { City = ReplacementCity };
        vm.Address = newAddress;

        await Assert.That(values).Contains(ReplacementCity);

        // Verify resubscription
        newAddress.City = "Eugene";

        await Assert.That(values).Contains("Eugene");
    }

    /// <summary>
    /// Verifies that a nullable deep chain preserves legacy ReactiveUI WhenAnyValue semantics.
    /// A missing intermediate suppresses emission, and replacing it reattaches the observation.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepChain_NullIntermediate_MatchesLegacyReactiveUISemantics()
    {
        var generatedHost = new HostTestFixture { Child = null };
        var fallbackHost = new HostTestFixture { Child = null };
        var generatedValues = new List<string>();
        var fallbackValues = new List<string>();

        using var generatedSub = WhenAnyValueScenarios.DeepChain_ChildName(generatedHost)
            .Subscribe(generatedValues.Add);
        using var fallbackSub = RuntimeObservationFallback
            .WhenAnyValue(fallbackHost, x => x.Child!.Name)
            .Subscribe(fallbackValues.Add);

        generatedHost.Child = new() { Name = InitialChildName };
        fallbackHost.Child = new() { Name = InitialChildName };
        generatedHost.Child.Name = "Bob";
        fallbackHost.Child.Name = "Bob";

        generatedHost.Child = null;
        fallbackHost.Child = null;

        generatedHost.Child = new() { Name = Charlie };
        fallbackHost.Child = new() { Name = Charlie };

        var generatedTrace = string.Join('|', generatedValues);
        var fallbackTrace = string.Join('|', fallbackValues);
        await Assert.That(generatedTrace).IsEqualTo(fallbackTrace);
        await Assert.That(generatedTrace).IsEqualTo("Alice|Bob|Charlie");
    }

    /// <summary>
    /// Verifies that breaking a three-link chain detaches the generated and legacy observers from
    /// the orphaned subtree, while repairing the chain reattaches both implementations.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepChain_ThreeLinks_DetachesOrphanedSubtreeLikeLegacyReactiveUI()
    {
        var generatedMiddle = new TestViewModel { Child = new() { Name = InitialChildName } };
        var fallbackMiddle = new TestViewModel { Child = new() { Name = InitialChildName } };
        var generatedHost = new HostTestFixture { Child = generatedMiddle };
        var fallbackHost = new HostTestFixture { Child = fallbackMiddle };
        var generatedValues = new List<string>();
        var fallbackValues = new List<string>();

        using var generatedSub = WhenAnyValueScenarios.DeepChain_GrandchildName(generatedHost)
            .Subscribe(generatedValues.Add);
        using var fallbackSub = RuntimeObservationFallback
            .WhenAnyValue(fallbackHost, x => x.Child!.Child!.Name)
            .Subscribe(fallbackValues.Add);

        generatedHost.Child = null;
        fallbackHost.Child = null;

        generatedMiddle.Child!.Name = "Orphan mutation";
        fallbackMiddle.Child!.Name = "Orphan mutation";

        generatedHost.Child = new() { Child = new() { Name = Charlie } };
        fallbackHost.Child = new() { Child = new() { Name = Charlie } };

        var generatedTrace = string.Join('|', generatedValues);
        var fallbackTrace = string.Join('|', fallbackValues);
        await Assert.That(generatedTrace).IsEqualTo(fallbackTrace);
        await Assert.That(generatedTrace).IsEqualTo("Alice|Charlie");
    }

    /// <summary>Verifies that a null leaf is still emitted when every intermediate object exists.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepChain_NullLeaf_StillEmits()
    {
        var host = new HostTestFixture { Child = new() { Name = InitialChildName } };
        var values = new List<string>();

        using var sub = WhenAnyValueScenarios.DeepChain_ChildName(host)
            .Subscribe(values.Add);

        host.Child.Name = null!;

        await Assert.That(values.Count).IsEqualTo(ExpectedEmissionCount);
        await Assert.That(values[1]).IsNull();
    }

    /// <summary>Verifies that a missing intermediate is distinct from a legitimate default-valued leaf.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepChain_DefaultValueLeaf_EmitsOnlyWhenIntermediateExists()
    {
        var host = new HostTestFixture { Child = null };
        var values = new List<int>();

        using var sub = WhenAnyValueScenarios.DeepChain_ChildAge(host)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsEqualTo(0);

        host.Child = new() { Age = 0 };

        await Assert.That(values.Count).IsEqualTo(1);
        await Assert.That(values[0]).IsEqualTo(0);
    }

    /// <summary>Verifies that WhenAnyValue with selector re-emits when properties change.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithSelector_EmitsOnChange()
    {
        var fixture = new WhenAnyTestFixture { Value1 = "Hello", Value2 = "World" };
        var values = new List<string>();

        using var sub = WhenAnyValueScenarios.WithSelector_TwoProperties(fixture)
            .Subscribe(values.Add);

        await Assert.That(values[0]).IsEqualTo("Hello_World");

        fixture.Value1 = "Goodbye";

        await Assert.That(values[^1]).IsEqualTo("Goodbye_World");
    }

    /// <summary>Verifies that rapid sequential property changes are all captured by WhenAnyValue.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RapidSequentialChanges_AllCaptured()
    {
        var fixture = new WhenAnyTestFixture { Value1 = "A" };
        var values = new List<string>();

        using var sub = WhenAnyValueScenarios.SingleProperty(fixture)
            .Subscribe(values.Add);

        for (var i = 0; i < ChangeCount; i++)
        {
            fixture.Value1 = $"Value_{i}";
        }

        await Assert.That(values.Count).IsEqualTo(ExpectedRapidEmissionCount); // initial + 100 changes
        await Assert.That(values[0]).IsEqualTo("A");
        await Assert.That(values[^1]).IsEqualTo("Value_99");
    }

    /// <summary>Verifies that multiple subscriptions to the same WhenAnyValue observable each receive independent emissions.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MultipleSubscriptions_ReceiveIndependentEmissions()
    {
        var fixture = new WhenAnyTestFixture { Value1 = InitialValue };
        var values1 = new List<string>();
        var values2 = new List<string>();

        var obs = WhenAnyValueScenarios.SingleProperty(fixture);

        using var sub1 = obs.Subscribe(values1.Add);
        using var sub2 = obs.Subscribe(values2.Add);

        fixture.Value1 = ReplacementName;

        await Assert.That(values1.Count).IsGreaterThanOrEqualTo(ExpectedEmissionCount);
        await Assert.That(values2.Count).IsGreaterThanOrEqualTo(ExpectedEmissionCount);
        await Assert.That(values1).Contains(ReplacementName);
        await Assert.That(values2).Contains(ReplacementName);
    }

    /// <summary>
    /// Verifies that twelve-property WhenAnyValue fires when any one of the 12 properties changes.
    /// This is a parity test matching ReactiveUI's exhaustive WhenAnyValue coverage.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TwelveProperties_EachPropertyFiresIndependently()
    {
        var fixture = new WhenAnyTestFixture();
        var count = 0;

        using var sub = WhenAnyValueExtendedScenarios.TwelveProperties_AllStrings(fixture)
            .Subscribe(_ => count++);

        // Initial emission
        await Assert.That(count).IsEqualTo(1);

        // Change each of the 12 properties — each should fire
        fixture.Value1 = "a";
        fixture.Value2 = "b";
        fixture.Value3 = "c";
        fixture.Value4 = "d";
        fixture.Value5 = "e";
        fixture.Value6 = "f";
        fixture.Value7 = "g";
        fixture.Value8 = "h";
        fixture.Value9 = "i";
        fixture.Value10 = "j";
        fixture.Value11 = "k";
        fixture.Value12 = "l";

        await Assert.That(count).IsEqualTo(ExpectedTwelvePropertyCount); // initial + 12 changes
    }
}
