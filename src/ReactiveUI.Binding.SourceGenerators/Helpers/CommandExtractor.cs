// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Plugins;
using ReactiveUI.Binding.SourceGenerators.Plugins.CommandBinding;
using ReactiveUI.Binding.SourceGenerators.Plugins.ViewThread;

namespace ReactiveUI.Binding.SourceGenerators.Helpers;

/// <summary>Extracts <see cref="BindCommandInvocationInfo"/> values from <c>BindCommand</c> invocations and provides compile-time command property detection helpers.</summary>
internal static class CommandExtractor
{
    /// <summary>The minimum number of arguments a BindCommand invocation must have (view model, property name, control name).</summary>
    private const int MinimumBindCommandArgumentCount = 3;

    /// <summary>The argument index at which optional <c>withParameter</c> lambdas may start.</summary>
    private const int WithParameterSearchStartIndex = 3;

    /// <summary>Extracts generator input metadata from a recognized <c>BindCommand</c> invocation.</summary>
    /// <param name="context">
    /// The generator syntax context whose node is expected to be an invocation expression.
    /// </param>
    /// <param name="ct">The token used to cancel semantic model operations.</param>
    /// <returns>
    /// A populated <see cref="BindCommandInvocationInfo"/> when the invocation targets a recognized
    /// extension method and its command/control expressions can be analyzed; otherwise, <see langword="null"/>.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the cancellation token is triggered.</exception>
    internal static BindCommandInvocationInfo? ExtractBindCommandInvocation(
        CallSiteContext context,
        CancellationToken ct)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;

        var semanticModel = context.SemanticModel;
        var methodSymbol = ExtractorValidation.ExtractMethodSymbol(semanticModel.GetSymbolInfo(invocation, ct));

        // Verify this is our stub or generated method
        if (methodSymbol is null
            || !ExtractorValidation.IsRecognizedExtensionClass(methodSymbol.ContainingType)
            || !CallSiteHosting.TryResolve(context, methodSymbol, ct, out var scope))
        {
            return null;
        }

        var args = invocation.ArgumentList.Arguments;
        InvalidOperationExceptionHelper.EnsureMinimumArguments(args.Count, MinimumBindCommandArgumentCount);

        // The command path comes from the 2nd argument, the control path from the 3rd.
        var commandPropertyArg = args[1].Expression;
        var controlPropertyArg = args[2].Expression;
        var commandPropertyPath = SyntaxHelpers.ExtractPropertyPathFromLambda(commandPropertyArg, semanticModel, scope, ct);
        var controlPropertyPath = SyntaxHelpers.ExtractPropertyPathFromLambda(controlPropertyArg, semanticModel, scope, ct);
        if (commandPropertyPath is null || controlPropertyPath is null)
        {
            return null;
        }

        // Determine parameter overload (Expression vs IObservable withParameter). A parameter path the generator cannot
        // read would be dropped, so the call is left to fail the build instead.
        var parameterOverload = DetectParameterOverload(methodSymbol, args, semanticModel, scope, ct);
        if (parameterOverload is { HasExpressionParameter: true, ParameterPropertyPath: null }
            || ResolveBindCommandSides(memberAccess, args, semanticModel, scope, ct) is not { } sides)
        {
            return null;
        }

        var controlBinding = ResolveControlBinding(methodSymbol, args, controlPropertyArg, semanticModel, ct);

        return new(
            invocation.SyntaxTree.FilePath,
            SyntaxHelpers.CallerLineNumber(invocation, ct),
            sides.ViewTypeFullName,
            sides.ViewModelTypeFullName,
            new(commandPropertyPath),
            new(controlPropertyPath),
            commandPropertyPath[^1].PropertyTypeFullName,
            controlPropertyPath[^1].PropertyTypeFullName,
            parameterOverload.HasObservableParameter,
            parameterOverload.HasExpressionParameter,
            parameterOverload.ParameterTypeFullName,
            parameterOverload.ParameterIsReferenceType,
            parameterOverload.ParameterPropertyPath,
            controlBinding.EventName,
            controlBinding.EventArgsTypeFullName,
            Constants.BindCommandMethodName,
            commandPropertyArg.ToString(),
            controlPropertyArg.ToString(),
            parameterOverload.ParameterExpressionText,
            controlBinding.Capabilities.HasCommand,
            controlBinding.Capabilities.HasCommandParameter,
            controlBinding.Capabilities.HasEnabled,
            InterceptableLocationReader.Read(semanticModel, invocation, ct),
            sides.ViewThreadInvoker,
            scope.Call)
        { HasExplicitEvent = controlBinding.HasExplicitEvent, NativeCommand = controlBinding.NativeCommand, ControlViewThreadInvoker = controlBinding.ViewThreadInvoker };
    }

    /// <summary>Searches invocation arguments for a valid <c>withParameter</c> lambda expression.</summary>
    /// <param name="args">The argument list from the invocation.</param>
    /// <param name="semanticModel">The semantic model used to resolve lambda property paths.</param>
    /// <param name="ct">The token used to cancel semantic model operations.</param>
    /// <returns>
    /// The parameter property path, leaf type name, and normalized expression text when a supported
    /// lambda is found; otherwise, <see langword="null"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ParameterLambda? FindParameterLambda(
            SeparatedSyntaxList<ArgumentSyntax> args,
            SemanticModel semanticModel,
            CancellationToken ct) =>
        FindParameterLambda(args, semanticModel, ReachScope.Assembly(semanticModel.Compilation), ct);

    /// <summary>Searches invocation arguments for a valid <c>withParameter</c> lambda expression.</summary>
    /// <param name="args">The argument list from the invocation.</param>
    /// <param name="semanticModel">The semantic model used to resolve lambda property paths.</param>
    /// <param name="scope">Where the generated code that reads the path is declared.</param>
    /// <param name="ct">The token used to cancel semantic model operations.</param>
    /// <returns>
    /// The parameter property path, leaf type name, and normalized expression text when a supported
    /// lambda is found; otherwise, <see langword="null"/>.
    /// </returns>
    internal static ParameterLambda? FindParameterLambda(
            SeparatedSyntaxList<ArgumentSyntax> args,
            SemanticModel semanticModel,
            ReachScope scope,
            CancellationToken ct)
    {
        for (var argumentIndex = WithParameterSearchStartIndex; argumentIndex < args.Count; argumentIndex++)
        {
            var paramPath = SyntaxHelpers.ExtractPropertyPathFromLambda(args[argumentIndex].Expression, semanticModel, scope, ct);
            if (paramPath is not null)
            {
                return new ParameterLambda(
                    paramPath,
                    paramPath[^1].PropertyTypeFullName,
                    args[argumentIndex].Expression.ToString());
            }
        }

        return null;
    }

    /// <summary>
    /// Determines whether an argument matches the "toEvent" parameter by either
    /// named argument syntax (<c>toEvent: "Click"</c>) or positional match.
    /// </summary>
    /// <param name="argument">The argument syntax to check.</param>
    /// <param name="argumentIndex">The index of this argument in the argument list.</param>
    /// <param name="parameterIndex">The index of the <c>toEvent</c> parameter in the method signature.</param>
    /// <returns><c>true</c> if the argument is the toEvent argument.</returns>
    internal static bool IsToEventArgument(
        ArgumentSyntax argument,
        int argumentIndex,
        int parameterIndex)
    {
        var nameColon = argument.NameColon;
        return nameColon is not null ? nameColon.Name.Identifier.Text == "toEvent" : argumentIndex == parameterIndex;
    }

    /// <summary>Resolves an explicit <c>toEvent</c> event name from the invocation arguments, if present.</summary>
    /// <param name="methodSymbol">The resolved method symbol.</param>
    /// <param name="args">The invocation argument list.</param>
    /// <param name="semanticModel">The semantic model.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The explicit event name, or null when none is supplied.</returns>
    internal static string? ResolveExplicitEventName(
        IMethodSymbol methodSymbol,
        SeparatedSyntaxList<ArgumentSyntax> args,
        SemanticModel semanticModel,
        CancellationToken ct)
    {
        for (var i = 0; i < methodSymbol.Parameters.Length; i++)
        {
            if (methodSymbol.Parameters[i].Name != "toEvent")
            {
                continue;
            }

            // Find matching argument
            for (var argumentIndex = 0; argumentIndex < args.Count; argumentIndex++)
            {
                if (!IsToEventArgument(args[argumentIndex], argumentIndex, i))
                {
                    continue;
                }

                var constant = semanticModel.GetConstantValue(args[argumentIndex].Expression, ct);
                return constant is { HasValue: true, Value: string eventName } && !string.IsNullOrEmpty(eventName)
                    ? eventName
                    : null;
            }

            return null;
        }

        return null;
    }

    /// <summary>Resolves the view and view model types the invocation binds between.</summary>
    /// <param name="memberAccess">The member access the invocation hangs off.</param>
    /// <param name="args">The invocation arguments.</param>
    /// <param name="semanticModel">The semantic model.</param>
    /// <param name="scope">Where the generated code is declared, which decides the types it can name.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The fully qualified view and view model type names, or <see langword="null"/> when either names a type
    /// a generated overload could not declare.
    /// </returns>
    internal static BindCommandSides? ResolveBindCommandSides(
        MemberAccessExpressionSyntax memberAccess,
        SeparatedSyntaxList<ArgumentSyntax> args,
        SemanticModel semanticModel,
        ReachScope scope,
        CancellationToken ct)
    {
        var viewType = semanticModel.GetTypeInfo(memberAccess.Expression, ct).Type;
        var viewTypeFullName = scope.NameOf(viewType);
        var viewModelTypeFullName = scope.NameOf(semanticModel.GetTypeInfo(args[0].Expression, ct).Type);

        return viewTypeFullName is null || viewModelTypeFullName is null
            ? null
            : new BindCommandSides(
                viewTypeFullName,
                viewModelTypeFullName,
                ViewThreadPluginRegistry.InvokerFor(viewType, semanticModel.Compilation));
    }

    /// <summary>
    /// Resolves the event the command binds to and what the control supports, from the explicit
    /// <c>toEvent</c> argument when present and the control's own members otherwise.
    /// </summary>
    /// <param name="methodSymbol">The resolved BindCommand method.</param>
    /// <param name="args">The invocation arguments.</param>
    /// <param name="controlPropertyArg">The control selector expression.</param>
    /// <param name="semanticModel">The semantic model.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The event name, its argument type, and the control's binding capabilities.</returns>
    internal static ControlBinding ResolveControlBinding(
        IMethodSymbol methodSymbol,
        SeparatedSyntaxList<ArgumentSyntax> args,
        ExpressionSyntax controlPropertyArg,
        SemanticModel semanticModel,
        CancellationToken ct)
    {
        var resolvedEventName = ResolveExplicitEventName(methodSymbol, args, semanticModel, ct);
        var explicitEvent = !string.IsNullOrEmpty(resolvedEventName);

        var controlLeafType = SymbolHelpers.ResolveNamedType(semanticModel, controlPropertyArg, ct);

        var resolvedEventArgsTypeFullName = ResolveEventArgsTypeFullName(controlLeafType, ref resolvedEventName);

        return new(resolvedEventName, resolvedEventArgsTypeFullName, DetectControlCapabilities(controlLeafType))
        {
            HasExplicitEvent = explicitEvent,
            NativeCommand = explicitEvent ? null : RuntimeSupported(CommandBindingPluginRegistry.InspectControl(controlLeafType), semanticModel.Compilation),
            ViewThreadInvoker = ViewThreadPluginRegistry.InvokerFor(controlLeafType, semanticModel.Compilation),
        };
    }

    /// <summary>Drops a native route whose runtime support the consumer does not reference.</summary>
    /// <param name="native">The native route the control offers, or null.</param>
    /// <param name="compilation">The consumer compilation.</param>
    /// <returns>The route, or null when generated code would name a runtime type the compilation cannot see.</returns>
    /// <remarks>
    /// AppKit target/action installs the runtime's <c>AppKitCommandTarget</c>, which only the macOS head of the
    /// runtime declares. Without it the binding takes the event or property route instead.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static NativeCommandInfo? RuntimeSupported(NativeCommandInfo? native, Compilation compilation) =>
        native is { Kind: NativeCommandKind.AppKitTargetAction }
        && !SymbolHelpers.ResolvesEither(compilation, GeneratedTypeNames.AppKitCommandTargetMetadataName, GeneratedTypeNames.ReactiveAppKitCommandTargetMetadataName)
            ? null
            : native;

    /// <summary>
    /// Inspects the method's <c>withParameter</c> parameter (if any) to determine whether the
    /// overload takes an <c>Expression</c> or <c>IObservable</c> parameter, and extracts the
    /// associated parameter property path / type / expression text.
    /// </summary>
    /// <param name="methodSymbol">The resolved method symbol.</param>
    /// <param name="args">The invocation argument list.</param>
    /// <param name="semanticModel">The semantic model.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The detected parameter overload information.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ParameterOverloadInfo DetectParameterOverload(
        IMethodSymbol methodSymbol,
        SeparatedSyntaxList<ArgumentSyntax> args,
        SemanticModel semanticModel,
        CancellationToken ct) =>
        DetectParameterOverload(methodSymbol, args, semanticModel, ReachScope.Assembly(semanticModel.Compilation), ct);

    /// <summary>Detects whether the method has a <c>withParameter</c> overload, reading its path in the given scope.</summary>
    /// <param name="methodSymbol">The resolved method symbol.</param>
    /// <param name="args">The invocation argument list.</param>
    /// <param name="semanticModel">The semantic model.</param>
    /// <param name="scope">Where the generated code that reads the path is declared.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The detected parameter overload information.</returns>
    internal static ParameterOverloadInfo DetectParameterOverload(
        IMethodSymbol methodSymbol,
        SeparatedSyntaxList<ArgumentSyntax> args,
        SemanticModel semanticModel,
        ReachScope scope,
        CancellationToken ct)
    {
        var result = new ParameterOverloadInfo();

        for (var i = 0; i < methodSymbol.Parameters.Length; i++)
        {
            var param = methodSymbol.Parameters[i];
            if (param.Name != "withParameter")
            {
                continue;
            }

            if (param.Type is INamedTypeSymbol { Name: "Expression" })
            {
                result.HasExpressionParameter = true;

                // Find the withParameter argument
                var paramResult = FindParameterLambda(args, semanticModel, scope, ct);
                if (paramResult is not null)
                {
                    result.ParameterPropertyPath = new EquatableArray<PropertyPathSegment>(paramResult.Value.PropertyPath);
                    result.ParameterTypeFullName = paramResult.Value.TypeFullName;
                    result.ParameterExpressionText = paramResult.Value.ExpressionText;

                    // The parameter selector's leaf type determines whether the generated withParameter
                    // expression is annotated nullable; without this the lambda over a nullable property raises CS8603.
                    // The lambda is only reported once its leaf has been read, so the path always has one.
                    result.ParameterIsReferenceType = paramResult.Value.PropertyPath[^1].IsReferenceType;
                }
            }
            else if (param.Type is INamedTypeSymbol observableType && SymbolHelpers.IsIObservable(observableType))
            {
                result.HasObservableParameter = true;
                result.ParameterTypeFullName = observableType.TypeArguments[0]
                    .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                result.ParameterIsReferenceType = observableType.TypeArguments[0].IsReferenceType;
            }
        }

        return result;
    }

    /// <summary>
    /// Resolves the event args type for the control. When no explicit event name was supplied,
    /// resolves the control's default event (updating <paramref name="resolvedEventName"/>);
    /// otherwise resolves the event args type for the explicit event name.
    /// </summary>
    /// <param name="controlLeafType">The resolved control leaf type, or null.</param>
    /// <param name="resolvedEventName">The explicit event name (updated to the default event when null).</param>
    /// <returns>The fully qualified event args type name, or null.</returns>
    internal static string? ResolveEventArgsTypeFullName(
        INamedTypeSymbol? controlLeafType,
        ref string? resolvedEventName)
    {
        if (controlLeafType is null)
        {
            return null;
        }

        if (resolvedEventName is null)
        {
            // No explicit toEvent: resolve the control's default event and its args type.
            resolvedEventName = EventHelpers.FindDefaultEvent(controlLeafType, out var defaultEventArgs);
            return defaultEventArgs;
        }

        // Resolve the event args type for the explicit event name.
        return EventHelpers.FindEventArgsType(controlLeafType, resolvedEventName);
    }

    /// <summary>Detects command/command-parameter/enabled property capabilities on the control leaf type.</summary>
    /// <param name="controlLeafType">The resolved control leaf type, or null.</param>
    /// <returns>The detected control capabilities (all false when <paramref name="controlLeafType"/> is null).</returns>
    internal static ControlCapabilities DetectControlCapabilities(INamedTypeSymbol? controlLeafType)
    {
        if (controlLeafType is null)
        {
            return default;
        }

        var hasCommandProperty = CommandPropertyBindingPlugin.HasCommandProperties(controlLeafType, out var hasCommandParameterProperty);
        var hasEnabledProperty = EventEnabledBindingPlugin.HasEnabledProperty(controlLeafType);
        return new(hasCommandProperty, hasCommandParameterProperty, hasEnabledProperty);
    }

    /// <summary>Command-related property capabilities detected on a control type.</summary>
    /// <param name="HasCommand">Whether the control exposes a settable <c>ICommand</c> property.</param>
    /// <param name="HasCommandParameter">Whether the control exposes a settable command-parameter property.</param>
    /// <param name="HasEnabled">Whether the control exposes a settable enabled property.</param>
    internal readonly record struct ControlCapabilities(
        bool HasCommand,
        bool HasCommandParameter,
        bool HasEnabled);

    /// <summary>How a control surfaces the interaction a command binds to.</summary>
    /// <param name="EventName">The event the binding subscribes to, or null when the control exposes none.</param>
    /// <param name="EventArgsTypeFullName">The fully qualified argument type of that event.</param>
    /// <param name="Capabilities">What the control can do for a command binding.</param>
    internal readonly record struct ControlBinding(
        string? EventName,
        string? EventArgsTypeFullName,
        ControlCapabilities Capabilities)
    {
        /// <summary>Gets a value indicating whether the caller selected an event.</summary>
        public bool HasExplicitEvent { get; init; }

        /// <summary>Gets the verified native command members.</summary>
        public NativeCommandInfo? NativeCommand { get; init; }

        /// <summary>Gets the invoker class a write to the control carries, or null for none.</summary>
        public string? ViewThreadInvoker { get; init; }
    }

    /// <summary>Holds the detected <c>withParameter</c> overload information for a BindCommand invocation.</summary>
    internal sealed class ParameterOverloadInfo
    {
        /// <summary>Gets or sets a value indicating whether the overload has an IObservable withParameter.</summary>
        public bool HasObservableParameter { get; set; }

        /// <summary>Gets or sets a value indicating whether the overload has an Expression withParameter.</summary>
        public bool HasExpressionParameter { get; set; }

        /// <summary>Gets or sets the fully qualified parameter type name, or null.</summary>
        public string? ParameterTypeFullName { get; set; }

        /// <summary>Gets or sets a value indicating whether the detected parameter type is a reference type.</summary>
        public bool ParameterIsReferenceType { get; set; }

        /// <summary>Gets or sets the parameter property path, or null.</summary>
        public EquatableArray<PropertyPathSegment>? ParameterPropertyPath { get; set; }

        /// <summary>Gets or sets the normalized parameter expression text, or null.</summary>
        public string? ParameterExpressionText { get; set; }
    }
}
