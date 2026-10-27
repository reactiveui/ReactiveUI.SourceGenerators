// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Plugins.Conversion;
using ReactiveUI.Binding.SourceGenerators.Plugins.SetMethod;
using ReactiveUI.Binding.SourceGenerators.Plugins.ViewThread;

namespace ReactiveUI.Binding.SourceGenerators.Helpers;

/// <summary>Extracts <see cref="BindToInvocationInfo"/> from <c>BindTo</c> invocations.</summary>
internal static class BindToExtractor
{
    /// <summary>The minimum number of arguments a BindTo invocation must have (target, property).</summary>
    private const int MinimumBindToArgumentCount = 2;

    /// <summary>Pipeline B transform: extracts <see cref="BindToInvocationInfo"/> from a <c>BindTo</c> invocation.</summary>
    /// <param name="context">The generator syntax context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="BindToInvocationInfo"/> POCO, or null if the invocation is not analyzable.</returns>
    internal static BindToInvocationInfo? ExtractBindToInvocation(CallSiteContext context, CancellationToken ct)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;

        var semanticModel = context.SemanticModel;
        var methodSymbol = ExtractorValidation.ExtractMethodSymbol(semanticModel.GetSymbolInfo(invocation, ct));
        if (methodSymbol is null
            || !ExtractorValidation.IsRecognizedExtensionClass(methodSymbol.ContainingType)
            || !CallSiteHosting.TryResolve(context, methodSymbol, ct, out var scope))
        {
            return null;
        }

        // Need at least 2 arguments: target, property.
        var args = invocation.ArgumentList.Arguments;
        if (!ExtractorValidation.HasMinimumArguments(args.Count, MinimumBindToArgumentCount))
        {
            return null;
        }

        // The source value type is the T in the receiver's IObservable<T>.
        var receiverType = semanticModel.GetTypeInfo(memberAccess.Expression, ct).Type;
        var sourceValueType = GetObservableValueType(receiverType);
        if (sourceValueType is null)
        {
            return null;
        }

        var targetPropertyArg = args[1].Expression;
        var targetPropertyPath = SyntaxHelpers.ExtractPropertyPathFromLambda(targetPropertyArg, semanticModel, scope, ct);
        var targetType = semanticModel.GetTypeInfo(args[0].Expression, ct).Type;
        var targetTypeName = scope.NameOf(targetType);

        // A target the model cannot name leaves nothing to declare a member against, generated or otherwise.
        if (targetTypeName is null)
        {
            return null;
        }

        // A selector the compiler cannot read names no path, so nothing is generated and the stub throws,
        // naming the Unsafe overload that resolves it.
        if (targetPropertyPath is not { Length: > 0 })
        {
            return null;
        }

        var leaf = targetPropertyPath[^1];
        var sourceValueTypeFullName = sourceValueType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        DetectConversionParameters(methodSymbol, out var hasConversionHint, out var hasConverterOverride);

        var targetValueType = ConversionPluginRegistry.SelectorType(targetPropertyArg, semanticModel, ct);
        var targetExpressionText = targetPropertyArg.ToString();

        return new(
            invocation.SyntaxTree.FilePath,
            SyntaxHelpers.CallerLineNumber(invocation, ct),
            sourceValueTypeFullName,
            targetTypeName,
            new(targetPropertyPath),
            leaf.PropertyTypeFullName,
            leaf.IsReferenceType,
            hasConversionHint,
            hasConverterOverride,
            targetExpressionText,
            InterceptableLocationReader.Read(semanticModel, invocation, ct),
            ViewThreadPluginRegistry.InvokerFor(targetType, semanticModel.Compilation),
            scope.Call)
        {
            Conversion = ConversionPluginRegistry.Select(sourceValueType, targetValueType, semanticModel.Compilation),
            SetMethod = SetMethodPluginRegistry.Select(sourceValueType, targetValueType),
        };
    }

    /// <summary>
    /// Returns the <c>T</c> of <c>System.IObservable&lt;T&gt;</c> for the given receiver type,
    /// checking the type itself and all implemented interfaces.
    /// </summary>
    /// <param name="receiver">The receiver type symbol.</param>
    /// <returns>The observable value type, or null if the receiver is not an observable.</returns>
    internal static ITypeSymbol? GetObservableValueType(ITypeSymbol? receiver)
    {
        if (receiver is INamedTypeSymbol direct && SymbolHelpers.IsIObservable(direct))
        {
            return direct.TypeArguments[0];
        }

        // A null receiver has no interfaces, so it returns null like one that implements none.
        foreach (var iface in receiver?.AllInterfaces ?? System.Collections.Immutable.ImmutableArray<INamedTypeSymbol>.Empty)
        {
            if (SymbolHelpers.IsIObservable(iface))
            {
                return iface.TypeArguments[0];
            }
        }

        return null;
    }

    /// <summary>
    /// Scans the method parameters to detect the presence of a <c>conversionHint</c> parameter
    /// and an <c>IBindingTypeConverter</c>-typed <c>converterOverride</c> parameter.
    /// </summary>
    /// <param name="methodSymbol">The resolved method symbol.</param>
    /// <param name="hasConversionHint">Set to true if a <c>conversionHint</c> parameter exists.</param>
    /// <param name="hasConverterOverride">Set to true if an <c>IBindingTypeConverter</c> converter override exists.</param>
    private static void DetectConversionParameters(
        IMethodSymbol methodSymbol,
        out bool hasConversionHint,
        out bool hasConverterOverride)
    {
        hasConversionHint = false;
        hasConverterOverride = false;

        foreach (var parameter in methodSymbol.Parameters)
        {
            if (parameter.Name == "conversionHint")
            {
                hasConversionHint = true;
            }
            else if (parameter.Name == "converterOverride" && parameter.Type.Name == "IBindingTypeConverter")
            {
                hasConverterOverride = true;
            }
        }
    }
}
