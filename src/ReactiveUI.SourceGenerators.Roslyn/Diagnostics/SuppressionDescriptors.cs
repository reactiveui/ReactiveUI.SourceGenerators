﻿// Copyright (c) 2025 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;

namespace ReactiveUI.SourceGenerators.Diagnostics;

internal static class SuppressionDescriptors
{
    public static readonly SuppressionDescriptor FieldOrPropertyAttributeListForReactiveCommandMethod = new(
        id: "RXUISPR0001",
        suppressedDiagnosticId: "CS0657",
        justification: "Methods using [ReactiveCommand] can use [field:] and [property:] attribute lists to forward attributes to the generated fields and properties");

    public static readonly SuppressionDescriptor FieldIsUsedToGenerateAObservableAsPropertyHelper = new(
        id: "RXUISPR0002",
        suppressedDiagnosticId: "IDE0052",
        justification: "Fields using [ObservableAsProperty] are never read");

    public static readonly SuppressionDescriptor ReactiveCommandDoesNotAccessInstanceData = new(
        id: "RXUISPR0003",
        suppressedDiagnosticId: "CA1822",
        justification: "Methods using [ReactiveCommand] or [ObservableAsProperty] do not need to be static");

    public static readonly SuppressionDescriptor ReactiveFieldsShouldNotBeReadOnly = new(
        id: "RXUISPR0004",
        suppressedDiagnosticId: "RCS1169",
        justification: "Fields using [Reactive] do not need to be ReadOnly");

    public static readonly SuppressionDescriptor FieldOrPropertyAttributeListForReactiveProperty = new(
        id: "RXUISPR0005",
        suppressedDiagnosticId: "CS0657",
        justification: "Fields using [Reactive] can use [property:] attribute lists to forward attributes to the generated properties");
}
