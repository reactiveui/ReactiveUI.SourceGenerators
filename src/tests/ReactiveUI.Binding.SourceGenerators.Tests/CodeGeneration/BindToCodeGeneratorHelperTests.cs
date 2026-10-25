// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.CodeGeneration;

/// <summary>Covers the <c>BindTo</c> worker for a value no compile-time conversion reaches.</summary>
public class BindToCodeGeneratorHelperTests
{
    /// <summary>The source value type, which no conversion plugin turns into the property's type.</summary>
    private const string SourceValueType = "global::TestApp.Money";

    /// <summary>The bound property's type.</summary>
    private const string StringType = "global::System.String";

    /// <summary>
    /// With no compile-time conversion the worker asks the runtime converter for each value, handing it the hint and
    /// the converter only where the call site supplied them, and writes only what converted.
    /// </summary>
    /// <param name="hasHint">Whether the call site passes a conversion hint.</param>
    /// <param name="hasOverride">Whether the call site passes an explicit converter.</param>
    /// <param name="arguments">The hint and converter arguments the runtime converter is expected to receive.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments(false, false, "null, null")]
    [Arguments(true, false, "conversionHint, null")]
    [Arguments(false, true, "null, converterOverride")]
    [Arguments(true, true, "conversionHint, converterOverride")]
    public async Task GenerateBindToMethod_WithoutAConversion_ConvertsAtRunTime(bool hasHint, bool hasOverride, string arguments)
    {
        var inv = new BindToInvocationInfo(
            string.Empty,
            1,
            SourceValueType,
            "global::TestApp.MyViewModel",
            new([ModelFactory.CreatePropertyPathSegment()]),
            StringType,
            true,
            hasHint,
            hasOverride,
            "x => x.Name");
        var sb = new SourceWriter().Indent().Indent();
        var memberLevel = sb.Level;

        BindToCodeGenerator.GenerateBindToMethod(sb, inv, "ABC");

        var result = sb.ToString();
        await Assert.That(result).Contains(
            $".TryConvert<{SourceValueType}, {StringType}>(value, {arguments}, out var __converted))");
        await Assert.That(result).Contains("target.Name = __converted;");
        await Assert.That(sb.Level).IsEqualTo(memberLevel);
    }
}
