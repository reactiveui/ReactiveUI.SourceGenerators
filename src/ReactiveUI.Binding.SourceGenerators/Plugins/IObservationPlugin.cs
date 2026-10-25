// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins;

/// <summary>
/// Compile-time plugin interface for property observation code generation.
/// Implementations determine whether a given type can be observed and emit
/// platform-specific observation code (PropertyObservable, EventObservable,
/// DependencyProperty callbacks, KVO, etc.).
/// </summary>
internal interface IObservationPlugin
{
    /// <summary>
    /// Gets the affinity score matching ReactiveUI's runtime values.
    /// Higher values win when multiple plugins match the same type.
    /// </summary>
    int Affinity { get; }

    /// <summary>Gets the observation kind identifier (e.g., "INPC", "ReactiveObject", "WpfDP").</summary>
    string ObservationKind { get; }

    /// <summary>Gets a value indicating whether this plugin supports before-change (PropertyChanging) observation.</summary>
    bool SupportsBeforeChanged { get; }

    /// <summary>Determines whether this plugin can handle the given type based on ClassBindingInfo flags.</summary>
    /// <param name="classInfo">The type-level binding info.</param>
    /// <returns>True if this plugin can generate observation code for this type.</returns>
    bool IsAMatch(ClassBindingInfo classInfo);

    /// <summary>Determines whether this plugin's mechanism reaches one particular property of a matched type.</summary>
    /// <param name="classInfo">The type-level binding info, which lists the properties the type declares.</param>
    /// <param name="propertyName">The property being observed.</param>
    /// <returns>True when the mechanism can observe that property.</returns>
    /// <remarks>
    /// A type advertises a mechanism; a property participates in it or does not. A dependency object can declare
    /// a plain CLR property and a component can declare one with no change event, and emitting the mechanism's
    /// code for those names a member that does not exist. A property the type does not declare - an inherited
    /// one - is unknown rather than absent, and stays observable so nothing regresses on the strength of a
    /// question this cannot answer.
    /// </remarks>
    bool CanObserveProperty(ClassBindingInfo classInfo, string propertyName);

    /// <summary>Scores this property for the requested notification timing; zero declines the observation.</summary>
    /// <param name="classInfo">The concrete owner's capabilities.</param>
    /// <param name="propertyName">The observed property.</param>
    /// <param name="isBeforeChange">Whether the caller requests before-change notifications.</param>
    /// <returns>The eligible mechanism's score, or zero.</returns>
    int GetAffinityForProperty(ClassBindingInfo classInfo, string propertyName, bool isBeforeChange);

    /// <summary>Emits a direct typed expression for this mechanism.</summary>
    /// <param name="sb">The output builder.</param>
    /// <param name="observation">The concrete property and notification timing.</param>
    void EmitObservation(SourceWriter sb, in ObservationExpression observation);
}
