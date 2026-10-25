// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Models;

/// <summary>The per-call-site model of a <c>ToProperty</c> invocation.</summary>
/// <param name="CallerFilePath">The source file path of the call site.</param>
/// <param name="CallerLineNumber">The line number of the call site.</param>
/// <param name="SourceTypeFullName">The fully qualified type that declares the property (the stub's <c>TObj</c>).</param>
/// <param name="ValueTypeFullName">The fully qualified property value type (the stub's <c>TRet</c>), without nullable annotations.</param>
/// <param name="ValueTypeDisplay">The same type with its nullable annotations, as the call site inferred it.</param>
/// <param name="PropertyName">The name of the property the helper backs.</param>
/// <param name="PropertyExpressionText">The property argument as written, which expression-text dispatch compares.</param>
/// <param name="Shape">The stub overload the call site resolved to.</param>
/// <param name="Raise">How generated code raises the source type's change notifications.</param>
/// <param name="Interceptor">The call site's location, for a build that intercepts call sites.</param>
/// <param name="Host">
/// The class the call's generated code is declared in, when only that class can name the call's types. Null when
/// generated code can name them from its own class.
/// </param>
internal sealed record ToPropertyInvocationInfo(
    string CallerFilePath,
    int CallerLineNumber,
    string SourceTypeFullName,
    string ValueTypeFullName,
    string ValueTypeDisplay,
    string PropertyName,
    string PropertyExpressionText,
    ToPropertyOverloadShape Shape,
    PropertyRaiseInfo Raise,
    InterceptorLocation Interceptor = default,
    HostedCall? Host = null) : IClaimableCallSite;
