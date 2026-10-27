// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace ReactiveUI.Binding.SourceGenerators.Models;

/// <summary>The property a command's parameter lambda selects, and how it was written.</summary>
/// <param name="PropertyPath">The path the lambda walks.</param>
/// <param name="TypeFullName">The fully qualified type of the selected value.</param>
/// <param name="ExpressionText">The lambda as the caller wrote it.</param>
[DebuggerDisplay("ParameterLambda: {ExpressionText,nq}")]
internal readonly record struct ParameterLambda(PropertyPathSegment[] PropertyPath, string TypeFullName, string ExpressionText);
