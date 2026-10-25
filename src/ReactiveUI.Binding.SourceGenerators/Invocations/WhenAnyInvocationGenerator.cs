// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Invocations;

/// <summary>
/// Detects WhenAny invocations (with IObservedChange selector) and generates per-invocation observation code.
/// Unlike WhenChanged/WhenAnyValue, WhenAny wraps each observed value in an <c>ObservedChange</c> before
/// passing it to the user's selector function.
/// </summary>
internal static class WhenAnyInvocationGenerator
{
    /// <summary>Registers the WhenAny invocation detection pipeline.</summary>
    /// <param name="context">The generator initialization context.</param>
    /// <param name="invocations">The detected invocations of this API.</param>
    /// <param name="languageFeatures">The consumer compilation's C# language-feature snapshot.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Register(
        in IncrementalGeneratorInitializationContext context,
        IncrementalValuesProvider<InvocationInfo> invocations,
        IncrementalValueProvider<LanguageFeatures> languageFeatures) =>
        InvocationPipeline.Register(
            context,
            invocations,
            languageFeatures,
            "WhenAnyDispatch.g.cs",
            static (invocations, classes, features) => WhenAnyCodeGenerator.Generate(invocations, classes, features));
}
