// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.CodeGeneration;

/// <summary>Tests retargeting generated references to the consumer's runtime flavour.</summary>
public class RuntimeFlavourRewriterTests
{
    /// <summary>Trivia after a member-access dot preserves a valid static call when its declaring type shifts.</summary>
    /// <param name="trivia">Whitespace or a comment separating the access dot from its member name.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [Arguments(" ")]
    [Arguments("/* member */")]
    public async Task Retarget_StaticCallWithMemberTrivia_CompilesAgainstReactiveRuntime(string trivia)
    {
        var source = $$"""
                       public static class Scenario
                       {
                           public static global::System.IObservable<int> Combine(
                               global::System.IObservable<int> first,
                               global::System.IObservable<int> second) =>
                               global::ReactiveUI.Primitives.LinqExtensions.{{trivia}}CombineLatest(first, second, (x, y) => x + y);
                       }
                       """;
        var features = new LanguageFeatures(
            SupportsCallerArgExpr: false,
            SupportsNullable: false,
            EmitGeneratedCodeMarkers: true,
            UsesReactiveRuntime: true,
            PrimitivesNamespaceMembers: new EquatableArray<string>(["LinqExtensions"]));

        var retargeted = RuntimeFlavourRewriter.Retarget(source, features);
        var result = TestHelper.RunGenerator(retargeted, LanguageVersion.CSharp7_3, null, true);

        await result.CompilationSucceeds();
        await Assert.That(retargeted).Contains($"global::ReactiveUI.Primitives.Reactive.LinqExtensions.{trivia}CombineLatest");
    }
}
