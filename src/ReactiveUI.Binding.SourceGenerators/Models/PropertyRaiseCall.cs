// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Models;

/// <summary>One way to raise one change notification.</summary>
/// <param name="Kind">The shape of the call.</param>
/// <param name="Member">The method or event name called; a constant, so the model holds no built string.</param>
/// <param name="Argument">What the call passes to name the property.</param>
/// <param name="Owner">The fully qualified class declaring a static method, or null for an instance member.</param>
internal readonly record struct PropertyRaiseCall(
    PropertyRaiseCallKind Kind,
    string Member,
    PropertyRaiseArgumentKind Argument,
    string? Owner = null);
