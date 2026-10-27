// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace ReactiveUI.Binding.SourceGenerators.Models;

/// <summary>The two types a binding call site joins.</summary>
/// <param name="SourceTypeFullName">The fully qualified source type.</param>
/// <param name="TargetTypeFullName">The fully qualified target type.</param>
/// <param name="SourceViewThreadInvoker">The invoker class a write to the source carries, or null for none.</param>
/// <param name="TargetViewThreadInvoker">The invoker class a write to the target carries, or null for none.</param>
[DebuggerDisplay("BindingSides: {SourceTypeFullName,nq} -> {TargetTypeFullName,nq}")]
internal readonly record struct BindingSides(
    string SourceTypeFullName,
    string TargetTypeFullName,
    string? SourceViewThreadInvoker = null,
    string? TargetViewThreadInvoker = null);
