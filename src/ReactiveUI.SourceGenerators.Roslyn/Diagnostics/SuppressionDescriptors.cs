// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;

namespace ReactiveUI.SourceGenerators.Diagnostics;

/// <summary>Contains descriptors for diagnostics that are intentionally suppressed by this generator.</summary>
internal static class SuppressionDescriptors
{
    /// <summary>Suppresses invalid field or property targets on generated reactive commands.</summary>
    internal static readonly SuppressionDescriptor FieldOrPropertyAttributeListForReactiveCommandMethod = new(
        id: "RXUISPR0001",
        suppressedDiagnosticId: "CS0657",
        justification: "Methods using [ReactiveCommand] can use [field:] and [property:] attribute lists to forward attributes to the generated fields and properties");

    /// <summary>Suppresses unused-field diagnostics for observable-as-property helpers.</summary>
    internal static readonly SuppressionDescriptor FieldIsUsedToGenerateAObservableAsPropertyHelper = new(
        id: "RXUISPR0002",
        suppressedDiagnosticId: "IDE0052",
        justification: "Fields using [ObservableAsProperty] are never read");

    /// <summary>Suppresses static-member recommendations for generator-backed reactive members.</summary>
    internal static readonly SuppressionDescriptor ReactiveCommandDoesNotAccessInstanceData = new(
        id: "RXUISPR0003",
        suppressedDiagnosticId: "CA1822",
        justification: "Methods using [ReactiveCommand] or [ObservableAsProperty] do not need to be static");

    /// <summary>Suppresses readonly-field recommendations for reactive fields.</summary>
    internal static readonly SuppressionDescriptor ReactiveFieldsShouldNotBeReadOnly = new(
        id: "RXUISPR0004",
        suppressedDiagnosticId: "RCS1169",
        justification: "Fields using [Reactive] do not need to be ReadOnly");

    /// <summary>Suppresses invalid field or property targets on generated reactive properties.</summary>
    internal static readonly SuppressionDescriptor FieldOrPropertyAttributeListForReactiveProperty = new(
        id: "RXUISPR0005",
        suppressedDiagnosticId: "CS0657",
        justification: "Fields using [Reactive] can use [property:] attribute lists to forward attributes to the generated properties");
}
