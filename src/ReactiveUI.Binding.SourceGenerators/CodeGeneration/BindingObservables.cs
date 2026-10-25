// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>The generated variable names each side of a binding reads its values from.</summary>
/// <param name="SourceVar">The variable holding the source side's values.</param>
/// <param name="TargetVar">The variable holding the target side's values.</param>
[DebuggerDisplay("BindingObservables: {SourceVar,nq} -> {TargetVar,nq}")]
internal readonly record struct BindingObservables(string SourceVar, string TargetVar);
