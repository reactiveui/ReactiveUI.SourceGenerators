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

/// <summary>Extracts BindingInvocationInfo from BindOneWay, BindTwoWay, OneWayBind, and Bind invocations.</summary>
internal static class BindingExtractor
{
    /// <summary>
    /// The minimum number of arguments a binding invocation must have
    /// (source/view, target/view model, source property, target property).
    /// </summary>
    private const int MinimumBindingArgumentCount = 3;

    /// <summary>Pipeline B transform: extracts BindingInvocationInfo from a BindOneWay/BindTwoWay/OneWayBind/Bind invocation.</summary>
    /// <param name="context">The generator syntax context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A BindingInvocationInfo POCO, or null if the invocation is not analyzable.</returns>
    /// <exception cref="OperationCanceledException">If the cancellation token is triggered.</exception>
    internal static BindingInvocationInfo? ExtractBindInvocation(CallSiteContext context, CancellationToken ct)
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

        var methodName = memberAccess.Name.Identifier.Text;
        var isTwoWay = methodName is Constants.BindTwoWayMethodName or Constants.BindMethodName;

        // Need at least 3 arguments: source/view (this), target/viewModel, sourceProp/vmProp, targetProp/viewProp
        var args = invocation.ArgumentList.Arguments;
        InvalidOperationExceptionHelper.EnsureMinimumArguments(args.Count, MinimumBindingArgumentCount);

        // Extract property paths
        var sourcePropertyArg = args[1].Expression;
        var targetPropertyArg = args[2].Expression;

        var sourcePropertyPath = SyntaxHelpers.ExtractPropertyPathFromLambda(sourcePropertyArg, semanticModel, scope, ct);
        var targetPropertyPath = SyntaxHelpers.ExtractPropertyPathFromLambda(targetPropertyArg, semanticModel, scope, ct);

        if (sourcePropertyPath is null || targetPropertyPath is null)
        {
            return null;
        }

        if (ResolveBindingSides(memberAccess, args, methodName, semanticModel, scope, ct) is not { } sides)
        {
            return null;
        }

        DetectBindingParameters(methodSymbol, out var hasConversion, out var hasScheduler, out var hasConverterOverride);

        var sourceValueType = ConversionPluginRegistry.SelectorType(sourcePropertyArg, semanticModel, ct);
        var targetValueType = ConversionPluginRegistry.SelectorType(targetPropertyArg, semanticModel, ct);

        return new(
            invocation.SyntaxTree.FilePath,
            SyntaxHelpers.CallerLineNumber(invocation, ct),
            sides.SourceTypeFullName,
            new(sourcePropertyPath),
            sides.TargetTypeFullName,
            new(targetPropertyPath),
            sourcePropertyPath[^1].PropertyTypeFullName,
            targetPropertyPath[^1].PropertyTypeFullName,
            hasConversion,
            hasScheduler,
            isTwoWay,
            methodName,
            sourcePropertyArg.ToString(),
            targetPropertyArg.ToString(),
            hasConverterOverride,
            InterceptableLocationReader.Read(semanticModel, invocation, ct),
            sides.SourceViewThreadInvoker,
            sides.TargetViewThreadInvoker,
            scope.Call)
        {
            ForwardConversion = ConversionPluginRegistry.Select(sourceValueType, targetValueType, semanticModel.Compilation),
            ReverseConversion = isTwoWay ? ConversionPluginRegistry.Select(targetValueType, sourceValueType, semanticModel.Compilation) : null,
            SetMethod = SelectSetMethod(isTwoWay, hasConversion, hasConverterOverride, sourceValueType, targetValueType),
        };
    }

    /// <summary>Selects the native mutation a one-way binding writes through, when nothing converts the value on the way.</summary>
    /// <param name="isTwoWay">Whether the binding drives both sides.</param>
    /// <param name="hasConversion">Whether the call site passes a delegate that converts the value.</param>
    /// <param name="hasConverterOverride">Whether the call site passes a converter object.</param>
    /// <param name="sourceValueType">The type the source produces.</param>
    /// <param name="targetValueType">The type the target holds.</param>
    /// <returns>The mutation, or <see langword="null"/> when the value is assigned.</returns>
    private static SetMethodInfo? SelectSetMethod(
        bool isTwoWay,
        bool hasConversion,
        bool hasConverterOverride,
        ITypeSymbol? sourceValueType,
        ITypeSymbol? targetValueType) =>
        isTwoWay || hasConversion || hasConverterOverride ? null : SetMethodPluginRegistry.Select(sourceValueType, targetValueType);

    /// <summary>
    /// Resolves which side of the binding is the source and which is the target. The view-first
    /// overloads take the view as the receiver, so the roles are swapped relative to the others.
    /// </summary>
    /// <param name="memberAccess">The member access the invocation hangs off.</param>
    /// <param name="args">The invocation arguments.</param>
    /// <param name="methodName">The invoked method name.</param>
    /// <param name="semanticModel">The semantic model.</param>
    /// <param name="scope">Where the generated code is declared, which decides the types it can name.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The fully qualified source and target type names, or <see langword="null"/> when either side names a
    /// type a generated overload could not declare.
    /// </returns>
    private static BindingSides? ResolveBindingSides(
        MemberAccessExpressionSyntax memberAccess,
        SeparatedSyntaxList<ArgumentSyntax> args,
        string methodName,
        SemanticModel semanticModel,
        ReachScope scope,
        CancellationToken ct)
    {
        var receiverType = semanticModel.GetTypeInfo(memberAccess.Expression, ct).Type;
        var firstArgType = semanticModel.GetTypeInfo(args[0].Expression, ct).Type;
        var receiverTypeName = scope.NameOf(receiverType);
        var firstArgTypeName = scope.NameOf(firstArgType);

        if (receiverTypeName is null || firstArgTypeName is null)
        {
            return null;
        }

        var receiverInvoker = ViewThreadPluginRegistry.InvokerFor(receiverType, semanticModel.Compilation);
        var firstArgInvoker = ViewThreadPluginRegistry.InvokerFor(firstArgType, semanticModel.Compilation);

        var isViewFirst = methodName is Constants.OneWayBindMethodName or Constants.BindMethodName;
        return isViewFirst
            ? new BindingSides(firstArgTypeName, receiverTypeName, firstArgInvoker, receiverInvoker)
            : new BindingSides(receiverTypeName, firstArgTypeName, receiverInvoker, firstArgInvoker);
    }

    /// <summary>
    /// Scans the method parameters to detect conversion, scheduler, and converter-override
    /// capabilities of the binding overload.
    /// </summary>
    /// <param name="methodSymbol">The resolved method symbol.</param>
    /// <param name="hasConversion">Set to true if a conversion/selector parameter exists.</param>
    /// <param name="hasScheduler">Set to true if a <c>scheduler</c> parameter exists.</param>
    /// <param name="hasConverterOverride">Set to true if an <c>IBindingTypeConverter</c> converter override exists.</param>
    private static void DetectBindingParameters(
        IMethodSymbol methodSymbol,
        out bool hasConversion,
        out bool hasScheduler,
        out bool hasConverterOverride)
    {
        hasConversion = false;
        hasScheduler = false;
        hasConverterOverride = false;

        foreach (var parameter in methodSymbol.Parameters)
        {
            // A converter object shares a name with the delegate the same parameter takes elsewhere, so the type
            // decides which one this is.
            if (SymbolHelpers.DetectHasConverterOverride(parameter))
            {
                hasConverterOverride = true;
            }
            else if (parameter.Name is "conversionFunc" or "sourceToTargetConv" or "selector" or "vmToViewConverter" or "viewModelToViewConverter")
            {
                hasConversion = true;
            }

            if (parameter.Name == "scheduler")
            {
                hasScheduler = true;
            }
        }
    }
}
