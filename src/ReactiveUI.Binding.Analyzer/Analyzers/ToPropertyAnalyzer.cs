// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using ReactiveUI.Binding.Helpers;
using ReactiveUI.Binding.SourceGenerators;
using ReactiveUI.Binding.SourceGenerators.Plugins.PropertyRaise;

namespace ReactiveUI.Binding.Analyzer.Analyzers;

/// <summary>
/// Reports <c>ToProperty</c> calls the generator cannot claim: a source whose change notifications generated code
/// cannot raise (RXUIBIND012), and a property named in a form the generator cannot read (RXUIBIND013).
/// </summary>
/// <remarks>
/// Both leave the call on the runtime stub, which throws when it runs. The raise check asks the same plugins the
/// generator asks, so the warning appears exactly when the generator declines the call.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ToPropertyAnalyzer : DiagnosticAnalyzer
{
    /// <summary>The stub parameter naming the property.</summary>
    private const string PropertyParameterName = "property";

    /// <summary>The stub parameter carrying the object that declares the property.</summary>
    private const string SourceParameterName = "source";

    /// <summary>The diagnostics this analyzer reports.</summary>
    private static readonly ImmutableArray<DiagnosticDescriptor> ReportedDiagnostics =
        new[] { DiagnosticWarnings.UnraisableToPropertySource, DiagnosticWarnings.UnreadableToPropertyName }.ToImmutableArray();

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ReportedDiagnostics;

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        ArgumentExceptionHelper.ThrowIfNull(context);
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterOperationAction(static operationContext => AnalyzeInvocation(in operationContext), OperationKind.Invocation);
    }

    /// <summary>Reports a <c>ToProperty</c> call the generator cannot claim.</summary>
    /// <param name="context">The operation analysis context.</param>
    internal static void AnalyzeInvocation(in OperationAnalysisContext context)
    {
        var invocation = (IInvocationOperation)context.Operation;
        var method = invocation.TargetMethod;

        // The name test comes first: it is one string comparison, and it turns away every other invocation.
        if (method.Name != Constants.ToPropertyMethodName
            || !AnalyzerHelpers.IsBindingExtensionMethod(method)
            || AnalyzerHelpers.IsIntercepted(invocation, context.CancellationToken))
        {
            return;
        }

        var property = FindArgument(invocation.Arguments, PropertyParameterName);
        var source = FindArgument(invocation.Arguments, SourceParameterName);

        if (!NamesPropertyReadably(property))
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticWarnings.UnreadableToPropertyName, property.Value.Syntax.GetLocation()));
        }

        // A source typed by a type parameter is not generated for either, and names no type the warning could fix.
        if (source.Parameter!.Type is not INamedTypeSymbol sourceType
            || PropertyRaisePluginRegistry.Select(sourceType, context.Compilation) is not null)
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            DiagnosticWarnings.UnraisableToPropertySource,
            source.Value.Syntax.GetLocation(),
            sourceType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)));
    }

    /// <summary>Determines whether the property argument names a property the generator can read.</summary>
    /// <param name="property">The property argument.</param>
    /// <returns><see langword="true"/> for <c>x =&gt; x.Property</c> or a constant, non-blank string.</returns>
    internal static bool NamesPropertyReadably(IArgumentOperation property) =>
        property.Parameter!.Type.SpecialType == SpecialType.System_String
            ? property.Value.ConstantValue is { HasValue: true, Value: string name } && !string.IsNullOrWhiteSpace(name)
            : IsDirectMemberSelector(property.Value.Syntax);

    /// <summary>Determines whether a selector reads one member straight off its own parameter.</summary>
    /// <param name="syntax">The selector syntax.</param>
    /// <returns><see langword="true"/> for <c>x =&gt; x.Property</c>.</returns>
    internal static bool IsDirectMemberSelector(SyntaxNode syntax)
    {
        var (parameterName, body) = syntax switch
        {
            SimpleLambdaExpressionSyntax simple => (simple.Parameter.Identifier.ValueText, simple.Body as ExpressionSyntax),
            ParenthesizedLambdaExpressionSyntax { ParameterList.Parameters.Count: 1 } parenthesized =>
                (parenthesized.ParameterList.Parameters[0].Identifier.ValueText, parenthesized.Body as ExpressionSyntax),
            _ => (null, null),
        };

        return body is not null
            && AnalyzerHelpers.SkipNullForgivingAndParentheses(body) is MemberAccessExpressionSyntax
            {
                RawKind: (int)SyntaxKind.SimpleMemberAccessExpression,
                Expression: IdentifierNameSyntax receiver,
            }
            && receiver.Identifier.ValueText == parameterName;
    }

    /// <summary>Finds the argument passed for a parameter, whatever position or name it was passed by.</summary>
    /// <param name="arguments">The invocation's arguments.</param>
    /// <param name="parameterName">The parameter name.</param>
    /// <returns>The argument.</returns>
    /// <remarks>Every <c>ToProperty</c> overload takes both the source and the property, so the argument is always there.</remarks>
    private static IArgumentOperation FindArgument(ImmutableArray<IArgumentOperation> arguments, string parameterName)
    {
        var i = 0;
        while (arguments[i].Parameter!.Name != parameterName)
        {
            i++;
        }

        return arguments[i];
    }
}
