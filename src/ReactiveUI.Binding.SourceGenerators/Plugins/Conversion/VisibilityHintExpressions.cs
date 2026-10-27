// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Conversion;

/// <summary>Reads supported visibility flags through the hint enum present in the consumer.</summary>
internal static class VisibilityHintExpressions
{
    /// <summary>Builds a flag test over every hint enum the consumer can pass, in either runtime flavour.</summary>
    /// <param name="compilation">The consumer compilation.</param>
    /// <param name="platform">The platform, which names the hint enums a binding to it accepts.</param>
    /// <param name="member">The flag member to test.</param>
    /// <returns>A typed flag test, or false when no hint enum is accessible.</returns>
    /// <remarks>
    /// ReactiveUI declares <c>BooleanToVisibilityHint</c>, and this library's platform packages declare
    /// <c>BooleanToVisibilityHints</c>. A consumer may pass either, so the test accepts each one that resolves.
    /// </remarks>
    internal static string Flag(Compilation compilation, VisibilityPlatform platform, string member)
    {
        var tests = new List<string>();
        if (platform.HintNamespace.Length != 0)
        {
            AddTest(tests, compilation, $"{platform.HintNamespace}.BooleanToVisibilityHint", member);
            AddTest(tests, compilation, $"{platform.HintNamespace.Replace("ReactiveUI", "ReactiveUI.Reactive")}.BooleanToVisibilityHint", member);
        }

        if (platform.BindingPlatform.Length != 0)
        {
            AddTest(tests, compilation, $"ReactiveUI.Binding.{platform.BindingPlatform}.BooleanToVisibilityHints", member);
            AddTest(tests, compilation, $"ReactiveUI.Binding.Reactive.{platform.BindingPlatform}.BooleanToVisibilityHints", member);
        }

        return tests.Count switch
        {
            0 => "false",
            1 => tests[0],
            _ => $"({string.Join(") || (", tests)})",
        };
    }

    /// <summary>Adds the flag test for one hint enum when the consumer can reach it.</summary>
    /// <param name="tests">The tests found so far; the count also names the pattern variable.</param>
    /// <param name="compilation">The consumer compilation.</param>
    /// <param name="metadataName">The hint enum metadata name.</param>
    /// <param name="member">The flag member.</param>
    private static void AddTest(List<string> tests, Compilation compilation, string metadataName, string member)
    {
        if (ForType(compilation, metadataName, member, tests.Count.ToString(System.Globalization.CultureInfo.InvariantCulture)) is { } test)
        {
            tests.Add(test);
        }
    }

    /// <summary>Tests a flag only when the exact enum and member are accessible.</summary>
    /// <param name="compilation">The consumer compilation.</param>
    /// <param name="metadataName">The hint enum metadata name.</param>
    /// <param name="member">The flag member.</param>
    /// <param name="suffix">The discriminator for the emitted pattern variable.</param>
    /// <returns>The flag expression, or null.</returns>
    private static string? ForType(Compilation compilation, string metadataName, string member, string suffix)
    {
        var type = compilation.GetTypeByMetadataName(metadataName);
        if (type is not { TypeKind: TypeKind.Enum } || type.GetMembers(member).IsEmpty
            || !compilation.IsSymbolAccessibleWithin(type, compilation.Assembly))
        {
            return null;
        }

        var typeName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var variable = $"__{member}Hint{suffix}";
        return $"__hint is {typeName} {variable} && ({variable} & {typeName}.{member}) != 0";
    }
}
