// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>Executes nullable, URI, equality and identity conversions without the runtime conversion engine.</summary>
public class ConversionEdgeParityTests
{
    /// <summary>A generated conversion delivers accepted values and preserves the target when it declines.</summary>
    /// <param name="sourceType">The stream value type.</param>
    /// <param name="targetType">The destination type.</param>
    /// <param name="value">The incoming value expression.</param>
    /// <param name="initial">The destination's initial value expression.</param>
    /// <param name="expected">The expected destination expression.</param>
    /// <param name="hint">The conversion hint expression.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [Arguments("int", "int?", "42", "null", "(int?)42", "null")]
    [Arguments("int?", "int", "42", "99", "42", "null")]
    [Arguments("int?", "int", "null", "99", "99", "null")]
    [Arguments("decimal", "decimal?", "1.5m", "null", "(decimal?)1.5m", "null")]
    [Arguments("string", "Uri", "\"relative/path\"", "null", "new Uri(\"relative/path\", UriKind.Relative)", "null")]
    [Arguments("string", "Uri", "null", "new Uri(\"initial\", UriKind.Relative)", "new Uri(\"initial\", UriKind.Relative)", "null")]
    [Arguments("Uri", "string", "new Uri(\"relative/path\", UriKind.Relative)", "null", "\"relative/path\"", "null")]
    [Arguments("Uri", "string", "null", "\"initial\"", "\"initial\"", "null")]
    [Arguments("object", "bool", "\"same\"", "false", "true", "\"same\"")]
    [Arguments("object", "bool", "\"different\"", "true", "false", "\"same\"")]
    [Arguments("string", "string", "null", "\"initial\"", "null", "null")]
    [Arguments("string", "string", "\"value\"", "\"initial\"", "\"value\"", "null")]
    public async Task BindTo_HandlesConversionEdges(string sourceType, string targetType, string value, string initial, string expected, string hint)
    {
        var result = TestHelper.RunGenerator(
            $$"""
            using System;
            using ReactiveUI.Binding;
            public class Target { public {{targetType}} Value { get; set; } = {{initial}}; }
            public static class Usage
            {
                public static bool Run()
                {
                    var target = new Target();
                    IObservable<{{sourceType}}> source = new ReactiveUI.Primitives.Advanced.ImmediateReturnSignal<{{sourceType}}>({{value}});
                    using (source.BindTo(target, x => x.Value, conversionHint: {{hint}}))
                        return object.Equals(target.Value, {{expected}});
                }
            }
            """,
            LanguageVersion.CSharp10);
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
}
