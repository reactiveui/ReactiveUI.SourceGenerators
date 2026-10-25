// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>Snapshot tests for Bind (view-first two-way binding) invocation generation.</summary>
public class BindGeneratorTests
{
    /// <summary>Verifies Bind with same-type string property binding.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_StringToString()
    {
        var source = SharedSourceReader.ReadScenario("Bind/SinglePropertyStringToString");
        var result = await TestHelper.TestPassWithResult(source, typeof(BindGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies Bind with multiple bindings on the same view/vm pair.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MultipleBindings()
    {
        var source = SharedSourceReader.ReadScenario("Bind/MultipleBindings");
        var result = await TestHelper.TestPassWithResult(source, typeof(BindGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies Bind with conversion functions between int and string.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_WithConverters()
    {
        var source = SharedSourceReader.ReadScenario("Bind/SinglePropertyWithConverters");
        var result = await TestHelper.TestPassWithResult(source, typeof(BindGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies Bind with conversion functions and a scheduler.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_WithConvertersAndScheduler()
    {
        var source = SharedSourceReader.ReadScenario("Bind/SinglePropertyWithConvertersAndScheduler");
        var result = await TestHelper.TestPassWithResult(source, typeof(BindGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies that Bind generates CallerFilePath dispatch when targeting pre-C# 10.
    /// CompilationSucceeds is omitted because the CallerFilePath stub signature is ambiguous
    /// with the runtime extension method in this test harness (both assemblies are referenced).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_StringToString_CallerFilePath()
    {
        var source = SharedSourceReader.ReadScenario("Bind/SinglePropertyStringToString");
        var result = await TestHelper.TestPassWithResult(
            source,
            typeof(BindGeneratorTests),
            TestHelper.FallbackLanguageVersion(nullableEnabled: true));
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies Bind with two same-type-signature bindings (string-to-string) generates
    /// "if" for the first invocation and "else if" for the second invocation in the dispatch method.
    /// Covers BindCodeGenerator line 157 (i == 0 ? "if" : "else if" with i > 0).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TwoSameTypeBindings_GeneratesElseIf()
    {
        var source = SharedSourceReader.ReadScenario("Bind/TwoSameTypeBindings");
        var result = await TestHelper.TestPassWithResult(source, typeof(BindGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }
}
