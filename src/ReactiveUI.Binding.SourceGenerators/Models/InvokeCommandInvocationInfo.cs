// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Models;

/// <summary>The per-call-site model of an <c>InvokeCommand</c> invocation.</summary>
/// <param name="CallerFilePath">The source file path of the call site, captured via <c>[CallerFilePath]</c>.</param>
/// <param name="CallerLineNumber">The line number of the call site, captured via <c>[CallerLineNumber]</c>.</param>
/// <param name="SourceValueTypeFullName">The fully qualified type produced by the source observable (the <c>T</c> in <c>IObservable&lt;T&gt;</c>).</param>
/// <param name="TargetTypeFullName">The fully qualified name of the type declaring the command property.</param>
/// <param name="CommandPropertyPath">The property path chain reaching the command.</param>
/// <param name="CommandExpressionText">The original expression text of the command lambda argument.</param>
/// <param name="Interceptor">The call site's location, for a build that intercepts call sites.</param>
/// <param name="Host">
/// The class the call's generated code is declared in, when only that class can name the call's types. Null when
/// generated code can name them from its own class.
/// </param>
internal sealed record InvokeCommandInvocationInfo(
    string CallerFilePath,
    int CallerLineNumber,
    string SourceValueTypeFullName,
    string TargetTypeFullName,
    EquatableArray<PropertyPathSegment> CommandPropertyPath,
    string CommandExpressionText,
    InterceptorLocation Interceptor = default,
    HostedCall? Host = null) : IClaimableCallSite;
