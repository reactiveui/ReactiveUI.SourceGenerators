// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>Snapshot tests for WhenAnyValue (ReactiveUI-compatible) invocation generation.</summary>
public class WhenAnyValueGeneratorTests
{
    /// <summary>Verifies WhenAnyValue with a single property on an INPC class.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_INPC()
    {
        var source = SharedSourceReader.ReadScenario("WhenAnyValue/SinglePropertyINPC");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(WhenAnyValueGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies WhenAnyValue on a ReactiveObject-based class.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_ReactiveObject()
    {
        var source = SharedSourceReader.ReadScenario("WhenAnyValue/SinglePropertyReactiveObject");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(WhenAnyValueGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies WhenAnyValue with two properties returns a tuple.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MultiProperty_TwoProperties()
    {
        var source = SharedSourceReader.ReadScenario("WhenAnyValue/MultiPropertyTwoProperties");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(WhenAnyValueGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies WhenAnyValue with three properties returns a tuple.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MultiProperty_ThreeProperties()
    {
        var source = SharedSourceReader.ReadScenario("WhenAnyValue/MultiPropertyThreeProperties");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(WhenAnyValueGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies WhenAnyValue with a selector function combining two properties.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MultiProperty_WithSelector()
    {
        var source = SharedSourceReader.ReadScenario("WhenAnyValue/MultiPropertyWithSelector");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(WhenAnyValueGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies WhenAnyValue with five properties.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MultiProperty_FiveProperties()
    {
        var source = SharedSourceReader.ReadScenario("WhenAnyValue/MultiPropertyFiveProperties");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(WhenAnyValueGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies WhenAnyValue with a deep property chain.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepPropertyChain()
    {
        var source = SharedSourceReader.ReadScenario("WhenAnyValue/DeepPropertyChain");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(WhenAnyValueGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies WhenAnyValue with nullable property types.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableProperties()
    {
        var source = SharedSourceReader.ReadScenario("WhenAnyValue/NullableProperties");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(WhenAnyValueGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>Verifies WhenAnyValue with twelve properties (maximum standard overload).</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MultiProperty_TwelveProperties()
    {
        var source = SharedSourceReader.ReadScenario("WhenAnyValue/MultiPropertyTwelveProperties");
        var result =
            await TestHelper.TestPassWithResult(source, typeof(WhenAnyValueGeneratorTests), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.HasNoGeneratorDiagnostics();
    }

    /// <summary>
    /// Verifies that WhenAnyValue generates CallerFilePath dispatch when targeting pre-C# 10.
    /// CompilationSucceeds is omitted because the CallerFilePath stub signature is ambiguous
    /// with the runtime extension method in this test harness (both assemblies are referenced).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_INPC_CallerFilePath()
    {
        var source = SharedSourceReader.ReadScenario("WhenAnyValue/SinglePropertyINPC");
        var result = await TestHelper.TestPassWithResult(
            source,
            typeof(WhenAnyValueGeneratorTests),
            TestHelper.FallbackLanguageVersion(nullableEnabled: true));
        await result.HasNoGeneratorDiagnostics();
    }
}
