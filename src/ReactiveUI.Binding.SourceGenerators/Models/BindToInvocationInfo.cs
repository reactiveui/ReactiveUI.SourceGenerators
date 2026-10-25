// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Models;

/// <summary>The per-call-site model of a <c>BindTo</c> invocation.</summary>
/// <param name="CallerFilePath">The source file path of the call site, captured via <c>[CallerFilePath]</c>.</param>
/// <param name="CallerLineNumber">The line number of the call site, captured via <c>[CallerLineNumber]</c>.</param>
/// <param name="SourceValueTypeFullName">The fully qualified type produced by the source observable (the <c>T</c> in <c>IObservable&lt;T&gt;</c>).</param>
/// <param name="TargetTypeFullName">The fully qualified name of the target object type.</param>
/// <param name="TargetPropertyPath">The property path chain from the target lambda expression.</param>
/// <param name="TargetPropertyTypeFullName">The fully qualified type of the target property (leaf of the target path).</param>
/// <param name="TargetPropertyIsReferenceType">Whether the target property type is a reference type, which annotates the selector parameter nullable.</param>
/// <param name="HasConversionHint">Whether the invocation supplies a <c>conversionHint</c> argument.</param>
/// <param name="HasConverterOverride">Whether the invocation supplies an explicit <c>IBindingTypeConverter</c> argument.</param>
/// <param name="TargetExpressionText">The original expression text of the target lambda argument.</param>
/// <param name="Interceptor">The call site's location, for a build that intercepts call sites.</param>
/// <param name="TargetViewThreadInvoker">The invoker class a write to the target carries, or null for none.</param>
/// <param name="Host">
/// The class the call's generated code is declared in, when only that class can name the call's types. Null when
/// generated code can name them from its own class.
/// </param>
internal sealed record BindToInvocationInfo(
    string CallerFilePath,
    int CallerLineNumber,
    string SourceValueTypeFullName,
    string TargetTypeFullName,
    EquatableArray<PropertyPathSegment> TargetPropertyPath,
    string TargetPropertyTypeFullName,
    bool TargetPropertyIsReferenceType,
    bool HasConversionHint,
    bool HasConverterOverride,
    string TargetExpressionText,
    InterceptorLocation Interceptor = default,
    string? TargetViewThreadInvoker = null,
    HostedCall? Host = null) : IClaimableCallSite
{
    /// <summary>Gets the typed mechanism converting stream values to the target.</summary>
    public ConversionInfo? Conversion { get; init; }

    /// <summary>Gets the native mutation of an existing target collection.</summary>
    public SetMethodInfo? SetMethod { get; init; }
}
