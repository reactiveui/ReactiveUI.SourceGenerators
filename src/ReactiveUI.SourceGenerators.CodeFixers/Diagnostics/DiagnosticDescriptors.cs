// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;

#pragma warning disable IDE0090 // Use 'new DiagnosticDescriptor(...)'

namespace ReactiveUI.SourceGenerators.CodeFixers.Diagnostics;

/// <summary>
/// A container for all <see cref="DiagnosticDescriptor"/> instances for errors reported by analyzers in this project.
/// </summary>
internal static class DiagnosticDescriptors
{
    /// <summary>
    /// The property to field rule.
    /// </summary>
    public static readonly DiagnosticDescriptor PropertyToReactiveFieldRule = new(
        id: "RXUISG0016",
        title: "Property To Reactive Field, change to [Reactive] private type _fieldName;",
        messageFormat: "Replace the property with a INPC Reactive Property for ReactiveUI",
        category: typeof(PropertyToReactiveFieldAnalyzer).FullName,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Used to create a Read Write INPC Reactive Property for ReactiveUI, annotated with [Reactive].",
        helpLinkUri: "https://www.reactiveui.net/docs/handbook/view-models/boilerplate-code.html");
}
