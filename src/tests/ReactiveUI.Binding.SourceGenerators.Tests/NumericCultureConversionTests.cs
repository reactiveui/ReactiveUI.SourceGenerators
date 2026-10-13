// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>Verifies numeric formatting follows the consumer's culture in generated and runtime bindings.</summary>
public class NumericCultureConversionTests
{
    /// <summary>Checks default, precision, format and unsupported hints for nullable and nonnullable values.</summary>
    /// <param name="type">The numeric source type.</param>
    /// <param name="name">The runtime converter name stem.</param>
    /// <param name="value">The source value expression.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [Arguments("float", "Single", "7.25f")]
    public async Task NumericBindings_UseCurrentCulture(string type, string name, string value)
    {
        var result = TestHelper.RunGenerator(Scenario(type, name, value), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.GeneratedSourceDoesNotContain("BindToDispatch.g.cs", "InvariantCulture");
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

    /// <summary>Builds formatting and round-trip probes with explicit culture-sensitive expectations.</summary>
    /// <param name="type">The numeric source type.</param>
    /// <param name="name">The runtime converter name stem.</param>
    /// <param name="value">The source value expression.</param>
    /// <returns>The executable consumer source.</returns>
    private static string Scenario(string type, string name, string value) => $$"""
            using System;
            using System.Globalization;
            using ReactiveUI.Binding;
            public class Target { public string Value { get; set; } }
            public class NumericTarget { public {{type}} Value { get; set; } }
            public static class Usage
            {
                public static bool Run()
                {
                    var previous = CultureInfo.CurrentCulture;
                    try
                    {
                        foreach (var culture in new[] { "en-US", "fr-FR" })
                        {
                            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
                            {{type}} value = {{value}};
                            foreach (object hint in new object[] { null, 3, "N2", new object() })
                            {
                                var expected = hint is int ? value.ToString("F" + (int)hint)
                                    : hint is string ? value.ToString((string)hint) : value.ToString();
                                var target = new Target();
                                IObservable<{{type}}> values = new global::ReactiveUI.Primitives.Advanced.ImmediateReturnSignal<{{type}}>(value);
                                using (values.BindTo(target, x => x.Value, conversionHint: hint)) Check(expected, target.Value);
                                string formatted;
                                new {{name}}ToStringTypeConverter().TryConvert(value, hint, out formatted);
                                Check(expected, formatted);
                                IObservable<{{type}}?> nullableValues = new global::ReactiveUI.Primitives.Advanced.ImmediateReturnSignal<{{type}}?>(value);
                                using (nullableValues.BindTo(target, x => x.Value, conversionHint: hint)) Check(expected, target.Value);
                                new Nullable{{name}}ToStringTypeConverter().TryConvert(value, hint, out formatted);
                                Check(expected, formatted);
                                var roundTrip = new NumericTarget();
                                IObservable<string> text = new global::ReactiveUI.Primitives.Advanced.ImmediateReturnSignal<string>(target.Value);
                                using (text.BindTo(roundTrip, x => x.Value))
                                    if (roundTrip.Value != value) throw new InvalidOperationException("Generated numeric round trip failed: " + target.Value);
                                {{type}} parsed;
                                if (!new StringTo{{name}}TypeConverter().TryConvert(formatted, null, out parsed) || parsed != value)
                                    throw new InvalidOperationException("Runtime numeric round trip failed: " + formatted);
                            }
                        }
                    }
                    finally { CultureInfo.CurrentCulture = previous; }
                    return true;
                }
                private static void Check(string expected, string actual)
                {
                    if (actual != expected) throw new InvalidOperationException(CultureInfo.CurrentCulture.Name + ": expected=" + expected + ", actual=" + actual);
                }
            }
            """;
}
