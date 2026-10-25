// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Helpers;

/// <summary>Extracts InvocationInfo from WhenChanged, WhenChanging, WhenAnyValue, and WhenAny invocations.</summary>
internal static class ObservationExtractor
{
    /// <summary>Pipeline B transform: extracts InvocationInfo from a WhenChanged invocation.</summary>
    /// <param name="context">The generator syntax context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An InvocationInfo POCO, or null if the invocation is not analyzable.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static InvocationInfo? ExtractWhenChangedInvocation(CallSiteContext context, CancellationToken ct) =>
        ExtractInvocationInfo(context, false, Constants.WhenChangedMethodName, ct);

    /// <summary>Pipeline B transform: extracts InvocationInfo from a WhenChanging invocation.</summary>
    /// <param name="context">The generator syntax context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An InvocationInfo POCO, or null if the invocation is not analyzable.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static InvocationInfo? ExtractWhenChangingInvocation(CallSiteContext context, CancellationToken ct) =>
        ExtractInvocationInfo(context, true, Constants.WhenChangingMethodName, ct);

    /// <summary>Pipeline B transform: extracts InvocationInfo from a WhenAnyValue invocation.</summary>
    /// <param name="context">The generator syntax context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An InvocationInfo POCO, or null if the invocation is not analyzable.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static InvocationInfo? ExtractWhenAnyValueInvocation(CallSiteContext context, CancellationToken ct) =>
        ExtractInvocationInfo(context, false, Constants.WhenAnyValueMethodName, ct);

    /// <summary>Pipeline B transform: extracts InvocationInfo from a WhenAny invocation (with IObservedChange selector).</summary>
    /// <param name="context">The generator syntax context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An InvocationInfo POCO, or null if the invocation is not analyzable.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static InvocationInfo? ExtractWhenAnyInvocation(CallSiteContext context, CancellationToken ct) =>
        ExtractInvocationInfo(context, false, Constants.WhenAnyMethodName, ct);

    /// <summary>Determines whether a parameter is the overload's projection over the observed values.</summary>
    /// <param name="parameterName">The parameter name to test.</param>
    /// <returns><see langword="true"/> when the parameter carries the projection.</returns>
    /// <remarks>
    /// The observation APIs spell it two ways: <c>WhenChanged</c> and <c>WhenChanging</c> take a
    /// <c>conversionFunc</c>, while <c>WhenAny</c> and <c>WhenAnyValue</c> take a <c>selector</c>. Either one
    /// means the same thing to the emitters - the overload returns the projection rather than the values.
    /// </remarks>
    internal static bool IsSelectorParameterName(string parameterName) =>
        parameterName is "conversionFunc" or "selector";

    /// <summary>Extracts the invocation info from the generator syntax context.</summary>
    /// <param name="context">The generator syntax context.</param>
    /// <param name="isBeforeChange">A value indicating whether the invocation is before a change.</param>
    /// <param name="expectedMethodName">The expected method name.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An InvocationInfo POCO, or null if the invocation is not analyzable.</returns>
    /// <exception cref="OperationCanceledException">If the cancellation token is triggered.</exception>
    internal static InvocationInfo? ExtractInvocationInfo(
        CallSiteContext context,
        bool isBeforeChange,
        string expectedMethodName,
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
            || !TryResolveScope(context, methodSymbol, ct, out var scope, out var genericResult))
        {
            return null;
        }

        var args = invocation.ArgumentList.Arguments;
        var propertyPaths = new List<EquatableArray<PropertyPathSegment>>(args.Count);
        var expressionTexts = new List<string>(args.Count);

        var hasSelector = CollectPropertyPaths(
            methodSymbol,
            args,
            semanticModel,
            scope,
            propertyPaths,
            expressionTexts,
            ct);

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

        var returnTypeFullName = genericResult.IsGeneric
            ? GenericResult.TypeParameterName
            : ComputeReturnTypeFullName(methodSymbol, propertyPaths, hasSelector);

        var filePath = invocation.SyntaxTree.FilePath;
        var lineNumber = SyntaxHelpers.CallerLineNumber(invocation, ct);

        return new(
            filePath,
            lineNumber,
            sourceTypeFullName,
            new([.. propertyPaths]),
            returnTypeFullName,
            isBeforeChange,
            hasSelector,
            expectedMethodName,
            new([.. expressionTexts]),
            InterceptableLocationReader.Read(semanticModel, invocation, ct),
            genericResult,
            scope.Call);
    }

    /// <summary>Decides where an observation call's generated code is declared, and whether its result is a type parameter.</summary>
    /// <param name="context">The call site and its semantic model.</param>
    /// <param name="method">The resolved binding method.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <param name="scope">The scope the generated code can name types in, which may move while the call is read.</param>
    /// <param name="genericResult">Where the result sits when generated code takes it as a type parameter.</param>
    /// <returns><see langword="true"/> when the call can be generated; otherwise <see langword="false"/>.</returns>
    /// <remarks>
    /// A result generated code cannot name is taken as a type parameter first, because that needs nothing of the
    /// caller, and the code then stays where it is. Any other call moves only when its types or paths need it.
    /// </remarks>
    internal static bool TryResolveScope(
        in CallSiteContext context,
        IMethodSymbol method,
        CancellationToken ct,
        out ReachScope scope,
        out GenericResult genericResult)
    {
        scope = ReachScope.ForCall(context, method, ct);
        if (!TryReadGenericResult(method, context.SemanticModel.Compilation, out genericResult))
        {
            return scope.CanNameAll(method.TypeArguments);
        }

        if (genericResult.IsGeneric)
        {
            scope.Pin();
        }

        return true;
    }

    /// <summary>Decides whether generated code can write a call's types, taking its selector's result as a type parameter if need be.</summary>
    /// <param name="method">The resolved binding method.</param>
    /// <param name="compilation">The consumer compilation.</param>
    /// <param name="genericResult">Where the result sits when generated code takes it as a type parameter.</param>
    /// <returns><see langword="true"/> when the call can be generated; otherwise <see langword="false"/>.</returns>
    /// <remarks>
    /// Only an interceptor can be generic over the result, because it takes the called method's own type
    /// parameters. A build that cannot describe a call site has no interceptor to write, so such a call stays on
    /// the diagnostic path there.
    /// </remarks>
    internal static bool TryReadGenericResult(IMethodSymbol method, Compilation compilation, out GenericResult genericResult)
    {
        genericResult = default;
        if (ExtractorValidation.NamesOnlyReachableTypes(method, compilation))
        {
            return true;
        }

        if (!InterceptableLocationReader.IsSupported
            || !ExtractorValidation.TryFindUnnameableResult(method, compilation, out var ordinal))
        {
            return false;
        }

        genericResult = new(method.TypeArguments.Length, ordinal);
        return true;
    }

    /// <summary>
    /// Loops over the method parameters, collecting property paths and expression texts for
    /// each <c>Expression&lt;Func&lt;...&gt;&gt;</c> argument and detecting a selector parameter.
    /// </summary>
    /// <param name="methodSymbol">The resolved method symbol.</param>
    /// <param name="args">The invocation argument list.</param>
    /// <param name="semanticModel">The semantic model.</param>
    /// <param name="scope">Where the generated code that reads the paths is declared.</param>
    /// <param name="propertyPaths">The list to append extracted property paths to.</param>
    /// <param name="expressionTexts">The list to append normalized expression texts to.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// <see langword="true"/> if a selector/conversion parameter was found. When a path cannot be read,
    /// <paramref name="propertyPaths"/> is left empty.
    /// </returns>
    private static bool CollectPropertyPaths(
        IMethodSymbol methodSymbol,
        SeparatedSyntaxList<ArgumentSyntax> args,
        SemanticModel semanticModel,
        ReachScope scope,
        List<EquatableArray<PropertyPathSegment>> propertyPaths,
        List<string> expressionTexts,
        CancellationToken ct)
    {
        var hasSelector = false;

        for (var i = 0; i < methodSymbol.Parameters.Length; i++)
        {
            var parameter = methodSymbol.Parameters[i];

            // The parameter list is walked by position, so a call that supplies fewer arguments than the
            // resolved method declares - the trailing caller-info ones are always omitted - has nothing at
            // this position. Reading past the arguments would throw out of the transform, and a generator
            // that throws contributes no source at all, taking every unrelated file down with it.
            if (i >= args.Count)
            {
                break;
            }

            // Check if parameter type is Expression<Func<...>>
            if (parameter.Type is INamedTypeSymbol { Name: "Expression" })
            {
                // One path generated code cannot read leaves the whole call to the runtime stub. Keeping the
                // others would generate a method with fewer parameters than the call, which an interceptor cannot claim.
                var path = SyntaxHelpers.ExtractPropertyPathFromLambda(args[i].Expression, semanticModel, scope, ct);
                if (path is null)
                {
                    propertyPaths.Clear();
                    return false;
                }

                propertyPaths.Add(new(path));
                expressionTexts.Add(
                    args[i].Expression.ToString());
            }
            else if (IsSelectorParameterName(parameter.Name))
            {
                hasSelector = true;
            }
        }

        return hasSelector;
    }

    /// <summary>
    /// Computes the observation return type from the collected property paths rather than from
    /// <c>methodSymbol.ReturnType</c>. This avoids an off-by-one issue where overload resolution
    /// might resolve to a previously-generated concrete overload with a different property count.
    /// </summary>
    /// <param name="methodSymbol">The resolved method symbol.</param>
    /// <param name="propertyPaths">The collected property paths.</param>
    /// <param name="hasSelector">Whether the overload has a selector/conversion parameter.</param>
    /// <returns>The fully qualified return type name.</returns>
    private static string ComputeReturnTypeFullName(
        IMethodSymbol methodSymbol,
        List<EquatableArray<PropertyPathSegment>> propertyPaths,
        bool hasSelector)
    {
        if (hasSelector)
        {
            // For selector overloads, find the selector parameter's Func<..., TReturn>
            // and extract TReturn (the last type argument).
            // hasSelector is only true when the parameter exists, so this always returns non-null.
            return ExtractorValidation.FindSelectorReturnType(
                methodSymbol.Parameters,
                "conversionFunc",
                "selector")!;
        }

        if (propertyPaths.Count == 1)
        {
            // Single property: return type is the leaf property type.
            var singlePath = propertyPaths[0];
            return singlePath[singlePath.Length - 1].PropertyTypeFullName;
        }

        // Multiple properties: the emission carries every value, so the return type names them as one.
        var valuesBuilder = new System.Text.StringBuilder(CodeGeneration.GeneratedTypeNames.PropertyValues).Append('<');
        for (var i = 0; i < propertyPaths.Count; i++)
        {
            var path = propertyPaths[i];
            var leafType = path[path.Length - 1].PropertyTypeFullName;
            _ = valuesBuilder.Append(leafType);
            if (i < propertyPaths.Count - 1)
            {
                _ = valuesBuilder.Append(", ");
            }
        }

        _ = valuesBuilder.Append('>');
        return valuesBuilder.ToString();
    }
}
