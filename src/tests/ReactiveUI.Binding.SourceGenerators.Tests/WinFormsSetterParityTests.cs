// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>Executes collection mutations through generated WinForms set-method adapters.</summary>
public class WinFormsSetterParityTests
{
    /// <summary>The native collection and layout contracts exercised by the generated consumer.</summary>
    internal const string Framework = """
        using System;
        using System.Collections.Generic;
        using System.ComponentModel;
        using ReactiveUI.Binding;
        namespace System.Windows.Forms
        {
            public class Control
            {
                public int Suspends, Resumes;
                public void SuspendLayout() { Suspends++; }
                public void ResumeLayout() { Resumes++; }
                public class ControlCollection : List<Control>
                {
                    public Control Owner { get; }
                    public bool ThrowOnAdd { get; set; }
                    public Control[] LastInput { get; private set; }
                    public ControlCollection(Control owner) { Owner = owner; }
                    public void AddRange(Control[] controls)
                    {
                        if (ThrowOnAdd) throw new InvalidOperationException("Native collection failure");
                        LastInput = controls;
                        base.AddRange(controls);
                    }
                }
            }
            public class Button : Control {}
            public class Panel : Control
            {
                public ControlCollection Controls { get; }
                public Panel() { Controls = new ControlCollection(this); }
            }
            public class TableLayoutControlCollection : Control.ControlCollection
            {
                public Control Container => Owner;
                public TableLayoutControlCollection(Control owner) : base(owner) {}
            }
            public class TableLayoutPanel : Control
            {
                public TableLayoutControlCollection Controls { get; }
                public TableLayoutPanel() { Controls = new TableLayoutControlCollection(this); }
            }
        }
        """;

    /// <summary>Read-only control collections receive typed values with balanced layout suspension.</summary>
    /// <param name="panel">The native collection owner.</param>
    /// <param name="api">The binding entry point.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [Arguments("Panel", "BindTo")]
    [Arguments("TableLayoutPanel", "BindTo")]
    [Arguments("Panel", "OneWayBind")]
    [Arguments("TableLayoutPanel", "OneWayBind")]
    public async Task CollectionBinding_MutatesExistingCollection(string panel, string api)
    {
        var result = TestHelper.RunGenerator(Scenario(panel, api), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.GeneratedSourceDoesNotContain($"{api}Dispatch.g.cs", "RuntimeBindingConverter");
        await AssertRuns(result);
    }

    /// <summary>The System.Reactive package applies the same native collection mutations.</summary>
    /// <param name="panel">The native collection owner.</param>
    /// <param name="api">The binding entry point.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [Arguments("Panel", "BindTo")]
    [Arguments("TableLayoutPanel", "OneWayBind")]
    public async Task CollectionBinding_ReactiveRuntimeMutatesExistingCollection(string panel, string api)
    {
        var source = Scenario(panel, api).Replace("using ReactiveUI.Binding;", "using ReactiveUI.Binding.Reactive;", StringComparison.Ordinal);
        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10, null, true);
        await result.CompilationSucceeds();
        await AssertRuns(result);
    }

    /// <summary>Concrete control arrays reach the native array API without an intermediate copy.</summary>
    /// <param name="panel">The native collection owner.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [Arguments("Panel")]
    [Arguments("TableLayoutPanel")]
    public async Task CollectionBinding_ReusesConcreteArray(string panel)
    {
        var source = Framework + $$"""
            public class Target { public System.Windows.Forms.{{panel}} Panel { get; } = new System.Windows.Forms.{{panel}}(); }
            public static class Usage
            {
                public static bool Run()
                {
                    var target = new Target();
                    var controls = new[] { new System.Windows.Forms.Button() };
                    IObservable<System.Windows.Forms.Button[]> source = new ReactiveUI.Primitives.Advanced.ImmediateReturnSignal<System.Windows.Forms.Button[]>(controls);
                    using (source.BindTo(target, x => x.Panel.Controls))
                        return ReferenceEquals(controls, target.Panel.Controls.LastInput)
                            && target.Panel.Controls.Count == 1 && ReferenceEquals(controls[0], target.Panel.Controls[0]);
                }
            }
            """;
        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await AssertRuns(result);
    }

    /// <summary>Runs the generated consumer's behavior checks.</summary>
    /// <param name="result">The compiled consumer.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    private static async Task AssertRuns(GeneratorTestResult result)
    {
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

    /// <summary>Creates a minimal WinForms collection contract and a notifying source.</summary>
    /// <param name="panel">The framework panel.</param>
    /// <param name="api">The binding API.</param>
    /// <returns>The executable consumer.</returns>
    private static string Scenario(string panel, string api)
    {
        var bind = api switch
        {
            "BindTo" => "model.WhenChanged(x => x.Items).BindTo(view, x => x.Panel.Controls)",
            _ => "view.OneWayBind(model, x => x.Items, x => x.Panel.Controls)",
        };
        return Framework + $$"""
            public class Model : INotifyPropertyChanged
            {
                private List<System.Windows.Forms.Button> _items = new List<System.Windows.Forms.Button>();
                public event PropertyChangedEventHandler PropertyChanged;
                public List<System.Windows.Forms.Button> Items
                {
                    get { return _items; }
                    set { _items = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Items")); }
                }
            }
            public class View : IViewFor<Model>
            {
                public Model ViewModel { get; set; }
                object IViewFor.ViewModel { get { return ViewModel; } set { ViewModel = (Model)value; } }
                public System.Windows.Forms.{{panel}} Panel { get; } = new System.Windows.Forms.{{panel}}();
            }
            public static class Usage
            {
                public static bool Run()
                {
                    var model = new Model();
                    var view = new View { ViewModel = model };
                    var collection = view.Panel.Controls;
                    var button = new System.Windows.Forms.Button();
                    using ({{bind}})
                    {
                        model.Items = new List<System.Windows.Forms.Button> { button };
                        if (collection.Count != 1 || collection[0] != button) return false;
                        model.Items = new List<System.Windows.Forms.Button>();
                        if (collection.Count != 0 || view.Panel.Suspends != 3 || view.Panel.Resumes != 3) return false;
                    }
                    model.Items = new List<System.Windows.Forms.Button> { button };
                    return ReferenceEquals(collection, view.Panel.Controls) && collection.Count == 0 && view.Panel.Suspends == 3;
                }
            }
            """;
    }
}
