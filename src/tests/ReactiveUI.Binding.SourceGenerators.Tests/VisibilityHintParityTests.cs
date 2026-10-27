// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>Executes the framework-specific visibility hint contracts in generated bindings.</summary>
public class VisibilityHintParityTests
{
    /// <summary>ReactiveUI's hint enum name.</summary>
    private const string ReactiveUIHint = "BooleanToVisibilityHint";

    /// <summary>The hint enum name this library's platform packages declare.</summary>
    private const string BindingHint = "BooleanToVisibilityHints";

    /// <summary>
    /// Inverse and hidden flags follow the selected framework's converters in both directions, whether the hint is
    /// ReactiveUI's enum or this library's, and whether it is a variable or a constant.
    /// </summary>
    /// <param name="framework">The framework enum namespace.</param>
    /// <param name="hintNamespace">The namespace declaring the hint enum.</param>
    /// <param name="hintName">The hint enum's name.</param>
    /// <param name="supportsHidden">Whether this framework uses Hidden for a false value.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [Arguments("System.Windows", "ReactiveUI", ReactiveUIHint, true)]
    [Arguments("Microsoft.Maui", "ReactiveUI", ReactiveUIHint, true)]
    [Arguments("Microsoft.UI.Xaml", "ReactiveUI", ReactiveUIHint, false)]
    [Arguments("Windows.UI.Xaml", "ReactiveUI.Uno", ReactiveUIHint, false)]
    [Arguments("System.Windows", "ReactiveUI.Reactive", ReactiveUIHint, true)]
    [Arguments("System.Windows", "ReactiveUI.Binding.Wpf", BindingHint, true)]
    [Arguments("System.Windows", "ReactiveUI.Binding.Reactive.Wpf", BindingHint, true)]
    [Arguments("Microsoft.Maui", "ReactiveUI.Binding.Maui", BindingHint, true)]
    [Arguments("Microsoft.Maui", "ReactiveUI.Binding.Reactive.Maui", BindingHint, true)]
    public async Task BindTo_UsesNativeVisibilityHints(string framework, string hintNamespace, string hintName, bool supportsHidden)
    {
        var result = TestHelper.RunGenerator(Scenario(framework, hintNamespace, hintName, supportsHidden), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.GeneratedSourceDoesNotContain("BindToDispatch.g.cs", "RuntimeBindingConverter");
        var (assembly, context) = TestHelper.EmitAndLoad(result);
        try
        {
            var run = assembly.GetType("Usage")!.GetMethod("Run", BindingFlags.Public | BindingFlags.Static)!;
            await Assert.That((bool)run.Invoke(null, null)!).IsTrue();
        }
        finally
        {
            context.Unload();
        }
    }

    /// <summary>
    /// BindTo with this library's WPF hint, as a constant, a combination and a variable, tests the hint's flags in
    /// the generated conversion.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task BindTo_TestsBindingWpfHintFlags()
    {
        const string Source = """
            using System;
            using System.ComponentModel;
            using ReactiveUI.Binding;
            using ReactiveUI.Binding.Wpf;

            namespace System.Windows
            {
                public enum Visibility { Visible, Collapsed, Hidden }
            }

            namespace ReactiveUI.Binding.Wpf
            {
                [Flags]
                public enum BooleanToVisibilityHints { None = 0, Inverse = 2, UseHidden = 4 }
            }

            namespace TestApp
            {
                public class PanelView : INotifyPropertyChanged
                {
                    public event PropertyChangedEventHandler PropertyChanged;

                    public System.Windows.Visibility Spinner { get; set; }

                    public System.Windows.Visibility Content { get; set; }

                    public System.Windows.Visibility Footer { get; set; }
                }

                public class Usage
                {
                    public void Bind(IObservable<bool> busy, PanelView view, BooleanToVisibilityHints hint)
                    {
                        busy.BindTo(view, v => v.Spinner, conversionHint: BooleanToVisibilityHints.Inverse);
                        busy.BindTo(view, v => v.Content, conversionHint: BooleanToVisibilityHints.Inverse | BooleanToVisibilityHints.UseHidden);
                        busy.BindTo(view, v => v.Footer, conversionHint: hint);
                    }
                }
            }
            """;

        var result = await TestHelper.TestPassWithResult(Source, typeof(VisibilityHintParityTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.GeneratedSourceContains("BindToDispatch.g.cs", "global::ReactiveUI.Binding.Wpf.BooleanToVisibilityHints.Inverse");
    }

    /// <summary>A consumer that can reach both ReactiveUI's hint enum and this library's has either honoured.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task BindTo_HonoursEitherHintWhenBothAreDeclared()
    {
        const string Source = """
            using System;
            using ReactiveUI.Binding;
            namespace System.Windows { public enum Visibility { Visible, Collapsed, Hidden } }
            namespace ReactiveUI { [Flags] public enum BooleanToVisibilityHint { None = 0, Inverse = 2, UseHidden = 4 } }
            namespace ReactiveUI.Binding.Wpf { [Flags] public enum BooleanToVisibilityHints { None = 0, Inverse = 2, UseHidden = 4 } }
            public class Target { public System.Windows.Visibility Value { get; set; } }
            public static class Usage
            {
                public static bool Run()
                {
                    IObservable<bool> source = new global::ReactiveUI.Primitives.Advanced.ImmediateReturnSignal<bool>(true);
                    var fromReactiveUI = new Target();
                    var fromBinding = new Target();
                    using (source.BindTo(fromReactiveUI, x => x.Value, conversionHint: global::ReactiveUI.BooleanToVisibilityHint.Inverse | global::ReactiveUI.BooleanToVisibilityHint.UseHidden))
                    using (source.BindTo(fromBinding, x => x.Value, conversionHint: global::ReactiveUI.Binding.Wpf.BooleanToVisibilityHints.Inverse))
                    {
                        return fromReactiveUI.Value == System.Windows.Visibility.Hidden
                            && fromBinding.Value == System.Windows.Visibility.Collapsed;
                    }
                }
            }
            """;

        var result = TestHelper.RunGenerator(Source, LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        var (assembly, context) = TestHelper.EmitAndLoad(result);
        try
        {
            var run = assembly.GetType("Usage")!.GetMethod("Run", BindingFlags.Public | BindingFlags.Static)!;
            await Assert.That((bool)run.Invoke(null, null)!).IsTrue();
        }
        finally
        {
            context.Unload();
        }
    }

    /// <summary>Builds a consumer covering every combination of the native visibility flags.</summary>
    /// <param name="framework">The framework enum namespace.</param>
    /// <param name="hintNamespace">The hint enum namespace.</param>
    /// <param name="hintName">The hint enum's name.</param>
    /// <param name="supportsHidden">Whether Hidden is meaningful on this framework.</param>
    /// <returns>The executable consumer.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string Scenario(string framework, string hintNamespace, string hintName, bool supportsHidden) => $$"""
        using System;
        using ReactiveUI.Binding;
        using Visibility = {{framework}}.Visibility;
        using Hint = {{hintNamespace}}.{{hintName}};
        namespace {{framework}} { public enum Visibility { Visible, Collapsed, Hidden } }
        namespace {{hintNamespace}} { [Flags] public enum {{hintName}} { None = 0, Inverse = 2, UseHidden = 4 } }
        public class Target<T> { public T Value { get; set; } }
        public static class Usage
        {
            public static bool Run()
            {
                foreach (var hint in new[] { Hint.None, Hint.Inverse, Hint.UseHidden, Hint.Inverse | Hint.UseHidden })
                {
                    foreach (var value in new[] { true, false })
                    {
                        var visible = (hint & Hint.Inverse) != 0 ? !value : value;
                        var hidden = {{(supportsHidden ? "true" : "false")}} && (hint & Hint.UseHidden) != 0;
                        var expected = visible ? Visibility.Visible : hidden ? Visibility.Hidden : Visibility.Collapsed;
                        var target = new Target<Visibility>();
                        IObservable<bool> source = new global::ReactiveUI.Primitives.Advanced.ImmediateReturnSignal<bool>(value);
                        using (source.BindTo(target, x => x.Value, conversionHint: hint))
                            if (target.Value != expected) return false;
                    }
                    foreach (var visibility in new[] { Visibility.Visible, Visibility.Collapsed, Visibility.Hidden })
                    {
                        var expected = (visibility == Visibility.Visible) != ((hint & Hint.Inverse) != 0);
                        var target = new Target<bool>();
                        IObservable<Visibility> source = new global::ReactiveUI.Primitives.Advanced.ImmediateReturnSignal<Visibility>(visibility);
                        using (source.BindTo(target, x => x.Value, conversionHint: hint))
                            if (target.Value != expected) return false;
                    }
                }
                var inverted = new Target<Visibility>();
                IObservable<bool> notShown = new global::ReactiveUI.Primitives.Advanced.ImmediateReturnSignal<bool>(false);
                using (notShown.BindTo(inverted, x => x.Value, conversionHint: Hint.Inverse))
                    if (inverted.Value != Visibility.Visible) return false;
                return true;
            }
        }
        """;
}
