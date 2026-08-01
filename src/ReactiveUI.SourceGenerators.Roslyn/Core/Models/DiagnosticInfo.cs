// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using ReactiveUI.SourceGenerators.Helpers;

namespace ReactiveUI.SourceGenerators.Models;

/// <summary>A model for a serializeable diagnostic info.</summary>
/// <param name="Descriptor">The wrapped <see cref="DiagnosticDescriptor"/> instance.</param>
/// <param name="SyntaxTree">The tree to use as location for the diagnostic, if available.</param>
/// <param name="TextSpan">The span to use as location for the diagnostic.</param>
/// <param name="Arguments">The diagnostic arguments.</param>
internal sealed record DiagnosticInfo(
    DiagnosticDescriptor Descriptor,
    SyntaxTree? SyntaxTree,
    TextSpan TextSpan,
    EquatableArray<string> Arguments)
{
    /// <summary>Creates a new <see cref="DiagnosticInfo"/> instance with the specified parameters.</summary>
    /// <param name="descriptor">The input <see cref="DiagnosticDescriptor"/> for the diagnostics to create.</param>
    /// <param name="symbol">The source <see cref="ISymbol"/> to attach the diagnostics to.</param>
    /// <param name="args">The optional arguments for the formatted message to include.</param>
    /// <returns>A new <see cref="DiagnosticInfo"/> instance with the specified parameters.</returns>
    internal static DiagnosticInfo Create(DiagnosticDescriptor descriptor, ISymbol symbol, params object[] args)
    {
        var location = symbol.Locations[0];

        return new(descriptor, location.SourceTree, location.SourceSpan, CreateArguments(args));
    }

    /// <summary>Creates a new <see cref="DiagnosticInfo"/> instance with the specified parameters.</summary>
    /// <param name="descriptor">The input <see cref="DiagnosticDescriptor"/> for the diagnostics to create.</param>
    /// <param name="node">The source <see cref="SyntaxNode"/> to attach the diagnostics to.</param>
    /// <param name="args">The optional arguments for the formatted message to include.</param>
    /// <returns>A new <see cref="DiagnosticInfo"/> instance with the specified parameters.</returns>
    internal static DiagnosticInfo Create(DiagnosticDescriptor descriptor, SyntaxNode node, params object[] args)
    {
        var location = node.GetLocation();

        return new(descriptor, location.SourceTree, location.SourceSpan, CreateArguments(args));
    }

    /// <summary>Creates a new <see cref="Diagnostic"/> instance with the state from this model.</summary>
    /// <returns>A new <see cref="Diagnostic"/> instance with the state from this model.</returns>
    internal Diagnostic ToDiagnostic() =>
        SyntaxTree is not null
            ? Diagnostic.Create(Descriptor, Location.Create(SyntaxTree, TextSpan), Arguments.ToArray())
            : Diagnostic.Create(Descriptor, null, Arguments.ToArray());

    /// <summary>Converts diagnostic arguments into an equatable array.</summary>
    /// <param name="args">The diagnostic arguments.</param>
    /// <returns>The string representation of every argument.</returns>
    private static EquatableArray<string> CreateArguments(object[] args)
    {
        using var arguments = ImmutableArrayBuilder<string>.Rent();
        foreach (var argument in args)
        {
            arguments.Add(argument.ToString() ?? string.Empty);
        }

        return arguments.ToImmutable();
    }
}
