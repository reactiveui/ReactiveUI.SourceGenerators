// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Models;

/// <summary>Describes a typed collection mutation selected from the consumer's native symbols.</summary>
/// <param name="Affinity">The generated mechanism's score.</param>
/// <param name="LayoutOwner">The collection property exposing its layout owner.</param>
internal sealed record SetMethodInfo(int Affinity, string LayoutOwner)
{
    /// <summary>Gets whether the source already supplies the native API's array shape.</summary>
    public bool SourceIsArray { get; init; }
}
