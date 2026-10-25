// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Conversion;

/// <summary>Describes a platform's visibility enum and the hint enums a binding to it accepts.</summary>
/// <param name="MetadataName">The platform's visibility enum.</param>
/// <param name="Hidden">The enum member a false value maps to.</param>
/// <param name="SupportsHidden">Whether the hidden hint flag applies.</param>
/// <param name="HintNamespace">The namespace of ReactiveUI's hint enum, or empty for none.</param>
/// <param name="BindingPlatform">The platform package declaring this library's hint enum, or empty for none.</param>
internal sealed record VisibilityPlatform(
    string MetadataName,
    string Hidden,
    bool SupportsHidden,
    string HintNamespace,
    string BindingPlatform);
