// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Invocations;

/// <summary>
/// Detects WhenAnyObservable invocations and generates per-invocation observation code.
/// Generates Switch, Merge, and CombineLatest patterns for observing properties that are
/// themselves observables.
/// </summary>
internal static class WhenAnyObservableInvocationGenerator
{
    /// <summary>Registers the WhenAnyObservable invocation detection pipeline.</summary>
    /// <param name="context">The generator initialization context.</param>
    /// <param name="invocations">The detected invocations of this API.</param>
    /// <param name="languageFeatures">The consumer compilation's C# language-feature snapshot.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Register(
        in IncrementalGeneratorInitializationContext context,
        IncrementalValuesProvider<WhenAnyObservableInvocationInfo> invocations,
        IncrementalValueProvider<LanguageFeatures> languageFeatures) =>
        InvocationPipeline.Register(
            context,
            invocations,
            languageFeatures,
            "WhenAnyObservableDispatch.g.cs",
            static (invocations, classes, features) => WhenAnyObservableCodeGenerator.Generate(invocations, classes, features));
}
