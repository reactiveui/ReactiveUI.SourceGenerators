// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding.SourceGenerators.Models;

using static ReactiveUI.Binding.SourceGenerators.CodeGeneration.GeneratedTypeNames;

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>Generates concrete typed extension method overloads and binding methods for BindInteraction invocations.</summary>
internal static class BindInteractionCodeGenerator
{
    /// <summary>The generated worker each dispatch branch hands the binding to.</summary>
    private const string WorkerMethodPrefix = "__BindInteraction_";

    /// <summary>The arguments a generated worker takes, in its own parameter order.</summary>
    /// <remarks>
    /// The view comes first because the interaction is observed through whichever view model the view holds,
    /// so the worker needs the view itself rather than only the instance the call site handed over.
    /// </remarks>
    private const string WorkerArguments = "view, viewModel, handler";

    /// <summary>Closes the view parameter of a generated binding worker.</summary>
    private const string ViewParameterSuffix = " view,";

    /// <summary>Closes the view model parameter of a generated binding worker.</summary>
    private const string ViewModelParameterSuffix = " viewModel,";

    /// <summary>Generates concrete typed overloads and binding methods for BindInteraction invocations.</summary>
    /// <param name="invocations">All detected BindInteraction invocations.</param>
    /// <param name="allClasses">All detected class binding info.</param>
    /// <param name="features">The consumer compilation's C# language-feature snapshot (dispatch strategy and nullable support).</param>
    /// <returns>Generated source code string, or null if no invocations.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string? Generate(
        ImmutableArray<BindInteractionInvocationInfo> invocations,
        ImmutableArray<ClassBindingInfo> allClasses,
        in LanguageFeatures features) =>
        CodeGeneratorHelpers.GenerateDispatchFile(
            invocations,
            features,
            GroupByTypeSignature,
            (sb, group, snapshot) => EmitGroup(sb, group, allClasses, snapshot));

    /// <summary>Groups BindInteraction invocations by their type signature for overload generation.</summary>
    /// <param name="invocations">The BindInteraction invocations to group.</param>
    /// <returns>A list of grouped invocations sharing the same type signature.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static List<BindInteractionTypeGroup> GroupByTypeSignature(
        ImmutableArray<BindInteractionInvocationInfo> invocations) =>
        SignatureGrouping.Group(
            invocations,
            static (key, inv) => _ = key
                .Append(inv.ViewTypeFullName).Append('|')
                .Append(inv.ViewModelTypeFullName).Append('|')
                .Append(inv.InputTypeFullName).Append('|')
                .Append(inv.OutputTypeFullName).Append('|')
                .Append(inv.IsTaskHandler).Append('|')
                .Append(inv.DontCareTypeFullName),
            static (first, members) => new BindInteractionTypeGroup(
                first.ViewTypeFullName,
                first.ViewModelTypeFullName,
                first.InputTypeFullName,
                first.OutputTypeFullName,
                first.IsTaskHandler,
                first.DontCareTypeFullName,
                members));

    /// <summary>Generates the concrete typed overload the consumer's language features call for.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The BindInteraction type group.</param>
    /// <param name="features">The consumer compilation's language-feature snapshot.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void GenerateConcreteOverload(SourceWriter sb, BindInteractionTypeGroup group, in LanguageFeatures features) =>
        GenerateConcreteOverload(sb, group, features.SupportsCallerArgExpr, features.SupportsNullable, features.StubHasExpressionParameters);

    /// <summary>Generates the concrete typed overload using the appropriate dispatch strategy.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The BindInteraction type group.</param>
    /// <param name="supportsCallerArgExpr">Whether CallerArgumentExpression is available.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    /// <param name="stubHasExpressionParameters">Whether the runtime stub declares the expression parameters this overload has to match.</param>
    internal static void GenerateConcreteOverload(
        SourceWriter sb,
        BindInteractionTypeGroup group,
        bool supportsCallerArgExpr,
        bool supportsNullable,
        bool stubHasExpressionParameters)
    {
        if (supportsCallerArgExpr)
        {
            GenerateCallerArgExprOverload(sb, group, supportsNullable);
        }
        else
        {
            GenerateCallerFilePathOverload(sb, group, supportsNullable, stubHasExpressionParameters);
        }
    }

    /// <summary>Generates the CallerArgumentExpression-based overload for BindInteraction dispatch.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The BindInteraction type group.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    internal static void GenerateCallerArgExprOverload(
        SourceWriter sb,
        BindInteractionTypeGroup group,
        bool supportsNullable)
    {
        AppendOverloadSummary(sb, group, "Uses CallerArgumentExpression for dispatch.");
        AppendParameterList(sb, group, true, supportsNullable, true);

        _ = sb.OpenBlock();

        for (var i = 0; i < group.Invocations.Length; i++)
        {
            var inv = group.Invocations[i];

            _ = CodeGeneratorHelpers.AppendExpressionTextTest(sb.BeginBranch(i), "propertyNameExpression", inv.ExpressionText)
                .CloseCondition();
            CodeGeneratorHelpers.AppendDispatchReturn(sb, WorkerMethodPrefix + MethodSuffix(inv), WorkerArguments);
        }

        CodeGeneratorHelpers.AppendBindingDispatchFallthrough(sb);
    }

    /// <summary>Generates the CallerFilePath-based overload for BindInteraction dispatch.</summary>
    /// <param name="sb">The writer, at the class's member level.</param>
    /// <param name="group">The BindInteraction type group.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    /// <param name="stubHasExpressionParameters">Whether the runtime stub declares the expression parameters this overload has to match.</param>
    internal static void GenerateCallerFilePathOverload(
        SourceWriter sb,
        BindInteractionTypeGroup group,
        bool supportsNullable,
        bool stubHasExpressionParameters)
    {
        AppendOverloadSummary(sb, group, "Uses CallerFilePath + CallerLineNumber for dispatch.");
        AppendParameterList(sb, group, false, supportsNullable, stubHasExpressionParameters);

        _ = sb.OpenBlock();

        for (var i = 0; i < group.Invocations.Length; i++)
        {
            var inv = group.Invocations[i];
            CodeGeneratorHelpers.AppendCallerInfoDispatchBranch(
                sb,
                i,
                inv.CallerLineNumber,
                inv.CallerFilePath,
                WorkerMethodPrefix + MethodSuffix(inv),
                WorkerArguments);
        }

        CodeGeneratorHelpers.AppendBindingDispatchFallthrough(sb);
    }

    /// <summary>Generates a private BindInteraction method for a specific invocation.</summary>
    /// <param name="sb">The writer, at the class's member level.</param>
    /// <param name="inv">The BindInteraction invocation info.</param>
    /// <param name="viewModelClassInfo">The view model type class binding info.</param>
    /// <param name="viewClassInfo">The view type class binding info, which says whether it exposes a view model.</param>
    /// <param name="suffix">The stable method name suffix.</param>
    internal static void GenerateBindInteractionMethod(
        SourceWriter sb,
        BindInteractionInvocationInfo inv,
        ClassBindingInfo? viewModelClassInfo,
        ClassBindingInfo? viewClassInfo,
        string suffix)
    {
        var handlerType = inv.IsTaskHandler
            ? $"{Func}<{IInteractionContext}<{inv.InputTypeFullName}, {inv.OutputTypeFullName}>, {GeneratedTypeNames.Task}>"
            : $"{Func}<{IInteractionContext}<{inv.InputTypeFullName}, {inv.OutputTypeFullName}>, {IObservable}<{inv.DontCareTypeFullName}>>";

        var interactionType =
            $"{IInteraction}<{inv.InputTypeFullName}, {inv.OutputTypeFullName}>";

        _ = sb.Append($"private static {GeneratedTypeNames.IDisposable} __BindInteraction_").Append(suffix).OpenParameterList()
            .Append(inv.ViewTypeFullName).Line(ViewParameterSuffix)
            .Append(inv.ViewModelTypeFullName).Line(ViewModelParameterSuffix)
            .Append(handlerType).Line(" handler)")
            .Outdent()
            .OpenBlock()
            .BeginComment().Append("BindInteraction: ");
        _ = CodeGeneratorHelpers.AppendPropertyPath(sb, inv.InteractionPropertyPath).EndLine()
            .Var("serial", $"new {SwapDisposable}()")
            .BlankLine();

        EmitInteractionObservation(sb, inv, viewModelClassInfo, viewClassInfo, interactionType);

        // Subscribe to the interaction observable and register the handler
        _ = sb.BlankLine()
            .Line($"var sub = {Subscribe}(interactionObs, interaction =>")
            .OpenBlock()
            .Append("serial.Disposable = interaction != null").OpenContinuation()
            .Line("? interaction.RegisterHandler(handler)")
            .Line($": {EmptyDisposableInstance};")
            .Outdent()
            .CloseBlock(");")
            .Return($"new {MultipleDisposable}(sub, serial)")
            .CloseBlock()
            .BlankLine();
    }

    /// <summary>Writes the summary and signature both dispatch overloads declare, opening the parameter list.</summary>
    /// <param name="sb">The writer, at the class's member level; left inside the parameter list.</param>
    /// <param name="group">The BindInteraction type group.</param>
    /// <param name="dispatchSummaryLine">The documentation line naming what the overload matches a call site on.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AppendOverloadSummary(
        SourceWriter sb,
        BindInteractionTypeGroup group,
        string dispatchSummaryLine) =>
        sb.OpenSummary()
            .BeginDocLine().Append("Concrete typed overload for BindInteraction on ").Append(group.ViewTypeFullName).Line(".")
            .DocLine(dispatchSummaryLine)
            .CloseSummary()
            .Append($"public static {GeneratedTypeNames.IDisposable} BindInteraction").OpenParameterList();

    /// <summary>Writes the parameters a BindInteraction member declares, closing the list.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The BindInteraction type group whose types the parameters are written from.</param>
    /// <param name="dispatchesOnExpressionText">Whether the captured expression text is what identifies a call site.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    /// <param name="stubHasExpressionParameters">Whether the runtime stub declares the expression parameter.</param>
    /// <remarks>
    /// One list serves the overload and the interceptor, because both have to be the stub's signature: the
    /// overload only wins resolution against a candidate it is otherwise indistinguishable from, and an
    /// interceptor is refused outright unless its signature is the intercepted method's. The view model is
    /// declared nullable, as the stub declares it: a view is bound before it has been given one.
    /// </remarks>
    private static void AppendParameterList(
        SourceWriter sb,
        BindInteractionTypeGroup group,
        bool dispatchesOnExpressionText,
        bool supportsNullable,
        bool stubHasExpressionParameters)
    {
        var handlerType = group.IsTaskHandler
            ? $"{Func}<{IInteractionContext}<{group.InputTypeFullName}, {group.OutputTypeFullName}>, {GeneratedTypeNames.Task}>"
            : $"{Func}<{IInteractionContext}<{group.InputTypeFullName}, {group.OutputTypeFullName}>, {IObservable}<{group.DontCareTypeFullName}>>";

        _ = sb.Append("this ").Append(group.ViewTypeFullName).Line(ViewParameterSuffix)
            .Append(CodeGeneratorHelpers.NullableSelectorType(group.ViewModelTypeFullName, true, supportsNullable)).Line(ViewModelParameterSuffix)
            .Append(Expression).Append('<').Append(Func).Append('<')
            .Append(group.ViewModelTypeFullName).Append(", ").Append(IInteraction).Append('<').Append(group.InputTypeFullName).Append(", ")
            .Append(group.OutputTypeFullName).Line(">>> propertyName,")
            .Append(handlerType).Line(" handler,");

        if (dispatchesOnExpressionText || stubHasExpressionParameters)
        {
            CodeGeneratorHelpers.AppendExpressionParameter(sb, "propertyName", "propertyNameExpression", dispatchesOnExpressionText);
        }

        CodeGeneratorHelpers.AppendCallerInfoParameters(sb);
    }

    /// <summary>Emits one interceptor per generated binding, claiming every call site that reaches it.</summary>
    /// <param name="sb">The writer, at the class's member level.</param>
    /// <param name="group">The group of call sites being claimed.</param>
    /// <param name="features">The consumer compilation's language-feature snapshot.</param>
    private static void GenerateInterceptors(SourceWriter sb, BindInteractionTypeGroup group, in LanguageFeatures features)
    {
        var dispatchesOnExpressionText = features.SupportsCallerArgExpr;
        var supportsNullable = features.SupportsNullable;
        var stubHasExpressionParameters = features.StubHasExpressionParameters;

        foreach (var entry in InterceptorEmitter.GroupCallSites(group.Invocations, static x => x.Interceptor, MethodSuffix))
        {
            InterceptorEmitter.AppendClaimingMethodOpen(
                sb,
                entry.Value,
                static x => x.Interceptor,
                $"internal static {GeneratedTypeNames.IDisposable} __Intercept_BindInteraction_",
                entry.Key);
            AppendParameterList(sb, group, dispatchesOnExpressionText, supportsNullable, stubHasExpressionParameters);

            _ = sb.Indent()
                .Append("=> ").Append(WorkerMethodPrefix).Append(entry.Key).Append('(').Append(WorkerArguments).Line(");")
                .Outdent()
                .BlankLine();
        }
    }

    /// <summary>Emits the overload and the workers for one group of call sites.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The group of call sites that share an overload.</param>
    /// <param name="allClasses">All detected class binding info.</param>
    /// <param name="features">The consumer compilation's language-feature snapshot.</param>
    private static void EmitGroup(
        SourceWriter sb,
        BindInteractionTypeGroup group,
        ImmutableArray<ClassBindingInfo> allClasses,
        in LanguageFeatures features)
    {
        var collapsed = features.CollapsesIndistinguishableCallSites
            ? group with
            {
                Invocations = CodeGeneratorHelpers.CollapseIndistinguishableCallSites(
                    group.Invocations,
                    static x => x.ExpressionText),
            }
            : group;

        if (features.SupportsInterceptors)
        {
            GenerateInterceptors(sb, collapsed, in features);
        }
        else
        {
            GenerateConcreteOverload(sb, collapsed, in features);
        }

        _ = sb.BlankLine();

        for (var i = 0; i < collapsed.Invocations.Length; i++)
        {
            var inv = collapsed.Invocations[i];
            GenerateBindInteractionMethod(
                sb,
                inv,
                CodeGeneratorHelpers.ResolveObservedTypeInfo(allClasses, inv.ViewModelTypeFullName, inv.InteractionPropertyPath),
                inv.ViewClassInfo,
                MethodSuffix(inv));
        }
    }

    /// <summary>Emits the null guard and the observation of the interaction property.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The BindInteraction invocation info.</param>
    /// <param name="viewModelClassInfo">The view model type's binding info, when known.</param>
    /// <param name="viewClassInfo">The view type's binding info, which says whether it exposes a view model.</param>
    /// <param name="interactionType">The fully qualified interaction type being observed.</param>
    /// <remarks>
    /// The shared chain emitter answers every path length, so a single-segment interaction property is offered
    /// to a registered plugin on the same terms as a deeper one and as every other binding API.
    /// </remarks>
    private static void EmitInteractionObservation(
        SourceWriter sb,
        BindInteractionInvocationInfo inv,
        ClassBindingInfo? viewModelClassInfo,
        ClassBindingInfo? viewClassInfo,
        string interactionType)
    {
        var observation = BindingEmitterHelpers.ResolveViewModelObservation(
            inv.ViewModelTypeFullName,
            inv.ViewTypeFullName,
            inv.InteractionPropertyPath,
            viewModelClassInfo,
            viewClassInfo);

        if (observation.RootVariable == "viewModel")
        {
            AppendViewModelGuard(sb);
        }

        ObservationCodeGenerator.EmitInlineObservation(
            sb,
            observation.RootVariable,
            observation.Path,
            interactionType,
            observation.RootClassInfo,
            "interactionObs");
    }

    /// <summary>Writes the guard that leaves the binding inert until the view is given a view model.</summary>
    /// <param name="sb">The writer, inside the worker's body.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AppendViewModelGuard(SourceWriter sb) =>
        sb.If("viewModel == null").Return("serial").CloseBlock();

    /// <summary>Names the generated worker a call site dispatches to.</summary>
    /// <param name="inv">The call site.</param>
    /// <returns>The stable suffix its worker is named with.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string MethodSuffix(BindInteractionInvocationInfo inv) =>
        CodeGeneratorHelpers.ComputeStableMethodSuffix(
            inv.ViewTypeFullName,
            inv.CallerFilePath,
            inv.CallerLineNumber,
            inv.ExpressionText);

    /// <summary>Groups BindInteraction invocations by type signature for overload generation.</summary>
    /// <param name="ViewTypeFullName">The fully qualified view type.</param>
    /// <param name="ViewModelTypeFullName">The fully qualified view model type.</param>
    /// <param name="InputTypeFullName">The fully qualified interaction input type.</param>
    /// <param name="OutputTypeFullName">The fully qualified interaction output type.</param>
    /// <param name="IsTaskHandler">Whether the handler returns a task rather than an observable.</param>
    /// <param name="DontCareTypeFullName">The fully qualified handler result type that the binding discards, when there is one.</param>
    /// <param name="Invocations">The call sites sharing this group's shape.</param>
    internal sealed record BindInteractionTypeGroup(
        string ViewTypeFullName,
        string ViewModelTypeFullName,
        string InputTypeFullName,
        string OutputTypeFullName,
        bool IsTaskHandler,
        string? DontCareTypeFullName,
        BindInteractionInvocationInfo[] Invocations);
}
