// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Plugins;
using ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

namespace ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

/// <summary>Supplies verified native member data to the static emission tests.</summary>
internal static class NativeObservationTestModels
{
    /// <summary>Creates the native property data consumed by one mechanism's emitter.</summary>
    /// <param name="plugin">The selected mechanism.</param>
    /// <param name="name">The property name.</param>
    /// <param name="type">The property type.</param>
    /// <param name="declaringType">The concrete owner type.</param>
    /// <returns>The property and its verified native members.</returns>
    internal static PropertyPathSegment CreateSegment(
        IObservationPlugin plugin,
        string name,
        string type,
        string declaringType = "global::TestApp.MyViewModel")
    {
        var kind = plugin.ObservationKind;
        var eventName = kind == "Android" ? AndroidWidgetEvents.FindChangeEvent(name) : $"{name}Changed";
        var candidates = kind == "Android" && eventName is null
            ? default
            : new EquatableArray<PlatformObservationInfo>([new(kind, plugin.Affinity, new([new(eventName!, "global::System.EventHandler")]), null, null, null)]);
        var property = new ObservablePropertyInfo(name, type, true, false, kind == "WinUIDP", eventName is not null, false, candidates, true);
        var info = ModelFactory.CreateClassBindingInfo(fullyQualifiedName: declaringType, properties: new([property]));
        return new(name, type, declaringType, true, info);
    }
}
