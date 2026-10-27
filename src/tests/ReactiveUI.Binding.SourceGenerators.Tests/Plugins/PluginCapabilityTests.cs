// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.Plugins.CommandBinding;
using ReactiveUI.Binding.SourceGenerators.Plugins.Observation;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.Plugins;

/// <summary>Checks what each mechanism reports about itself to the registry.</summary>
public class PluginCapabilityTests
{
    /// <summary>A property name used where the mechanism does not look at it.</summary>
    private const string AnyProperty = "Name";

    /// <summary>The score of the plain-property fallback, the lowest any mechanism competes with.</summary>
    private const int FallbackAffinity = 1;

    /// <summary><c>INotifyPropertyChanged</c> observation reports both timings and every property of a matched type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task INPCPlugin_ReportsBothTimingsAndEveryProperty()
    {
        var plugin = new INPCObservationPlugin();

        await Assert.That(plugin.SupportsBeforeChanged).IsTrue();
        await Assert.That(plugin.CanObserveProperty(ModelFactory.CreateClassBindingInfo(), AnyProperty)).IsTrue();
    }

    /// <summary><c>INotifyPropertyChanged</c> observation claims a notifying type, but leaves a reactive object to its own mechanism.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task INPCPlugin_MatchesANotifyingTypeThatIsNotAReactiveObject()
    {
        var plugin = new INPCObservationPlugin();

        await Assert.That(plugin.IsAMatch(ModelFactory.CreateClassBindingInfo(implementsINPC: true))).IsTrue();
        await Assert.That(plugin.IsAMatch(ModelFactory.CreateClassBindingInfo(implementsIReactiveObject: true, implementsINPC: true))).IsFalse();
        await Assert.That(plugin.IsAMatch(ModelFactory.CreateClassBindingInfo())).IsFalse();
    }

    /// <summary>Reactive object observation reports both timings and every property of a matched type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveObjectPlugin_ReportsBothTimingsAndEveryProperty()
    {
        var plugin = new ReactiveObjectObservationPlugin();

        await Assert.That(plugin.SupportsBeforeChanged).IsTrue();
        await Assert.That(plugin.CanObserveProperty(ModelFactory.CreateClassBindingInfo(), AnyProperty)).IsTrue();
    }

    /// <summary>The plain-property fallback scores lowest, answers both timings, and names itself.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task PocoPlugin_IsTheLowestScoringFallback()
    {
        var plugin = new PocoObservationPlugin();

        await Assert.That(plugin.Affinity).IsEqualTo(FallbackAffinity);
        await Assert.That(plugin.ObservationKind).IsEqualTo("POCO");
        await Assert.That(plugin.SupportsBeforeChanged).IsTrue();
        await Assert.That(plugin.IsAMatch(ModelFactory.CreateClassBindingInfo())).IsTrue();
        await Assert.That(plugin.CanObserveProperty(ModelFactory.CreateClassBindingInfo(), AnyProperty)).IsTrue();
    }

    /// <summary>Key-value observing reports before-change observation, which KVO's old-value option answers.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task KVOPlugin_SupportsBeforeChange() =>
        await Assert.That(new KVOObservationPlugin().SupportsBeforeChanged).IsTrue();

    /// <summary>AppKit target/action defers to a registered custom binder, as ReactiveUI's AppKit binding did.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AppKitCommandPlugin_FallsBackToACustomBinder() =>
        await Assert.That(new AppKitCommandBindingPlugin().RequiresCustomBinderFallback).IsTrue();
}
