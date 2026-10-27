// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Helpers;

/// <summary>Extracts BindInteractionInvocationInfo from BindInteraction invocations.</summary>
internal static class InteractionExtractor
{
    /// <summary>The minimum number of arguments a BindInteraction invocation must have (view model, property name, handler).</summary>
    private const int MinimumBindInteractionArgumentCount = 3;

    /// <summary>Pipeline B transform: extracts BindInteractionInvocationInfo from a BindInteraction invocation.</summary>
    /// <param name="context">The generator syntax context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A BindInteractionInvocationInfo POCO, or null if the invocation is not analyzable.</returns>
    /// <exception cref="OperationCanceledException">If the cancellation token is triggered.</exception>
    internal static BindInteractionInvocationInfo? ExtractBindInteractionInvocation(
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
            || !CallSiteHosting.TryResolve(context, methodSymbol, ct, out var scope))
        {
            return null;
        }

        var args = invocation.ArgumentList.Arguments;
        InvalidOperationExceptionHelper.EnsureMinimumArguments(args.Count, MinimumBindInteractionArgumentCount);

        // Extract the interaction property path from the second argument (propertyName)
        var propertyNameArg = args[1].Expression;
        var interactionPropertyPath = SyntaxHelpers.ExtractPropertyPathFromLambda(propertyNameArg, semanticModel, scope, ct);
        if (interactionPropertyPath is null)
        {
            return null;
        }

        ResolveValidatedInteractionTypes(
            propertyNameArg,
            semanticModel,
            ct,
            out var inputTypeFullName,
            out var outputTypeFullName);

        // Determine handler type (Task vs Observable)
        var isTaskHandler = DetermineHandlerVariant(methodSymbol, out var dontCareTypeFullName);

        // Get types
        var viewTypeFullName = ResolveViewType(memberAccess, semanticModel, scope, ct, out var viewClassInfo);
        if (viewTypeFullName is null)
        {
            return null;
        }

        var viewModelTypeFullName = InvalidOperationExceptionHelper.EnsureNotNull(
            ExtractorValidation.GetTypeDisplayName(semanticModel.GetTypeInfo(args[0].Expression, ct).Type),
            "view model type display name");

        var filePath = invocation.SyntaxTree.FilePath;
        var lineNumber = SyntaxHelpers.CallerLineNumber(invocation, ct);
        var expressionText = propertyNameArg.ToString();

        return new(
            filePath,
            lineNumber,
            viewTypeFullName,
            viewModelTypeFullName,
            new(interactionPropertyPath),
            inputTypeFullName,
            outputTypeFullName,
            isTaskHandler,
            dontCareTypeFullName,
            Constants.BindInteractionMethodName,
            expressionText,
            viewClassInfo,
            InterceptableLocationReader.Read(semanticModel, invocation, ct),
            scope.Call);
    }

    /// <summary>Resolves the interaction's two type arguments, refusing a call site that names neither.</summary>
    /// <param name="propertyNameArg">The lambda naming the interaction property.</param>
    /// <param name="semanticModel">The semantic model.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <param name="inputTypeFullName">The fully qualified interaction input type.</param>
    /// <param name="outputTypeFullName">The fully qualified interaction output type.</param>
    private static void ResolveValidatedInteractionTypes(
        ExpressionSyntax propertyNameArg,
        SemanticModel semanticModel,
        CancellationToken ct,
        out string inputTypeFullName,
        out string outputTypeFullName)
    {
        ResolveInteractionTypeArguments(propertyNameArg, semanticModel, ct, out var input, out var output);

        inputTypeFullName =
            InvalidOperationExceptionHelper.EnsureNotNullOrEmpty(input, "interaction TInput type argument");
        outputTypeFullName =
            InvalidOperationExceptionHelper.EnsureNotNullOrEmpty(output, "interaction TOutput type argument");
    }

    /// <summary>Names the view type the call was made on, and reads how it notifies from the same symbol.</summary>
    /// <param name="memberAccess">The member access naming the view the call was made on.</param>
    /// <param name="semanticModel">The semantic model.</param>
    /// <param name="scope">Where the generated code is declared, which decides the types it can name.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <param name="viewClassInfo">How the view notifies, or <see langword="null"/> when no type was named.</param>
    /// <returns>The fully qualified view type name, or <see langword="null"/> when the view names no type.</returns>
    /// <remarks>
    /// This API takes no lambda rooted on the view, so no property path carries the view's mechanism the way
    /// the other view-first APIs' paths do, and the declaration scan only sees types the consumer writes.
    /// Reading it from the symbol is what lets a view declared in a referenced assembly still be followed
    /// through the view model it holds.
    /// </remarks>
    private static string? ResolveViewType(
        MemberAccessExpressionSyntax memberAccess,
        SemanticModel semanticModel,
        ReachScope scope,
        CancellationToken ct,
        out ClassBindingInfo? viewClassInfo)
    {
        viewClassInfo = null;
        var viewType = semanticModel.GetTypeInfo(memberAccess.Expression, ct).Type;

        // A view typed by the caller's type parameter only reaches here when the generated code is generic over it, and
        // how it notifies is read from its constraint.
        if (NotifyingType(viewType) is { } viewTypeSymbol)
        {
            viewClassInfo = TypeDetectionExtractor.ExtractFromSymbol(viewTypeSymbol, semanticModel.Compilation, ct);
        }

        return scope.NameOf(viewType);
    }

    /// <summary>Reads the type whose notifications a view raises: the view's own, or its type parameter's first constraint.</summary>
    /// <param name="viewType">The view's type.</param>
    /// <returns>The type, or null when there is none to read.</returns>
    private static INamedTypeSymbol? NotifyingType(ITypeSymbol? viewType) =>
        viewType switch
        {
            INamedTypeSymbol named => named,
            ITypeParameterSymbol { ConstraintTypes: [INamedTypeSymbol first, ..] } => first,
            _ => null,
        };

    /// <summary>
    /// Resolves the <c>TInput</c> and <c>TOutput</c> type arguments of the targeted
    /// <c>IInteraction&lt;TInput, TOutput&gt;</c> member from the type of the lambda body.
    /// </summary>
    /// <param name="propertyNameArg">The property-name lambda expression.</param>
    /// <param name="semanticModel">The semantic model.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <param name="inputTypeFullName">The resolved fully qualified TInput type, or empty string.</param>
    /// <param name="outputTypeFullName">The resolved fully qualified TOutput type, or empty string.</param>
    /// <remarks>
    /// The caller only reaches here once it has resolved a property path from this same argument, which fails
    /// unless it is a lambda with an expression body, so neither is tested again. The body's type is read rather
    /// than its member's symbol, so an interaction held in a field resolves as well as one held in a property.
    /// </remarks>
    private static void ResolveInteractionTypeArguments(
        ExpressionSyntax propertyNameArg,
        SemanticModel semanticModel,
        CancellationToken ct,
        out string inputTypeFullName,
        out string outputTypeFullName)
    {
        var body = ((LambdaExpressionSyntax)propertyNameArg).Body;
        _ = SymbolHelpers.ExtractInteractionTypeArguments(
            semanticModel.GetTypeInfo(body, ct).Type!,
            out inputTypeFullName,
            out outputTypeFullName);
    }

    /// <summary>
    /// Determines whether the BindInteraction handler is a Task-based or Observable-based handler
    /// by inspecting the method's <c>handler</c> parameter type.
    /// </summary>
    /// <param name="methodSymbol">The resolved method symbol.</param>
    /// <param name="dontCareTypeFullName">
    /// For observable handlers, the fully qualified <c>TDontCare</c> type argument; otherwise null.
    /// </param>
    /// <returns><see langword="true"/> for a Task-based handler; otherwise <see langword="false"/>.</returns>
    private static bool DetermineHandlerVariant(IMethodSymbol methodSymbol, out string? dontCareTypeFullName)
    {
        dontCareTypeFullName = null;

        // The handler parameter is the 3rd argument (index 2).
        // Check the method's parameter type to determine handler variant.
        for (var i = 0; i < methodSymbol.Parameters.Length; i++)
        {
            var param = methodSymbol.Parameters[i];

            // Observable handler: Func<IInteractionContext<TInput, TOutput>, IObservable<TDontCare>>
            if (param is not { Name: "handler", Type: INamedTypeSymbol handlerType }
                || handlerType.TypeArguments.Length != 2
                || handlerType.TypeArguments[1] is not INamedTypeSymbol returnType
                || !SymbolHelpers.IsIObservable(returnType))
            {
                continue;
            }

            dontCareTypeFullName = returnType.TypeArguments[0]
                .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            return false;
        }

        return true;
    }
}
