// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>Checks the native Apple property mappings and scores against ReactiveUI.</summary>
public class AppleObservationParityTests
{
    /// <summary>The native contracts consumed by generated Apple observations.</summary>
    private const string Framework = """
        using System;
        using System.Collections.Generic;
        using ReactiveUI.Binding;
        namespace Foundation
        {
            public class NSObject : IDisposable { public void Dispose() {} }
            public class NSNotification : NSObject {}
            public class NSNotificationCenter
            {
                public static NSNotificationCenter DefaultCenter { get; } = new NSNotificationCenter();
                private readonly List<Entry> _entries = new List<Entry>();
                public int Count => _entries.Count;
                public NSObject AddObserver(string name, Action<NSNotification> callback, NSObject sender)
                {
                    var entry = new Entry { Name = name, Callback = callback, Sender = sender };
                    _entries.Add(entry);
                    return entry;
                }
                public void RemoveObserver(NSObject observer) => _entries.Remove((Entry)observer);
                public void Post(string name, NSObject sender)
                {
                    foreach (var entry in _entries.ToArray())
                        if (entry.Name == name && ReferenceEquals(entry.Sender, sender)) entry.Callback(new NSNotification());
                }
                private sealed class Entry : NSObject
                {
                    public string Name;
                    public Action<NSNotification> Callback;
                    public NSObject Sender;
                }
            }
        }
        namespace UIKit
        {
            public class UIControl : Foundation.NSObject
            {
                public event EventHandler ValueChanged;
                public int Value { get; set; }
                public void EmitChange() => ValueChanged?.Invoke(this, EventArgs.Empty);
                public bool HasHandlers => ValueChanged != null;
            }
            public class UITextField : UIControl
            {
                public static string TextFieldTextDidChangeNotification => "FieldText";
                public int Text { get; set; }
            }
            public class UITextView : Foundation.NSObject
            {
                public static string TextDidChangeNotification => "ViewText";
                public int Text { get; set; }
            }
            public class UIDatePicker : UIControl { public int Date { get; set; } }
            public class UISegmentedControl : UIControl { public int SelectedSegment { get; set; } }
            public class UISwitch : UIControl { public int On { get; set; } }
            public class UITabBar : Foundation.NSObject
            {
                public event EventHandler<EventArgs> ItemSelected;
                public int SelectedItem { get; set; }
                public void EmitChange() => ItemSelected?.Invoke(this, EventArgs.Empty);
                public bool HasHandlers => ItemSelected != null;
            }
            public class UISearchBar : Foundation.NSObject
            {
                public event EventHandler<EventArgs> TextChanged;
                public int Text { get; set; }
                public void EmitChange() => TextChanged?.Invoke(this, EventArgs.Empty);
                public bool HasHandlers => TextChanged != null;
            }
        }
        namespace AppKit
        {
            public class NSControl : Foundation.NSObject
            {
                public static string TextDidChangeNotification => "ControlText";
                public int AlphaValue { get; set; }
                public int DoubleValue { get; set; }
                public int FloatValue { get; set; }
                public int IntValue { get; set; }
                public int NintValue { get; set; }
                public int ObjectValue { get; set; }
                public int StringValue { get; set; }
                public int AttributedStringValue { get; set; }
            }
        }
        """;

    /// <summary>Each native property emits its own subscription and the score a custom provider must beat.</summary>
    /// <param name="type">The framework control type.</param>
    /// <param name="property">The observed property.</param>
    /// <param name="affinity">The native provider's score.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [Arguments("UIKit.UIControl", "Value", 20)]
    [Arguments("UIKit.UITextField", "Text", 30)]
    [Arguments("UIKit.UITextView", "Text", 30)]
    [Arguments("UIKit.UIDatePicker", "Date", 30)]
    [Arguments("UIKit.UISegmentedControl", "SelectedSegment", 30)]
    [Arguments("UIKit.UISwitch", "On", 30)]
    [Arguments("UIKit.UITabBar", "SelectedItem", 30)]
    [Arguments("UIKit.UISearchBar", "Text", 30)]
    [Arguments("AppKit.NSControl", "AlphaValue", 20)]
    [Arguments("AppKit.NSControl", "DoubleValue", 20)]
    [Arguments("AppKit.NSControl", "FloatValue", 20)]
    [Arguments("AppKit.NSControl", "IntValue", 20)]
    [Arguments("AppKit.NSControl", "NintValue", 20)]
    [Arguments("AppKit.NSControl", "ObjectValue", 20)]
    [Arguments("AppKit.NSControl", "StringValue", 20)]
    [Arguments("AppKit.NSControl", "AttributedStringValue", 20)]
    public async Task NativeProperty_ObservesAndDetachesWithCorrectAffinity(string type, string property, int affinity)
    {
        var result = TestHelper.RunGenerator(Scenario(type, property), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.GeneratedSourceContains("WhenChangedDispatch.g.cs", $"\"{property}\", {affinity}, false");
        await result.GeneratedSourceContains("WhenChangedDispatch.g.cs", "global::ReactiveUI.Binding.Observables.CallbackPropertyObservable<");
        var (assembly, context) = TestHelper.EmitAndLoad(result);
        try
        {
            var run = assembly.GetType("Usage")!.GetMethod("Run", BindingFlags.Static | BindingFlags.Public)!;
            await Assert.That((bool)run.Invoke(null, null)!).IsTrue();
        }
        finally
        {
            context.Unload();
        }
    }

    /// <summary>Builds a native subscription that checks filtering, delivery and deterministic detachment.</summary>
    /// <param name="type">The framework control type.</param>
    /// <param name="property">The observed property.</param>
    /// <returns>The executable consumer source.</returns>
    private static string Scenario(string type, string property)
    {
        var notification = type switch
        {
            "UIKit.UITextField" => "TextFieldTextDidChangeNotification",
            "UIKit.UITextView" or "AppKit.NSControl" => "TextDidChangeNotification",
            _ => null,
        };
        var raise = notification is null ? "value.EmitChange();" : $"Foundation.NSNotificationCenter.DefaultCenter.Post({type}.{notification}, value);";
        var detached = notification is null ? "!value.HasHandlers" : "Foundation.NSNotificationCenter.DefaultCenter.Count == 0";
        return Framework + $$"""

            public static class Usage
            {
                public static bool Run()
                {
                    var value = new {{type}}();
                    var values = new List<int>();
                    var subscription = value.WhenChanged(x => x.{{property}}).Subscribe(new Observer(values));
                    value.{{property}} = 1;
                    {{raise}}
                    {{raise}}
                    subscription.Dispose();
                    subscription.Dispose();
                    value.{{property}} = 2;
                    {{raise}}
                    return values.Count == 2 && values[0] == 0 && values[1] == 1 && {{detached}};
                }
                private sealed class Observer : IObserver<int>
                {
                    private readonly List<int> _values;
                    public Observer(List<int> values) { _values = values; }
                    public void OnNext(int value) => _values.Add(value);
                    public void OnError(Exception error) { throw error; }
                    public void OnCompleted() {}
                }
            }
            """;
    }
}
