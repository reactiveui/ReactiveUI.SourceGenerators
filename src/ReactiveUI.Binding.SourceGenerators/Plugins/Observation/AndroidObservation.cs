// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

/// <summary>Observes Android widget properties through their native typed events.</summary>
internal static class AndroidObservation
{
    /// <summary>The mechanism's identity.</summary>
    internal const string Kind = "Android";

    /// <summary>Gets the plugin the registry selects this mechanism through.</summary>
    internal static NativeObservationPlugin Plugin { get; } =
        new(Kind, BindingAffinity.Explicit, Inspect, NativeEventSubscriptionEmitter.AppendSubscription);

    /// <summary>Offers a candidate for a widget property whose declaring widget raises its change event.</summary>
    /// <param name="owner">The concrete type through which the property is accessed.</param>
    /// <param name="property">The property being observed.</param>
    /// <returns>The eligible candidate, or null.</returns>
    internal static PlatformObservationInfo? Inspect(INamedTypeSymbol owner, IPropertySymbol property)
    {
        var widget = WidgetFor(property.Name);
        if (widget is null || !PlatformSymbols.DerivesFrom(property.ContainingType, widget))
        {
            return null;
        }

        var first = PlatformSymbols.FindEvent(owner, AndroidWidgetEvents.FindChangeEvent(property.Name)!);
        if (first is null)
        {
            return null;
        }

        var events = new EquatableArray<NotificationEventInfo>([first]);
        if (property.Name == "SelectedItem")
        {
            var second = PlatformSymbols.FindEvent(owner, "NothingSelected");
            if (second is null)
            {
                return null;
            }

            events = new([first, second]);
        }

        return new(Kind, BindingAffinity.Explicit, events, null, null, null);
    }

    /// <summary>Names the widget that defines each observable property contract.</summary>
    /// <param name="propertyName">The observed property.</param>
    /// <returns>The native widget type, or null.</returns>
    private static string? WidgetFor(string propertyName) => propertyName switch
    {
        "Text" => "Android.Widget.TextView",
        "Value" => "Android.Widget.NumberPicker",
        "Rating" => "Android.Widget.RatingBar",
        "Checked" => "Android.Widget.CompoundButton",
        "Date" => "Android.Widget.CalendarView",
        "CurrentTab" => "Android.Widget.TabHost",
        "SelectedItem" => "Android.Widget.AdapterView",
        "Hour" or "Minute" or "CurrentHour" or "CurrentMinute" => "Android.Widget.TimePicker",
        _ => null,
    };
}
