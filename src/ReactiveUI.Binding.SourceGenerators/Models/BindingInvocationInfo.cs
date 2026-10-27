// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Models;

/// <summary>
/// Per-call-site value-equatable POCO for BindOneWay/BindTwoWay invocations.
/// Contains source and target property path information.
/// Contains no ISymbol, SyntaxNode, or Location references.
/// </summary>
/// <param name="CallerFilePath">The source file path of the call site, captured via <c>[CallerFilePath]</c>.</param>
/// <param name="CallerLineNumber">The line number of the call site, captured via <c>[CallerLineNumber]</c>.</param>
/// <param name="SourceTypeFullName">The fully qualified name of the source (data) type.</param>
/// <param name="SourcePropertyPath">The property path chain from the source lambda expression.</param>
/// <param name="TargetTypeFullName">The fully qualified name of the target (UI) type.</param>
/// <param name="TargetPropertyPath">The property path chain from the target lambda expression.</param>
/// <param name="SourcePropertyTypeFullName">The fully qualified type of the source property (leaf of the source path).</param>
/// <param name="TargetPropertyTypeFullName">The fully qualified type of the target property (leaf of the target path).</param>
/// <param name="HasConversion">Whether the invocation includes an inline Func-based conversion parameter.</param>
/// <param name="HasScheduler">Whether the invocation includes an explicit scheduler parameter.</param>
/// <param name="IsTwoWay">Whether this is a two-way binding (BindTwoWay/Bind) rather than one-way.</param>
/// <param name="MethodName">The name of the invoked method (e.g., BindOneWay, BindTwoWay, OneWayBind, Bind).</param>
/// <param name="SourceExpressionText">The original expression text of the source lambda argument.</param>
/// <param name="TargetExpressionText">The original expression text of the target lambda argument.</param>
/// <param name="HasConverterOverride">Whether the invocation uses an explicit <c>IBindingTypeConverter</c> parameter.</param>
/// <param name="Interceptor">
/// Where this call site is, for a build that claims call sites outright rather than competing for them.
/// </param>
/// <param name="SourceViewThreadInvoker">The invoker class a write to the source carries, or null for none.</param>
/// <param name="TargetViewThreadInvoker">The invoker class a write to the target carries, or null for none.</param>
/// <param name="Host">
/// The class the call's generated code is declared in, when only that class can name the call's types. Null when
/// generated code can name them from its own class.
/// </param>
internal sealed record BindingInvocationInfo(
    string CallerFilePath,
    int CallerLineNumber,
    string SourceTypeFullName,
    EquatableArray<PropertyPathSegment> SourcePropertyPath,
    string TargetTypeFullName,
    EquatableArray<PropertyPathSegment> TargetPropertyPath,
    string SourcePropertyTypeFullName,
    string TargetPropertyTypeFullName,
    bool HasConversion,
    bool HasScheduler,
    bool IsTwoWay,
    string MethodName,
    string SourceExpressionText,
    string TargetExpressionText,
    bool HasConverterOverride,
    InterceptorLocation Interceptor = default,
    string? SourceViewThreadInvoker = null,
    string? TargetViewThreadInvoker = null,
    HostedCall? Host = null) : IClaimableCallSite
{
    /// <summary>Gets the typed mechanism converting source values to the target.</summary>
    public ConversionInfo? ForwardConversion { get; init; }

    /// <summary>Gets the typed mechanism converting target values to the source.</summary>
    public ConversionInfo? ReverseConversion { get; init; }

    /// <summary>Gets the native mutation used by a one-way collection binding.</summary>
    public SetMethodInfo? SetMethod { get; init; }
}
