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
        title: "Property can be a `[Reactive]` property",
        messageFormat: "Replace the property with a INPC Reactive Property for ReactiveUI",
        category: typeof(PropertyToReactiveFieldAnalyzer).FullName,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Used to create a Read Write INPC Reactive Property for ReactiveUI, annotated with `[Reactive]`.",
        helpLinkUri: "https://www.reactiveui.net/docs/handbook/view-models/boilerplate-code.html");

    /// <summary>A <c>[ReactiveCommand]</c> method takes more parameters than a command can pass it.</summary>
    internal static readonly DiagnosticDescriptor InvalidReactiveCommandMethodSignatureRule = new(
        id: "RXUISG0002",
        title: "Invalid [ReactiveCommand] method signature",
        messageFormat: "`{0}` takes more than one parameter besides a CancellationToken, so no command is generated for it",
        category: typeof(ReactiveCommandAnalyzer).FullName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "A command passes at most one parameter to its method; a task-returning method may also take a CancellationToken.",
        helpLinkUri: "https://www.reactiveui.net/docs/handbook/view-models/boilerplate-code.html");

    /// <summary>A <c>[ReactiveCommand]</c> method is <c>async void</c>.</summary>
    internal static readonly DiagnosticDescriptor AsyncVoidReactiveCommandMethodRule = new(
        id: "RXUISG0008",
        title: "[ReactiveCommand] method is async void",
        messageFormat: "`{0}` is async void, so the command cannot await it or observe its exceptions; return Task instead",
        category: typeof(ReactiveCommandAnalyzer).FullName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "A command generated from an async void method completes before the method does, and its exceptions escape the command.",
        helpLinkUri: "https://www.reactiveui.net/docs/handbook/view-models/boilerplate-code.html");

    /// <summary>A <c>[ReactiveCommand]</c> scheduler name does not resolve to a scheduler.</summary>
    internal static readonly DiagnosticDescriptor UnresolvedReactiveCommandSchedulerRule = new(
        id: "RXUISG0021",
        title: "[ReactiveCommand] scheduler does not resolve",
        messageFormat: "`{0}` = \"{1}\" does not name a scheduler the command can use, so the command is generated without it",
        category: typeof(ReactiveCommandAnalyzer).FullName,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "A scheduler is a field, property or parameterless method of the command's type, or a static one elsewhere "
            + "such as RxSchedulers.MainThreadScheduler, whose type is the scheduler type ReactiveUI's commands take.",
        helpLinkUri: "https://www.reactiveui.net/docs/handbook/view-models/boilerplate-code.html");

    /// <summary>A WinForms host follows its properties without ReactiveUI.Binding's <c>ObservedProperty</c>.</summary>
    internal static readonly DiagnosticDescriptor ControlHostWithoutObservedPropertyRule = new(
        id: "RXUISG0022",
        title: "WinForms host follows its properties without ObservedProperty",
        messageFormat: "`{0}` follows its own properties through PropertyChanged; with ReactiveUI.Binding {1} or later it would follow them with WhenAnyValue semantics",
        category: typeof(ControlHostAnalyzer).FullName,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "The [RoutedControlHost] and [ViewModelControlHost] hosts use ReactiveUI.Binding's ObservedProperty when the referenced ReactiveUI.Binding has it.",
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
