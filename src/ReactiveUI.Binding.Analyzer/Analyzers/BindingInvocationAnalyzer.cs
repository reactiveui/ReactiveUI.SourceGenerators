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
using ReactiveUI.Binding.SourceGenerators.Helpers;
using ReactiveUI.Binding.SourceGenerators.Plugins.CommandBinding;

namespace ReactiveUI.Binding.Analyzer.Analyzers;

/// <summary>
/// Analyzes binding and observation invocations (WhenChanged, WhenChanging, BindOneWay, BindTwoWay,
/// OneWayBind, Bind, BindTo, BindCommand, BindInteraction) for common issues. The generic lambda checks
/// (RXUIBIND001/003/006/010) apply to generated binding entry points; the remaining checks are
/// method-specific (RXUIBIND004, RXUIBIND005, RXUIBIND007, RXUIBIND008).
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BindingInvocationAnalyzer : DiagnosticAnalyzer
{
    /// <summary>Runtime entry points accept expressions that the generator cannot inspect.</summary>
    private const string UnsafeMethodSuffix = "Unsafe";

    /// <summary>The parameter type a property path arrives as, which marks it out from the other arguments.</summary>
    private const string ExpressionTypeName = "System.Linq.Expressions.Expression`1";

    /// <summary>The diagnostics this analyzer reports.</summary>
    private static readonly ImmutableArray<DiagnosticDescriptor> ReportedDiagnostics =
        new[]
        {
            DiagnosticWarnings.NonInlineLambda,
            DiagnosticWarnings.PrivateMember,
            DiagnosticWarnings.NoBeforeChangeSupport,
            DiagnosticWarnings.ValidationNotGenerated,
            DiagnosticWarnings.UnsupportedPathSegment,
            DiagnosticWarnings.NoBindableEvent,
            DiagnosticWarnings.InvalidInteractionType,
            DiagnosticWarnings.SilentPathLink,
        }.ToImmutableArray();

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

    /// <summary>Analyzes a method invocation operation for binding-related diagnostics.</summary>
    /// <param name="context">The operation analysis context.</param>
    internal static void AnalyzeInvocation(in OperationAnalysisContext context)
    {
        var invocationOp = (IInvocationOperation)context.Operation;

        var methodSymbol = invocationOp.TargetMethod;

        // ToProperty only reads a name from its selector, so the path checks do not apply; ToPropertyAnalyzer covers it.
        if (!ExtractorValidation.IsRecognizedExtensionClass(methodSymbol.ContainingType)
            || methodSymbol.Name.EndsWith(UnsafeMethodSuffix, StringComparison.Ordinal)
            || methodSymbol.Name == Constants.ToPropertyMethodName)
        {
            return;
        }

        var methodName = methodSymbol.Name;
        var arguments = invocationOp.Arguments;

        // RXUIBIND001, 003 and 006 name a path the generator cannot read, so the call throws. A call an interceptor
        // claims was read after all, from the caller's own partial class, and has nothing to report.
        if (!AnalyzerHelpers.IsIntercepted(invocationOp, context.CancellationToken))
        {
            CheckNonInlineLambda(context, arguments);
            CheckPrivateMember(context, arguments);
            CheckUnsupportedPathSegment(context, arguments);
        }

        // Check RXUIBIND010: A link in the middle of the path that raises no notification
        CheckSilentPathLink(context, arguments);

        // Check RXUIBIND004: Before-change support
        if (methodName == Constants.WhenChangingMethodName)
        {
            CheckBeforeChangeSupport(context, invocationOp, methodSymbol);
        }

        // Check RXUIBIND005: Validation support
        if (methodName is Constants.BindOneWayMethodName or Constants.BindTwoWayMethodName)
        {
            CheckValidationSupport(context, invocationOp);
        }

        // Check RXUIBIND008: Invalid interaction type
        if (methodName == Constants.BindInteractionMethodName)
        {
            CheckInteractionType(context, invocationOp);
        }

        // Check RXUIBIND007: No bindable event on control
        if (methodName != Constants.BindCommandMethodName)
        {
            return;
        }

        CheckBindableEvent(context, invocationOp);
    }

    /// <summary>Checks for RXUIBIND001: Expression arguments that are not inline lambdas.</summary>
    /// <param name="context">The operation analysis context.</param>
    /// <param name="arguments">The invocation arguments to inspect.</param>
    internal static void CheckNonInlineLambda(
        in OperationAnalysisContext context,
        ImmutableArray<IArgumentOperation> arguments)
    {
        // Find the Expression<Func<...>> arguments
        for (var i = 0; i < arguments.Length; i++)
        {
            var arg = arguments[i];
            if (arg.Value.Syntax is not ExpressionSyntax expr)
            {
                continue;
            }

            // Check if this is an Expression<Func<...>> parameter
            var parameterType = arg.Parameter!.Type;

            if (!NativeTypeIdentity.Matches(parameterType, ExpressionTypeName))
            {
                continue;
            }

            // Must be an inline lambda
            if (!AnalyzerHelpers.IsInlineLambda(expr))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(DiagnosticWarnings.NonInlineLambda, expr.GetLocation()));
            }
        }
    }

    /// <summary>Checks for RXUIBIND003: Lambda expressions that access private or protected members.</summary>
    /// <param name="context">The operation analysis context.</param>
    /// <param name="arguments">The invocation arguments to inspect.</param>
    internal static void CheckPrivateMember(
        in OperationAnalysisContext context,
        ImmutableArray<IArgumentOperation> arguments)
    {
        for (var i = 0; i < arguments.Length; i++)
        {
            var arg = arguments[i];
            if (arg.Value.Syntax is not LambdaExpressionSyntax lambda)
            {
                continue;
            }

            // Walk the lambda body looking for member accesses
            var body = GetLambdaBody(lambda);
            if (body is null)
            {
                continue;
            }

            var current = AnalyzerHelpers.SkipNullForgivingAndParentheses(body);
            while (current is MemberAccessExpressionSyntax memberAccess)
            {
                var memberSymbol = context.Operation.SemanticModel!.GetSymbolInfo(memberAccess, context.CancellationToken).Symbol;
                if (memberSymbol is { DeclaredAccessibility: Accessibility.Private or Accessibility.Protected })
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticWarnings.PrivateMember,
                            memberAccess.Name.GetLocation(),
                            memberSymbol.Name));
                    break;
                }

                current = AnalyzerHelpers.SkipNullForgivingAndParentheses(memberAccess.Expression);
            }
        }
    }

    /// <summary>Checks for RXUIBIND004: WhenChanging invocations on types that do not support before-change notifications.</summary>
    /// <param name="context">The operation analysis context.</param>
    /// <param name="invocationOp">The invocation operation being analyzed.</param>
    /// <param name="methodSymbol">The method symbol of the invocation target.</param>
    internal static void CheckBeforeChangeSupport(
        in OperationAnalysisContext context,
        IInvocationOperation invocationOp,
        IMethodSymbol methodSymbol)
    {
        if (!AnalyzerHelpers.LacksBeforeChangeSupport(
            methodSymbol,
            context.Compilation,
            out var receiverType,
            out var mechanism))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticWarnings.NoBeforeChangeSupport,
                invocationOp.Syntax.GetLocation(),
                receiverType!.Name,
                mechanism));
    }

    /// <summary>
    /// Checks for RXUIBIND005: Binding invocations where the source type implements
    /// <see cref="System.ComponentModel.INotifyDataErrorInfo"/>, which requires the runtime engine for validation binding.
    /// </summary>
    /// <param name="context">The operation analysis context.</param>
    /// <param name="invocationOp">The invocation operation being analyzed.</param>
    internal static void CheckValidationSupport(
        in OperationAnalysisContext context,
        IInvocationOperation invocationOp)
    {
        if (!AnalyzerHelpers.ImplementsDataErrorInfo(
                invocationOp.TargetMethod,
                context.Compilation,
                out var sourceType))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticWarnings.ValidationNotGenerated,
                invocationOp.Syntax.GetLocation(),
                sourceType!.Name));
    }

    /// <summary>
    /// Checks for RXUIBIND008: BindInteraction invocations where the property type does not implement
    /// the <c>IInteraction&lt;TInput, TOutput&gt;</c> interface.
    /// </summary>
    /// <param name="context">The operation analysis context.</param>
    /// <param name="invocationOp">The invocation operation being analyzed.</param>
    internal static void CheckInteractionType(
        in OperationAnalysisContext context,
        IInvocationOperation invocationOp)
    {
        // Find the propertyName argument (the Expression<Func<TViewModel, IInteraction<...>>> parameter)
        var arguments = invocationOp.Arguments;
        for (var i = 0; i < arguments.Length; i++)
        {
            var arg = arguments[i];
            if (arg.Parameter!.Name != "propertyName")
            {
                continue;
            }

            if (arg.Value.Syntax is not LambdaExpressionSyntax lambda)
            {
                break;
            }

            // Get the lambda body return type
            var body = GetLambdaBody(lambda);

            if (body is null)
            {
                break;
            }

            var typeInfo = context.Operation.SemanticModel!.GetTypeInfo(body, context.CancellationToken);
            if (typeInfo.Type is null)
            {
                break;
            }

            var interactionType = context.Compilation.GetTypeByMetadataName(Constants.IInteractionMetadataName);
            if (interactionType is null)
            {
                break;
            }

            // Check if the type implements IInteraction<,>
            var propertyType = typeInfo.Type;
            if (!ImplementsOpenGenericInterface(propertyType, interactionType))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticWarnings.InvalidInteractionType,
                        body.GetLocation(),
                        propertyType.ToDisplayString()));
            }

            break;
        }
    }

    /// <summary>
    /// Checks for RXUIBIND007: BindCommand invocations where the target control type
    /// does not expose a default bindable event (Click, TouchUpInside, MouseUp, or Pressed).
    /// </summary>
    /// <param name="context">The operation analysis context.</param>
    /// <param name="invocationOp">The invocation operation being analyzed.</param>
    internal static void CheckBindableEvent(
        in OperationAnalysisContext context,
        IInvocationOperation invocationOp)
    {
        _ = invocationOp.TargetMethod;
        var arguments = invocationOp.Arguments;

        // If toEvent is explicitly specified, skip this check
        if (IsToEventSpecified(arguments))
        {
            return;
        }

        // Find the controlName argument to get the control type
        for (var i = 0; i < arguments.Length; i++)
        {
            var arg = arguments[i];
            if (arg.Parameter!.Name != "controlName")
            {
                continue;
            }

            if (arg.Value.Syntax is not LambdaExpressionSyntax lambda)
            {
                break;
            }

            var body = GetLambdaBody(lambda);
            if (body is null)
            {
                break;
            }

            var typeInfo = context.Operation.SemanticModel!.GetTypeInfo(body, context.CancellationToken);
            if (typeInfo.Type is not INamedTypeSymbol controlType)
            {
                break;
            }

            if (!HasDefaultBindableEvent(controlType))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticWarnings.NoBindableEvent,
                        body.GetLocation(),
                        controlType.Name));
            }

            break;
        }
    }

    /// <summary>Determines whether a type implements a specific open generic interface.</summary>
    /// <param name="type">The type symbol to check.</param>
    /// <param name="openGenericInterface">The open generic interface to look for (e.g., <c>IInteraction&lt;,&gt;</c>).</param>
    /// <returns><c>true</c> if the type implements the specified open generic interface; otherwise, <c>false</c>.</returns>
    internal static bool ImplementsOpenGenericInterface(ITypeSymbol type, INamedTypeSymbol openGenericInterface)
    {
        if (type is INamedTypeSymbol { IsGenericType: true } named
            && SymbolEqualityComparer.Default.Equals(named.ConstructedFrom, openGenericInterface))
        {
            return true;
        }

        var allInterfaces = type.AllInterfaces;
        for (var i = 0; i < allInterfaces.Length; i++)
        {
            if (allInterfaces[i].IsGenericType
                && SymbolEqualityComparer.Default.Equals(allInterfaces[i].ConstructedFrom, openGenericInterface))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks for RXUIBIND006: Lambda expressions containing unsupported path segments
    /// such as indexers, field accesses, or method calls.
    /// </summary>
    /// <param name="context">The operation analysis context.</param>
    /// <param name="arguments">The invocation arguments to inspect.</param>
    internal static void CheckUnsupportedPathSegment(
        in OperationAnalysisContext context,
        ImmutableArray<IArgumentOperation> arguments)
    {
        for (var i = 0; i < arguments.Length; i++)
        {
            var arg = arguments[i];

            // Only check Expression<Func<...>> parameters
            var parameterType = arg.Parameter!.Type;

            if (!NativeTypeIdentity.Matches(parameterType, ExpressionTypeName))
            {
                continue;
            }

            if (arg.Value.Syntax is not LambdaExpressionSyntax lambda)
            {
                continue;
            }

            var body = GetLambdaBody(lambda);

            if (body is null)
            {
                continue;
            }

            WalkForUnsupportedSegments(context, body);
        }
    }

    /// <summary>Checks for RXUIBIND010: an observed path passing through a type that raises no notification.</summary>
    /// <param name="context">The operation analysis context.</param>
    /// <param name="arguments">The invocation arguments to inspect.</param>
    /// <remarks>
    /// Only links past the first are reported. The type the call site is made on is already answered for by the
    /// warning on the type itself, whereas a type reached part way along a path is never named at the call site
    /// and is where an observation silently stops following.
    /// </remarks>
    internal static void CheckSilentPathLink(
        in OperationAnalysisContext context,
        ImmutableArray<IArgumentOperation> arguments)
    {
        for (var i = 0; i < arguments.Length; i++)
        {
            if (!IsExpressionArgument(arguments[i], out var lambda))
            {
                continue;
            }

            var body = GetLambdaBody(lambda!);
            if (body is null)
            {
                continue;
            }

            ReportSilentLinks(context, body);
        }
    }

    /// <summary>Walks a member access chain looking for unsupported path segments (method calls, indexers, static fields, or a read-only field at the leaf).</summary>
    /// <param name="context">The operation analysis context.</param>
    /// <param name="expression">The expression to walk.</param>
    internal static void WalkForUnsupportedSegments(
        in OperationAnalysisContext context,
        ExpressionSyntax expression)
    {
        var current = AnalyzerHelpers.SkipNullForgivingAndParentheses(expression);
        var isLeaf = true;
        while (current is not null)
        {
            if (current is InvocationExpressionSyntax invocation)
            {
                // Method call in path: x => x.GetValue().Name
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticWarnings.UnsupportedPathSegment,
                        invocation.GetLocation(),
                        invocation.ToString()));
                return;
            }

            if (current is ElementAccessExpressionSyntax elementAccess)
            {
                // Indexer in path: x => x.Items[0].Name
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticWarnings.UnsupportedPathSegment,
                        elementAccess.GetLocation(),
                        elementAccess.ToString()));
                return;
            }

            if (current is MemberAccessExpressionSyntax memberAccess)
            {
                // Check if the member is a field
                var memberSymbol = context.Operation.SemanticModel!.GetSymbolInfo(memberAccess, context.CancellationToken).Symbol;

                // An instance field is read once, like any link that raises no notification. The leaf may be
                // assigned, so a read-only one there is not generated.
                if (memberSymbol is IFieldSymbol { IsConst: false } field && (field.IsStatic || (isLeaf && field.IsReadOnly)))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticWarnings.UnsupportedPathSegment,
                            memberAccess.Name.GetLocation(),
                            memberSymbol.Name));
                    return;
                }

                isLeaf = false;
                current = AnalyzerHelpers.SkipNullForgivingAndParentheses(memberAccess.Expression);
                continue;
            }

            // Lambda parameter or other terminal — stop walking
            break;
        }
    }

    /// <summary>Extracts the body expression from a lambda expression syntax node.</summary>
    /// <param name="lambda">The lambda expression syntax node to extract the body from.</param>
    /// <returns>
    /// The body as an <see cref="ExpressionSyntax"/>, or <c>null</c> if the lambda body is a block statement.
    /// </returns>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    internal static ExpressionSyntax? GetLambdaBody(LambdaExpressionSyntax lambda) =>
        lambda.Body as ExpressionSyntax;

    /// <summary>Reports every link past the first whose declaring type raises no notification.</summary>
    /// <param name="context">The operation analysis context.</param>
    /// <param name="body">The lambda body naming the observed path.</param>
    private static void ReportSilentLinks(in OperationAnalysisContext context, ExpressionSyntax body)
    {
        var model = context.Operation.SemanticModel!;

        // The path is written outermost-first, so walking down it reaches the root last. Every access whose
        // own receiver is itself an access is a link past the first.
        var current = AnalyzerHelpers.SkipNullForgivingAndParentheses(body) as MemberAccessExpressionSyntax;
        while (current is not null)
        {
            var parent = AnalyzerHelpers.SkipNullForgivingAndParentheses(current.Expression) as MemberAccessExpressionSyntax;
            if (parent is not null)
            {
                ReportIfSilent(context, model, parent);
            }

            current = parent;
        }
    }

    /// <summary>Reports a link when the type it yields raises no notification.</summary>
    /// <param name="context">The operation analysis context.</param>
    /// <param name="model">The semantic model the link is read through.</param>
    /// <param name="link">The member access naming the link.</param>
    private static void ReportIfSilent(
        in OperationAnalysisContext context,
        SemanticModel model,
        MemberAccessExpressionSyntax link)
    {
        if (model.GetSymbolInfo(link, context.CancellationToken).Symbol
            is not IPropertySymbol { Type: INamedTypeSymbol linkType })
        {
            return;
        }

        if (AnalyzerHelpers.HasObservableMechanism(linkType, context.Compilation))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticWarnings.SilentPathLink,
                link.GetLocation(),
                linkType.Name,
                link.ToString()));
    }

    /// <summary>Determines whether an argument is an inline lambda passed as an expression tree.</summary>
    /// <param name="argument">The argument to inspect.</param>
    /// <param name="lambda">The lambda, when the argument is one.</param>
    /// <returns><see langword="true"/> when the argument is an expression-tree lambda.</returns>
    private static bool IsExpressionArgument(IArgumentOperation argument, out LambdaExpressionSyntax? lambda)
    {
        lambda = null;

        return NativeTypeIdentity.Matches(argument.Parameter!.Type, ExpressionTypeName)
            && (lambda = argument.Value.Syntax as LambdaExpressionSyntax) is not null;
    }

    /// <summary>Determines whether a non-empty constant <c>toEvent</c> argument was explicitly supplied.</summary>
    /// <param name="arguments">The invocation arguments to inspect.</param>
    /// <returns><c>true</c> if a non-empty <c>toEvent</c> string was specified; otherwise, <c>false</c>.</returns>
    private static bool IsToEventSpecified(ImmutableArray<IArgumentOperation> arguments)
    {
        for (var i = 0; i < arguments.Length; i++)
        {
            var toEventArg = arguments[i];
            if (toEventArg.Parameter!.Name != "toEvent")
            {
                continue;
            }

            if (!toEventArg.IsImplicit
                && toEventArg.Value.ConstantValue.HasValue
                && toEventArg.Value.ConstantValue.Value is string { Length: > 0 })
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether a control type exposes one of the default bindable events
    /// (Click, TouchUpInside, MouseUp, or Pressed).
    /// </summary>
    /// <param name="controlType">The control type to inspect.</param>
    /// <returns><c>true</c> if a default bindable event is found; otherwise, <c>false</c>.</returns>
    private static bool HasDefaultBindableEvent(INamedTypeSymbol controlType)
    {
        if (AppKitCommandSymbols.CanBind(controlType) || UIKitCommandSymbols.CanBindTouch(controlType)
            || UIKitCommandSymbols.ControlEvent(controlType) is not null)
        {
            return true;
        }

        string[] defaultEvents = ["Click", "TouchUpInside", "MouseUp", "Pressed"];
        for (var j = 0; j < defaultEvents.Length; j++)
        {
            if (NativeCommandMembers.HasEvent(controlType, defaultEvents[j]))
            {
                return true;
            }
        }

        return false;
    }
}
