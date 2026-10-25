// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using ReactiveUI.Binding.Helpers;
using ReactiveUI.Binding.SourceGenerators;
using ReactiveUI.Binding.SourceGenerators.Plugins.ViewThread;

namespace ReactiveUI.Binding.Analyzer.Analyzers;

/// <summary>
/// Reports a binding that writes to a WPF, WinForms or MAUI object when the platform package whose invoker marshals
/// the write onto the object's thread is not referenced (RXUIBIND017).
/// </summary>
/// <remarks>
/// Every type a binding call is closed over is checked, which covers the view of a view-first binding, the target of
/// <c>BindOneWay</c>, <c>BindTwoWay</c> and <c>BindTo</c>, and the control of <c>BindCommand</c>.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ViewThreadInvokerAnalyzer : DiagnosticAnalyzer
{
    /// <summary>The binding APIs that write to a target, without their <c>Unsafe</c> suffix.</summary>
    private static readonly ImmutableHashSet<string> WritingApiNames = new[]
    {
        Constants.BindMethodName,
        Constants.OneWayBindMethodName,
        Constants.BindOneWayMethodName,
        Constants.BindTwoWayMethodName,
        Constants.BindToMethodName,
        Constants.BindCommandMethodName,
    }.ToImmutableHashSet(StringComparer.Ordinal);

    /// <summary>The diagnostics this analyzer reports.</summary>
    private static readonly ImmutableArray<DiagnosticDescriptor> ReportedDiagnostics =
        new[] { DiagnosticWarnings.MissingViewThreadInvoker }.ToImmutableArray();

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

    /// <summary>Reports a binding onto a UI object whose platform package is not referenced.</summary>
    /// <param name="context">The operation analysis context.</param>
    internal static void AnalyzeInvocation(in OperationAnalysisContext context)
    {
        var invocation = (IInvocationOperation)context.Operation;
        var method = invocation.TargetMethod;
        if (!AnalyzerHelpers.IsBindingExtensionMethod(method) || !IsWritingApi(method.Name))
        {
            return;
        }

        var typeArguments = method.TypeArguments;
        for (var i = 0; i < typeArguments.Length; i++)
        {
            if (ViewThreadPluginRegistry.MissingPackageFor(typeArguments[i], context.Compilation) is not { } package)
            {
                continue;
            }

            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticWarnings.MissingViewThreadInvoker,
                invocation.Syntax.GetLocation(),
                typeArguments[i].ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat),
                package));
            return;
        }
    }

    /// <summary>Determines whether a binding method writes to a target, whether generated or <c>Unsafe</c>.</summary>
    /// <param name="name">The method name.</param>
    /// <returns><see langword="true"/> for a binding that writes to a view, target or control.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsWritingApi(string name) =>
        WritingApiNames.Contains(name.EndsWith(AnalyzerHelpers.UnsafeMethodSuffix, StringComparison.Ordinal)
            ? name[..^AnalyzerHelpers.UnsafeMethodSuffix.Length]
            : name);
}
