// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.RuntimeExecution;

/// <summary>Executes concrete platform conversions emitted from consumer symbols.</summary>
public class ConversionPluginRuntimeTests
{
    /// <summary>The selected visibility mapping runs in both directions without a platform registration.</summary>
    /// <param name="platformNamespace">The framework namespace exposed by the consumer.</param>
    /// <param name="enumName">The native visibility enum.</param>
    /// <param name="hidden">The value representing false.</param>
    /// <returns>The asynchronous test operation.</returns>
    [Test]
    [Arguments("System.Windows", "Visibility", "Collapsed")]
    [Arguments("Microsoft.UI.Xaml", "Visibility", "Collapsed")]
    [Arguments("Windows.UI.Xaml", "Visibility", "Collapsed")]
    [Arguments("Microsoft.Maui", "Visibility", "Collapsed")]
    [Arguments("Android.Views", "ViewStates", "Gone")]
    public async Task VisibilityBinding_ConvertsBothDirections(string platformNamespace, string enumName, string hidden)
    {
        var source = VisibilityScenario(platformNamespace, enumName, hidden);
        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        var (assembly, context) = TestHelper.EmitAndLoad(result);
        try
        {
            var run = assembly.GetType("TestApp.Usage")!.GetMethod("Run", BindingFlags.Public | BindingFlags.Static)!;
            await Assert.That((bool)run.Invoke(null, null)!).IsTrue();
        }
        finally
        {
            context.Unload();
        }
    }

    /// <summary>Builds a consumer with native visibility on one side and a boolean on the other.</summary>
    /// <param name="platformNamespace">The framework namespace.</param>
    /// <param name="enumName">The visibility enum name.</param>
    /// <param name="hidden">The false enum member.</param>
    /// <returns>The complete consumer source.</returns>
    private static string VisibilityScenario(string platformNamespace, string enumName, string hidden) => $$"""
            using System;
            using System.ComponentModel;
            using ReactiveUI.Binding;
            namespace {{platformNamespace}}
            {
                public enum {{enumName}} { Visible, {{hidden}} }
            }
            namespace TestApp
            {
                public class Model : INotifyPropertyChanged
                {
                    private bool _value;
                    public event PropertyChangedEventHandler PropertyChanged;
                    public bool Value
                    {
                        get { return _value; }
                        set { _value = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Value")); }
                    }
                }
                public class View : INotifyPropertyChanged, IViewFor<Model>
                {
                    public Model ViewModel { get; set; }
                    object IViewFor.ViewModel { get { return ViewModel; } set { ViewModel = (Model)value; } }
                    private {{platformNamespace}}.{{enumName}} _value;
                    public event PropertyChangedEventHandler PropertyChanged;
                    public {{platformNamespace}}.{{enumName}} Value
                    {
                        get { return _value; }
                        set { _value = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Value")); }
                    }
                }
                public static class Usage
                {
                    public static bool Run()
                    {
                        var model = new Model { Value = true };
                        var view = new View { ViewModel = model };
                        using (view.Bind(model, x => x.Value, x => x.Value))
                        {
                            model.Value = false;
                            if (view.Value != {{platformNamespace}}.{{enumName}}.{{hidden}}) return false;
                            view.Value = {{platformNamespace}}.{{enumName}}.Visible;
                            return model.Value;
                        }
                    }
                }
            }
            """;
}
