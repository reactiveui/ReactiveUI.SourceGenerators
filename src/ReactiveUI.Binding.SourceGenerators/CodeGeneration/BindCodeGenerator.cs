// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Plugins;
using static ReactiveUI.Binding.SourceGenerators.CodeGeneration.GeneratedTypeNames;

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>
/// Generates concrete typed extension method overloads and binding methods for Bind (view-first two-way) invocations.
/// The generated methods return <c>IReactiveBinding&lt;TView, BindingChange&gt;</c> and use view-first parameter ordering.
/// </summary>
internal static class BindCodeGenerator
{
    /// <summary>Gets what distinguishes this API's generated dispatch overload from the other three.</summary>
    internal static readonly BindingEmitterHelpers.BindingDispatchApi DispatchApi = new()
    {
        Name = "Bind",
        ReceiverParameterName = "view",
        OtherParameterName = "viewModel",
        ReceiverIsTarget = true,
        SourceSelectorName = "viewModelProperty",
        TargetSelectorName = "viewProperty",
        WorkerMethodPrefix = "__Bind_",
        WorkerSourceParameterName = "viewModel",
        WorkerTargetParameterName = "view",
        IsTwoWay = true,
        HookRefusalValue = "null",
        SourceObservableName = ViewModelObservableName,
        TargetObservableName = ViewObservableName,
        SourceConvertedName = "__vmSelected",
        TargetConvertedName = "__viewSelected",
        SourceScheduledName = "vmBind",
        TargetScheduledName = "viewBind",
        ForwardConverterArgument = ForwardConverterName,
        ReverseConverterArgument = ReverseConverterName,
        OverrideForwardName = ForwardConverterName,
        OverrideReverseName = ReverseConverterName,
        FormatReturnType = FormatReturnType,
        FormatWorkerReturnType = FormatMethodReturnType,
        AppendExtraParameters = AppendExtraParameters,
        FormatWorkerParameters = FormatExtraMethodParams,
        FormatExtraArguments = FormatExtraArgs,
    };

    /// <summary>The local a two-way callback holds the value it applies in.</summary>
    private const string AppliedValueLocal = "value";

    /// <summary>What this API calls the view-model-to-view converter in its generated signatures.</summary>
    private const string ForwardConverterName = "viewModelToViewConverter";

    /// <summary>What this API calls the view-to-view-model converter in its generated signatures.</summary>
    private const string ReverseConverterName = "viewToViewModelConverter";

    /// <summary>Name of the generated local holding the view model side observable.</summary>
    private const string ViewModelObservableName = "vmObs";

    /// <summary>Name of the generated local holding the view side observable.</summary>
    private const string ViewObservableName = "viewObs";

    /// <summary>Generates a private Bind method for a specific invocation.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The binding invocation info.</param>
    /// <param name="sourceClassInfo">The source type class binding info.</param>
    /// <param name="targetClassInfo">The target type class binding info.</param>
    /// <param name="suffix">The stable method name suffix.</param>
    internal static void GenerateBindMethod(
        SourceWriter sb,
        BindingInvocationInfo inv,
        ClassBindingInfo? sourceClassInfo,
        ClassBindingInfo? targetClassInfo,
        string suffix)
    {
        // Both directions follow the view's current view model, so the write walks the same re-rooted path the
        // observation does and its guard drops the write while the view holds none.
        var observation = BindingEmitterHelpers.ResolveViewModelObservation(inv, sourceClassInfo, targetClassInfo);
        BindingEmitterHelpers.AppendWorkerMethodHeader(sb, DispatchApi, inv, suffix);

        // Emit inline observation code instead of delegating to WhenChanged dispatch
        ObservationCodeGenerator.EmitInlineObservation(
            sb,
            observation.RootVariable,
            observation.Path,
            inv.SourcePropertyTypeFullName,
            observation.RootClassInfo,
            ViewModelObservableName,
            NullParentObservationBehavior.SuppressEmission);

        ObservationCodeGenerator.EmitInlineObservation(
            sb,
            "view",
            inv.TargetPropertyPath,
            inv.TargetPropertyTypeFullName,
            targetClassInfo,
            ViewObservableName);

        var (viewModelVar, viewVar) = BindingEmitterHelpers.EmitDualStreamStages(sb, DispatchApi, inv);

        EmitTwoWaySubscription(sb, inv, viewModelVar, viewVar, observation);
    }

    /// <summary>Appends extra parameters (converters, scheduler) to the concrete overload signature.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The binding type group.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AppendExtraParameters(SourceWriter sb, BindingTypeGroup group, bool supportsNullable) =>
        BindingEmitterHelpers.AppendTwoWayExtraParameters(sb, group, ForwardConverterName, ReverseConverterName, supportsNullable);

    /// <summary>Formats extra arguments (converters, scheduler) for forwarding to the binding method.</summary>
    /// <param name="group">The binding type group.</param>
    /// <returns>Extra arguments string or empty.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string FormatExtraArgs(BindingTypeGroup group) =>
        BindingEmitterHelpers.FormatTwoWayExtraArgs(group, ForwardConverterName, ReverseConverterName);

    /// <summary>Formats extra method parameters for the private binding method signature.</summary>
    /// <param name="inv">The binding invocation info.</param>
    /// <returns>Extra parameters string for converter and scheduler parameters.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string FormatExtraMethodParams(BindingInvocationInfo inv) =>
        BindingEmitterHelpers.FormatTwoWayExtraMethodParams(inv, ForwardConverterName, ReverseConverterName);

    /// <summary>Formats the return type for a concrete Bind overload.</summary>
    /// <param name="group">The binding type group.</param>
    /// <returns>The fully qualified return type string.</returns>
    internal static string FormatReturnType(BindingTypeGroup group) =>
        $"{IReactiveBinding}<{group.TargetTypeFullName}, {BindingChange}>";

    /// <summary>Formats the return type for a private Bind method.</summary>
    /// <param name="inv">The binding invocation info.</param>
    /// <returns>The fully qualified return type string.</returns>
    internal static string FormatMethodReturnType(BindingInvocationInfo inv) =>
        $"{IReactiveBinding}<{inv.TargetTypeFullName}, {BindingChange}>";

    /// <summary>Emits the two-way pipeline and the <c>ReactiveBinding</c> return block.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The binding invocation info.</param>
    /// <param name="viewModelVar">The view model observable variable name.</param>
    /// <param name="viewVar">The view observable variable name.</param>
    /// <param name="observation">The re-rooted path the view model side is written through.</param>
    /// <remarks>
    /// Both directions become one stream, which is then routed once and applied once. Three things follow from
    /// that, and none of them hold when each direction is wired on its own.
    /// <para>
    /// The view model side is merged first, so its value reaches the view before the view's own first value is
    /// weighed - and the guard, seeing them equal, drops that one. Dropping the view's first value by position
    /// would eat a real change wherever the view reports nothing to begin with, which a chain through a null
    /// intermediate does.
    /// </para>
    /// <para>
    /// Routing the merged stream carries the view's read and the view model's write, not just the write to the
    /// view. Routing one direction leaves the other running wherever its notification was raised.
    /// </para>
    /// <para>
    /// What was applied is published to a subject the caller subscribes to, so a subscriber shares the one
    /// upstream subscription rather than attaching another set of handlers, and never sees a value the guard
    /// refused to write.
    /// </para>
    /// </remarks>
    private static void EmitTwoWaySubscription(
        SourceWriter sb,
        BindingInvocationInfo inv,
        string viewModelVar,
        string viewVar,
        in BindingEmitterHelpers.ViewModelObservation observation)
    {
        var changeType = $"global::System.ValueTuple<bool, {inv.TargetPropertyTypeFullName}, {inv.SourcePropertyTypeFullName}>";
        _ = sb.BlankLine()
            .BeginVar("__vmTagged").Append("new ").Append(MapSignal).Append('<').Append(inv.TargetPropertyTypeFullName).Append(", ")
            .Append(changeType).Append(">(").Append(viewModelVar).Append(", v => new ").Append(changeType)
            .Append("(true, v, default(").Append(inv.SourcePropertyTypeFullName).Line(")));")
            .BeginVar("__viewTagged").Append("new ").Append(MapSignal).Append('<').Append(inv.SourcePropertyTypeFullName).Append(", ")
            .Append(changeType).Append(">(").Append(viewVar).Append(", v => new ").Append(changeType)
            .Append("(false, default(").Append(inv.TargetPropertyTypeFullName).Line("), v));")
            .BeginVar("__sides").Append("new ").Append(MergeSignal).Append('<').Append(changeType).Line(">(__vmTagged, __viewTagged);");

        var routedVar = BindingEmitterHelpers.EmitViewThreadStage(sb, inv, "__sides", "__routed", "view", inv.TargetViewThreadInvoker);

        _ = sb.Var("changed", $"new {Observables}.AppliedChangeObservable()")
            .BlankLine()
            .BeginVar("disposable").Append(BindingErrors).Append(".Subscribe(").Append(routedVar).Line(", __change =>")
            .OpenBlock()
            .If("__change.Item1")
            .Var(AppliedValueLocal, "__change.Item2");
        CodeGeneratorHelpers.AppendGuardedAssignment(sb, "view", inv.TargetPropertyPath, AppliedValueLocal);
        _ = AppendAppliedChange(sb, fromViewModel: true)
            .CloseBlock()
            .Else()
            .Var(AppliedValueLocal, "__change.Item3");
        CodeGeneratorHelpers.AppendGuardedAssignment(sb, observation.RootVariable, observation.Path, AppliedValueLocal);
        _ = AppendAppliedChange(sb, fromViewModel: false)
            .CloseBlock()
            .CloseBlockInline().Append(", \"").AppendEscaped(inv.SourceExpressionText).Append(" / ")
            .AppendEscaped(inv.TargetExpressionText).Line("\");")
            .BlankLine()
            .BeginReturn().Append($"new {ReactiveBinding}<").Append(inv.TargetTypeFullName).Append(", ")
            .Append(BindingChange).Append(">(").OpenContinuation()
            .Line("view,")
            .Line("changed,")
            .Line($"{BindingDirection}.TwoWay,")
            .Line("disposable);")
            .Outdent()
            .CloseBlock()
            .BlankLine();
    }

    /// <summary>Writes the publication of a change the guard let through, to whoever subscribed to the binding.</summary>
    /// <param name="sb">The writer, inside the branch that applied the change.</param>
    /// <param name="fromViewModel">Whether the change travelled from the view model to the view.</param>
    /// <returns>The writer, for chaining.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static SourceWriter AppendAppliedChange(SourceWriter sb, bool fromViewModel) =>
        sb.If("changed.HasObservers")
            .Append("changed.OnNext(new ").Append(BindingChange).Append("(value, ").AppendLiteral(fromViewModel).Line("));")
            .CloseBlock();
}
