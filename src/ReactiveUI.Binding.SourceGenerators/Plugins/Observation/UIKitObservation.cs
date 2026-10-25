// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

/// <summary>Observes UIKit text, selection, date and switch properties through their native notifications.</summary>
internal static class UIKitObservation
{
    /// <summary>The mechanism's identity.</summary>
    internal const string Kind = "UIKit";

    /// <summary>The native UIKit property-specific score.</summary>
    private const int PropertyAffinity = 30;

    /// <summary>Gets the plugin the registry selects this mechanism through.</summary>
    internal static NativeObservationPlugin Plugin { get; } = new(Kind, PropertyAffinity, Inspect, AppendSubscription);

    /// <summary>Offers a candidate for a text control's notification or a control's typed change event.</summary>
    /// <param name="owner">The concrete type through which the property is accessed.</param>
    /// <param name="property">The property being observed.</param>
    /// <returns>The eligible candidate, or null.</returns>
    internal static PlatformObservationInfo? Inspect(INamedTypeSymbol owner, IPropertySymbol property)
    {
        var notification = NotificationFor(owner, property.Name);
        if (notification is not null)
        {
            return new(Kind, PropertyAffinity, default, notification, null, null);
        }

        var widget = property.Name switch
        {
            "Date" => "UIKit.UIDatePicker",
            "SelectedSegment" => "UIKit.UISegmentedControl",
            "On" => "UIKit.UISwitch",
            "SelectedItem" => "UIKit.UITabBar",
            "Text" => "UIKit.UISearchBar",
            _ => null,
        };
        if (widget is null || !PlatformSymbols.DerivesFrom(owner, widget))
        {
            return null;
        }

        var eventName = property.Name switch
        {
            "SelectedItem" => "ItemSelected",
            "Text" => "TextChanged",
            _ => "ValueChanged",
        };
        var changeEvent = PlatformSymbols.FindEvent(owner, eventName);
        return changeEvent is null ? null : new(Kind, PropertyAffinity, new([changeEvent]), null, null, null);
    }

    /// <summary>Emits the native notification selected for this property.</summary>
    /// <param name="sb">The output builder.</param>
    /// <param name="segment">The observed property.</param>
    /// <param name="info">The verified native notification.</param>
    internal static void AppendSubscription(SourceWriter sb, PropertyPathSegment segment, PlatformObservationInfo info)
    {
        if (info.NotificationName is not null)
        {
            AppleNotificationEmitter.AppendSubscription(sb, segment, info);
        }
        else
        {
            NativeEventSubscriptionEmitter.Append(sb, info.Events);
        }
    }

    /// <summary>Resolves sender-scoped text notifications for UIKit text controls.</summary>
    /// <param name="owner">The property owner.</param>
    /// <param name="name">The property name.</param>
    /// <returns>The notification constant, or null.</returns>
    internal static string? NotificationFor(INamedTypeSymbol owner, string name)
    {
        if (name != "Text")
        {
            return null;
        }

        if (PlatformSymbols.DerivesFrom(owner, "UIKit.UITextField"))
        {
            return AppleNotificationEmitter.ResolveNotification(owner, "TextFieldTextDidChangeNotification");
        }

        return PlatformSymbols.DerivesFrom(owner, "UIKit.UITextView")
            ? AppleNotificationEmitter.ResolveNotification(owner, "TextDidChangeNotification")
            : null;
    }
}
