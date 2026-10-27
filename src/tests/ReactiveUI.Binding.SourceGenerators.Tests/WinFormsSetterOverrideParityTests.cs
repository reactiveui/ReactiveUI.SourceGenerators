// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>Checks custom set-method precedence and layout recovery after a native collection fault.</summary>
public class WinFormsSetterOverrideParityTests
{
    /// <summary>The strongest set-method provider is selected once and must beat the native score.</summary>
    /// <param name="panel">The native owner type.</param>
    /// <param name="score">The custom provider score.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [Arguments("Panel", 9)]
    [Arguments("Panel", 10)]
    [Arguments("Panel", 11)]
    [Arguments("TableLayoutPanel", 9)]
    [Arguments("TableLayoutPanel", 10)]
    [Arguments("TableLayoutPanel", 11)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task BindTo_CustomSetterRequiresHigherAffinity(string panel, int score) =>
        AssertRuns(OverrideScenario(panel, score));

    /// <summary>Layout resumes when the native collection rejects a write.</summary>
    /// <param name="panel">The native owner type.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [Arguments("Panel")]
    [Arguments("TableLayoutPanel")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task BindTo_NativeFailureResumesLayout(string panel) =>
        AssertRuns(WinFormsSetterParityTests.Framework + $$"""
            public class Target { public System.Windows.Forms.{{panel}} Panel { get; } = new System.Windows.Forms.{{panel}}(); }
            public static class Usage
            {
                public static bool Run()
                {
                    var target = new Target();
                    var source = new ReactiveUI.Primitives.Signals.Signal<List<System.Windows.Forms.Button>>();
                    using (source.BindTo(target, x => x.Panel.Controls))
                    {
                        target.Panel.Controls.ThrowOnAdd = true;
                        try { source.OnNext(new List<System.Windows.Forms.Button>()); }
                        catch (InvalidOperationException) {}
                        return target.Panel.Suspends == 1 && target.Panel.Resumes == 1;
                    }
                }
            }
            """);

    /// <summary>Executes the generated consumer with private converter registrations.</summary>
    /// <param name="source">The executable consumer.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    private static async Task AssertRuns(string source)
    {
        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        var (assembly, context) = TestHelper.EmitAndLoad(result, true);
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

    /// <summary>Builds a custom setter whose calls and affinity queries remain observable.</summary>
    /// <param name="panel">The collection owner type.</param>
    /// <param name="score">The custom affinity.</param>
    /// <returns>The executable consumer.</returns>
    private static string OverrideScenario(string panel, int score) => WinFormsSetterParityTests.Framework + $$"""
        public class Target { public System.Windows.Forms.{{panel}} Panel { get; } = new System.Windows.Forms.{{panel}}(); }
        public class Setter : ISetMethodBindingConverter
        {
            public int Calls, Votes;
            public int GetAffinityForObjects(Type from, Type to) { Votes++; return {{score}}; }
            public object PerformSet(object target, object value, object[] args)
            {
                var collection = (System.Windows.Forms.Control.ControlCollection)target;
                if (!(value is List<System.Windows.Forms.Button>) || args != null) throw new InvalidOperationException("Invalid adapter arguments");
                Calls++;
                return collection;
            }
        }
        public static class Usage
        {
            public static bool Run()
            {
                var setter = new Setter();
                BindingConverters.Current.SetMethodConverters.Register(setter);
                var target = new Target();
                var source = new ReactiveUI.Primitives.Signals.Signal<List<System.Windows.Forms.Button>>();
                using (source.BindTo(target, x => x.Panel.Controls))
                {
                    var votes = setter.Votes;
                    source.OnNext(new List<System.Windows.Forms.Button>());
                    source.OnNext(new List<System.Windows.Forms.Button>());
                    var custom = {{score}} > 10;
                    return setter.Calls == (custom ? 2 : 0) && setter.Votes == votes
                        && target.Panel.Suspends == (custom ? 0 : 2);
                }
            }
        }
        """;
}
