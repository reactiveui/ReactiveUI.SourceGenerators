// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using ReactiveUI.Binding.Helpers;
using ReactiveUI.Binding.SourceGenerators;

namespace ReactiveUI.Binding.Analyzer.Analyzers;

/// <summary>
/// Reports RXUIBIND021 on a binding call that has no generated binding, so the runtime method it names throws when it
/// runs.
/// </summary>
/// <remarks>
/// <para>
/// Analyzers read the compilation with every generator's output in it, so a call that the generator bound either resolves
/// to a generated overload or is claimed by a generated interceptor. A call that still resolves to the runtime stub, and
/// that no interceptor claims, got nothing. That covers every cause at once, including a member another source generator
/// adds, which the generator cannot see.
/// </para>
/// <para>
/// The call would throw, so it is an error. The one exception is a member that only generated code declares: no generator
/// sees another's output, and only that generator could change it, so that call stays a warning. A call another error
/// already reports, because it names a type generated code cannot name, is not reported twice.
/// </para>
/// <para>
/// Interceptors exist only on Roslyn 4.13 and newer, so the analyzer built against an older Roslyn has no interceptor to
/// ask about. The generator that loads beside it cannot write one either, so the stub is always the whole answer there.
/// </para>
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class NoGeneratedBindingAnalyzer : DiagnosticAnalyzer
{
    /// <summary>The diagnostics this analyzer reports.</summary>
    private static readonly ImmutableArray<DiagnosticDescriptor> ReportedDiagnostics =
        new[] { DiagnosticWarnings.NoGeneratedBinding }.ToImmutableArray();

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

    /// <summary>Reports a call to a runtime stub that no generated binding replaces.</summary>
    /// <param name="context">The operation analysis context.</param>
    internal static void AnalyzeInvocation(in OperationAnalysisContext context)
    {
        var invocation = (IInvocationOperation)context.Operation;
        var method = invocation.TargetMethod;
        if (!AnalyzerHelpers.GeneratedApiNames.Contains(method.Name)
            || !IsRuntimeStub(method.ContainingType)
            || !Throws(method)
            || AnalyzerHelpers.IsIntercepted(invocation, context.CancellationToken)
            || IsReportedAsUnreachable(invocation, in context))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            DiagnosticWarnings.NoGeneratedBinding,
            invocation.Syntax.GetLocation(),
            NamesAnotherGeneratorsMember(invocation, context.CancellationToken) ? DiagnosticSeverity.Warning : DiagnosticSeverity.Error,
            null,
            null,
            method.Name));
    }

    /// <summary>Determines whether a call's lambdas name a member that only generated code declares.</summary>
    /// <param name="invocation">The call.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see langword="true"/> when some member the call names is declared only in generated files.</returns>
    /// <remarks>
    /// No generator sees another generator's output, so such a member is invisible to this one. The caller cannot change
    /// that, so this one cause stays a warning. Every other cause is something the caller can fix, and fails the build.
    /// </remarks>
    internal static bool NamesAnotherGeneratorsMember(IInvocationOperation invocation, CancellationToken cancellationToken)
    {
        var model = invocation.SemanticModel!;
        foreach (var argument in invocation.Arguments)
        {
            if (argument.Value.Syntax is not LambdaExpressionSyntax { Body: ExpressionSyntax body })
            {
                continue;
            }

            for (var current = AnalyzerHelpers.SkipNullForgivingAndParentheses(body);
                 current is MemberAccessExpressionSyntax access;
                 current = AnalyzerHelpers.SkipNullForgivingAndParentheses(access.Expression))
            {
                if (model.GetSymbolInfo(access, cancellationToken).Symbol is { } member && IsDeclaredOnlyInGeneratedCode(member))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>Determines whether every declaration of a member sits in a generated file.</summary>
    /// <param name="member">The member.</param>
    /// <returns><see langword="true"/> when the member has declarations and all of them are generated.</returns>
    internal static bool IsDeclaredOnlyInGeneratedCode(ISymbol member)
    {
        var references = member.DeclaringSyntaxReferences;
        if (references.IsEmpty)
        {
            return false;
        }

        foreach (var reference in references)
        {
            if (!IsGeneratedFile(reference.SyntaxTree))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>Determines whether a method is declared by a runtime stub class, whose binding methods throw.</summary>
    /// <param name="containingType">The type declaring the method.</param>
    /// <returns><see langword="true"/> for the stub classes, directly or through an extension block's grouping type.</returns>
    /// <remarks>
    /// An extension block declares its members in a grouping type nested in the static class. Its name is empty when read
    /// from source and starts with <c>&lt;</c> when read from metadata.
    /// </remarks>
    internal static bool IsRuntimeStub(INamedTypeSymbol containingType) =>
        IsStubName(containingType.Name)
        || ((containingType.Name.Length == 0 || containingType.Name[0] == '<') && IsStubName(containingType.ContainingType!.Name));

    /// <summary>Determines whether a stub class's method throws, rather than doing the work itself.</summary>
    /// <param name="method">The method.</param>
    /// <returns><see langword="true"/> for <c>ToProperty</c> and for every method that takes an expression tree.</returns>
    /// <remarks>
    /// Every method that takes an expression tree throws, and so does every <c>ToProperty</c> overload, which names its
    /// property by a delegate or a string. <c>InvokeCommand</c> with a command object runs the command itself.
    /// </remarks>
    private static bool Throws(IMethodSymbol method)
    {
        if (method.Name == Constants.ToPropertyMethodName)
        {
            return true;
        }

        foreach (var parameter in method.Parameters)
        {
            if (parameter.Type is INamedTypeSymbol { Name: "Expression", Arity: 1 })
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Determines whether a type name is one of the runtime stub classes.</summary>
    /// <param name="name">The type name.</param>
    /// <returns><see langword="true"/> for the stub classes.</returns>
    private static bool IsStubName(string name) =>
        name is Constants.StubExtensionClassName or Constants.SchedulerExtensionClassName;

    /// <summary>Determines whether RXUIBIND015 or RXUIBIND016 already reports the call, which names a type generated code cannot name.</summary>
    /// <param name="invocation">The call.</param>
    /// <param name="context">The operation analysis context.</param>
    /// <returns><see langword="true"/> when the call names an anonymous, private, protected or type-parameter type.</returns>
    /// <remarks>That error already fails the build at the call, so a second report of the same call adds nothing.</remarks>
    private static bool IsReportedAsUnreachable(IInvocationOperation invocation, in OperationAnalysisContext context) =>
        UnreachableTypeAnalyzer.FindUnreachable(invocation, in context) is not null;

    /// <summary>Determines whether a file is generated, by its name or its auto-generated marker.</summary>
    /// <param name="tree">The file.</param>
    /// <returns><see langword="true"/> for a generated file.</returns>
    private static bool IsGeneratedFile(SyntaxTree tree) =>
        tree.FilePath.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase)
        || tree.FilePath.EndsWith(".generated.cs", StringComparison.OrdinalIgnoreCase)
        || tree.GetRoot().GetLeadingTrivia().ToString().IndexOf("<auto-generated", StringComparison.Ordinal) >= 0;
}
