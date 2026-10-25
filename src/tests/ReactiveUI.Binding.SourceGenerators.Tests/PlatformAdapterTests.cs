// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>Checks platform subscriptions against the consumer's concrete framework members.</summary>
public class PlatformAdapterTests
{
    /// <summary>The observation dispatch output.</summary>
    private const string DispatchFile = "WhenChangedDispatch.g.cs";

    /// <summary>Android event delegates carry their framework argument type through every chain link.</summary>
    /// <param name="path">The observed property path.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [Arguments("x => x.Text")]
    [Arguments("x => x.Child.Text")]
    public async Task AndroidText_UsesTypedEvent(string path)
    {
        var source = $$"""
            using System;
            using ReactiveUI.Binding;
            namespace Android.Views { public class View {} }
            namespace Android.Widget
            {
                public class TextChangedEventArgs : EventArgs {}
                public class TextView : Android.Views.View
                {
                    public event EventHandler<TextChangedEventArgs> TextChanged;
                    public string Text { get; set; }
                    public TextView Child { get; set; }
                }
            }
            public static class Usage
            {
                public static IObservable<string> Observe(Android.Widget.TextView view)
                    => view.WhenChanged({{path}});
            }
            """;

        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.GeneratedSourceContains(DispatchFile, "global::System.EventHandler<global::Android.Widget.TextChangedEventArgs>");
        await result.GeneratedSourceContains(DispatchFile, "TextChanged +=");
        await result.GeneratedSourceContains(DispatchFile, "TextChanged -=");
    }

    /// <summary>A matching property name on a different widget cannot claim a nonexistent event.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task AndroidProperty_WithoutWidgetEvent_DoesNotClaimNotification()
    {
        const string source = """
            using System;
            using ReactiveUI.Binding;
            namespace Android.Views { public class View {} }
            namespace Android.Widget
            {
                public class OtherView : Android.Views.View
                {
                    public string Text { get; set; }
                }
            }
            public static class Usage
            {
                public static IObservable<string> Observe(Android.Widget.OtherView view)
                    => view.WhenChanged(x => x.Text);
            }
            """;

        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.GeneratedSourceContains(DispatchFile, "\"Text\", 1, false");
        await result.GeneratedSourceContains(DispatchFile, "global::ReactiveUI.Binding.Observables.DeferredPropertyObservable<");
    }
}
