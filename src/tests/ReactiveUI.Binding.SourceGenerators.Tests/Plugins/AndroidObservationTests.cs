// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Plugins.Observation;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.Plugins;

/// <summary>Tests for <see cref="AndroidObservation.Inspect"/> against widget types compiled from source.</summary>
public class AndroidObservationTests
{
    /// <summary>The widgets each report one property through one event, as the Android bindings declare them.</summary>
    private const string WidgetSource = """
        using System;

        namespace Android.Views { public class View { } }

        namespace Android.Widget
        {
            public class TextView : Android.Views.View
            {
                public string Text { get; set; }
                public event EventHandler TextChanged;
            }

            public class NumberPicker : Android.Views.View
            {
                public int Value { get; set; }
                public event EventHandler ValueChanged;
            }

            public class RatingBar : Android.Views.View
            {
                public float Rating { get; set; }
                public event EventHandler RatingBarChange;
            }

            public class CompoundButton : Android.Views.View
            {
                public bool Checked { get; set; }
                public event EventHandler CheckedChange;
            }

            public class CalendarView : Android.Views.View
            {
                public long Date { get; set; }
                public event EventHandler DateChange;
            }

            public class TabHost : Android.Views.View
            {
                public int CurrentTab { get; set; }
                public event EventHandler TabChanged;
            }

            public class AdapterView : Android.Views.View
            {
                public object SelectedItem { get; set; }
                public event EventHandler ItemSelected;
                public event EventHandler NothingSelected;
            }

            public class TimePicker : Android.Views.View
            {
                public int Hour { get; set; }
                public int Minute { get; set; }
                public int CurrentHour { get; set; }
                public int CurrentMinute { get; set; }
                public event EventHandler TimeChanged;
            }
        }
        """;

    /// <summary>The fully qualified delegate type every fake widget event is declared with.</summary>
    private const string HandlerType = "global::System.EventHandler";

    /// <summary>A property its widget reports is offered with that widget's change event.</summary>
    /// <param name="widget">The widget's metadata name.</param>
    /// <param name="property">The reported property.</param>
    /// <param name="changeEvent">The event the widget raises for it.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments("Android.Widget.TextView", "Text", "TextChanged")]
    [Arguments("Android.Widget.NumberPicker", "Value", "ValueChanged")]
    [Arguments("Android.Widget.RatingBar", "Rating", "RatingBarChange")]
    [Arguments("Android.Widget.CompoundButton", "Checked", "CheckedChange")]
    [Arguments("Android.Widget.CalendarView", "Date", "DateChange")]
    [Arguments("Android.Widget.TabHost", "CurrentTab", "TabChanged")]
    [Arguments("Android.Widget.TimePicker", "Hour", "TimeChanged")]
    [Arguments("Android.Widget.TimePicker", "Minute", "TimeChanged")]
    [Arguments("Android.Widget.TimePicker", "CurrentHour", "TimeChanged")]
    [Arguments("Android.Widget.TimePicker", "CurrentMinute", "TimeChanged")]
    public async Task Inspect_ReportedProperty_OffersTheWidgetsEvent(string widget, string property, string changeEvent)
    {
        var info = Inspect(WidgetSource, widget, property);

        await Assert.That(info).IsNotNull();
        await Assert.That(info!.Kind).IsEqualTo(AndroidObservation.Kind);
        await Assert.That(info.Events.Length).IsEqualTo(1);
        await Assert.That(info.Events[0].Name).IsEqualTo(changeEvent);
        await Assert.That(info.Events[0].HandlerType).IsEqualTo(HandlerType);
    }

    /// <summary>A selection needs both of the events a selection change can raise, and is offered with both.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Inspect_SelectedItem_OffersBothSelectionEvents()
    {
        var info = Inspect(WidgetSource, "Android.Widget.AdapterView", "SelectedItem");

        await Assert.That(info).IsNotNull();
        await Assert.That(string.Join(",", info!.Events.Select(static e => e.Name))).IsEqualTo("ItemSelected,NothingSelected");
    }

    /// <summary>A selection whose widget cannot report the selection clearing is not offered at all.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Inspect_SelectedItemWithoutNothingSelected_OffersNothing()
    {
        const string source = """
            using System;

            namespace Android.Views { public class View { } }

            namespace Android.Widget
            {
                public class AdapterView : Android.Views.View
                {
                    public object SelectedItem { get; set; }
                    public event EventHandler ItemSelected;
                }
            }
            """;

        await Assert.That(Inspect(source, "Android.Widget.AdapterView", "SelectedItem")).IsNull();
    }

    /// <summary>A reported property whose widget does not declare the change event is not offered.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Inspect_WidgetWithoutItsChangeEvent_OffersNothing()
    {
        const string source = """
            namespace Android.Views { public class View { } }

            namespace Android.Widget
            {
                public class TextView : Android.Views.View
                {
                    public string Text { get; set; }
                }
            }
            """;

        await Assert.That(Inspect(source, "Android.Widget.TextView", "Text")).IsNull();
    }

    /// <summary>A property no widget reports is not offered, whatever the type declares.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Inspect_PropertyNoWidgetReports_OffersNothing()
    {
        const string source = """
            namespace Android.Views { public class View { } }

            namespace Android.Widget
            {
                public class TextView : Android.Views.View
                {
                    public string Hint { get; set; }
                }
            }
            """;

        await Assert.That(Inspect(source, "Android.Widget.TextView", "Hint")).IsNull();
    }

    /// <summary>Compiles the widgets and inspects one property through the type that declares it.</summary>
    /// <param name="source">The widget declarations.</param>
    /// <param name="widget">The declaring widget's metadata name.</param>
    /// <param name="propertyName">The property to inspect.</param>
    /// <returns>The candidate the mechanism offers, or null.</returns>
    private static PlatformObservationInfo? Inspect(string source, string widget, string propertyName)
    {
        var owner = TestHelper.CreateCompilation(source).GetTypeByMetadataName(widget)!;
        var property = owner.GetMembers(propertyName).OfType<IPropertySymbol>().Single();
        return AndroidObservation.Inspect(owner, property);
    }
}
