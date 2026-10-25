// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using ReactiveUI.Binding.Helpers;
using ReactiveUI.Binding.SourceGenerators;

namespace ReactiveUI.Binding.Analyzer.Analyzers;

/// <summary>Reports a <c>ToProperty</c> call below C# 13 made ambiguous by a positional initial value (RXUIBIND014).</summary>
/// <remarks>
/// For a <c>string</c> property, a positional initial value fits the initial-value overload and the optional
/// caller-information <c>string</c> parameter of the overload without one. The initial-value overloads carry
/// <c>[OverloadResolutionPriority(1)]</c>, which only C# 13 and later honour. The compiler's own CS0121 names neither
/// cause nor fix, so this error names the argument to write.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ToPropertyInitialValueAnalyzer : DiagnosticAnalyzer
{
    /// <summary>The stub parameter carrying a plain initial value.</summary>
    private const string InitialValueParameterName = "initialValue";

    /// <summary>The diagnostics this analyzer reports.</summary>
    private static readonly ImmutableArray<DiagnosticDescriptor> ReportedDiagnostics =
        new[] { DiagnosticWarnings.UnnamedToPropertyInitialValue }.ToImmutableArray();

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ReportedDiagnostics;

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        ArgumentExceptionHelper.ThrowIfNull(context);
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(static nodeContext => AnalyzeInvocation(in nodeContext), SyntaxKind.InvocationExpression);
    }

    /// <summary>Reports an ambiguous <c>ToProperty</c> call whose initial value is passed by position.</summary>
    /// <param name="context">The syntax node analysis context.</param>
    internal static void AnalyzeInvocation(in SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        // The name and the language version come from syntax, so every other invocation leaves before the model is asked.
        if (invocation.Expression is not MemberAccessExpressionSyntax { Name.Identifier.ValueText: Constants.ToPropertyMethodName }
            || invocation.SyntaxTree.Options is not CSharpParseOptions { LanguageVersion: <= LanguageVersion.CSharp12 })
        {
            return;
        }

        // The compiler reports CS0121 with the candidates as an overload resolution failure.
        var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken);
        if (symbolInfo.CandidateReason is not (CandidateReason.OverloadResolutionFailure or CandidateReason.Ambiguous))
        {
            return;
        }

        var arguments = invocation.ArgumentList.Arguments;
        var index = FindPositionalInitialValue(arguments, symbolInfo.CandidateSymbols);
        if (index < 0)
        {
            return;
        }

        var argument = arguments[index];
        context.ReportDiagnostic(Diagnostic.Create(
            DiagnosticWarnings.UnnamedToPropertyInitialValue,
            argument.GetLocation(),
            argument.Expression.ToString()));
    }

    /// <summary>
    /// Finds the positional argument that one candidate stub takes as its initial value and another takes as an optional
    /// <c>string</c> parameter.
    /// </summary>
    /// <param name="arguments">The call's arguments.</param>
    /// <param name="candidates">The overloads the compiler could not choose between.</param>
    /// <returns>The argument's index, or -1 when no positional argument is contested that way.</returns>
    internal static int FindPositionalInitialValue(SeparatedSyntaxList<ArgumentSyntax> arguments, ImmutableArray<ISymbol> candidates)
    {
        for (var i = 0; i < arguments.Count && arguments[i].NameColon is null; i++)
        {
            if (AnyCandidateTakes(candidates, i, static parameter => parameter.Name == InitialValueParameterName)
                && AnyCandidateTakes(candidates, i, static parameter => parameter.IsOptional && parameter.Type.SpecialType == SpecialType.System_String))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>Determines whether any candidate stub takes the argument at a position as a matching parameter.</summary>
    /// <param name="candidates">The overloads the compiler could not choose between.</param>
    /// <param name="position">The positional argument's index.</param>
    /// <param name="matches">Tests the parameter the candidate takes the argument as.</param>
    /// <returns><see langword="true"/> when a candidate stub takes the argument as a matching parameter.</returns>
    private static bool AnyCandidateTakes(ImmutableArray<ISymbol> candidates, int position, Func<IParameterSymbol, bool> matches)
    {
        for (var c = 0; c < candidates.Length; c++)
        {
            if (PositionalParameter(candidates[c], position) is { } parameter && matches(parameter))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Gets the parameter a candidate stub takes a positional argument as.</summary>
    /// <param name="candidate">The candidate.</param>
    /// <param name="position">The positional argument's index.</param>
    /// <returns>The parameter, or null when the candidate is not a stub or has too few parameters.</returns>
    private static IParameterSymbol? PositionalParameter(ISymbol candidate, int position)
    {
        if (candidate is not IMethodSymbol method || !AnalyzerHelpers.IsBindingExtensionMethod(method))
        {
            return null;
        }

        // An extension method called on its receiver is reduced: its parameter list starts after 'this'.
        var index = method.MethodKind == MethodKind.ReducedExtension ? position : position + 1;
        return index < method.Parameters.Length ? method.Parameters[index] : null;
    }
}
