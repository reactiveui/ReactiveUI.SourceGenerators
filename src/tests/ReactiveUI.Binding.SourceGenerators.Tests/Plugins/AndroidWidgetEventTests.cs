// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

namespace ReactiveUI.Binding.SourceGenerators.Tests.Plugins;

/// <summary>Tests for <see cref="AndroidWidgetEvents"/>, which maps a widget property to the event it reports on.</summary>
public class AndroidWidgetEventTests
{
    /// <summary>Each widget property in the map resolves to the event that widget raises for it.</summary>
    /// <param name="propertyName">The property being observed.</param>
    /// <param name="eventName">The event the widget raises when it changes.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments("Text", "TextChanged")]
    [Arguments("Value", "ValueChanged")]
    [Arguments("Rating", "RatingBarChange")]
    [Arguments("Checked", "CheckedChange")]
    [Arguments("Date", "DateChange")]
    [Arguments("CurrentTab", "TabChanged")]
    [Arguments("Hour", "TimeChanged")]
    [Arguments("Minute", "TimeChanged")]
    [Arguments("CurrentHour", "TimeChanged")]
    [Arguments("CurrentMinute", "TimeChanged")]
    [Arguments("SelectedItem", "ItemSelected")]
    public async Task FindChangeEvent_ReportingProperty_ResolvesItsEvent(string propertyName, string eventName)
    {
        var resolved = AndroidWidgetEvents.FindChangeEvent(propertyName);

        await Assert.That(resolved).IsEqualTo(eventName);
    }

    /// <summary>
    /// A property outside the map reports nothing, so the observation falls through to whatever else the
    /// type carries rather than being watched on an event that does not exist.
    /// </summary>
    /// <param name="propertyName">A property no widget in the map reports.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments("Alpha")]
    [Arguments("Visibility")]
    [Arguments("Enabled")]
    [Arguments("text")]
    [Arguments("")]
    public async Task FindChangeEvent_SilentProperty_ResolvesNothing(string propertyName)
    {
        var resolved = AndroidWidgetEvents.FindChangeEvent(propertyName);

        await Assert.That(resolved).IsNull();
    }
}
