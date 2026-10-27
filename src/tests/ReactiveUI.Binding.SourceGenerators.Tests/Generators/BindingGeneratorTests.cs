// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.Generators;

/// <summary>Tests that generated infrastructure follows the operations a consumer uses.</summary>
public class BindingGeneratorTests
{
    /// <summary>The output containing selected native observation helpers.</summary>
    private const string ObservationHelpersFile = "ObservationHelpers.g.cs";

    /// <summary>The native callback contract and a control exposing native and ordinary properties.</summary>
    private const string WinUISource = """
        namespace Microsoft.UI.Xaml
        {
            public class DependencyProperty { }
            public class DependencyObject
            {
                public long RegisterPropertyChangedCallback(DependencyProperty dp, DependencyPropertyChangedCallback callback) => 0;
                public void UnregisterPropertyChangedCallback(DependencyProperty dp, long token) { }
            }
            public delegate void DependencyPropertyChangedCallback(DependencyObject sender, DependencyProperty dp);
        }
        namespace Framework
        {
            public class Control : Microsoft.UI.Xaml.DependencyObject, System.ComponentModel.INotifyPropertyChanged
            {
                public static readonly Microsoft.UI.Xaml.DependencyProperty NativeProperty = new Microsoft.UI.Xaml.DependencyProperty();
                public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
                public string Native { get; set; }
                public string Ordinary { get; set; }
            }
        }
        """;

    /// <summary>Unused native types do not require observation helpers.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Initialize_UnusedNativeType_EmitsNoObservationHelpers()
    {
        var result = TestHelper.RunGenerator(ApplePlatformSource.TypeDetectionScenario());

        await Assert.That(result.GeneratedSources.ContainsKey(ObservationHelpersFile)).IsFalse();
        await result.CompilationSucceeds();
    }

    /// <summary>Compile-time observation selection needs no runtime registration output.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Initialize_ObservableType_EmitsNoBinderRegistration()
    {
        var result = TestHelper.RunGenerator(ApplePlatformSource.TypeDetectionScenario());

        await Assert.That(result.GeneratedSources.ContainsKey("GeneratedBinderRegistration.g.cs")).IsFalse();
    }

    /// <summary>A framework reference alone does not require a view-thread invoker.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Initialize_UnusedFrameworkReference_EmitsNoViewThreadInvokers()
    {
        const string source = """
            namespace System.Windows.Threading
            {
                public class DispatcherObject { }
            }
            """;

        var result = TestHelper.RunGenerator(source);

        await Assert.That(result.GeneratedSources.ContainsKey("ViewThreadInvokers.g.cs")).IsFalse();
        await result.CompilationSucceeds();
    }

    /// <summary>A property using INPC does not need its type's native callback helper.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Initialize_OrdinaryProperty_EmitsNoNativeHelper()
    {
        const string scenario = """
            using ReactiveUI.Binding;
            public static class Scenario
            {
                public static System.IObservable<string> Observe(Framework.Control control)
                    => control.WhenChanged(x => x.Ordinary);
            }
            """;
        var result = TestHelper.RunGenerator(scenario + WinUISource, LanguageVersion.CSharp10);

        await Assert.That(result.GeneratedSources.ContainsKey(ObservationHelpersFile)).IsFalse();
        await result.CompilationSucceeds();
    }

    /// <summary>
    /// Native properties from referenced assemblies are observed through the runtime's callback observable, including
    /// chain links, and no helper is declared in the consumer.
    /// </summary>
    /// <param name="sourceType">The type the observation starts from.</param>
    /// <param name="propertyPath">The observed access chain.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [Arguments("Framework.Control", "x => x.Native")]
    [Arguments("Parent", "x => x.Child.Native")]
    public async Task Initialize_ReferencedNativeProperty_ObservesThroughTheRuntime(string sourceType, string propertyPath)
    {
        var framework = TestHelper.CreateCompilation(WinUISource, LanguageVersion.CSharp10)
            .WithAssemblyName("NativeFramework");
        var source = $$"""
            using ReactiveUI.Binding;
            public class Parent
            {
                public Framework.Control Child { get; set; }
            }
            public static class Scenario
            {
                public static System.IObservable<string> Observe({{sourceType}} control)
                    => control.WhenChanged({{propertyPath}});
            }
            """;
        var compilation = TestHelper.CreateCompilation(source, LanguageVersion.CSharp10)
            .AddReferences(framework.ToMetadataReference());
        var result = TestHelper.RunGenerator(compilation, LanguageVersion.CSharp10, null, false);

        await Assert.That(result.GeneratedSources.ContainsKey(ObservationHelpersFile)).IsFalse();
        await result.GeneratedSourceContains("WhenChangedDispatch.g.cs", "global::ReactiveUI.Binding.Observables.CallbackPropertyObservable<");
        await result.CompilationSucceeds();
    }
}
