// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>Snapshot tests for OneWayBind (view-first one-way binding) invocation generation.</summary>
public class OneWayBindGeneratorTests
{
    /// <summary>Verifies OneWayBind with same-type string property binding.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_StringToString()
    {
        var source = SharedSourceReader.ReadScenario("OneWayBind/SinglePropertyStringToString");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(OneWayBindGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies OneWayBind with multiple bindings on the same view/vm pair.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MultipleBindings()
    {
        var source = SharedSourceReader.ReadScenario("OneWayBind/MultipleBindings");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(OneWayBindGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies OneWayBind with int-to-int property binding.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_IntToInt()
    {
        var source = SharedSourceReader.ReadScenario("OneWayBind/SinglePropertyIntToInt");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(OneWayBindGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies OneWayBind with a selector (conversion) function from int to string.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_WithSelector()
    {
        var source = SharedSourceReader.ReadScenario("OneWayBind/SinglePropertyWithSelector");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(OneWayBindGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies OneWayBind with a selector (conversion) function and scheduler.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_WithSelectorAndScheduler()
    {
        var source = SharedSourceReader.ReadScenario("OneWayBind/SinglePropertyWithSelectorAndScheduler");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(OneWayBindGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies that OneWayBind generates CallerFilePath dispatch when targeting pre-C# 10.
    /// CompilationSucceeds is omitted because the CallerFilePath stub signature is ambiguous
    /// with the runtime extension method in this test harness (both assemblies are referenced).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_StringToString_CallerFilePath()
    {
        var source = SharedSourceReader.ReadScenario("OneWayBind/SinglePropertyStringToString");
        var result = await TestHelper.TestPassWithResult(
            source,
            typeof(OneWayBindGeneratorTests),
            TestHelper.FallbackLanguageVersion(nullableEnabled: true));
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies OneWayBind with two same-type-signature bindings (string-to-string) generates
    /// "if" for the first invocation and "else if" for the second invocation in the dispatch method.
    /// Covers OneWayBindCodeGenerator line 147 (i == 0 ? "if" : "else if" with i > 0).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TwoSameTypeBindings_GeneratesElseIf()
    {
        var source = SharedSourceReader.ReadScenario("OneWayBind/TwoSameTypeBindings");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(OneWayBindGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }
}
