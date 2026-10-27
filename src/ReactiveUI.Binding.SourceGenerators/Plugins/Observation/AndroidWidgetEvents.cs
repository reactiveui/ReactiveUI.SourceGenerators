// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

/// <summary>The Android widget properties that report their own changes, and the event each reports them on.</summary>
/// <remarks>
/// <para>
/// An Android view is not observable in general: it implements no change-notification interface, and most of
/// its properties are plain CLR properties that change silently. A handful of widgets do raise an event for one
/// property, and that handful is the whole of what this mechanism reaches - the runtime engine keeps the same
/// list and scores everything outside it zero, so a property that is not here falls through to whatever else
/// the type carries rather than being observed badly.
/// </para>
/// <para>
/// Keyed by property name alone, which is enough: no two widgets in the list report different events for the
/// same property name, and a property only resolves at all on a type that declares it, so matching the name
/// implies the declaring widget and therefore the event.
/// </para>
/// </remarks>
internal static class AndroidWidgetEvents
{
    /// <summary>Finds the event a widget raises when the named property changes.</summary>
    /// <param name="propertyName">The property being observed.</param>
    /// <returns>The event name, or <see langword="null"/> when no widget reports that property.</returns>
    /// <remarks>
    /// Compared in order rather than looked up: the set is closed and known here, so nothing is built at
    /// startup and nothing is held for the life of the generator.
    /// </remarks>
    internal static string? FindChangeEvent(string propertyName)
    {
        // TextView and everything built on it.
        if (propertyName == "Text")
        {
            return "TextChanged";
        }

        // NumberPicker.
        if (propertyName == "Value")
        {
            return "ValueChanged";
        }

        // RatingBar.
        if (propertyName == "Rating")
        {
            return "RatingBarChange";
        }

        // CompoundButton, and so CheckBox, RadioButton and Switch.
        if (propertyName == "Checked")
        {
            return "CheckedChange";
        }

        // CalendarView.
        if (propertyName == "Date")
        {
            return "DateChange";
        }

        // TabHost.
        if (propertyName == "CurrentTab")
        {
            return "TabChanged";
        }

        // AdapterView, and so Spinner and ListView.
        if (propertyName == "SelectedItem")
        {
            return "ItemSelected";
        }

        return IsTimePickerField(propertyName) ? "TimeChanged" : null;
    }

    /// <summary>Determines whether a name is one of the fields TimePicker reports its time change for.</summary>
    /// <param name="propertyName">The property being observed.</param>
    /// <returns><see langword="true"/> when TimePicker reports it.</returns>
    /// <remarks>
    /// The one widget in the list that names the same two values two ways: the hour and the minute are
    /// <c>Hour</c> and <c>Minute</c> from API 23, and <c>CurrentHour</c> and <c>CurrentMinute</c> before it.
    /// All four report on the same event.
    /// </remarks>
    private static bool IsTimePickerField(string propertyName) =>
        propertyName == "Hour" || propertyName == "Minute"
        || propertyName == "CurrentHour" || propertyName == "CurrentMinute";
}
