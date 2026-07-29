// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.SourceGenerators.Helpers;

namespace ReactiveUI.SourceGenerators.Models;

/// <summary>Contains the metadata required to generate a routed control host.</summary>
/// <param name="FileHintName">The generated source file hint name.</param>
/// <param name="TargetName">The target type name.</param>
/// <param name="TargetNamespace">The target namespace.</param>
/// <param name="TargetNamespaceWithNamespace">The target namespace declaration.</param>
/// <param name="TargetVisibility">The target type visibility.</param>
/// <param name="TargetType">The target type keyword.</param>
/// <param name="BaseTypeName">The routed control host base type name.</param>
/// <param name="ForwardedAttributes">The attributes forwarded to the generated host.</param>
internal sealed record RoutedControlHostInfo(
    string FileHintName,
    string TargetName,
    string TargetNamespace,
    string TargetNamespaceWithNamespace,
    string TargetVisibility,
    string TargetType,
    string BaseTypeName,
    EquatableArray<string> ForwardedAttributes);
