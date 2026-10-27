// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Models;

/// <summary>Describes one typed property observation expression.</summary>
/// <param name="Source">The variable carrying the property owner.</param>
/// <param name="Segment">The property and its selected mechanism data.</param>
/// <param name="SourceType">The concrete owner type used by the getter.</param>
/// <param name="BeforeChange">Whether to read before the property changes.</param>
/// <param name="Distinct">Whether equal consecutive values are suppressed.</param>
internal readonly record struct ObservationExpression(
    string Source,
    PropertyPathSegment Segment,
    string SourceType,
    bool BeforeChange,
    bool Distinct);
