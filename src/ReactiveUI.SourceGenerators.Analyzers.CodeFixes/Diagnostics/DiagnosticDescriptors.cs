// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;

namespace ReactiveUI.SourceGenerators.CodeFixers.Diagnostics;

/// <summary>A container for all <see cref="DiagnosticDescriptor"/> instances for errors reported by analyzers in this project.</summary>
internal static class DiagnosticDescriptors
{
    /// <summary>The property to field rule.</summary>
    internal static readonly DiagnosticDescriptor PropertyToReactiveFieldRule = new(
        id: "RXUISG0016",
        title: "Property To Reactive Field, change to `[Reactive]` private type _fieldName;",
        messageFormat: "Replace the property with a INPC Reactive Property for ReactiveUI",
        category: typeof(PropertyToReactiveFieldAnalyzer).FullName,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Used to create a Read Write INPC Reactive Property for ReactiveUI, annotated with `[Reactive]`.",
        helpLinkUri: "https://www.reactiveui.net/docs/handbook/view-models/boilerplate-code.html");

    /// <summary>The `[Reactive]` attribute was used on a property, but required `partial` modifiers are missing.</summary>
    internal static readonly DiagnosticDescriptor ReactiveAttributeRequiresPartialRule = new(
        id: "RXUISG0020",
        title: "[Reactive] requires partial property and containing type",
        messageFormat: "`[Reactive]` requires the property to be `partial` and the containing type to be partial so source generation can run",
        category: typeof(ReactiveAttributeMisuseAnalyzer).FullName,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Warns when `[Reactive]` is placed on a property or type that is not `partial`.",
        helpLinkUri: "https://www.reactiveui.net/docs/handbook/view-models/boilerplate-code.html");
}
