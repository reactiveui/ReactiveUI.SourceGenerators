// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using ReactiveUI.Binding.Helpers;
using ReactiveUI.Binding.SourceGenerators;

namespace ReactiveUI.Binding.Analyzer.Analyzers;

/// <summary>Analyzes types used in binding invocations to detect types with no observable properties. Reports RXUIBIND002.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class TypeAnalyzer : DiagnosticAnalyzer
{
    /// <summary>The diagnostics this analyzer reports.</summary>
    private static readonly ImmutableArray<DiagnosticDescriptor> ReportedDiagnostics =
        new[] { DiagnosticWarnings.NoObservableProperties }.ToImmutableArray();

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

    /// <summary>
    /// Analyzes a method invocation operation to determine whether the source type
    /// has any observable property notification mechanism.
    /// </summary>
    /// <param name="context">The operation analysis context.</param>
    internal static void AnalyzeInvocation(in OperationAnalysisContext context)
    {
        var invocationOp = (IInvocationOperation)context.Operation;

        var methodSymbol = invocationOp.TargetMethod;
        if (!AnalyzerHelpers.IsBindingExtensionMethod(methodSymbol))
        {
            return;
        }

        // Which type argument names the observed object differs by API, and some APIs observe none of them.
        if (!AnalyzerHelpers.LacksObservableMechanism(
            methodSymbol,
            context.Compilation,
            AnalyzerHelpers.ObservedTypeArgumentIndex(methodSymbol),
            out var sourceType))
        {
            return;
        }

        // An interface may be implemented by a notifying runtime type. Unsafe calls discover its provider
        // at runtime, while generated calls must rely on the declared type's mechanism.
        if (sourceType?.TypeKind == TypeKind.Interface && AnalyzerHelpers.IsUnsafeBindingMethod(methodSymbol))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticWarnings.NoObservableProperties,
                invocationOp.Syntax.GetLocation(),
                sourceType!.Name));
    }
}
