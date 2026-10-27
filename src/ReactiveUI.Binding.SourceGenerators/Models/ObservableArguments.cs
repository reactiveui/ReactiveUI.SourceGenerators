// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace ReactiveUI.Binding.SourceGenerators.Models;

/// <summary>The observable arguments one WhenAnyObservable call site passes, and whether it projects them.</summary>
/// <param name="PropertyPaths">The path each observable argument was reached by.</param>
/// <param name="ExpressionTexts">Each argument as the caller wrote it.</param>
/// <param name="InnerObservableTypes">The element type each observable carries.</param>
/// <param name="HasSelector">Whether the call site supplied a projection.</param>
[DebuggerDisplay("ObservableArguments: {PropertyPaths.Count} paths, selector={HasSelector}")]
internal sealed record ObservableArguments(
    List<EquatableArray<PropertyPathSegment>> PropertyPaths,
    List<string> ExpressionTexts,
    List<string> InnerObservableTypes,
    bool HasSelector);
