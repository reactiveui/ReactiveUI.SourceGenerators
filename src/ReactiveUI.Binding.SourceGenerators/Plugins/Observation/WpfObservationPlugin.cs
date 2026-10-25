// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

/// <summary>Observes WPF dependency properties through <c>DependencyPropertyDescriptor.AddValueChanged</c>.</summary>
internal sealed class WpfObservationPlugin : IPlatformObservationPlugin
{
    /// <summary>The affinity this plugin bids with.</summary>
    private static readonly int WpfAffinity = BindingAffinity.WpfDependencyObject;

    /// <inheritdoc/>
    public int Affinity => WpfAffinity;

    /// <inheritdoc/>
    public string ObservationKind => "WpfDP";

    /// <inheritdoc/>
    public bool SupportsBeforeChanged => false;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetAffinityForProperty(ClassBindingInfo classInfo, string propertyName, bool isBeforeChange) =>
        IsAMatch(classInfo) && CanObserveProperty(classInfo, propertyName) ? Affinity : 0;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAMatch(ClassBindingInfo classInfo) =>
        classInfo.InheritsWpfDependencyObject;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanObserveProperty(ClassBindingInfo classInfo, string propertyName) =>
        ObservedProperties.Find(classInfo, propertyName) is { SymbolsInspected: true }
            ? PlatformSymbols.Candidate(classInfo, propertyName, ObservationKind) is not null
            : ObservedProperties.IsDependencyProperty(classInfo, propertyName);

    /// <inheritdoc/>
    public PlatformObservationInfo? InspectProperty(INamedTypeSymbol owner, IPropertySymbol property) =>
        PlatformSymbols.DerivesFrom(owner, "System.Windows.DependencyObject")
        && PlatformSymbols.HasDependencyProperty(owner, property.Name, "System.Windows.DependencyProperty")
            ? new(ObservationKind, Affinity, default, null, "global::System.Windows.DependencyObject", null)
            : null;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EmitObservation(SourceWriter sb, in ObservationExpression observation) =>
        WpfObservationEmitter.Emit(sb, observation.Source, observation.Segment, observation.SourceType, observation.Distinct);
}
