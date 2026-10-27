// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Helpers;

/// <summary>Extracts <see cref="InvokeCommandInvocationInfo"/> from <c>InvokeCommand</c> invocations.</summary>
internal static class InvokeCommandExtractor
{
    /// <summary>The minimum number of arguments this overload carries (target, command property).</summary>
    private const int MinimumInvokeCommandArgumentCount = 2;

    /// <summary>Pipeline B transform: extracts <see cref="InvokeCommandInvocationInfo"/> from an <c>InvokeCommand</c> invocation.</summary>
    /// <param name="context">The generator syntax context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An <see cref="InvokeCommandInvocationInfo"/> POCO, or null if the invocation is not analyzable.</returns>
    internal static InvokeCommandInvocationInfo? ExtractInvokeCommandInvocation(
        CallSiteContext context,
        CancellationToken ct)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;

        var semanticModel = context.SemanticModel;
        var methodSymbol = ExtractorValidation.ExtractMethodSymbol(semanticModel.GetSymbolInfo(invocation, ct));
        if (methodSymbol is null)
        {
            return null;
        }

        if (!ExtractorValidation.IsRecognizedExtensionClass(methodSymbol.ContainingType)
            || !CallSiteHosting.TryResolve(context, methodSymbol, ct, out var scope))
        {
            return null;
        }

        // The overload taking the command itself has one argument and no selector to observe.
        var args = invocation.ArgumentList.Arguments;
        if (!ExtractorValidation.HasMinimumArguments(args.Count, MinimumInvokeCommandArgumentCount))
        {
            return null;
        }

        // The values offered as the command parameter are the T of the receiver's IObservable<T>.
        var sourceValueType = BindToExtractor.GetObservableValueType(
            semanticModel.GetTypeInfo(memberAccess.Expression, ct).Type);
        if (sourceValueType is null)
        {
            return null;
        }

        var commandArg = args[1].Expression;
        var commandPropertyPath = SyntaxHelpers.ExtractPropertyPathFromLambda(commandArg, semanticModel, scope, ct);
        var targetTypeName =
            scope.NameOf(semanticModel.GetTypeInfo(args[0].Expression, ct).Type);

        // A target the model cannot name leaves nothing to declare a member against, generated or otherwise.
        if (targetTypeName is null)
        {
            return null;
        }

        // A selector the compiler cannot read names no path, so nothing is generated and the stub throws,
        // naming the Unsafe overload that resolves it.
        return commandPropertyPath is not { Length: > 0 }
            ? null
            : new(
                invocation.SyntaxTree.FilePath,
                SyntaxHelpers.CallerLineNumber(invocation, ct),
                sourceValueType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                targetTypeName,
                new(commandPropertyPath),
                commandArg.ToString(),
                InterceptableLocationReader.Read(semanticModel, invocation, ct),
                scope.Call);
    }
}
