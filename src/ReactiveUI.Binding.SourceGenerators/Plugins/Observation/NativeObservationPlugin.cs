// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

/// <summary>Observes a property through one platform's native after-change notification.</summary>
/// <remarks>
/// A platform supplies the parts that differ: its identity, its affinity, which properties it reaches, and the
/// statements that attach to the notification and detach from it. Every native mechanism answers the registry the
/// same way from those parts, and wraps its statements in the runtime's <c>CallbackPropertyObservable</c>.
/// </remarks>
internal sealed class NativeObservationPlugin : IPlatformObservationPlugin
{
    /// <summary>Offers the platform's candidate for one property, or null when the platform does not reach it.</summary>
    private readonly Func<INamedTypeSymbol, IPropertySymbol, PlatformObservationInfo?> _inspect;

    /// <summary>Emits the statements that attach to the platform's notification and detach from it.</summary>
    private readonly Action<SourceWriter, PropertyPathSegment, PlatformObservationInfo> _appendSubscription;

    /// <summary>Initializes a new instance of the <see cref="NativeObservationPlugin"/> class.</summary>
    /// <param name="observationKind">The platform mechanism's identity.</param>
    /// <param name="affinity">The score the mechanism competes with.</param>
    /// <param name="inspect">Offers the platform's candidate for one property.</param>
    /// <param name="appendSubscription">Emits the statements that attach to the notification and detach from it.</param>
    internal NativeObservationPlugin(
        string observationKind,
        int affinity,
        Func<INamedTypeSymbol, IPropertySymbol, PlatformObservationInfo?> inspect,
        Action<SourceWriter, PropertyPathSegment, PlatformObservationInfo> appendSubscription)
    {
        ObservationKind = observationKind;
        Affinity = affinity;
        _inspect = inspect;
        _appendSubscription = appendSubscription;
    }

    /// <inheritdoc/>
    public int Affinity { get; }

    /// <inheritdoc/>
    public string ObservationKind { get; }

    /// <inheritdoc/>
    public bool SupportsBeforeChanged => false;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetAffinityForProperty(ClassBindingInfo classInfo, string propertyName, bool isBeforeChange) =>
        !isBeforeChange && CanObserveProperty(classInfo, propertyName) ? Affinity : 0;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PlatformObservationInfo? InspectProperty(INamedTypeSymbol owner, IPropertySymbol property) =>
        _inspect(owner, property);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAMatch(ClassBindingInfo classInfo) => PlatformSymbols.HasCandidate(classInfo, ObservationKind);

    /// <inheritdoc/>
    public bool CanObserveProperty(ClassBindingInfo classInfo, string propertyName) =>
        PlatformSymbols.Candidate(classInfo, propertyName, ObservationKind) is not null;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EmitObservation(SourceWriter sb, in ObservationExpression observation) =>
        NativeObservationEmitter.Emit(sb, observation, ObservationKind, _appendSubscription);
}
