// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace ReactiveUI.Binding.SourceGenerators.Models;

/// <summary>The two types a BindCommand call site joins.</summary>
/// <param name="ViewTypeFullName">The fully qualified view type.</param>
/// <param name="ViewModelTypeFullName">The fully qualified view model type.</param>
/// <param name="ViewThreadInvoker">The invoker class a write to the view carries, or null for none.</param>
[DebuggerDisplay("BindCommandSides: {ViewTypeFullName,nq} <- {ViewModelTypeFullName,nq}")]
internal readonly record struct BindCommandSides(
    string ViewTypeFullName,
    string ViewModelTypeFullName,
    string? ViewThreadInvoker = null);
