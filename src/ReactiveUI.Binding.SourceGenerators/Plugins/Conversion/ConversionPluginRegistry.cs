// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Conversion;

/// <summary>Selects typed framework conversions from the consumer's symbols.</summary>
internal static class ConversionPluginRegistry
{
    /// <summary>The framework adapters, in deterministic tie order.</summary>
    private static readonly IConversionPlugin[] Plugins =
    [
        new WpfConversionPlugin(),
        new WinUIConversionPlugin(),
        new UnoConversionPlugin(),
        new MauiConversionPlugin(),
        new AndroidConversionPlugin(),
        new AppleConversionPlugin(),
        new NumericStringConversionPlugin(),
        new BooleanStringConversionPlugin(),
        new GuidStringConversionPlugin(),
        new TemporalStringConversionPlugin(),
        new NullableValueConversionPlugin(),
        new UriConversionPlugin(),
        new StringIdentityConversionPlugin(),
        new EqualityConversionPlugin(),
        new LanguageConversionPlugin(),
    ];

    /// <summary>Reads the declared value type of a property selector.</summary>
    /// <param name="expression">The selector expression.</param>
    /// <param name="model">The consumer's semantic model.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The selected value type.</returns>
    internal static ITypeSymbol? SelectorType(ExpressionSyntax expression, SemanticModel model, CancellationToken token) =>
        expression is LambdaExpressionSyntax { Body: ExpressionSyntax body } ? model.GetTypeInfo(body, token).Type : null;

    /// <summary>Compares applicable framework mechanisms and retains the highest affinity.</summary>
    /// <param name="source">The declared input type.</param>
    /// <param name="target">The declared output type.</param>
    /// <param name="compilation">The consumer compilation.</param>
    /// <returns>The winning direct conversion, or null when the registry must resolve it.</returns>
    internal static ConversionInfo? Select(ITypeSymbol? source, ITypeSymbol? target, Compilation compilation)
    {
        if (source is null || target is null)
        {
            return null;
        }

        ConversionInfo? winner = null;
        foreach (var plugin in Plugins)
        {
            var candidate = plugin.Select(source, target, compilation);
            if (candidate is not null && (winner is null || candidate.Affinity > winner.Affinity))
            {
                winner = candidate;
            }
        }

        return winner is null ? null : ConversionSymbols.WithAssignment(winner, source, target, compilation);
    }
}
