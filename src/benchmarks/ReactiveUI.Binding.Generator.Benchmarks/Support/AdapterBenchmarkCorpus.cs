// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Generator.Benchmarks.Support;

/// <summary>Provides small consumer compilations for the platform adapter families.</summary>
internal static class AdapterBenchmarkCorpus
{
    /// <summary>Observes native control events and sender-filtered Apple notifications.</summary>
    private const string Observation = """
        using System;
        using ReactiveUI.Binding;
        namespace Foundation
        {
            public class NSObject : IDisposable { public void Dispose() {} }
            public class NSString : NSObject {}
            public class NSNotification : NSObject {}
            public class NSNotificationCenter
            {
                public static NSNotificationCenter DefaultCenter { get; } = new NSNotificationCenter();
                public NSObject AddObserver(NSString name, Action<NSNotification> handler, NSObject sender) => new NSObject();
                public void RemoveObserver(NSObject token) {}
            }
        }
        namespace UIKit
        {
            public class UIControl : Foundation.NSObject { public double Value { get; set; } public event EventHandler ValueChanged; }
            public class UITextField : UIControl
            {
                public string Text { get; set; }
                public static Foundation.NSString TextFieldTextDidChangeNotification { get; } = new Foundation.NSString();
            }
        }
        public static class Usage
        {
            public static void Observe(UIKit.UIControl control, UIKit.UITextField text)
            {
                var value = control.WhenChanged(x => x.Value);
                var content = text.WhenChanged(x => x.Text);
            }
        }
        """;

    /// <summary>Binds Android, UIKit and AppKit controls through their native command members.</summary>
    private const string Commands = """
        using System;
        using System.Windows.Input;
        using ReactiveUI.Binding;
        namespace Foundation
        {
            public class NSObject : IDisposable { public void Dispose() {} }
            public class ExportAttribute : Attribute { public ExportAttribute(string name) {} }
        }
        namespace ObjCRuntime { public class Selector : IDisposable { public Selector(string name) {} public void Dispose() {} } }
        namespace Android.Views { public class View { public event EventHandler Click; public bool Enabled { get; set; } } }
        namespace UIKit
        {
            public enum UIControlEvent { TouchUpInside }
            public class UIControl : Foundation.NSObject
            {
                public bool Enabled { get; set; }
                public void AddTarget(EventHandler handler, UIControlEvent kind) {}
                public void RemoveTarget(EventHandler handler, UIControlEvent kind) {}
            }
        }
        namespace AppKit
        {
            public class NSControl : Foundation.NSObject
            {
                public Foundation.NSObject Target { get; set; }
                public ObjCRuntime.Selector Action { get; set; }
                public bool Enabled { get; set; }
            }
            public class NSMenuItem : NSControl {}
        }
        public class Model { public ICommand Command { get; set; } }
        public class View : IViewFor<Model>
        {
            public Model ViewModel { get; set; }
            object IViewFor.ViewModel { get { return ViewModel; } set { ViewModel = (Model)value; } }
            public Android.Views.View Android { get; set; }
            public UIKit.UIControl UIKit { get; set; }
            public AppKit.NSControl AppKit { get; set; }
            public void Bind(Model model)
            {
                this.BindCommand(model, x => x.Command, x => x.Android);
                this.BindCommand(model, x => x.Command, x => x.UIKit);
                this.BindCommand(model, x => x.Command, x => x.AppKit);
            }
        }
        """;

    /// <summary>Writes typed control arrays into both WinForms collection contracts.</summary>
    private const string Collections = """
        using System;
        using ReactiveUI.Binding;
        namespace System.Windows.Forms
        {
            public class Control
            {
                public void SuspendLayout() {}
                public void ResumeLayout() {}
                public class ControlCollection
                {
                    public Control Owner { get; set; }
                    public void Clear() {}
                    public void AddRange(Control[] controls) {}
                }
            }
            public class Button : Control {}
            public class TableLayoutControlCollection : Control.ControlCollection { public Control Container => Owner; }
        }
        public class Target
        {
            public System.Windows.Forms.Control.ControlCollection Panel { get; }
            public System.Windows.Forms.TableLayoutControlCollection Table { get; }
        }
        public static class Usage
        {
            public static void Bind(IObservable<System.Windows.Forms.Button[]> source, Target target)
            {
                source.BindTo(target, x => x.Panel);
                source.BindTo(target, x => x.Table);
            }
        }
        """;

    /// <summary>Converts nullable numbers, visibility values and Apple dates with concrete types.</summary>
    private const string Conversions = """
        using System;
        using ReactiveUI.Binding;
        namespace System.Windows { public enum Visibility { Visible, Collapsed, Hidden } }
        namespace Foundation
        {
            public class NSDate
            {
                public static explicit operator NSDate(DateTime value) => new NSDate();
                public static explicit operator DateTime(NSDate value) => default(DateTime);
            }
        }
        public class Target
        {
            public string Text { get; set; }
            public int? Number { get; set; }
            public System.Windows.Visibility Visibility { get; set; }
            public Foundation.NSDate Date { get; set; }
        }
        public static class Usage
        {
            public static void Bind(IObservable<int> number, IObservable<bool> visible, IObservable<DateTime?> date, Target target)
            {
                number.BindTo(target, x => x.Text);
                number.BindTo(target, x => x.Number);
                visible.BindTo(target, x => x.Visibility);
                date.BindTo(target, x => x.Date);
            }
        }
        """;

    /// <summary>Returns the requested adapter family's consumer.</summary>
    /// <param name="family">The family named by the benchmark parameter.</param>
    /// <returns>The C# source to compile.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The family is unknown.</exception>
    internal static string Source(string family) => family switch
    {
        nameof(Observation) => Observation,
        nameof(Commands) => Commands,
        nameof(Collections) => Collections,
        nameof(Conversions) => Conversions,
        _ => throw new ArgumentOutOfRangeException(nameof(family), family, "Unknown adapter family."),
    };
}
