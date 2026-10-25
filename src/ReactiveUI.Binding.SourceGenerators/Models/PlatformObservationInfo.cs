// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Models;

/// <summary>Stores the native notification members verified in the consumer compilation.</summary>
/// <param name="Kind">The plugin that owns extraction and emission.</param>
/// <param name="Affinity">The score of the native notification mechanism.</param>
/// <param name="Events">The concrete event delegates to subscribe.</param>
/// <param name="NotificationName">The fully qualified Apple notification constant.</param>
/// <param name="DependencyObjectType">The WinUI or Uno dependency-object type.</param>
/// <param name="KvoKeyPath">The exported Objective-C getter selector.</param>
internal sealed record PlatformObservationInfo(
    string Kind,
    int Affinity,
    EquatableArray<NotificationEventInfo> Events,
    string? NotificationName,
    string? DependencyObjectType,
    string? KvoKeyPath);
