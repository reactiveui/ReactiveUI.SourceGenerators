// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.SourceGenerators.Helpers;

namespace ReactiveUI.SourceGenerators.Models;

/// <summary>Describes compile-time persistence metadata extracted from a type marked with <c>[AutoPersist]</c>.</summary>
/// <param name="TargetInfo">The target type metadata.</param>
/// <param name="HasDataContract">Whether the target type is annotated with <c>[DataContract]</c>.</param>
/// <param name="PersistablePropertyNames">The public instance property names annotated with <c>[DataMember]</c>.</param>
internal sealed record AutoPersistInfo(
    TargetInfo TargetInfo,
    bool HasDataContract,
    EquatableArray<string> PersistablePropertyNames);
