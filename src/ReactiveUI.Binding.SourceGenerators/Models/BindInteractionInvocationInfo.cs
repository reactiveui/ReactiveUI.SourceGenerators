// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Models;

/// <summary>
/// Per-call-site value-equatable POCO for BindInteraction invocations.
/// Contains no ISymbol, SyntaxNode, or Location references.
/// </summary>
/// <param name="CallerFilePath">The source file path of the call site.</param>
/// <param name="CallerLineNumber">The line number of the call site.</param>
/// <param name="ViewTypeFullName">The fully qualified name of the view type.</param>
/// <param name="ViewModelTypeFullName">The fully qualified name of the view model type.</param>
/// <param name="InteractionPropertyPath">The property path chain to the interaction property.</param>
/// <param name="InputTypeFullName">The fully qualified name of the interaction input type.</param>
/// <param name="OutputTypeFullName">The fully qualified name of the interaction output type.</param>
/// <param name="IsTaskHandler">True if the handler is task-based; false if observable-based.</param>
/// <param name="DontCareTypeFullName">The TDontCare type for observable handlers, or null for task handlers.</param>
/// <param name="MethodName">The name of the invoked method (BindInteraction).</param>
/// <param name="ExpressionText">The normalized expression text of the property path lambda.</param>
/// <param name="ViewClassInfo">
/// How the view type notifies, or <see langword="null"/> when the call site was built without a symbol to read
/// it from. This API takes no lambda rooted on the view, so nothing else in the call site carries the view's
/// mechanism and the declaration scan only sees types the consumer writes. Captured here so a view declared in
/// a referenced assembly is still followed through the view model it holds rather than reduced to the instance
/// the call was handed.
/// </param>
/// <param name="Interceptor">
/// Where this call site is, for a build that claims call sites outright rather than competing for them.
/// </param>
/// <param name="Host">
/// The class the call's generated code is declared in, when only that class can name the call's types. Null when
/// generated code can name them from its own class.
/// </param>
internal sealed record BindInteractionInvocationInfo(
    string CallerFilePath,
    int CallerLineNumber,
    string ViewTypeFullName,
    string ViewModelTypeFullName,
    EquatableArray<PropertyPathSegment> InteractionPropertyPath,
    string InputTypeFullName,
    string OutputTypeFullName,
    bool IsTaskHandler,
    string? DontCareTypeFullName,
    string MethodName,
    string ExpressionText,
    ClassBindingInfo? ViewClassInfo = null,
    InterceptorLocation Interceptor = default,
    HostedCall? Host = null) : IClaimableCallSite;
