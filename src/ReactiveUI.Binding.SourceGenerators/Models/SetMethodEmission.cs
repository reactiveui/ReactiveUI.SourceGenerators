// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Models;

/// <summary>The typed values needed to emit a native collection write.</summary>
/// <param name="Root">The target variable.</param>
/// <param name="Path">The target collection path.</param>
/// <param name="SourceType">The source value type.</param>
/// <param name="Mechanism">The selected native mechanism.</param>
/// <param name="Expression">The expression used for binding error reporting.</param>
/// <param name="ReportChanges">Whether the public binding exposes applied collections.</param>
internal readonly record struct SetMethodEmission(
    string Root,
    EquatableArray<PropertyPathSegment> Path,
    string SourceType,
    SetMethodInfo Mechanism,
    string Expression,
    bool ReportChanges);
