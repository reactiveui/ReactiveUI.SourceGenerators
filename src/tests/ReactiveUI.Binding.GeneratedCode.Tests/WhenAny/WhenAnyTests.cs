// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.GeneratedCode.TestModels.Scenarios;
using ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

namespace ReactiveUI.Binding.GeneratedCode.Tests.WhenAny;

/// <summary>Tests that the source-generator-generated WhenAny code (IObservedChange wrapper) works correctly at runtime.</summary>
public class WhenAnyTests
{
    /// <summary>The initial name value used across the WhenAny tests.</summary>
    private const string InitialName = "Initial";

    /// <summary>The expected emission count after an initial value plus one change.</summary>
    private const int ExpectedEmissionCount = 2;

    /// <summary>The replacement age value used in change-emission tests.</summary>
    private const int InitialAge = 25;

    /// <summary>The age used by the two-property fixtures in these tests.</summary>
    private const int AdultAge = 30;

    /// <summary>Verifies that a single-property WhenAny with value selector emits the initial value.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_EmitsInitialValue()
    {
        var vm = new TestViewModel { Name = InitialName };
        var values = new List<string>();

        using var sub = WhenAnyScenarios.SingleProperty_Name(vm)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0]).IsEqualTo(InitialName);
    }

    /// <summary>Verifies that a single-property WhenAny emits on property change.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_EmitsOnChange()
    {
        var vm = new TestViewModel { Name = InitialName };
        var values = new List<string>();

        using var sub = WhenAnyScenarios.SingleProperty_Name(vm)
            .Subscribe(values.Add);

        vm.Name = "Changed";

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(ExpectedEmissionCount);
        await Assert.That(values).Contains("Changed");
    }

    /// <summary>Verifies that the IObservedChange wrapper provides Sender and Value.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_IObservedChange_HasSenderAndValue()
    {
        var vm = new TestViewModel { Name = "Hello" };
        var changes = new List<IObservedChange<TestViewModel, string>>();

        using var sub = WhenAnyScenarios.SingleProperty_Name_ObservedChange(vm)
            .Subscribe(changes.Add);

        await Assert.That(changes.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(changes[0].Value).IsEqualTo("Hello");
        await Assert.That(changes[0].Sender).IsEqualTo(vm);
    }

    /// <summary>Verifies that two-property WhenAny with a selector emits the combined initial value.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TwoProperties_EmitsInitialCombinedValue()
    {
        var vm = new TestViewModel { Name = "Alice", Age = AdultAge };
        var values = new List<string>();

        using var sub = WhenAnyScenarios.TwoProperties_NameAge(vm)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0]).IsEqualTo("Alice_30");
    }

    /// <summary>Verifies that two-property WhenAny emits when either property changes.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TwoProperties_EmitsOnEitherChange()
    {
        var vm = new TestViewModel { Name = "Alice", Age = AdultAge };
        var values = new List<string>();

        using var sub = WhenAnyScenarios.TwoProperties_NameAge(vm)
            .Subscribe(values.Add);

        vm.Name = "Bob";

        await Assert.That(values[^1]).IsEqualTo("Bob_30");

        vm.Age = InitialAge;

        await Assert.That(values[^1]).IsEqualTo("Bob_25");
    }

    /// <summary>Verifies that disposing the WhenAny subscription stops listening for changes.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Disposal_StopsListening()
    {
        var vm = new TestViewModel { Name = InitialName };
        var values = new List<string>();

        var sub = WhenAnyScenarios.SingleProperty_Name(vm)
            .Subscribe(values.Add);

        sub.Dispose();

        vm.Name = "AfterDisposal";

        await Assert.That(values.Count).IsEqualTo(1);
        await Assert.That(values[0]).IsEqualTo(InitialName);
    }
}
