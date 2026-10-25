// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Helpers;

/// <summary>Extracts WhenAnyObservableInvocationInfo from WhenAnyObservable invocations.</summary>
internal static class WhenAnyObservableExtractor
{
    /// <summary>
    /// Pipeline B transform: extracts InvocationInfo from a WhenAnyObservable invocation.
    /// For each Expression&lt;Func&lt;TSender, IObservable&lt;T&gt;?&gt;&gt; parameter, extracts the property path
    /// and the inner type T by unwrapping IObservable&lt;T&gt; from the leaf property type.
    /// </summary>
    /// <param name="context">The generator syntax context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A WhenAnyObservableInvocationInfo POCO, or null if the invocation is not analyzable.</returns>
    /// <exception cref="OperationCanceledException">If the cancellation token is triggered.</exception>
    internal static WhenAnyObservableInvocationInfo? ExtractWhenAnyObservableInvocation(
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

        // Verify this is our stub or generated method
        if (!ExtractorValidation.IsRecognizedExtensionClass(methodSymbol.ContainingType)
            || !ObservationExtractor.TryResolveScope(context, methodSymbol, ct, out var scope, out var genericResult))
        {
            return null;
        }

        var (propertyPaths, expressionTexts, innerObservableTypes, hasSelector) =
            CollectObservableArguments(methodSymbol, invocation.ArgumentList.Arguments, semanticModel, scope, ct);

        if (propertyPaths.Count == 0)
        {
            return null;
        }

        // Get the source type from the receiver
        var sourceTypeFullName =
            scope.NameOf(semanticModel.GetTypeInfo(memberAccess.Expression, ct).Type);
        if (sourceTypeFullName is null)
        {
            return null;
        }

        // Compute return type
        var namedReturnTypeFullName = hasSelector
            ? ExtractorValidation.FindSelectorReturnType(
                methodSymbol.Parameters,
                "selector")!
            : InvalidOperationExceptionHelper.EnsureNotNull(
                innerObservableTypes[0],
                "inner observable types");
        var returnTypeFullName = genericResult.IsGeneric ? GenericResult.TypeParameterName : namedReturnTypeFullName;

        var filePath = invocation.SyntaxTree.FilePath;
        var lineNumber = SyntaxHelpers.CallerLineNumber(invocation, ct);

        return new(
            filePath,
            lineNumber,
            sourceTypeFullName,
            new([.. propertyPaths]),
            new([.. innerObservableTypes]),
            returnTypeFullName,
            hasSelector,
            new([.. expressionTexts]),
            InterceptableLocationReader.Read(semanticModel, invocation, ct),
            genericResult,
            scope.Call);
    }

    /// <summary>
    /// Walks the invocation's parameters, collecting one property path, expression text and inner
    /// observable type per <c>Expression&lt;Func&lt;TSender, IObservable&lt;T&gt;&gt;&gt;</c> argument.
    /// </summary>
    /// <param name="methodSymbol">The resolved method.</param>
    /// <param name="args">The invocation arguments.</param>
    /// <param name="semanticModel">The semantic model.</param>
    /// <param name="scope">Where the generated code that reads the paths is declared.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The observed paths, their expression texts, their inner types, and whether a selector was supplied.</returns>
    private static ObservableArguments CollectObservableArguments(
            IMethodSymbol methodSymbol,
            SeparatedSyntaxList<ArgumentSyntax> args,
            SemanticModel semanticModel,
            ReachScope scope,
            CancellationToken ct)
    {
        var propertyPaths = new List<EquatableArray<PropertyPathSegment>>(args.Count);
        var expressionTexts = new List<string>(args.Count);
        var innerObservableTypes = new List<string>(args.Count);
        var hasSelector = false;

        for (var i = 0; i < methodSymbol.Parameters.Length; i++)
        {
            var parameter = methodSymbol.Parameters[i];

            if (parameter.Name == "selector")
            {
                hasSelector = true;
                continue;
            }

            if (parameter.Type is not INamedTypeSymbol { Name: "Expression" })
            {
                continue;
            }

            // One path generated code cannot read leaves the whole call to the runtime stub. Keeping the others
            // would generate a method with fewer parameters than the call, which an interceptor cannot claim.
            var path = SyntaxHelpers.ExtractPropertyPathFromLambda(args[i].Expression, semanticModel, scope, ct);
            if (path is null)
            {
                propertyPaths.Clear();
                break;
            }

            propertyPaths.Add(new(path));
            expressionTexts.Add(
                args[i].Expression.ToString());

            // The leaf property type is IObservable<T>; the generated code needs T.
            innerObservableTypes.Add(
                SymbolHelpers.ExtractInnerObservableType(path[^1], semanticModel, args[i].Expression, ct));
        }

        return new(propertyPaths, expressionTexts, innerObservableTypes, hasSelector);
    }
}
