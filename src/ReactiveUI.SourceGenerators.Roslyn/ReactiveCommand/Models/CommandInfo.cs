// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.SourceGenerators.Helpers;

namespace ReactiveUI.SourceGenerators.Models;

/// <summary>Contains the metadata needed to generate a reactive command.</summary>
/// <param name="TargetInfo">The enclosing type metadata.</param>
/// <param name="MethodName">The source method name.</param>
/// <param name="MethodReturnType">The method return type.</param>
/// <param name="ArgumentType">The optional command argument type.</param>
/// <param name="IsTask">Whether the method returns a task.</param>
/// <param name="IsReturnTypeVoid">Whether the command output is void.</param>
/// <param name="IsObservable">Whether the method returns an observable.</param>
/// <param name="CanExecuteObservableName">The optional can-execute member name.</param>
/// <param name="CanExecuteTypeInfo">The kind of can-execute member.</param>
/// <param name="OutputScheduler">The optional output scheduler expression.</param>
/// <param name="ForwardedPropertyAttributes">Attributes copied to the generated property.</param>
/// <param name="AccessModifier">The generated property access modifier.</param>
/// <param name="XmlComment">XML documentation copied to the generated property.</param>
internal sealed record CommandInfo(
    TargetInfo TargetInfo,
    string MethodName,
    string MethodReturnType,
    string? ArgumentType,
    bool IsTask,
    bool IsReturnTypeVoid,
    bool IsObservable,
    string? CanExecuteObservableName,
    CanExecuteTypeInfo? CanExecuteTypeInfo,
    string? OutputScheduler,
    EquatableArray<string> ForwardedPropertyAttributes,
    string AccessModifier,
    string? XmlComment)
{
    /// <summary>Gets the output type used by the generated command.</summary>
    /// <param name="voidTypeName">The framework-specific void type name.</param>
    /// <returns>The output type name.</returns>
    internal string GetOutputTypeText(string voidTypeName) => IsReturnTypeVoid
        ? voidTypeName
        : MethodReturnType;

    /// <summary>Gets the input type used by the generated command.</summary>
    /// <param name="voidTypeName">The framework-specific void type name.</param>
    /// <returns>The input type name.</returns>
    internal string GetInputTypeText(string voidTypeName) => string.IsNullOrWhiteSpace(ArgumentType)
        ? voidTypeName
        : ArgumentType!;
}
