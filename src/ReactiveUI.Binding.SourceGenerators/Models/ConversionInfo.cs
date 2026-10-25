// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Models;

/// <summary>A symbol-validated conversion whose input is named <c>__value</c>.</summary>
/// <param name="Expression">The typed expression producing the converted value.</param>
/// <param name="Condition">The condition under which the conversion succeeds.</param>
/// <param name="Affinity">The score a registered converter must exceed.</param>
internal sealed record ConversionInfo(string Expression, string Condition, int Affinity)
{
    /// <summary>Gets typed local declarations required by a parsing operation.</summary>
    public string Preparation { get; init; } = string.Empty;

    /// <summary>Gets the assignable value used when a registered converter declines the conversion.</summary>
    public string? AssignmentFallback { get; init; }

    /// <summary>Gets a value indicating whether the native path can return the source observable directly.</summary>
    public bool IsIdentity { get; init; }
}
