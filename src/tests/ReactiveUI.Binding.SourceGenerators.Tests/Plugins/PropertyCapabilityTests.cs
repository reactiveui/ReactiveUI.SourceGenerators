// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Plugins.Observation;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.Plugins;

/// <summary>
/// Covers which properties each mechanism reaches. A type advertises a mechanism; a property participates in it
/// or does not, and the mechanisms differ in what participation means - a companion field, a companion event, or
/// nothing at all beyond the type implementing an interface.
/// </summary>
public class PropertyCapabilityTests
{
    /// <summary>The property the capability questions are asked about, which no widget reports.</summary>
    private const string PropertyName = "Caption";

    /// <summary>A property an Android widget raises an event for.</summary>
    private const string ReportedWidgetPropertyName = "Text";

    /// <summary>
    /// The Android mechanism reaches the widget properties that raise an event of their own. Everything else on
    /// a view changes silently, so claiming it would replace a mechanism the type may carry with one that
    /// reports nothing.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AndroidPlugin_ReachesOnlyThePropertiesAWidgetReports()
    {
        var plugin = AndroidObservation.Plugin;
        var classInfo = NativeObservationTestModels.CreateSegment(plugin, ReportedWidgetPropertyName, "string").DeclaringTypeInfo!;

        await Assert.That(plugin.CanObserveProperty(classInfo, ReportedWidgetPropertyName)).IsTrue();
        await Assert.That(plugin.CanObserveProperty(classInfo, PropertyName)).IsFalse();
    }

    /// <summary>
    /// A property the consumer declared on its own subclass is the consumer's, not the widget's, so the widget
    /// mechanism does not claim it even when the name matches one a widget reports.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AndroidPlugin_PropertyDeclaredByTheConsumer_IsNotReached()
    {
        var classInfo = ModelFactory.CreateClassBindingInfo(
            inheritsAndroidView: true,
            properties: new EquatableArray<ObservablePropertyInfo>(
                [ModelFactory.CreateObservablePropertyInfo(ReportedWidgetPropertyName)]));

        await Assert.That(AndroidObservation.Plugin.CanObserveProperty(classInfo, ReportedWidgetPropertyName))
            .IsFalse();
    }

    /// <summary>The notification interfaces name the property in the event, so every property is reachable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NotifyPlugin_ReachesAnyPropertyOfAMatchedType()
    {
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        await Assert.That(new INPCObservationPlugin().CanObserveProperty(classInfo, PropertyName)).IsTrue();
    }

    /// <summary>
    /// Key-value observing reaches the properties the Apple frameworks declare, which are the ones the Obj-C
    /// runtime backs. A property the consumer added to its own subclass is an ordinary CLR property that no key
    /// path resolves, so it falls through to whatever else the type carries.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task KvoPlugin_ReachesOnlyThePropertiesTheFrameworkDeclares()
    {
        var frameworkProperty = ModelFactory.CreateClassBindingInfo(inheritsNSObject: true);
        var consumerProperty = ModelFactory.CreateClassBindingInfo(
            inheritsNSObject: true,
            properties: new EquatableArray<ObservablePropertyInfo>(
                [ModelFactory.CreateObservablePropertyInfo(PropertyName)]));
        var plugin = new KVOObservationPlugin();

        await Assert.That(plugin.CanObserveProperty(frameworkProperty, PropertyName)).IsTrue();
        await Assert.That(plugin.CanObserveProperty(consumerProperty, PropertyName)).IsFalse();
    }

    /// <summary>A plain CLR property cannot be observed through a native dependency-property mechanism.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DependencyPropertyPlugins_RejectPlainClrProperty()
    {
        var property = ModelFactory.CreateObservablePropertyInfo(PropertyName) with { SymbolsInspected = true };
        var classInfo = ModelFactory.CreateClassBindingInfo(inheritsWpfDependencyObject: true, properties: new([property]));

        await Assert.That(new WpfObservationPlugin().CanObserveProperty(classInfo, PropertyName)).IsFalse();
        await Assert.That(WinUIObservation.Plugin.CanObserveProperty(classInfo, PropertyName)).IsFalse();
    }

    /// <summary>A declared property with no companion change event is out of the component mechanism's reach.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ComponentPlugin_DoesNotReachAPropertyWithoutAChangeEvent()
    {
        var classInfo = ModelFactory.CreateClassBindingInfo(
            inheritsWinFormsComponent: true,
            properties: new EquatableArray<ObservablePropertyInfo>(
                [ModelFactory.CreateObservablePropertyInfo(PropertyName)]));

        await Assert.That(WinFormsObservation.Plugin.CanObserveProperty(classInfo, PropertyName)).IsFalse();
    }

    /// <summary>A declared property with a companion change event is within the component mechanism's reach.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ComponentPlugin_ReachesAPropertyWithAChangeEvent()
    {
        var plugin = WinFormsObservation.Plugin;
        var classInfo = NativeObservationTestModels.CreateSegment(plugin, PropertyName, "string").DeclaringTypeInfo!;
        await Assert.That(plugin.CanObserveProperty(classInfo, PropertyName)).IsTrue();
    }
}
