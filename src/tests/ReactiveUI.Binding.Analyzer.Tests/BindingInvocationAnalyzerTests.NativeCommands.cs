// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Analyzer.Analyzers;
using ReactiveUI.Binding.Analyzer.Tests.Helpers;

namespace ReactiveUI.Binding.Analyzer.Tests;

/// <summary>Checks native command contracts against the bindable-event diagnostic.</summary>
public partial class BindingInvocationAnalyzerTests
{
    /// <summary>The framework command members available in the test compilation.</summary>
    private const string NativeCommandFramework = """
        namespace Foundation { public class NSObject {} }
        namespace ObjCRuntime { public class Selector {} }
        namespace AppKit
        {
            public class NSControl : Foundation.NSObject { public Foundation.NSObject Target { get; set; } }
            public class NSCell : NSControl {}
            public class NSMenu : NSControl {}
            public class NSMenuItem : NSControl {}
            public class NSToolbarItem : NSControl {}
        }
        namespace UIKit
        {
            public enum UIControlEvent { TouchUpInside }
            public class UIControl
            {
                public bool Enabled { get; set; }
                public void AddTarget(System.EventHandler handler, UIControlEvent kind) {}
                public void RemoveTarget(System.EventHandler handler, UIControlEvent kind) {}
            }
            public class UIRefreshControl : UIControl { public event System.EventHandler ValueChanged; }
            public class UIBarButtonItem { public bool Enabled { get; set; } public event System.EventHandler Clicked; }
        }
        public class ClickBase { public event System.EventHandler Click; }
        public class DerivedButton : ClickBase {}
        """;

    /// <summary>Supported native command mechanisms do not require a generic default event.</summary>
    /// <param name="controlType">The native control being bound.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [Arguments("AppKit.NSControl")]
    [Arguments("AppKit.NSCell")]
    [Arguments("AppKit.NSMenu")]
    [Arguments("AppKit.NSMenuItem")]
    [Arguments("AppKit.NSToolbarItem")]
    [Arguments("UIKit.UIControl")]
    [Arguments("UIKit.UIRefreshControl")]
    [Arguments("UIKit.UIBarButtonItem")]
    [Arguments("DerivedButton")]
    public async Task RXUIBIND007_NativeCommand_NoDiagnostic(string controlType)
    {
        var source = InteractionCommandPreamble + NativeCommandFramework + $$"""
            public class NativeViewModel { public ICommand Save { get; set; } }
            public class NativeView : ReactiveUI.Binding.IViewFor
            {
                public object ViewModel { get; set; }
                public {{controlType}} Button { get; } = new {{controlType}}();
            }
            public class NativeUsage
            {
                public void Bind(NativeView view, NativeViewModel model)
                {
                    ReactiveUI.Binding.ReactiveUIBindingExtensions.BindCommand(view, model, x => x.Save, x => x.Button);
                }
            }
            """;
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(source);
        await Assert.That(diagnostics.Any(static diagnostic => diagnostic.Id == NoBindableEventDiagnosticId)).IsFalse();
    }
}
