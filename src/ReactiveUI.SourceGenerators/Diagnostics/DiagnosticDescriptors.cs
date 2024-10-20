// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using ReactiveUI.SourceGenerators.CodeAnalyzers;

#pragma warning disable IDE0090 // Use 'new DiagnosticDescriptor(...)'

namespace ReactiveUI.SourceGenerators.Diagnostics;

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
        title: "Unsupported C# language version",
        messageFormat: "The source generator features from ReactiveUI require consuming projects to set the C# language version to at least C# 9.0",
        category: typeof(UnsupportedCSharpLanguageVersionAnalyzer).FullName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The source generator features from ReactiveUI require consuming projects to set the C# language version to at least C# 9.0. Make sure to add <LangVersion>9.0</LangVersion> (or above) to your .csproj file.",
        helpLinkUri: "https://www.reactiveui.net/errors/RXUISG0001");

    /// <summary>
    /// Gets a <see cref="DiagnosticDescriptor"/> indicating when an annotated method to generate a command for has an invalid signature.
    /// <para>
    /// Format: <c>"The method {0}.{1} cannot be used to generate a command property, as its signature isn't compatible with any of the existing reactive command types"</c>.
    /// </para>
    /// </summary>
    public static readonly DiagnosticDescriptor InvalidReactiveCommandMethodSignatureError = new DiagnosticDescriptor(
        id: "RXUISG0002",
        title: "Invalid ReactiveCommand method signature",
        messageFormat: "The method {0}.{1} cannot be used to generate a command property, as its signature isn't compatible with any of the existing reactive command types",
        category: typeof(ReactiveCommandGenerator).FullName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Cannot apply [ReactiveCommand] to methods with a signature that doesn't match any of the existing reactive command types.",
        helpLinkUri: "https://www.reactiveui.net/errors/RXUISG0002");

    /// <summary>
    /// Gets a <see cref="DiagnosticDescriptor"/> indicating when a specified <c>CanExecute</c> name has no matching member.
    /// <para>
    /// Format: <c>"The CanExecute name must refer to a valid member, but "{0}" has no matches in type {1}"</c>.
    /// </para>
    /// </summary>
    public static readonly DiagnosticDescriptor InvalidCanExecuteMemberNameError = new DiagnosticDescriptor(
        id: "RXUISG0003",
        title: "Invalid ReactiveCommand.CanExecute member name",
        messageFormat: "The CanExecute name must refer to a valid member, but \"{0}\" has no matches in type {1}",
        category: typeof(ReactiveCommandGenerator).FullName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The CanExecute name in [ReactiveCommand] must refer to a valid member in its parent type.",
        helpLinkUri: "https://www.reactiveui.net/errors/RXUISG0003");

    /// <summary>
    /// Gets a <see cref="DiagnosticDescriptor"/> indicating when a specified <c>CanExecute</c> name maps to multiple members.
    /// <para>
    /// Format: <c>"The CanExecute name must refer to a single member, but "{0}" has multiple matches in type {1}"</c>.
    /// </para>
    /// </summary>
    public static readonly DiagnosticDescriptor MultipleCanExecuteMemberNameMatchesError = new DiagnosticDescriptor(
        id: "RXUISG0004",
        title: "Multiple ReactiveCommand.CanExecute member name matches",
        messageFormat: "The CanExecute name must refer to a single member, but \"{0}\" has multiple matches in type {1}",
        category: typeof(ReactiveCommandGenerator).FullName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Cannot set the CanExecute name in [ReactiveCommand] to one that has multiple matches in its parent type (it must refer to a single compatible member).",
        helpLinkUri: "https://www.reactiveui.net/errors/RXUISG0004");

    /// <summary>
    /// Gets a <see cref="DiagnosticDescriptor"/> indicating when a a specified <c>CanExecute</c> name maps to an invalid member.
    /// <para>
    /// Format: <c>"The CanExecute name must refer to a compatible member, but no valid members were found for "{0}" in type {1}"</c>.
    /// </para>
    /// </summary>
    public static readonly DiagnosticDescriptor InvalidCanExecuteMemberError = new DiagnosticDescriptor(
        id: "RXUISG0005",
        title: "No valid ReactiveCommand.CanExecute member match",
        messageFormat: "The CanExecute name must refer to a compatible member, but no valid members were found for \"{0}\" in type {1}",
        category: typeof(ReactiveCommandGenerator).FullName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The CanExecute name in [ReactiveCommand] must refer to a compatible member (either a property or a method) to be used in a generated command.",
        helpLinkUri: "https://www.reactiveui.net/errors/RXUISG0005");

    /// <summary>
    /// Gets a <see cref="DiagnosticDescriptor"/> indicating when a method with <c>[ReactiveCommand]</c> is using an invalid attribute targeting the field or property.
    /// <para>
    /// Format: <c>"The method {0} annotated with [ReactiveCommand] is using attribute "{1}" which was not recognized as a valid type (are you missing a using directive?)"</c>.
    /// </para>
    /// </summary>
    public static readonly DiagnosticDescriptor InvalidFieldOrPropertyTargetedAttributeOnReactiveCommandMethod = new DiagnosticDescriptor(
        id: "RXUISG0006",
        title: "Invalid field or property targeted attribute type",
        messageFormat: "The method {0} annotated with [ReactiveCommand] is using attribute \"{1}\" which was not recognized as a valid type (are you missing a using directive?)",
        category: typeof(ReactiveCommandGenerator).FullName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "All attributes targeting the generated field or property for a method annotated with [ReactiveCommand] must correctly be resolved to valid types.",
        helpLinkUri: "https://www.reactiveui.net/errors/RXUISG0006");

    /// <summary>
    /// Gets a <see cref="DiagnosticDescriptor"/> indicating when a method with <c>[ReactiveCommand]</c> is using an invalid attribute targeting the field or property.
    /// <para>
    /// Format: <c>"The method {0} annotated with [ReactiveCommand] is using attribute "{1}" with an invalid expression (are you passing any incorrect parameters to the attribute constructor?)"</c>.
    /// </para>
    /// </summary>
    public static readonly DiagnosticDescriptor InvalidFieldOrPropertyTargetedAttributeExpressionOnReactiveCommandMethod = new DiagnosticDescriptor(
        id: "RXUISG0007",
        title: "Invalid field or property targeted attribute expression",
        messageFormat: "The method {0} annotated with [ReactiveCommand] is using attribute \"{1}\" with an invalid expression (are you passing any incorrect parameters to the attribute constructor?)",
        category: typeof(ReactiveCommandGenerator).FullName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "All attributes targeting the generated field or property for a method annotated with [ReactiveCommand] must be using valid expressions.",
        helpLinkUri: "https://www.reactiveui.net/errors/RXUISG0007");

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
        helpLinkUri: "https://www.reactiveui.net/errors/RXUISG0008");

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
        helpLinkUri: "https://www.reactiveui.net/errors/RXUISG0009");

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
        helpLinkUri: "https://www.reactiveui.net/errors/RXUISG0010");

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
        helpLinkUri: "https://www.reactiveui.net/errors/RXUISG0011");

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
        category: typeof(ObservableAsPropertyGenerator).FullName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "All attributes targeting the generated property for a field annotated with [ObservableAsProperty] must correctly be resolved to valid types.",
        helpLinkUri: "https://www.reactiveui.net/errors/RXUISG0012");

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
        category: typeof(ObservableAsPropertyGenerator).FullName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "All attributes targeting the generated property for a field annotated with [ObservableAsProperty] must be using valid expressions.",
        helpLinkUri: "https://www.reactiveui.net/errors/RXUISG0013");

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
        category: typeof(ObservableAsPropertyGenerator).FullName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The fields annotated with [ObservableAsProperty] cannot result in a property name or have a type that would cause conflicts with other generated members.",
        helpLinkUri: "https://www.reactiveui.net/errors/RXUISG0014");

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
        helpLinkUri: "https://www.reactiveui.net/errors/RXUISG0015");

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
        helpLinkUri: "https://www.reactiveui.net/errors/RXUISG0016");

    /// <summary>
    /// The observable as property method has parameters error.
    /// </summary>
    public static readonly DiagnosticDescriptor ObservableAsPropertyMethodHasParametersError = new DiagnosticDescriptor(
        id: "RXUISG0017",
        title: "Invalid generated property declaration",
        messageFormat: "The method {0} cannot be used to generate an observable As property, as it has parameters",
        category: typeof(ObservableAsPropertyGenerator).FullName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The method annotated with [ObservableAsProperty] cannot currently initialize methods with parameters.",
        helpLinkUri: "https://www.reactiveui.net/errors/RXUISG0017");
}
