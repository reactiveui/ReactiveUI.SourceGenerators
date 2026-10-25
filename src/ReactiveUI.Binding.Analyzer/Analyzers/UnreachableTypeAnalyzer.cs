// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using ReactiveUI.Binding.Helpers;
using ReactiveUI.Binding.SourceGenerators;

namespace ReactiveUI.Binding.Analyzer.Analyzers;

/// <summary>
/// Reports binding calls that name a type generated code cannot reach (RXUIBIND015): an anonymous type, a private or
/// protected nested type, or a type built from one, in the call's signature or along an observed path.
/// </summary>
/// <remarks>
/// <para>
/// Generated overloads and interceptors are declared in a class of their own. The checks mirror the generator's: the
/// closed type arguments of the resolved method, then the owner of each link of every selector lambda. A link's value
/// type is the next link's owner, or at the leaf one of the type arguments, so it needs no check of its own.
/// </para>
/// <para>
/// On Roslyn 4.13 and newer the generator claims some of these calls with a generic interceptor. A claimed call is
/// not reported. Any other such call would reach the runtime stub and throw, so RXUIBIND015 is an error that fails
/// the build at the call.
/// </para>
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UnreachableTypeAnalyzer : DiagnosticAnalyzer
{
    /// <summary>The diagnostics this analyzer reports.</summary>
    private static readonly ImmutableArray<DiagnosticDescriptor> ReportedDiagnostics =
        new[] { DiagnosticWarnings.UnreachableType, DiagnosticWarnings.TypeParameterCall }.ToImmutableArray();

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

    /// <summary>Reports a binding call that names a type generated code cannot reach.</summary>
    /// <param name="context">The operation analysis context.</param>
    internal static void AnalyzeInvocation(in OperationAnalysisContext context)
    {
        var invocation = (IInvocationOperation)context.Operation;
        var method = invocation.TargetMethod;

        if (!SourceGenerators.Helpers.ExtractorValidation.IsRecognizedExtensionClass(method.ContainingType)
            || AnalyzerHelpers.IsUnsafeBindingMethod(method)
            || !IsGeneratedMethodName(method.Name))
        {
            return;
        }

        var unreachable = FindUnreachable(invocation, in context);
        if (unreachable is null || AnalyzerHelpers.IsIntercepted(invocation, context.CancellationToken))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            SourceGenerators.Helpers.ExtractorValidation.ContainsTypeParameter(unreachable)
                ? DiagnosticWarnings.TypeParameterCall
                : DiagnosticWarnings.UnreachableType,
            invocation.Syntax.GetLocation(),
            unreachable.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat)));
    }

    /// <summary>Finds the first type a binding call names that generated code cannot name.</summary>
    /// <param name="invocation">The binding call.</param>
    /// <param name="context">The operation analysis context.</param>
    /// <returns>The unreachable type, or null when every type is reachable.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ITypeSymbol? FindUnreachable(IInvocationOperation invocation, in OperationAnalysisContext context) =>
        FindUnreachableInSignature(invocation.TargetMethod, context.Compilation)
            ?? FindUnreachableInPaths(invocation.Arguments, context);

    /// <summary>Finds the first type argument of a resolved method that generated code cannot name.</summary>
    /// <param name="method">The resolved binding method.</param>
    /// <param name="compilation">The consumer compilation.</param>
    /// <returns>The unreachable type, or null when every type is reachable.</returns>
    /// <remarks>
    /// A member of an extension block takes the block's type parameters through its grouping type, so those type
    /// arguments are read as well.
    /// </remarks>
    internal static ITypeSymbol? FindUnreachableInSignature(IMethodSymbol method, Compilation compilation) =>
        FindUnreachable(method.TypeArguments, compilation) ?? FindUnreachable(method.ContainingType.TypeArguments, compilation);

    /// <summary>Finds the first link of a selector lambda whose owner generated code cannot name.</summary>
    /// <param name="arguments">The invocation arguments.</param>
    /// <param name="context">The operation analysis context.</param>
    /// <returns>The unreachable type, or null when every link is reachable.</returns>
    internal static ITypeSymbol? FindUnreachableInPaths(ImmutableArray<IArgumentOperation> arguments, in OperationAnalysisContext context)
    {
        var semanticModel = context.Operation.SemanticModel!;
        for (var i = 0; i < arguments.Length; i++)
        {
            if (arguments[i].Value.Syntax is not LambdaExpressionSyntax { ExpressionBody: { } body })
            {
                continue;
            }

            var current = AnalyzerHelpers.SkipNullForgivingAndParentheses(body);
            while (current is MemberAccessExpressionSyntax memberAccess)
            {
                var owner = semanticModel.GetTypeInfo(memberAccess.Expression, context.CancellationToken).Type;
                if (owner is not null && !IsReachable(owner, context.Compilation))
                {
                    return owner;
                }

                current = AnalyzerHelpers.SkipNullForgivingAndParentheses(memberAccess.Expression);
            }
        }

        return null;
    }

    /// <summary>Finds the first of a list of types that generated code cannot name.</summary>
    /// <param name="types">The types.</param>
    /// <param name="compilation">The consumer compilation.</param>
    /// <returns>The unreachable type, or null when every type is reachable.</returns>
    private static ITypeSymbol? FindUnreachable(ImmutableArray<ITypeSymbol> types, Compilation compilation)
    {
        for (var i = 0; i < types.Length; i++)
        {
            if (!IsReachable(types[i], compilation))
            {
                return types[i];
            }
        }

        return null;
    }

    /// <summary>Determines whether the generator produces code for calls to a binding method of this name.</summary>
    /// <param name="name">The method name.</param>
    /// <returns><see langword="true"/> for a generated entry point; runtime-only APIs such as WhenAnyDynamic are not.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsGeneratedMethodName(string name) => AnalyzerHelpers.GeneratedApiNames.Contains(name);

    /// <summary>Determines whether generated code in the consumer's assembly can name a type.</summary>
    /// <param name="type">The type.</param>
    /// <param name="compilation">The consumer compilation.</param>
    /// <returns><see langword="true"/> when the type is accessible from outside every type that declares it.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsReachable(ITypeSymbol type, Compilation compilation) =>
        SourceGenerators.Helpers.ExtractorValidation.IsReachableFromGeneratedCode(type, compilation);
}
