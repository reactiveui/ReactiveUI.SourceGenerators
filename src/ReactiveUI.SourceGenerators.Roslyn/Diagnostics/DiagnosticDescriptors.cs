// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;

#pragma warning disable IDE0090 // Use 'new DiagnosticDescriptor(...)'

namespace ReactiveUI.SourceGenerators.Diagnostics;

/// <summary>
/// A container for all <see cref="DiagnosticDescriptor"/> instances for errors reported by analyzers in this project.
/// </summary>
internal static class DiagnosticDescriptors
{
    /// <summary>
    /// Gets a <see cref="DiagnosticDescriptor"/> indicating when a generated property created with <c>[Reactive]</c> would collide with the source field.
    /// <para>
    /// Format: <c>"The field {0}.{1} cannot be used to generate an observable property, as its name would collide with the field name (instance fields should use the "lowerCamel", "_lowerCamel" or "m_lowerCamel" pattern)</c>.
    /// </para>
    /// </summary>
    public static readonly DiagnosticDescriptor ReactivePropertyNameCollisionError = new DiagnosticDescriptor(
        id: "RXUISG0009",
        title: "Name collision for generated property",
        messageFormat: "The field {0}.{1} cannot be used to generate an reactive property, as its name would collide with the field name (instance fields should use the \"lowerCamel\", \"_lowerCamel\" or \"m_lowerCamel\" pattern)",
        category: typeof(ReactiveGenerator).FullName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The name of fields annotated with [Reactive] should use \"lowerCamel\", \"_lowerCamel\" or \"m_lowerCamel\" pattern to avoid collisions with the generated properties.",
        helpLinkUri: "https://www.reactiveui.net/docs/handbook/view-models/boilerplate-code.html");

    /// <summary>
    /// Gets a <see cref="DiagnosticDescriptor"/> indicating when a field with <c>[Reactive]</c> is using an invalid attribute targeting the property.
    /// <para>
    /// Format: <c>"The field {0} annotated with [Reactive] is using attribute "{1}" which was not recognized as a valid type (are you missing a using directive?)"</c>.
    /// </para>
    /// </summary>
    public static readonly DiagnosticDescriptor InvalidPropertyTargetedAttributeOnReactiveField = new DiagnosticDescriptor(
        id: "RXUISG0010",
        title: "Invalid property targeted attribute type",
        messageFormat: "The field {0} annotated with [Reactive] is using attribute \"{1}\" which was not recognized as a valid type (are you missing a using directive?)",
        category: typeof(ReactiveGenerator).FullName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "All attributes targeting the generated property for a field annotated with [Reactive] must correctly be resolved to valid types.",
        helpLinkUri: "https://www.reactiveui.net/docs/handbook/view-models/boilerplate-code.html");

    /// <summary>
    /// Gets a <see cref="DiagnosticDescriptor"/> indicating when a field with <c>[Reactive]</c> is using an invalid attribute expression targeting the property.
    /// <para>
    /// Format: <c>"The field {0} annotated with [Reactive] is using attribute "{1}" with an invalid expression (are you passing any incorrect parameters to the attribute constructor?)"</c>.
    /// </para>
    /// </summary>
    public static readonly DiagnosticDescriptor InvalidPropertyTargetedAttributeExpressionOnReactiveField = new DiagnosticDescriptor(
        id: "RXUISG0011",
        title: "Invalid property targeted attribute expression",
        messageFormat: "The field {0} annotated with [Reactive] is using attribute \"{1}\" with an invalid expression (are you passing any incorrect parameters to the attribute constructor?)",
        category: typeof(ReactiveGenerator).FullName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "All attributes targeting the generated property for a field annotated with [Reactive] must be using valid expressions.",
        helpLinkUri: "https://www.reactiveui.net/docs/handbook/view-models/boilerplate-code.html");

    /// <summary>
    /// Gets a <see cref="DiagnosticDescriptor"/> indicating when a field with <c>[ObservableAsProperty]</c> is using an invalid attribute targeting the property.
    /// <para>
    /// Format: <c>"The field {0} annotated with [ObservableAsProperty] is using attribute "{1}" which was not recognized as a valid type (are you missing a using directive?)"</c>.
    /// </para>
    /// </summary>
    public static readonly DiagnosticDescriptor InvalidPropertyTargetedAttributeOnObservableAsPropertyField = new DiagnosticDescriptor(
        id: "RXUISG0012",
        title: "Invalid property targeted attribute type",
        messageFormat: "The field {0} annotated with [ObservableAsProperty] is using attribute \"{1}\" which was not recognized as a valid type (are you missing a using directive?)",
        category: "ReactiveUI.SourceGenerators.ObservableAsPropertyGenerator",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "All attributes targeting the generated property for a field annotated with [ObservableAsProperty] must correctly be resolved to valid types.",
        helpLinkUri: "https://www.reactiveui.net/docs/handbook/view-models/boilerplate-code.html");

    /// <summary>
    /// Gets a <see cref="DiagnosticDescriptor"/> indicating when a field with <c>[ObservableAsProperty]</c> is using an invalid attribute expression targeting the property.
    /// <para>
    /// Format: <c>"The field {0} annotated with [ObservableAsProperty] is using attribute "{1}" with an invalid expression (are you passing any incorrect parameters to the attribute constructor?)"</c>.
    /// </para>
    /// </summary>
    public static readonly DiagnosticDescriptor InvalidPropertyTargetedAttributeExpressionOnObservableAsPropertyField = new DiagnosticDescriptor(
        id: "RXUISG0013",
        title: "Invalid property targeted attribute expression",
        messageFormat: "The field {0} annotated with [ObservableAsProperty] is using attribute \"{1}\" with an invalid expression (are you passing any incorrect parameters to the attribute constructor?)",
        category: "ReactiveUI.SourceGenerators.ObservableAsPropertyGenerator",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "All attributes targeting the generated property for a field annotated with [ObservableAsProperty] must be using valid expressions.",
        helpLinkUri: "https://www.reactiveui.net/docs/handbook/view-models/boilerplate-code.html");

    /// <summary>
    /// Gets a <see cref="DiagnosticDescriptor"/> indicating when a generated property created with <c>[ObservableAsProperty]</c> would cause conflicts with other generated members.
    /// <para>
    /// Format: <c>"The field {0}.{1} cannot be used to generate an observable property, as its name or type would cause conflicts with other generated members"</c>.
    /// </para>
    /// </summary>
    public static readonly DiagnosticDescriptor InvalidObservableAsPropertyError = new DiagnosticDescriptor(
        id: "RXUISG0014",
        title: "Invalid generated property declaration",
        messageFormat: "The field {0}.{1} cannot be used to generate an observable As property, as its name or type would cause conflicts with other generated members",
        category: "ReactiveUI.SourceGenerators.ObservableAsPropertyGenerator",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The fields annotated with [ObservableAsProperty] cannot result in a property name or have a type that would cause conflicts with other generated members.",
        helpLinkUri: "https://www.reactiveui.net/docs/handbook/view-models/boilerplate-code.html");

    /// <summary>
    /// Gets a <see cref="DiagnosticDescriptor"/> indicating when a generated property created with <c>[Reactive]</c> would cause conflicts with other generated members.
    /// <para>
    /// Format: <c>"The field {0}.{1} cannot be used to generate an observable property, as its name or type would cause conflicts with other generated members"</c>.
    /// </para>
    /// </summary>
    public static readonly DiagnosticDescriptor InvalidReactiveError = new DiagnosticDescriptor(
        id: "RXUISG0015",
        title: "Invalid generated property declaration",
        messageFormat: "The field {0}.{1} cannot be used to generate an reactive property, as its name or type would cause conflicts with other generated members",
        category: typeof(ReactiveGenerator).FullName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The fields annotated with [Reactive] cannot result in a property name or have a type that would cause conflicts with other generated members.",
        helpLinkUri: "https://www.reactiveui.net/docs/handbook/view-models/boilerplate-code.html");

    /// <summary>
    /// The observable as property method has parameters error.
    /// </summary>
    public static readonly DiagnosticDescriptor ObservableAsPropertyMethodHasParametersError = new DiagnosticDescriptor(
        id: "RXUISG0017",
        title: "Invalid generated property declaration",
        messageFormat: "The method {0} cannot be used to generate an observable As property, as it has parameters",
        category: "ReactiveUI.SourceGenerators.ObservableAsPropertyGenerator",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The method annotated with [ObservableAsProperty] cannot currently initialize methods with parameters.",
        helpLinkUri: "https://www.reactiveui.net/docs/handbook/view-models/boilerplate-code.html");

    /// <summary>
    /// The invalid reactive object error.
    /// </summary>
    public static readonly DiagnosticDescriptor InvalidReactiveObjectError = new DiagnosticDescriptor(
        id: "RXUISG0018",
        title: "Invalid class, does not inherit ReactiveObject",
        messageFormat: "The field {0}.{1} cannot be used to generate an ReactiveUI property, as it is not part of a class that inherits from ReactiveObject",
        category: typeof(ReactiveGenerator).FullName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The fields annotated with [Reactive] or [ObservableAsProperty] must be part of a class that inherits from ReactiveObject.",
        helpLinkUri: "https://www.reactiveui.net/docs/handbook/view-models/boilerplate-code.html");

    /// <summary>
    /// The invalid reactive object error.
    /// </summary>
    public static readonly DiagnosticDescriptor ReadOnlyObservableCollectionTypeRequiredError = new DiagnosticDescriptor(
        id: "RXUISG0019",
        title: "Invalid field, does not inherit ReadOnlyObservableCollection",
        messageFormat: "The field {0}.{1} cannot be used to generate an ReadOnlyObservableCollection",
        category: typeof(BindableDerivedListGenerator).FullName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The fields annotated with [BindableDerivedList] must inherit from ReadOnlyObservableCollection.",
        helpLinkUri: "https://www.reactiveui.net/docs/handbook/view-models/boilerplate-code.html");
}
