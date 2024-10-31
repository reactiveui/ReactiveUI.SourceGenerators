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
    /// Gets a <see cref="DiagnosticDescriptor"/> indicating when an unsupported C# language version is being used.
    /// </summary>
    public static readonly DiagnosticDescriptor UnsupportedCSharpLanguageVersionError = new DiagnosticDescriptor(
        id: "RXUISG0001",
        title: "Unsupported C# language version (< 12.0)",
        messageFormat: "The source generator features from ReactiveUI require consuming projects to set the C# language version to at least C# 12.0",
        category: typeof(UnsupportedCSharpLanguageVersionAnalyzer).FullName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The source generator features from ReactiveUI require consuming projects to set the C# language version to at least C# 12.0. Make sure to add <LangVersion>12.0</LangVersion> (or above) to your .csproj file.",
        helpLinkUri: "https://www.reactiveui.net/docs/handbook/view-models/boilerplate-code.html");

    /// <summary>
    /// Gets a <see cref="DiagnosticDescriptor"/> indicating when a method with <c>[ReactiveCommand]</c> is async void.
    /// <para>
    /// Format: <c>"The method {0} annotated with [ReactiveCommand] is async void (make sure to return a Task type instead)"</c>.
    /// </para>
    /// </summary>
    public static readonly DiagnosticDescriptor AsyncVoidReturningReactiveCommandMethod = new DiagnosticDescriptor(
        id: "RXUISG0008",
        title: "Async void returning method annotated with ReactiveCommand",
        messageFormat: "The method {0} annotated with [ReactiveCommand] is async void (make sure to return a Task type instead)",
        category: typeof(AsyncVoidReturningReactiveCommandMethodAnalyzer).FullName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "All asynchronous methods annotated with [ReactiveCommand] should return a Task type, to benefit from the additional support provided by ReactiveCommand.FromTask.",
        helpLinkUri: "https://www.reactiveui.net/docs/handbook/view-models/boilerplate-code.html");

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
