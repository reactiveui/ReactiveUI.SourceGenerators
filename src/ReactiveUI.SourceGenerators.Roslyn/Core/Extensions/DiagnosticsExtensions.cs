// Copyright (c) 2025 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using ReactiveUI.SourceGenerators.Helpers;
using ReactiveUI.SourceGenerators.Models;

namespace ReactiveUI.SourceGenerators.Extensions;

/// <summary>
/// Extension methods for <see cref="DiagnosticInfo"/>, specifically for reporting diagnostics.
/// </summary>
internal static class DiagnosticsExtensions
{
    /// <summary>
    /// Adds a new diagnostics to the target builder.
    /// </summary>
    /// <param name="diagnostics">The collection of produced <see cref="DiagnosticInfo"/> instances.</param>
    /// <param name="descriptor">The input <see cref="DiagnosticDescriptor"/> for the diagnostics to create.</param>
    /// <param name="symbol">The source <see cref="ISymbol"/> to attach the diagnostics to.</param>
    /// <param name="args">The optional arguments for the formatted message to include.</param>
    public static void Add(
        this ImmutableArrayBuilder<DiagnosticInfo> diagnostics,
        DiagnosticDescriptor descriptor,
        ISymbol symbol,
        params object[] args) => diagnostics.Add(DiagnosticInfo.Create(descriptor, symbol, args));

    /// <summary>
    /// Adds a new diagnostics to the target builder.
    /// </summary>
    /// <param name="diagnostics">The collection of produced <see cref="DiagnosticInfo"/> instances.</param>
    /// <param name="descriptor">The input <see cref="DiagnosticDescriptor"/> for the diagnostics to create.</param>
    /// <param name="node">The source <see cref="SyntaxNode"/> to attach the diagnostics to.</param>
    /// <param name="args">The optional arguments for the formatted message to include.</param>
    public static void Add(
        this ImmutableArrayBuilder<DiagnosticInfo> diagnostics,
        DiagnosticDescriptor descriptor,
        SyntaxNode node,
        params object[] args) => diagnostics.Add(DiagnosticInfo.Create(descriptor, node, args));
}
