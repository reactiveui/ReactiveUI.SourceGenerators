// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>Executes converter voting with private per-test registries.</summary>
public class ConversionOverrideParityTests
{
    /// <summary>Registrations win only with higher scores; explicit overrides win regardless of score.</summary>
    /// <param name="score">The registered converter score.</param>
    /// <param name="explicitOverride">Whether the call supplies the converter directly.</param>
    /// <param name="accepts">Whether the converter accepts the incoming value.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [Arguments(1, false, true)]
    [Arguments(2, false, true)]
    [Arguments(3, false, true)]
    [Arguments(3, false, false)]
    [Arguments(1, true, true)]
    [Arguments(1, true, false)]
    public async Task BindTo_SelectsConverterByAffinity(int score, bool explicitOverride, bool accepts)
    {
        var result = TestHelper.RunGenerator(Scenario(score, explicitOverride, accepts, false), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await AssertRuns(result);
    }

    /// <summary>Identity assignments allow a higher-affinity converter to replace the value.</summary>
    /// <param name="score">The custom identity converter's affinity.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [Arguments(1)]
    [Arguments(2)]
    public async Task BindTo_IdentityStillHonorsCustomConverters(int score)
    {
        var result = TestHelper.RunGenerator(Scenario(score, false, true, true), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await AssertRuns(result);
    }

    /// <summary>Runs the generated consumer with private converter state.</summary>
    /// <param name="result">The generated consumer.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    private static async Task AssertRuns(GeneratorTestResult result)
    {
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

    /// <summary>Builds a typed custom visibility converter whose object adapter must never run.</summary>
    /// <param name="score">The custom affinity.</param>
    /// <param name="explicitOverride">Whether the converter is an explicit argument.</param>
    /// <param name="accepts">Whether conversion succeeds.</param>
    /// <param name="identity">Whether source and destination have the same type.</param>
    /// <returns>The executable consumer.</returns>
    private static string Scenario(int score, bool explicitOverride, bool accepts, bool identity)
    {
        var sourceType = identity ? "Visibility" : "bool";
        return $$"""
        using System;
        using ReactiveUI.Binding;
        using Visibility = System.Windows.Visibility;
        namespace System.Windows { public enum Visibility { Visible, Collapsed, Hidden } }
        public class Target { public Visibility Value { get; set; } = Visibility.Collapsed; }
        public class Converter : IBindingTypeConverter<{{sourceType}}, Visibility>
        {
            public Type FromType => typeof({{sourceType}});
            public Type ToType => typeof(Visibility);
            public int Calls;
            public int GetAffinityForObjects() => {{score}};
            public bool TryConvert({{sourceType}} value, object hint, out Visibility result)
            {
                if (!object.Equals(hint, "hint")) throw new InvalidOperationException("Hint was lost");
                Calls++;
                result = Visibility.Hidden;
                return {{(accepts ? "true" : "false")}};
            }
            public bool TryConvertTyped(object value, object hint, out object result)
                => throw new InvalidOperationException("Typed converter was boxed");
        }
        public static class Usage
        {
            public static bool Run()
            {
                var converter = new Converter();
                BindingConverters.Current.TypedConverters.Register(converter);
                var target = new Target();
                IObservable<{{sourceType}}> source =
                    new ReactiveUI.Primitives.Advanced.ImmediateReturnSignal<{{sourceType}}>({{(identity ? "Visibility.Visible" : "true")}});
                using (source.BindTo(target, x => x.Value, conversionHint: "hint", converterOverride: {{(explicitOverride ? "converter" : "null")}}))
                {
                    var custom = {{(explicitOverride ? "true" : "false")}} || {{score}} > {{(identity ? "1" : "2")}};
                    var expected = custom ? {{(accepts ? "Visibility.Hidden" : "Visibility.Collapsed")}} : Visibility.Visible;
                    return target.Value == expected && converter.Calls == (custom ? 1 : 0);
                }
            }
        }
        """;
    }
}
