// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>Executes every Foundation date conversion, including nullable failure contracts.</summary>
public class AppleConversionParityTests
{
    /// <summary>Every registered date direction preserves the native converter's value and null behavior.</summary>
    /// <param name="from">The input type.</param>
    /// <param name="to">The output type.</param>
    /// <param name="input">The input expression.</param>
    /// <param name="expected">The expected assignment behavior.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [Arguments("DateTime", "Foundation.NSDate", "date", "target.Value.Value == date")]
    [Arguments("DateTime?", "Foundation.NSDate", "date", "target.Value.Value == date")]
    [Arguments("DateTime?", "Foundation.NSDate", "null", "target.Writes == 0")]
    [Arguments("DateTimeOffset", "Foundation.NSDate", "new DateTimeOffset(date)", "target.Value.Value == date")]
    [Arguments("DateTimeOffset?", "Foundation.NSDate", "new DateTimeOffset(date)", "target.Value.Value == date")]
    [Arguments("DateTimeOffset?", "Foundation.NSDate", "null", "target.Writes == 0")]
    [Arguments("Foundation.NSDate", "DateTime", "new Foundation.NSDate(date)", "target.Value == date")]
    [Arguments("Foundation.NSDate", "DateTime?", "new Foundation.NSDate(date)", "target.Value == date")]
    [Arguments("Foundation.NSDate", "DateTimeOffset", "new Foundation.NSDate(date)", "target.Value == new DateTimeOffset(date)")]
    [Arguments("Foundation.NSDate", "DateTimeOffset?", "new Foundation.NSDate(date)", "target.Value == new DateTimeOffset(date)")]
    [Arguments("Foundation.NSDate", "DateTime", "null", "target.Writes == 0")]
    [Arguments("Foundation.NSDate", "DateTime?", "null", "target.Writes == 0")]
    [Arguments("Foundation.NSDate", "DateTimeOffset", "null", "target.Writes == 0")]
    [Arguments("Foundation.NSDate", "DateTimeOffset?", "null", "target.Writes == 0")]
    public async Task BindTo_PreservesNativeDateConversion(string from, string to, string input, string expected)
    {
        var result = TestHelper.RunGenerator(Scenario(from, to, input, expected), LanguageVersion.CSharp10);
        await result.CompilationSucceeds();
        await result.GeneratedSourceContains("BindToDispatch.g.cs", "GetAffinityForObjects() <= 8");
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

    /// <summary>Builds a typed binding whose target counts successful assignments.</summary>
    /// <param name="from">The input type.</param>
    /// <param name="to">The output type.</param>
    /// <param name="input">The input expression.</param>
    /// <param name="expected">The expected result.</param>
    /// <returns>The executable consumer.</returns>
    private static string Scenario(string from, string to, string input, string expected) => $$"""
        using System;
        using ReactiveUI.Binding;
        namespace Foundation
        {
            public class NSDate
            {
                public DateTime Value { get; }
                public NSDate(DateTime value) { Value = value; }
                public static explicit operator NSDate(DateTime value) => new NSDate(value);
                public static explicit operator DateTime(NSDate value) => value.Value;
            }
        }
        public class Target
        {
            private {{to}} _value;
            public int Writes { get; private set; }
            public {{to}} Value { get { return _value; } set { _value = value; Writes++; } }
        }
        public static class Usage
        {
            public static bool Run()
            {
                var date = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
                IObservable<{{from}}> source = new global::ReactiveUI.Primitives.Advanced.ImmediateReturnSignal<{{from}}>({{input}});
                var target = new Target();
                using (source.BindTo(target, x => x.Value))
                {
                    return {{expected}};
                }
            }
        }
        """;
}
