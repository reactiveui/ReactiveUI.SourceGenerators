// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Models;

/// <summary>Which of the stub's <c>ToProperty</c> overloads a call site resolved to.</summary>
/// <param name="NamesPropertyByString">Whether the property is named by a string rather than a selector expression.</param>
/// <param name="HasResult">Whether the overload also returns the helper through an <c>out</c> parameter.</param>
/// <param name="InitialValue">How the overload supplies the initial value.</param>
/// <param name="HasDeferSubscription">Whether the overload takes <c>deferSubscription</c>.</param>
/// <param name="HasScheduler">Whether the overload takes <c>scheduler</c>.</param>
/// <remarks>
/// The optional parameters always appear in this order, so these flags reproduce the stub's parameter list
/// exactly, which the generated overload and interceptor have to match.
/// </remarks>
internal readonly record struct ToPropertyOverloadShape(
    bool NamesPropertyByString,
    bool HasResult,
    ToPropertyInitialValueKind InitialValue,
    bool HasDeferSubscription,
    bool HasScheduler);
