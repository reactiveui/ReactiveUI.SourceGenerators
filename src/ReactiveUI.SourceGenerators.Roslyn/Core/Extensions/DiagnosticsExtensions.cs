// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using ReactiveUI.SourceGenerators.Helpers;
using ReactiveUI.SourceGenerators.Models;

namespace ReactiveUI.SourceGenerators.Extensions;

/// <summary>Extension methods for <see cref="DiagnosticInfo"/>, specifically for reporting diagnostics.</summary>
internal static class DiagnosticsExtensions
{
    /// <summary>Provides extension members for a diagnostic builder.</summary>
    /// <param name="diagnostics">The diagnostic builder to extend.</param>
    extension(ImmutableArrayBuilder<DiagnosticInfo> diagnostics)
    {
        /// <summary>Adds a diagnostic associated with a symbol to this builder.</summary>
        /// <param name="descriptor">The input <see cref="DiagnosticDescriptor"/> for the diagnostic to create.</param>
        /// <param name="symbol">The source <see cref="ISymbol"/> to attach the diagnostic to.</param>
        /// <param name="args">The optional arguments for the formatted message to include.</param>
        internal void Add(DiagnosticDescriptor descriptor, ISymbol symbol, params object[] args) => diagnostics.Add(DiagnosticInfo.Create(descriptor, symbol, args));

        /// <summary>Adds a diagnostic associated with a syntax node to this builder.</summary>
        /// <param name="descriptor">The input <see cref="DiagnosticDescriptor"/> for the diagnostic to create.</param>
        /// <param name="node">The source <see cref="SyntaxNode"/> to attach the diagnostic to.</param>
        /// <param name="args">The optional arguments for the formatted message to include.</param>
        internal void Add(DiagnosticDescriptor descriptor, SyntaxNode node, params object[] args) => diagnostics.Add(DiagnosticInfo.Create(descriptor, node, args));
    }
}
