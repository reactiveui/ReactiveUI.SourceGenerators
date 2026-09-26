// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using ReactiveUI.SourceGenerators.Helpers;
using static ReactiveUI.SourceGenerators.CodeFixers.Diagnostics.DiagnosticDescriptors;

namespace ReactiveUI.SourceGenerators.CodeFixers;

/// <summary>Reports <c>[ReactiveCommand]</c> methods and options the command generator cannot use.</summary>
/// <remarks>
/// The generator skips what it cannot generate from without a word, so these are reported here. The rules are the
/// generator's own, from the <see cref="ReactiveCommandRules"/> both compile.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ReactiveCommandAnalyzer : DiagnosticAnalyzer
{
    /// <summary>The metadata name of <c>[ReactiveCommand]</c>.</summary>
    private const string ReactiveCommandAttributeName = "ReactiveUI.SourceGenerators.ReactiveCommandAttribute";

    /// <summary>Gets the diagnostics this analyzer reports.</summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(InvalidReactiveCommandMethodSignatureRule, AsyncVoidReactiveCommandMethodRule, UnresolvedReactiveCommandSchedulerRule);

    /// <summary>Registers the method analysis.</summary>
    /// <param name="context">The analysis context.</param>
    public override void Initialize(AnalysisContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(static nodeContext => AnalyzeMethod(in nodeContext), SyntaxKind.MethodDeclaration);
    }

    /// <summary>Analyzes a method declaration that carries an attribute.</summary>
    /// <param name="context">The syntax-node analysis context.</param>
    private static void AnalyzeMethod(in SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MethodDeclarationSyntax { AttributeLists.Count: > 0 } declaration
            || context.SemanticModel.GetDeclaredSymbol(declaration, context.CancellationToken) is not { } method
            || FindReactiveCommandAttribute(method) is not { } attribute)
        {
            return;
        }

        if (ReactiveCommandRules.HasTooManyParameters(method))
        {
            context.ReportDiagnostic(Diagnostic.Create(InvalidReactiveCommandMethodSignatureRule, declaration.Identifier.GetLocation(), method.Name));
        }

        if (ReactiveCommandRules.IsAsyncVoid(method))
        {
            context.ReportDiagnostic(Diagnostic.Create(AsyncVoidReactiveCommandMethodRule, declaration.Identifier.GetLocation(), method.Name));
        }

        if (attribute.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken) is not AttributeSyntax { ArgumentList: { } arguments })
        {
            return;
        }

        foreach (var argument in arguments.Arguments)
        {
            AnalyzeScheduler(context, method, attribute, argument);
        }
    }

    /// <summary>Reports a scheduler argument whose name the generator cannot resolve.</summary>
    /// <param name="context">The syntax-node analysis context.</param>
    /// <param name="method">The attributed method.</param>
    /// <param name="attribute">The <c>[ReactiveCommand]</c> attribute.</param>
    /// <param name="argument">One of the attribute's arguments.</param>
    private static void AnalyzeScheduler(in SyntaxNodeAnalysisContext context, IMethodSymbol method, AttributeData attribute, AttributeArgumentSyntax argument)
    {
        var argumentName = argument.NameEquals?.Name.Identifier.ValueText;
        if (argumentName is not (ReactiveCommandRules.OutputSchedulerArgument or ReactiveCommandRules.BackgroundSchedulerArgument)
            || GetNamedString(attribute, argumentName) is not { } name
            || ReactiveCommandRules.TryResolveScheduler(context.SemanticModel, argument.SpanStart, method.ContainingType, name, out _))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(UnresolvedReactiveCommandSchedulerRule, argument.Expression.GetLocation(), argumentName, name));
    }

    /// <summary>Finds a method's <c>[ReactiveCommand]</c> attribute.</summary>
    /// <param name="method">The method.</param>
    /// <returns>The attribute, or <see langword="null"/> when the method has none.</returns>
    private static AttributeData? FindReactiveCommandAttribute(IMethodSymbol method)
    {
        foreach (var attribute in method.GetAttributes())
        {
            if (attribute.AttributeClass?.ToDisplayString() == ReactiveCommandAttributeName)
            {
                return attribute;
            }
        }

        return null;
    }

    /// <summary>Gets the string value of an attribute's named argument.</summary>
    /// <param name="attribute">The attribute.</param>
    /// <param name="name">The argument's name.</param>
    /// <returns>The value, or <see langword="null"/> when the argument is absent or not a string.</returns>
    private static string? GetNamedString(AttributeData attribute, string name) =>
        attribute.NamedArguments.FirstOrDefault(argument => argument.Key == name).Value.Value as string;
}
