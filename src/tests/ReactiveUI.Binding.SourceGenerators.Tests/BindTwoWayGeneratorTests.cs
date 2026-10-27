// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>Snapshot tests for BindTwoWay (two-way binding) invocation generation.</summary>
public class BindTwoWayGeneratorTests
{
    /// <summary>Verifies BindTwoWay with same-type string property binding.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_StringToString()
    {
        var source = SharedSourceReader.ReadScenario("BindTwoWay/SinglePropertyStringToString");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(BindTwoWayGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies BindTwoWay with int property binding.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_IntToInt()
    {
        var source = SharedSourceReader.ReadScenario("BindTwoWay/SinglePropertyIntToInt");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(BindTwoWayGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies BindTwoWay with multiple bindings on the same source/target pair.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MultipleBindings()
    {
        var source = SharedSourceReader.ReadScenario("BindTwoWay/MultipleBindings");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(BindTwoWayGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies BindTwoWay on a ReactiveObject-based ViewModel and View.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveObject_Both()
    {
        var source = SharedSourceReader.ReadScenario("BindTwoWay/ReactiveObjectBoth");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(BindTwoWayGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies BindTwoWay combined with BindOneWay in the same compilation.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MixedWithBindOneWay()
    {
        var source = SharedSourceReader.ReadScenario("BindTwoWay/MixedWithBindOneWay");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(BindTwoWayGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies BindTwoWay with conversion functions between int and string.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_WithConverters()
    {
        var source = SharedSourceReader.ReadScenario("BindTwoWay/SinglePropertyWithConverters");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(BindTwoWayGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies BindTwoWay with a scheduler parameter.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_WithScheduler()
    {
        var source = SharedSourceReader.ReadScenario("BindTwoWay/SinglePropertyWithScheduler");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(BindTwoWayGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies that BindTwoWay generates CallerFilePath dispatch when targeting pre-C# 10.
    /// CompilationSucceeds is omitted because the CallerFilePath stub signature is ambiguous
    /// with the runtime extension method in this test harness (both assemblies are referenced).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_StringToString_CallerFilePath()
    {
        var source = SharedSourceReader.ReadScenario("BindTwoWay/SinglePropertyStringToString");
        var result = await TestHelper.TestPassWithResult(
            source,
            typeof(BindTwoWayGeneratorTests),
            TestHelper.FallbackLanguageVersion(nullableEnabled: true));
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies BindTwoWay with both conversion functions and a scheduler parameter.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_WithConvertersAndScheduler()
    {
        var source = SharedSourceReader.ReadScenario("BindTwoWay/SinglePropertyWithConvertersAndScheduler");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(BindTwoWayGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies BindTwoWay with multiple bindings sharing the same type signature to cover the else-if dispatch branch.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MultipleSameTypeBindings()
    {
        var source = SharedSourceReader.ReadScenario("BindTwoWay/MultipleSameTypeBindings");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(BindTwoWayGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies that BindTwoWay with multiple same-type bindings generates CallerFilePath dispatch when targeting pre-C# 10.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MultipleSameTypeBindings_CallerFilePath()
    {
        var source = SharedSourceReader.ReadScenario("BindTwoWay/MultipleSameTypeBindings");
        var result = await TestHelper.TestPassWithResult(
            source,
            typeof(BindTwoWayGeneratorTests),
            TestHelper.FallbackLanguageVersion(nullableEnabled: true));
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies BindTwoWay against an NSObject target, whose observation instantiates the KVO helper
    /// classes. Those helpers are declared in a file of their own, so a compilation that reaches them
    /// only through a binding - never through WhenChanged - still compiles.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task KVO_NSObject_Target()
    {
        var source = ApplePlatformSource.BindingScenario("vm.BindTwoWay(view, x => x.Name, x => x.Text)");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(BindTwoWayGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
        await result.GeneratedSourceContains("BindTwoWayDispatch.g.cs", ApplePlatformSource.KvoObservable);
    }
}
