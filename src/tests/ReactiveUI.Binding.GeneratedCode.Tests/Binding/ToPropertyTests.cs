// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Reactive.Subjects;
using ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;
using ReactiveUI.Primitives.Concurrency;

namespace ReactiveUI.Binding.GeneratedCode.Tests.Binding;

/// <summary>Runtime tests for generated ToProperty code, through each way the generator raises notifications.</summary>
public class ToPropertyTests
{
    /// <summary>The derived name property the scenarios back.</summary>
    private const string FullName = "FullName";

    /// <summary>A value pushed into a numeric property.</summary>
    private const int PushedValue = 3;

    /// <summary>An age pushed into the summary view model.</summary>
    private const int PushedAge = 36;

    /// <summary>A partial view model raises its own event, once per distinct value, for the named property.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task PartialEvent_RaisesPropertyChangedPerDistinctValue()
    {
        var names = new Subject<string>();
        var vm = SharedScenarios.ToProperty.PartialEvent.Scenario.Execute(names);
        var raised = Record(vm);

        names.OnNext("Ada");
        names.OnNext("Ada");
        names.OnNext("Grace");

        await Assert.That(vm.FullName).IsEqualTo("Grace");
        await Assert.That(raised).IsEquivalentTo([FullName, FullName]);
    }

    /// <summary>A partial view model raises through its base class's protected methods, changing before changed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task PartialProtectedBase_RaisesChangingThenChanged()
    {
        var names = new Subject<string>();
        var vm = SharedScenarios.ToProperty.PartialProtectedBase.Scenario.Execute(names);
        var order = new List<string>();
        vm.PropertyChanging += (_, e) => order.Add($"changing:{e.PropertyName}:{vm.FullName}");
        vm.PropertyChanged += (_, e) => order.Add($"changed:{e.PropertyName}:{vm.FullName}");

        names.OnNext("Ada");

        await Assert.That(order).IsEquivalentTo(["changing:FullName:", "changed:FullName:Ada"]);
    }

    /// <summary>A view model with a public raise method is raised through it.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task PublicRaiseMethod_RaisesPropertyChanged()
    {
        var counts = new Subject<int>();
        var vm = SharedScenarios.ToProperty.PublicRaiseMethod.Scenario.Execute(counts);
        var raised = Record(vm);

        counts.OnNext(PushedValue);

        await Assert.That(vm.Count).IsEqualTo(PushedValue);
        await Assert.That(raised).IsEquivalentTo(["Count"]);
    }

    /// <summary>A ReactiveObject raises through ReactiveUI, so its suppressed notifications stay suppressed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveObject_HonoursSuppressedNotifications()
    {
        var names = new Subject<string>();
        var vm = SharedScenarios.ToProperty.ReactiveObject.Scenario.Execute(names);
        var raised = Record(vm);

        using (vm.SuppressChangeNotifications())
        {
            names.OnNext("hidden");
        }

        names.OnNext("shown");

        await Assert.That(vm.FullName).IsEqualTo("shown");
        await Assert.That(raised).IsEquivalentTo([FullName]);
    }

    /// <summary>A property named by nameof raises for that property.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NameOf_RaisesPropertyChanged()
    {
        var names = new Subject<string>();
        var vm = SharedScenarios.ToProperty.NameOf.Scenario.Execute(names);
        var raised = Record(vm);

        names.OnNext("Ada");

        await Assert.That(vm.FullName).IsEqualTo("Ada");
        await Assert.That(raised).IsEquivalentTo([FullName]);
    }

    /// <summary>A deferred helper holds its initial value and subscribes only when first read.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task InitialValueDeferred_SubscribesOnFirstRead()
    {
        var names = new BehaviorSubject<string>("Ada");
        var vm = SharedScenarios.ToProperty.InitialValueDeferScheduler.Scenario.Execute(names, null);

        await Assert.That(names.HasObservers).IsFalse();
        await Assert.That(vm.FullName).IsEqualTo("Ada");
        await Assert.That(names.HasObservers).IsTrue();
    }

    /// <summary>The immediate scheduler delivers on the producing thread.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task InitialValueDeferred_ImmediateScheduler_DeliversInline()
    {
        var names = new Subject<string>();
        var vm = SharedScenarios.ToProperty.InitialValueDeferScheduler.Scenario.Execute(names, Sequencer.Immediate);

        await Assert.That(vm.FullName).IsEqualTo("(none)");
        names.OnNext("Ada");
        await Assert.That(vm.FullName).IsEqualTo("Ada");
    }

    /// <summary>The out parameter receives the helper, and the factory supplies the first value.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FactoryOutResult_UsesFactoryAndAssignsHelper()
    {
        var counts = new Subject<int>();
        var vm = SharedScenarios.ToProperty.FactoryOutResult.Scenario.Execute(counts);

        await Assert.That(vm.Count).IsEqualTo(-1);
        counts.OnNext(PushedValue);
        await Assert.That(vm.Count).IsEqualTo(PushedValue);
    }

    /// <summary>An [ObservableAsProperty] property reads its helper, and a null after a value reads back as null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObservableAsProperty_ReadsHelperAndRoundTripsNull()
    {
        var names = new Subject<string?>();
        var ages = new Subject<int?>();
        var vm = new SummaryViewModel(names, ages);
        var raised = Record(vm);

        names.OnNext("Ada");
        ages.OnNext(PushedAge);
        names.OnNext(null);

        await Assert.That(vm.FullName).IsNull();
        await Assert.That(vm.Age).IsEqualTo(PushedAge);
        await Assert.That(raised).IsEquivalentTo([FullName, "Age", FullName]);
    }

    /// <summary>Records the property names a view model raises PropertyChanged for.</summary>
    /// <param name="source">The view model.</param>
    /// <returns>The recorded names, in order.</returns>
    private static List<string> Record(INotifyPropertyChanged source)
    {
        var raised = new List<string>();
        source.PropertyChanged += (_, e) => raised.Add(e.PropertyName ?? string.Empty);
        return raised;
    }
}
