// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.SourceGenerators.Models;

/// <summary>Describes a view registration extracted from <c>[RegisterView]</c> or <c>[RegisterView&lt;TViewModel&gt;]</c>.</summary>
/// <param name="ViewTypeName">The fully qualified view type name.</param>
/// <param name="ViewModelTypeName">The fully qualified view model type name.</param>
/// <param name="Contract">The optional view contract, or <see langword="null"/> for the default contract.</param>
/// <param name="IsSingleton">Whether to register the view as a singleton.</param>
internal sealed record RegisteredViewInfo(
    string ViewTypeName,
    string ViewModelTypeName,
    string? Contract,
    bool IsSingleton);
