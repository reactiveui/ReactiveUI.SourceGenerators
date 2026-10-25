// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

/// <summary>
/// Observation plugin for types implementing <c>IReactiveObject</c>, which claims them ahead of the
/// plain INPC plugin. <c>IReactiveObject</c> implements both notification interfaces, so the emitted
/// observation is the same.
/// </summary>
internal sealed class ReactiveObjectObservationPlugin : IObservationPlugin
{
    /// <inheritdoc/>
    public int Affinity => BindingAffinity.ExactType;

    /// <inheritdoc/>
    public string ObservationKind => "ReactiveObject";

    /// <inheritdoc/>
    public bool SupportsBeforeChanged => true;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetAffinityForProperty(ClassBindingInfo classInfo, string propertyName, bool isBeforeChange) =>
        IsAMatch(classInfo) ? Affinity : 0;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAMatch(ClassBindingInfo classInfo) => classInfo.ImplementsIReactiveObject;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanObserveProperty(ClassBindingInfo classInfo, string propertyName) => true;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EmitObservation(SourceWriter sb, in ObservationExpression observation) =>
        NotifyPropertyEmitter.EmitShallowObservation(
            sb,
            observation.Source,
            observation.Segment,
            observation.SourceType,
            observation.BeforeChange,
            observation.Distinct);
}
