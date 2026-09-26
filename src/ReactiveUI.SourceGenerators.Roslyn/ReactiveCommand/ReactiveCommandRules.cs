// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ReactiveUI.SourceGenerators.Helpers;

/// <summary>The rules that decide what a <c>[ReactiveCommand]</c> method generates.</summary>
/// <remarks>
/// The command generator and the command analyzer both compile this file, so a method the generator skips, or a
/// scheduler it cannot use, is exactly what the analyzer reports.
/// </remarks>
internal static class ReactiveCommandRules
{
    /// <summary>The attribute property naming the scheduler a command delivers its results on.</summary>
    internal const string OutputSchedulerArgument = "OutputScheduler";

    /// <summary>The attribute property naming the scheduler a synchronous command runs on.</summary>
    internal const string BackgroundSchedulerArgument = "BackgroundScheduler";

    /// <summary>The metadata name of the <c>CancellationToken</c> a task-returning method may take.</summary>
    private const string CancellationTokenMetadataName = "System.Threading.CancellationToken";

    /// <summary>Determines whether a method takes more command parameters than a command can pass.</summary>
    /// <param name="method">The attributed method.</param>
    /// <returns><see langword="true"/> when more than one parameter is not a <c>CancellationToken</c>.</returns>
    internal static bool HasTooManyParameters(IMethodSymbol method)
    {
        var commandParameters = 0;
        foreach (var parameter in method.Parameters)
        {
            if (parameter.Type.ToDisplayString() != CancellationTokenMetadataName)
            {
                commandParameters++;
            }
        }

        return commandParameters > 1;
    }

    /// <summary>Determines whether a method is <c>async void</c>, whose failures a command cannot observe.</summary>
    /// <param name="method">The attributed method.</param>
    /// <returns><see langword="true"/> when the method is <c>async</c> and returns <see langword="void"/>.</returns>
    internal static bool IsAsyncVoid(IMethodSymbol method) => method.IsAsync && method.ReturnsVoid;

    /// <summary>Resolves the scheduler an attribute names to an expression generated code can use.</summary>
    /// <param name="semanticModel">The semantic model of the attributed method's tree.</param>
    /// <param name="position">A position in the attribute, where the name is bound.</param>
    /// <param name="commandType">The type declaring the command.</param>
    /// <param name="name">The scheduler as written: a member name, or an expression such as <c>RxSchedulers.MainThreadScheduler</c>.</param>
    /// <param name="expression">The scheduler expression for generated code, when the name resolves.</param>
    /// <returns><see langword="true"/> when the name is a field, property or parameterless method of the scheduler type.</returns>
    /// <remarks>
    /// The name is bound as an expression at the attribute, so the file's usings and the type's members apply. A member
    /// of the command's type or a base type is written by name; any other member must be static and is written fully
    /// qualified, because generated code does not carry the file's usings. The scheduler type is the one the ReactiveUI
    /// command factories take, so each ReactiveUI flavour gets its own.
    /// </remarks>
    internal static bool TryResolveScheduler(
        SemanticModel semanticModel,
        int position,
        INamedTypeSymbol commandType,
        string name,
        [NotNullWhen(true)] out string? expression)
    {
        expression = null;
        var schedulerType = GetSchedulerType(semanticModel.Compilation);
        if (schedulerType is null || BindMember(semanticModel, position, name) is not { } symbol
            || !TryGetSchedulerMember(symbol, out var memberType, out var callSuffix)
            || !semanticModel.Compilation.ClassifyCommonConversion(memberType, schedulerType).IsImplicit)
        {
            return false;
        }

        if (IsSameOrBaseOf(symbol.ContainingType, commandType))
        {
            expression = symbol.Name + callSuffix;
            return true;
        }

        if (!symbol.IsStatic)
        {
            return false;
        }

        expression = $"{symbol.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.{symbol.Name}{callSuffix}";
        return true;
    }

    /// <summary>Binds a name as an expression at a position.</summary>
    /// <param name="semanticModel">The semantic model of the tree holding the position.</param>
    /// <param name="position">The position the name is bound at.</param>
    /// <param name="name">The name, or an expression such as <c>RxSchedulers.MainThreadScheduler</c>.</param>
    /// <returns>The member the name binds to, or <see langword="null"/>.</returns>
    private static ISymbol? BindMember(SemanticModel semanticModel, int position, string name)
    {
        var syntax = SyntaxFactory.ParseExpression(name);
        if (syntax.ContainsDiagnostics)
        {
            return null;
        }

        // A parameterless call binds as its method, and is written back as a call.
        if (syntax is InvocationExpressionSyntax { ArgumentList.Arguments.Count: 0 } invocation)
        {
            syntax = invocation.Expression;
        }

        // A method named without a call binds as a method group: one accessible overload is the method it names.
        var info = semanticModel.GetSpeculativeSymbolInfo(position, syntax, SpeculativeBindingOption.BindAsExpression);
        return info.Symbol
            ?? (info is { CandidateReason: not CandidateReason.Inaccessible, CandidateSymbols: [IMethodSymbol method] } ? method : null);
    }

    /// <summary>Gets the scheduler type the ReactiveUI command factories take.</summary>
    /// <param name="compilation">The compilation.</param>
    /// <returns>The type of a command factory's <c>outputScheduler</c> parameter, or <see langword="null"/> without ReactiveUI.</returns>
    private static ITypeSymbol? GetSchedulerType(Compilation compilation)
    {
        var command = compilation.GetTypeByMetadataName("ReactiveUI.Reactive.ReactiveCommand")
            ?? compilation.GetTypeByMetadataName("ReactiveUI.ReactiveCommand");
        if (command is null)
        {
            return null;
        }

        foreach (var member in command.GetMembers("Create"))
        {
            if (member is not IMethodSymbol method)
            {
                continue;
            }

            foreach (var parameter in method.Parameters)
            {
                if (parameter.Name == "outputScheduler")
                {
                    return parameter.Type.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
                }
            }
        }

        return null;
    }

    /// <summary>Gets the type a scheduler member provides, and how generated code reads it.</summary>
    /// <param name="symbol">The bound member.</param>
    /// <param name="memberType">The member's type, or its return type for a method.</param>
    /// <param name="callSuffix">The text written after the member's name: a call for a method, otherwise nothing.</param>
    /// <returns><see langword="true"/> when the member is a field, a readable property, or a parameterless method.</returns>
    private static bool TryGetSchedulerMember(
        ISymbol? symbol,
        [NotNullWhen(true)] out ITypeSymbol? memberType,
        out string callSuffix)
    {
        callSuffix = string.Empty;
        switch (symbol)
        {
            case IFieldSymbol field:
            {
                memberType = field.Type;
                return true;
            }

            case IPropertySymbol { GetMethod: not null } property:
            {
                memberType = property.Type;
                return true;
            }

            case IMethodSymbol { Parameters.IsEmpty: true, ReturnsVoid: false, MethodKind: MethodKind.Ordinary } method:
            {
                memberType = method.ReturnType;
                callSuffix = "()";
                return true;
            }

            default:
            {
                memberType = null;
                return false;
            }
        }
    }

    /// <summary>Determines whether a type is another type or one of its base types.</summary>
    /// <param name="candidate">The possible base type.</param>
    /// <param name="type">The type.</param>
    /// <returns><see langword="true"/> when <paramref name="candidate"/> is <paramref name="type"/> or a base of it.</returns>
    private static bool IsSameOrBaseOf(INamedTypeSymbol? candidate, INamedTypeSymbol type)
    {
        for (INamedTypeSymbol? current = type; current is not null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current.OriginalDefinition, candidate?.OriginalDefinition))
            {
                return true;
            }
        }

        return false;
    }
}
