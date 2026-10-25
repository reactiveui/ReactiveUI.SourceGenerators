// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Plugins;
using ReactiveUI.Binding.SourceGenerators.Plugins.CommandBinding;

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>Generates concrete typed extension method overloads and binding methods for BindCommand invocations.</summary>
internal static class BindCommandCodeGenerator
{
    /// <summary>The parameter carrying the text of the selector naming the command.</summary>
    private const string CommandExpressionParameter = "propertyNameExpression";

    /// <summary>The parameter carrying the text of the selector naming the control.</summary>
    private const string ControlExpressionParameter = "controlNameExpression";

    /// <summary>The parameter carrying the text of the selector naming the command parameter.</summary>
    private const string ParameterExpressionParameter = "withParameterExpression";

    /// <summary>The generated worker each dispatch branch hands the binding to.</summary>
    private const string WorkerMethodPrefix = "__BindCommand_";

    /// <summary>The two objects a generated worker binds, in its own parameter order.</summary>
    private const string WorkerArguments = "view, viewModel";

    /// <summary>Closes the view parameter of a generated binding worker.</summary>
    private const string ViewParameterSuffix = " view,";

    /// <summary>The caller-supplied parameter stream a worker takes on top of the two bound objects.</summary>
    private const string ObservableParameterArgument = ", withParameter";

    /// <summary>Generates concrete typed overloads and binding methods for BindCommand invocations.</summary>
    /// <param name="invocations">All detected BindCommand invocations.</param>
    /// <param name="allClasses">All detected class binding info.</param>
    /// <param name="features">The consumer compilation's C# language-feature snapshot (dispatch strategy and nullable support).</param>
    /// <returns>Generated source code string, or null if no invocations.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string? Generate(
        ImmutableArray<BindCommandInvocationInfo> invocations,
        ImmutableArray<ClassBindingInfo> allClasses,
        in LanguageFeatures features) =>
        CodeGeneratorHelpers.GenerateDispatchFile(
            invocations,
            features,
            GroupByTypeSignature,
            (sb, group, snapshot) => EmitGroup(sb, group, allClasses, snapshot));

    /// <summary>Groups BindCommand invocations by their type signature for overload generation.</summary>
    /// <param name="invocations">The BindCommand invocations to group.</param>
    /// <returns>A list of grouped invocations sharing the same type signature.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static List<BindCommandTypeGroup> GroupByTypeSignature(
        ImmutableArray<BindCommandInvocationInfo> invocations) =>
        SignatureGrouping.Group(
            invocations,
            static (key, inv) => _ = key
                .Append(inv.ViewTypeFullName).Append('|')
                .Append(inv.ViewModelTypeFullName).Append('|')
                .Append(inv.CommandTypeFullName).Append('|')
                .Append(inv.ControlTypeFullName).Append('|')
                .Append(inv.HasObservableParameter).Append('|')
                .Append(inv.HasExpressionParameter).Append('|')
                .Append(inv.ParameterTypeFullName ?? string.Empty),
            static (first, members) => new BindCommandTypeGroup(
                first.ViewTypeFullName,
                first.ViewModelTypeFullName,
                first.CommandTypeFullName,
                first.ControlTypeFullName,
                first.HasObservableParameter,
                first.HasExpressionParameter,
                first.ParameterTypeFullName,
                members));

    /// <summary>Generates the concrete typed overload the consumer's language features call for.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The BindCommand type group.</param>
    /// <param name="features">The consumer compilation's language-feature snapshot.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void GenerateConcreteOverload(SourceWriter sb, BindCommandTypeGroup group, in LanguageFeatures features) =>
        GenerateConcreteOverload(sb, group, features.SupportsCallerArgExpr, features.SupportsNullable, features.StubHasExpressionParameters);

    /// <summary>Generates the concrete typed overload using the appropriate dispatch strategy.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The BindCommand type group.</param>
    /// <param name="supportsCallerArgExpr">Whether CallerArgumentExpression is available.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    /// <param name="stubHasExpressionParameters">Whether the runtime stub declares the expression parameters this overload has to match.</param>
    internal static void GenerateConcreteOverload(
        SourceWriter sb,
        BindCommandTypeGroup group,
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

    /// <summary>Generates the CallerArgumentExpression-based overload for BindCommand dispatch.</summary>
    /// <param name="sb">The writer, at the class's member level.</param>
    /// <param name="group">The BindCommand type group.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    internal static void GenerateCallerArgExprOverload(
        SourceWriter sb,
        BindCommandTypeGroup group,
        bool supportsNullable)
    {
        AppendOverloadSummary(sb, group, "Uses CallerArgumentExpression for dispatch.");
        AppendParameterList(sb, group, true, supportsNullable, true);

        _ = sb.OpenBlock();

        EmitExpressionDispatchBranches(sb, group);
        CodeGeneratorHelpers.AppendBindingDispatchFallthrough(sb);
    }

    /// <summary>Generates the CallerFilePath-based overload for BindCommand dispatch.</summary>
    /// <param name="sb">The writer, at the class's member level.</param>
    /// <param name="group">The BindCommand type group.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    /// <param name="stubHasExpressionParameters">Whether the runtime stub declares the expression parameters this overload has to match.</param>
    internal static void GenerateCallerFilePathOverload(
        SourceWriter sb,
        BindCommandTypeGroup group,
        bool supportsNullable,
        bool stubHasExpressionParameters)
    {
        AppendOverloadSummary(sb, group, "Uses CallerFilePath + CallerLineNumber for dispatch.");
        AppendParameterList(sb, group, false, supportsNullable, stubHasExpressionParameters);

        _ = sb.OpenBlock();

        EmitFilePathDispatchBranches(sb, group);
        CodeGeneratorHelpers.AppendBindingDispatchFallthrough(sb);
    }

    /// <summary>Generates a private BindCommand method for a specific invocation.</summary>
    /// <param name="sb">The writer, at the class's member level.</param>
    /// <param name="inv">The BindCommand invocation info.</param>
    /// <param name="viewModelClassInfo">The view model type class binding info.</param>
    /// <param name="viewClassInfo">The view type class binding info, which says whether it exposes a view model.</param>
    /// <param name="suffix">The stable method name suffix.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    internal static void GenerateBindCommandMethod(
        SourceWriter sb,
        BindCommandInvocationInfo inv,
        ClassBindingInfo? viewModelClassInfo,
        ClassBindingInfo? viewClassInfo,
        string suffix,
        bool supportsNullable)
    {
        var commandObservation = BindingEmitterHelpers.ResolveViewModelObservation(
            inv.ViewModelTypeFullName,
            inv.ViewTypeFullName,
            inv.CommandPropertyPath,
            viewModelClassInfo,
            viewClassInfo);

        _ = sb.Append("private static ").Append(GeneratedTypeNames.IDisposable).Append(" __BindCommand_").Append(suffix).OpenParameterList()
            .Append(inv.ViewTypeFullName).Line(ViewParameterSuffix)
            .Append(inv.ViewModelTypeFullName).Append(" viewModel");

        // Only a caller-supplied stream is passed in; a parameter named as a property is observed inside the worker.
        if (inv.HasObservableParameter)
        {
            _ = sb.Line(",").Append(GeneratedTypeNames.IObservable).Append('<').Append(inv.ParameterTypeFullName).Append("> withParameter");
        }

        _ = sb.Line(")").Outdent().OpenBlock()
            .BeginComment().Append("BindCommand: ");
        _ = CodeGeneratorHelpers.AppendPropertyPath(sb, inv.CommandPropertyPath).Append(" -> ");
        _ = CodeGeneratorHelpers.AppendPropertyPath(sb, inv.ControlPropertyPath)
            .Append(" (event: ").Append(inv.ResolvedEventName ?? "none").Line(")");

        if (commandObservation.RootVariable == "viewModel")
        {
            _ = sb.If("viewModel == null").Return(GeneratedTypeNames.EmptyDisposableInstance).CloseBlock();
        }

        _ = sb.BlankLine();

        EmitViewModelObservations(sb, inv, viewModelClassInfo, viewClassInfo, in commandObservation);
        CommandControlEmitter.EmitRebinding(sb, inv, viewClassInfo, suffix);

        var plugin = CommandBindingPluginRegistry.GetBestPlugin(inv);
        var generatedAffinity = plugin is not null ? plugin.Affinity : -1;
        EmitCommandAffinityCheck(sb, inv, "__control", generatedAffinity, inv.HasExplicitEvent);

        if (plugin is not null)
        {
            plugin.EmitBinding(sb, inv, "__control", supportsNullable);
        }
        else
        {
            // No plugin matched — throw after the affinity check fallback
            _ = sb.Append("throw new ").Append(GeneratedTypeNames.InvalidOperationException).Append('(')
                .OpenContinuation()
                .Line("\"No bindable event found on the control. Specify the 'toEvent' parameter.\");")
                .Outdent()
                .CloseBlock();
        }

        _ = sb.BlankLine();
    }

    /// <summary>Emits the check that hands the binding to a registered <c>ICreatesCommandBinding</c> with a higher affinity.</summary>
    /// <param name="sb">The writer, inside the worker's body.</param>
    /// <param name="inv">The BindCommand invocation info.</param>
    /// <param name="controlAccess">The control access chain (e.g., "view.MyButton").</param>
    /// <param name="generatedAffinity">The affinity of the source-generated plugin, or -1 if none.</param>
    /// <param name="hasEvent">Whether a resolved event was found at compile time.</param>
    internal static void EmitCommandAffinityCheck(
        SourceWriter sb,
        BindCommandInvocationInfo inv,
        string controlAccess,
        int generatedAffinity,
        bool hasEvent)
    {
        _ = sb.BlankLine()
            .Line("if (global::ReactiveUI.Binding.Fallback.CommandBindingAffinityChecker")
            .Indent()
            .Append(".HasHigherAffinityPlugin<").Append(inv.ControlTypeFullName).Append(">(").Append(generatedAffinity).Append(", ")
            .AppendLiteral(hasEvent).Line("))")
            .Outdent()
            .OpenBlock()
            .Line("var __customBinder = global::ReactiveUI.Binding.CommandBinding.CommandBinderService")
            .Indent()
            .Append(".GetBinder<").Append(inv.ControlTypeFullName).Append(">(").AppendLiteral(hasEvent).Line(");")
            .Outdent()
            .If("__customBinder != null")
            .Var("__serial", $"new {GeneratedTypeNames.SwapDisposable}()")
            .Line($"var __binderCmdSub = {GeneratedTypeNames.Subscribe}(commandObs, __cmd =>")
            .OpenBlock()
            .Append("__serial.Disposable = ").Append(GeneratedTypeNames.EmptyDisposableInstance).EndStatement()
            .Append($"{GeneratedTypeNames.IObservable}<object> __paramObs = ");
        _ = AppendParameterObservable(sb, inv).EndStatement()
            .Append("__serial.Disposable = __customBinder.BindCommandToObject<").Append(inv.ControlTypeFullName);
        if (hasEvent)
        {
            _ = sb.Append(", ").Append(inv.ResolvedEventArgsTypeFullName ?? GeneratedTypeNames.EventArgs);
        }

        _ = sb.Append(">(").OpenContinuation()
            .Append("__cmd, ").Append(controlAccess).Append(", __paramObs");
        if (hasEvent)
        {
            _ = sb.Append(", \"").Append(inv.ResolvedEventName).Append('"');
        }

        _ = sb.Line(")")
            .Append("?? ").Append(GeneratedTypeNames.EmptyDisposableInstance).EndStatement()
            .Outdent()
            .CloseBlock(");")
            .Return($"new {GeneratedTypeNames.MultipleDisposable}(__binderCmdSub, __serial)")
            .CloseBlock()
            .CloseBlock()
            .BlankLine();
    }

    /// <summary>Writes the parameter observable a custom binder is handed.</summary>
    /// <param name="sb">The writer, part way through the line the expression belongs to.</param>
    /// <param name="inv">The BindCommand invocation info.</param>
    /// <returns>The writer, for chaining.</returns>
    internal static SourceWriter AppendParameterObservable(SourceWriter sb, BindCommandInvocationInfo inv) =>
        inv.HasObservableParameter || inv is { HasExpressionParameter: true, ParameterPropertyPath: not null }
            ? sb.Append($"new {GeneratedTypeNames.MapSignal}<").Append(inv.ParameterTypeFullName).Append(", object>(withParameter, __p => __p)")
            : sb.Append($"{GeneratedTypeNames.ImmutableEmptySignal}<object>.Instance");

    /// <summary>Emits the observations of the command, and of a parameter named as a property, through the view's current view model.</summary>
    /// <param name="sb">The writer, inside the worker's body.</param>
    /// <param name="inv">The BindCommand invocation info.</param>
    /// <param name="viewModelClassInfo">The view model type class binding info.</param>
    /// <param name="viewClassInfo">The view type class binding info, which says whether it exposes a view model.</param>
    /// <param name="commandObservation">The command path rooted at the view or the supplied model.</param>
    private static void EmitViewModelObservations(
        SourceWriter sb,
        BindCommandInvocationInfo inv,
        ClassBindingInfo? viewModelClassInfo,
        ClassBindingInfo? viewClassInfo,
        in BindingEmitterHelpers.ViewModelObservation commandObservation)
    {
        ObservationCodeGenerator.EmitInlineObservation(
            sb,
            commandObservation.RootVariable,
            commandObservation.Path,
            inv.CommandTypeFullName,
            commandObservation.RootClassInfo,
            "__commandChanges");

        // Each rebind touches the control, so the command arrives on the view's thread.
        _ = BindingEmitterHelpers.AppendViewThreadCall(sb.BeginVar("commandObs"), "__commandChanges", "view", inv.ViewThreadInvoker)
            .EndStatement();

        if (inv is not { HasObservableParameter: false, HasExpressionParameter: true, ParameterPropertyPath: not null })
        {
            return;
        }

        // Observed rather than read once, so the control sees every value the parameter takes.
        var parameterObservation = BindingEmitterHelpers.ResolveViewModelObservation(
            inv.ViewModelTypeFullName,
            inv.ViewTypeFullName,
            inv.ParameterPropertyPath.Value,
            viewModelClassInfo,
            viewClassInfo);

        ObservationCodeGenerator.EmitInlineObservation(
            sb,
            parameterObservation.RootVariable,
            parameterObservation.Path,
            inv.ParameterTypeFullName ?? GeneratedTypeNames.ObjectType,
            parameterObservation.RootClassInfo,
            "withParameter");
    }

    /// <summary>Writes the summary and signature both dispatch overloads declare, opening the parameter list.</summary>
    /// <param name="sb">The writer, at the class's member level; left inside the parameter list.</param>
    /// <param name="group">The BindCommand type group.</param>
    /// <param name="dispatchSummaryLine">The documentation line naming what the overload matches a call site on.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AppendOverloadSummary(
        SourceWriter sb,
        BindCommandTypeGroup group,
        string dispatchSummaryLine) =>
        sb.OpenSummary()
            .BeginDocLine().Append("Concrete typed overload for BindCommand on ").Append(group.ViewTypeFullName).Line(".")
            .DocLine(dispatchSummaryLine)
            .CloseSummary()
            .Append("public static ").Append(GeneratedTypeNames.IDisposable).Append(" BindCommand").OpenParameterList();

    /// <summary>Writes the stub's parameter list, which the overload and the interceptor both have to match exactly.</summary>
    /// <param name="sb">The writer, inside the parameter list.</param>
    /// <param name="group">The BindCommand type group whose types the parameters are written from.</param>
    /// <param name="dispatchesOnExpressionText">Whether the captured expression text is what identifies a call site.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    /// <param name="stubHasExpressionParameters">Whether the runtime stub declares the expression parameters.</param>
    private static void AppendParameterList(
        SourceWriter sb,
        BindCommandTypeGroup group,
        bool dispatchesOnExpressionText,
        bool supportsNullable,
        bool stubHasExpressionParameters)
    {
        // The command and parameter selectors are nullable and the control selector is not, as the stub declares them.
        var commandType = CodeGeneratorHelpers.NullableSelectorLeafType(group.Invocations[0].CommandPropertyPath, supportsNullable);

        _ = sb.Append("this ").Append(group.ViewTypeFullName).Line(ViewParameterSuffix)
            .Append(CodeGeneratorHelpers.NullableSelectorType(group.ViewModelTypeFullName, true, supportsNullable)).Line(" viewModel,")
            .Append(GeneratedSyntax.SelectorTypeOpen).Append(group.ViewModelTypeFullName).Append(", ")
            .Append(commandType).Line(">> propertyName,")
            .Append(GeneratedSyntax.SelectorTypeOpen).Append(group.ViewTypeFullName).Append(", ").Append(group.ControlTypeFullName).Line(">> controlName,");

        if (group.HasObservableParameter)
        {
            _ = sb.Append(GeneratedTypeNames.IObservable).Append('<').Append(group.ParameterTypeFullName).Line("> withParameter,");
        }
        else if (group.HasExpressionParameter)
        {
            _ = sb.Append(GeneratedSyntax.SelectorTypeOpen).Append(group.ViewModelTypeFullName).Append(", ").Append(group.ParameterTypeFullName)
                .Append(supportsNullable && group.Invocations[0].ParameterIsReferenceType ? "?" : string.Empty).Line(">> withParameter,");
        }

        _ = sb.Append("string").Append(supportsNullable ? "?" : string.Empty).Line(" toEvent = null,");

        if (dispatchesOnExpressionText || stubHasExpressionParameters)
        {
            CodeGeneratorHelpers.AppendExpressionParameter(sb, "propertyName", CommandExpressionParameter, dispatchesOnExpressionText);
            CodeGeneratorHelpers.AppendExpressionParameter(sb, "controlName", ControlExpressionParameter, dispatchesOnExpressionText);

            if (group.HasExpressionParameter)
            {
                CodeGeneratorHelpers.AppendExpressionParameter(sb, "withParameter", ParameterExpressionParameter, dispatchesOnExpressionText);
            }
        }

        CodeGeneratorHelpers.AppendCallerInfoParameters(sb);
    }

    /// <summary>Emits one interceptor per generated binding, claiming every call site that reaches it.</summary>
    /// <param name="sb">The writer, at the class's member level.</param>
    /// <param name="group">The group of call sites being claimed.</param>
    /// <param name="features">The consumer compilation's language-feature snapshot.</param>
    private static void GenerateInterceptors(SourceWriter sb, BindCommandTypeGroup group, in LanguageFeatures features)
    {
        var extraArgs = group.HasObservableParameter ? ObservableParameterArgument : string.Empty;
        var dispatchesOnExpressionText = features.SupportsCallerArgExpr;
        var supportsNullable = features.SupportsNullable;
        var stubHasExpressionParameters = features.StubHasExpressionParameters;

        foreach (var entry in InterceptorEmitter.GroupCallSites(group.Invocations, static x => x.Interceptor, MethodSuffix))
        {
            InterceptorEmitter.AppendClaimingMethodOpen(
                sb,
                entry.Value,
                static x => x.Interceptor,
                $"internal static {GeneratedTypeNames.IDisposable} __Intercept_BindCommand_",
                entry.Key);
            AppendParameterList(sb, group, dispatchesOnExpressionText, supportsNullable, stubHasExpressionParameters);

            _ = sb.Indent()
                .Append("=> ").Append(WorkerMethodPrefix).Append(entry.Key).Append('(').Append(WorkerArguments).Append(extraArgs).Line(");")
                .Outdent()
                .BlankLine();
        }
    }

    /// <summary>Emits the overload and the workers for one group of call sites.</summary>
    /// <param name="sb">The writer, at the class's member level.</param>
    /// <param name="group">The group of call sites that share an overload.</param>
    /// <param name="allClasses">All detected class binding info.</param>
    /// <param name="features">The consumer compilation's language-feature snapshot.</param>
    private static void EmitGroup(
        SourceWriter sb,
        BindCommandTypeGroup group,
        ImmutableArray<ClassBindingInfo> allClasses,
        in LanguageFeatures features)
    {
        var collapsed = features.CollapsesIndistinguishableCallSites
            ? group with { Invocations = ExplicitEventsFirst(CodeGeneratorHelpers.CollapseIndistinguishableCallSites(group.Invocations, DispatchKey)) }
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
            GenerateBindCommandMethod(
                sb,
                inv,
                CodeGeneratorHelpers.ResolveObservedTypeInfo(allClasses, inv.ViewModelTypeFullName, inv.CommandPropertyPath),
                CodeGeneratorHelpers.ResolveObservedTypeInfo(allClasses, inv.ViewTypeFullName, inv.ControlPropertyPath),
                MethodSuffix(inv),
                features.SupportsNullable);
        }
    }

    /// <summary>Emits one expression-text comparison branch per call site in the group.</summary>
    /// <param name="sb">The writer, inside the overload's body.</param>
    /// <param name="group">The BindCommand type group.</param>
    private static void EmitExpressionDispatchBranches(SourceWriter sb, BindCommandTypeGroup group)
    {
        var extraArgs = group.HasObservableParameter ? ObservableParameterArgument : string.Empty;

        for (var i = 0; i < group.Invocations.Length; i++)
        {
            var inv = group.Invocations[i];

            AppendExpressionCondition(sb, i, inv, group.HasExpressionParameter);
            CodeGeneratorHelpers.AppendDispatchReturn(sb, WorkerMethodPrefix + MethodSuffix(inv), WorkerArguments + extraArgs);
        }
    }

    /// <summary>Opens the branch that matches a call site by everything that shapes what it binds.</summary>
    /// <param name="sb">The writer, inside the overload's body; left inside the branch's block.</param>
    /// <param name="index">The branch's position, which decides whether it opens with <c>if</c> or <c>else if</c>.</param>
    /// <param name="inv">The call site the branch stands for.</param>
    /// <param name="comparesParameter">Whether the overload captures the text of the parameter selector.</param>
    /// <remarks>
    /// The command and control selectors are not all of it. A parameter selector picks which property feeds the
    /// control, and an explicit event replaces the mechanism the control would have bound through, so two call
    /// sites that spell the same command and control differently in either bind differently. A call site that
    /// names no event carries no event condition, which is why the ones that do are tried first.
    /// </remarks>
    private static void AppendExpressionCondition(
        SourceWriter sb,
        int index,
        BindCommandInvocationInfo inv,
        bool comparesParameter)
    {
        _ = CodeGeneratorHelpers.AppendExpressionTextTest(sb.BeginBranch(index), CommandExpressionParameter, inv.CommandExpressionText)
            .OpenContinuation()
            .Append("&& ");
        _ = CodeGeneratorHelpers.AppendExpressionTextTest(sb, ControlExpressionParameter, inv.ControlExpressionText);

        if (comparesParameter && inv.ParameterExpressionText is not null)
        {
            _ = CodeGeneratorHelpers.AppendExpressionTextTest(sb.EndLine().Append("&& "), ParameterExpressionParameter, inv.ParameterExpressionText);
        }

        if (inv.HasExplicitEvent && inv.ResolvedEventName is not null)
        {
            _ = CodeGeneratorHelpers.AppendExpressionTextTest(sb.EndLine().Append("&& "), "toEvent", inv.ResolvedEventName);
        }

        _ = sb.Outdent().CloseCondition();
    }

    /// <summary>Keys a call site by everything the condition that dispatches on it compares.</summary>
    /// <param name="inv">The call site.</param>
    /// <returns>The key two call sites share only when nothing can tell them apart.</returns>
    private static string DispatchKey(BindCommandInvocationInfo inv) =>
        $"{inv.CommandExpressionText}|{inv.ControlExpressionText}|{inv.ParameterExpressionText}|{(inv.HasExplicitEvent ? inv.ResolvedEventName : null)}";

    /// <summary>Puts the call sites that name an event ahead of those that do not, keeping the order within each.</summary>
    /// <param name="invocations">The call sites of one group.</param>
    /// <returns>The same call sites, with the explicit-event ones first.</returns>
    private static BindCommandInvocationInfo[] ExplicitEventsFirst(BindCommandInvocationInfo[] invocations)
    {
        var ordered = new List<BindCommandInvocationInfo>(invocations.Length);

        for (var i = 0; i < invocations.Length; i++)
        {
            if (invocations[i].HasExplicitEvent)
            {
                ordered.Add(invocations[i]);
            }
        }

        for (var i = 0; i < invocations.Length; i++)
        {
            if (!invocations[i].HasExplicitEvent)
            {
                ordered.Add(invocations[i]);
            }
        }

        return [.. ordered];
    }

    /// <summary>Emits one file-and-line comparison branch per call site in the group.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The BindCommand type group.</param>
    private static void EmitFilePathDispatchBranches(SourceWriter sb, BindCommandTypeGroup group)
    {
        var extraArgs = group.HasObservableParameter ? ObservableParameterArgument : string.Empty;

        for (var i = 0; i < group.Invocations.Length; i++)
        {
            var inv = group.Invocations[i];
            CodeGeneratorHelpers.AppendCallerInfoDispatchBranch(
                sb,
                i,
                inv.CallerLineNumber,
                inv.CallerFilePath,
                WorkerMethodPrefix + MethodSuffix(inv),
                WorkerArguments + extraArgs);
        }
    }

    /// <summary>Names the generated worker a call site dispatches to.</summary>
    /// <param name="inv">The call site.</param>
    /// <returns>The stable suffix its worker is named with.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string MethodSuffix(BindCommandInvocationInfo inv) =>
        CodeGeneratorHelpers.ComputeStableMethodSuffix(
            inv.ViewTypeFullName,
            inv.CallerFilePath,
            inv.CallerLineNumber,
            $"{inv.CommandExpressionText}|{inv.ControlExpressionText}");

    /// <summary>Groups BindCommand invocations by type signature for overload generation.</summary>
    /// <param name="ViewTypeFullName">The fully qualified view type.</param>
    /// <param name="ViewModelTypeFullName">The fully qualified view model type.</param>
    /// <param name="CommandTypeFullName">The fully qualified type of the bound command property.</param>
    /// <param name="ControlTypeFullName">The fully qualified type of the control the command binds to.</param>
    /// <param name="HasObservableParameter">Whether the overload takes an observable command parameter.</param>
    /// <param name="HasExpressionParameter">Whether the overload takes an expression command parameter.</param>
    /// <param name="ParameterTypeFullName">The fully qualified command parameter type, when the overload has one.</param>
    /// <param name="Invocations">The call sites sharing this group's shape.</param>
    internal sealed record BindCommandTypeGroup(
        string ViewTypeFullName,
        string ViewModelTypeFullName,
        string CommandTypeFullName,
        string ControlTypeFullName,
        bool HasObservableParameter,
        bool HasExpressionParameter,
        string? ParameterTypeFullName,
        BindCommandInvocationInfo[] Invocations);
}
