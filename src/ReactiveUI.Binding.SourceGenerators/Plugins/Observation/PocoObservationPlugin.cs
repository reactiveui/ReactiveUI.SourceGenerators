// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

/// <summary>Reads a non-notifying property at subscription time and keeps the observation open.</summary>
internal sealed class PocoObservationPlugin : IObservationPlugin
{
    /// <inheritdoc/>
    public int Affinity => BindingAffinity.Fallback;

    /// <inheritdoc/>
    public string ObservationKind => "POCO";

    /// <inheritdoc/>
    public bool SupportsBeforeChanged => true;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAMatch(ClassBindingInfo classInfo) => true;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanObserveProperty(ClassBindingInfo classInfo, string propertyName) => true;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetAffinityForProperty(ClassBindingInfo classInfo, string propertyName, bool isBeforeChange) => Affinity;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EmitObservation(SourceWriter sb, in ObservationExpression observation) =>
        sb.Append(GeneratedTypeNames.OpenDeferredProperty).Append(observation.SourceType).Append(", ")
            .Append(observation.Segment.PropertyTypeFullName).Append(">(").Append(observation.Source)
            .Append(", __source => ").Append(GeneratedTypeNames.ReadProperty(observation.Segment, observation.SourceType, "__source"))
            .Append(')');
}
