// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Invocations;

/// <summary>Detects BindTwoWay invocations and generates per-invocation bidirectional binding code.</summary>
internal static class BindTwoWayInvocationGenerator
{
    /// <summary>Registers the BindTwoWay invocation detection pipeline.</summary>
    /// <param name="context">The generator initialization context.</param>
    /// <param name="invocations">The detected invocations of this API.</param>
    /// <param name="languageFeatures">The consumer compilation's C# language-feature snapshot.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Register(
        in IncrementalGeneratorInitializationContext context,
        IncrementalValuesProvider<BindingInvocationInfo> invocations,
        IncrementalValueProvider<LanguageFeatures> languageFeatures) =>
        InvocationPipeline.Register(
            context,
            invocations,
            languageFeatures,
            "BindTwoWayDispatch.g.cs",
            static (invocations, classes, features) => BindingEmitterHelpers.Generate(
                invocations,
                classes,
                features,
                BindTwoWayCodeGenerator.DispatchApi,
                static (sb, c) => BindTwoWayCodeGenerator.GenerateBindTwoWayMethod(
                    sb,
                    c.Invocation,
                    c.SourceClassInfo,
                    c.TargetClassInfo,
                    c.Suffix)));
}
