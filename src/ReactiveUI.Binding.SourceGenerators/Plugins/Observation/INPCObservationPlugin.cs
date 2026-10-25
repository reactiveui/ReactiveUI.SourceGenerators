// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

/// <summary>
/// Observation plugin for types implementing <see cref="System.ComponentModel.INotifyPropertyChanged"/>.
/// Supports both after-change and before-change (if the type also implements INotifyPropertyChanging).
/// </summary>
internal sealed class INPCObservationPlugin : IObservationPlugin
{
    /// <inheritdoc/>
    public int Affinity => BindingAffinity.Explicit;

    /// <inheritdoc/>
    public string ObservationKind => "INPC";

    /// <inheritdoc/>
    public bool SupportsBeforeChanged => true;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetAffinityForProperty(ClassBindingInfo classInfo, string propertyName, bool isBeforeChange)
    {
        var supported = isBeforeChange ? classInfo.ImplementsINPChanging : classInfo.ImplementsINPC;
        return supported ? Affinity : 0;
    }

    /// <inheritdoc/>
    public bool IsAMatch(ClassBindingInfo classInfo) =>
        classInfo.ImplementsINPC && !classInfo.ImplementsIReactiveObject;

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
