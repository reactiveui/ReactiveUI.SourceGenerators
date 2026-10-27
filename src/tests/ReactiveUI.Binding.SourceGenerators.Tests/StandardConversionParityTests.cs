// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>Compares generated conversions with the runtime converters that ReactiveUI registers.</summary>
public class StandardConversionParityTests
{
    /// <summary>Formatting, parsing, nullable values and conversion hints agree with the runtime providers.</summary>
    /// <param name="type">The converted CLR type.</param>
    /// <param name="name">The runtime converter's type-name stem.</param>
    /// <param name="value">A representative typed input expression.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [Arguments("byte", "Byte", "(byte)7")]
    [Arguments("short", "Short", "(short)7")]
    [Arguments("int", "Integer", "7")]
    [Arguments("long", "Long", "7L")]
    [Arguments("float", "Single", "7.25f")]
    [Arguments("double", "Double", "7.25d")]
    [Arguments("decimal", "Decimal", "7.25m")]
    [Arguments("bool", "Boolean", "true")]
    [Arguments("Guid", "Guid", "new Guid(\"11111111-2222-3333-4444-555555555555\")")]
    [Arguments("DateTime", "DateTime", "new DateTime(2026, 1, 2, 3, 4, 5)")]
    [Arguments("DateTimeOffset", "DateTimeOffset", "new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero)")]
    [Arguments("TimeSpan", "TimeSpan", "TimeSpan.FromMinutes(75)")]
    [Arguments("DateOnly", "DateOnly", "new DateOnly(2026, 1, 2)")]
    [Arguments("TimeOnly", "TimeOnly", "new TimeOnly(3, 4, 5)")]
    public async Task BindTo_MatchesStandardProviders(string type, string name, string value)
    {
        var result = TestHelper.RunGenerator(Scenario(type, name, value), LanguageVersion.CSharp10);
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

    /// <summary>Builds formatting and parsing probes with success, invalid, empty and null inputs.</summary>
    /// <param name="type">The converted CLR type.</param>
    /// <param name="name">The converter type-name stem.</param>
    /// <param name="value">The representative input expression.</param>
    /// <returns>The executable consumer source.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string Scenario(string type, string name, string value) => $$"""
        using System;
        using System.Globalization;
        using ReactiveUI.Binding;
        public class Target<T>
        {
            private T _value;
            public Target(T initial) { _value = initial; Initial = initial; }
            public T Initial { get; }
            public int Writes { get; private set; }
            public T Value { get { return _value; } set { _value = value; Writes++; } }
        }
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
                        foreach (object hint in new object[] { null, 4 })
                        {
                            if (!Format({{value}}, hint) || !FormatNullable({{value}}, hint) || !FormatNullable(null, hint)) return false;
                        }
                        var provider = new global::ReactiveUI.Binding.{{name}}ToStringTypeConverter();
                        object formatted;
                        provider.TryConvertTyped({{value}}, null, out formatted);
                        foreach (var text in new[] { (string)formatted, "not a value", "", null })
                        {
                            if (!Parse(text) || !ParseNullable(text)) return false;
                        }
                    }
                    return true;
                }
                finally { CultureInfo.CurrentCulture = previous; }
            }
            {{Formatting(type, name, false)}}
            {{Formatting(type, name, true)}}
            {{Parsing(type, name, false)}}
            {{Parsing(type, name, true)}}
            private static bool Matches<TIn, TOut>(global::ReactiveUI.Binding.IBindingTypeConverter provider, TIn value, object hint, Target<TOut> target)
            {
                object expected;
                var success = provider.TryConvertTyped(value, hint, out expected);
                if (!success && typeof(TOut).IsAssignableFrom(typeof(TIn))) { success = true; expected = value; }
                var writes = success && !object.Equals(target.Initial, expected) ? 1 : 0;
                if (target.Writes != writes || (success && !object.Equals(target.Value, expected)))
                {
                    throw new InvalidOperationException(provider.GetType().Name + ": input=" + (object)value + ", hint=" + hint
                        + ", success=" + success + ", expected=" + expected + ", actual=" + target.Value + ", writes=" + target.Writes);
                }
                return true;
            }
        }
        """;

    /// <summary>Creates one formatting binding alongside its runtime provider comparison.</summary>
    /// <param name="type">The formatted value type.</param>
    /// <param name="name">The converter type-name stem.</param>
    /// <param name="nullable">Whether the input is nullable.</param>
    /// <returns>The consumer method.</returns>
    private static string Formatting(string type, string name, bool nullable)
    {
        var sourceType = nullable ? $"{type}?" : type;
        var method = nullable ? "FormatNullable" : "Format";
        var provider = nullable ? $"Nullable{name}ToStringTypeConverter" : $"{name}ToStringTypeConverter";
        return $$"""
            private static bool {{method}}({{sourceType}} value, object hint)
            {
                var target = new Target<string>("unconverted");
                IObservable<{{sourceType}}> source = new global::ReactiveUI.Primitives.Advanced.ImmediateReturnSignal<{{sourceType}}>(value);
                using (source.BindTo(target, x => x.Value, conversionHint: hint))
                    return Matches(new global::ReactiveUI.Binding.{{provider}}(), value, hint, target);
            }
            """;
    }

    /// <summary>Creates one parsing binding alongside its runtime provider comparison.</summary>
    /// <param name="type">The parsed value type.</param>
    /// <param name="name">The converter type-name stem.</param>
    /// <param name="nullable">Whether the output is nullable.</param>
    /// <returns>The consumer method.</returns>
    private static string Parsing(string type, string name, bool nullable)
    {
        var targetType = nullable ? $"{type}?" : type;
        var method = nullable ? "ParseNullable" : "Parse";
        var provider = nullable ? $"StringToNullable{name}TypeConverter" : $"StringTo{name}TypeConverter";
        return $$"""
            private static bool {{method}}(string value)
            {
                var target = new Target<{{targetType}}>(({{targetType}})default({{type}}));
                IObservable<string> source = new global::ReactiveUI.Primitives.Advanced.ImmediateReturnSignal<string>(value);
                using (source.BindTo(target, x => x.Value))
                    return Matches(new global::ReactiveUI.Binding.{{provider}}(), value, null, target);
            }
            """;
    }
}
