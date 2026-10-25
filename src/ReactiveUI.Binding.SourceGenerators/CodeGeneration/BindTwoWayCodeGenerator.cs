// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>
/// Generates concrete typed extension method overloads and binding methods for BindTwoWay invocations.
/// Supports basic bindings, inline Func converters (source-to-target and target-to-source), and scheduler overloads.
/// </summary>
internal static class BindTwoWayCodeGenerator
{
    /// <summary>Gets what distinguishes this API's generated dispatch overload from the other three.</summary>
    internal static readonly BindingEmitterHelpers.BindingDispatchApi DispatchApi = new()
    {
        Name = "BindTwoWay",
        ReceiverParameterName = "source",
        OtherParameterName = "target",
        ReceiverIsTarget = false,
        SourceSelectorName = "sourceProperty",
        TargetSelectorName = "targetProperty",
        WorkerMethodPrefix = "__BindTwoWay_",
        WorkerSourceParameterName = SourceParameterName,
        WorkerTargetParameterName = TargetParameterName,
        IsTwoWay = true,
        HookRefusalValue = GeneratedTypeNames.EmptyDisposableInstance,
        SourceObservableName = BindTwoWayCodeGenerator.SourceObservableName,
        TargetObservableName = BindTwoWayCodeGenerator.TargetObservableName,
        SourceConvertedName = "__srcSelected",
        TargetConvertedName = "__tgtSelected",
        SourceScheduledName = "sourceBind",
        TargetScheduledName = "targetBind",
        ForwardConverterArgument = ForwardConverterName,
        ReverseConverterArgument = ReverseConverterName,
        OverrideForwardName = "sourceToTargetConverter",
        OverrideReverseName = "targetToSourceConverter",
        AppendExtraParameters = AppendExtraParameters,
        FormatWorkerParameters = FormatExtraMethodParams,
        FormatExtraArguments = FormatExtraArgs,
    };

    /// <summary>What this API calls the source-to-target converter in its generated signatures.</summary>
    private const string ForwardConverterName = "sourceToTargetConv";

    /// <summary>What this API calls the target-to-source converter in its generated signatures.</summary>
    private const string ReverseConverterName = "targetToSourceConv";

    /// <summary>Name of the source parameter on the generated binding method.</summary>
    private const string SourceParameterName = "source";

    /// <summary>Name of the target parameter on the generated binding method.</summary>
    private const string TargetParameterName = "target";

    /// <summary>Name of the generated local holding the source side observable.</summary>
    private const string SourceObservableName = "sourceObs";

    /// <summary>Name of the generated local holding the target side observable.</summary>
    private const string TargetObservableName = "targetObs";

    /// <summary>Generates a private BindTwoWay method for a specific invocation.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The binding invocation info.</param>
    /// <param name="sourceClassInfo">The source type class binding info.</param>
    /// <param name="targetClassInfo">The target type class binding info.</param>
    /// <param name="suffix">The stable method name suffix.</param>
    internal static void GenerateBindTwoWayMethod(
        SourceWriter sb,
        BindingInvocationInfo inv,
        ClassBindingInfo? sourceClassInfo,
        ClassBindingInfo? targetClassInfo,
        string suffix)
    {
        BindingEmitterHelpers.AppendWorkerMethodHeader(sb, DispatchApi, inv, suffix);

        // Emit inline observation code instead of delegating to WhenChanged dispatch
        ObservationCodeGenerator.EmitInlineObservation(
            sb,
            SourceParameterName,
            inv.SourcePropertyPath,
            inv.SourcePropertyTypeFullName,
            sourceClassInfo,
            SourceObservableName);

        ObservationCodeGenerator.EmitInlineObservation(
            sb,
            TargetParameterName,
            inv.TargetPropertyPath,
            inv.TargetPropertyTypeFullName,
            targetClassInfo,
            TargetObservableName);

        var (sourceVar, targetVar) = BindingEmitterHelpers.EmitDualStreamStages(sb, DispatchApi, inv);

        // Both directions are routed, and the source direction first, so its initial value is queued ahead of
        // the target's. That ordering is what seeds the target before the target's own first value is weighed.
        sourceVar = BindingEmitterHelpers.EmitViewThreadStage(sb, inv, sourceVar, "targetThreadObs", TargetParameterName, inv.TargetViewThreadInvoker);
        targetVar = BindingEmitterHelpers.EmitViewThreadStage(sb, inv, targetVar, "sourceThreadObs", SourceParameterName, inv.SourceViewThreadInvoker);

        EmitTwoWaySubscription(sb, inv, sourceVar, targetVar);
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
    /// <returns>Extra arguments string like ", sourceToTargetConv, targetToSourceConv, scheduler" or empty.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string FormatExtraArgs(BindingTypeGroup group) =>
        BindingEmitterHelpers.FormatTwoWayExtraArgs(group, ForwardConverterName, ReverseConverterName);

    /// <summary>Formats extra method parameters for the private binding method signature.</summary>
    /// <param name="inv">The binding invocation info.</param>
    /// <returns>Extra parameters string for two-way converter and scheduler parameters.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string FormatExtraMethodParams(BindingInvocationInfo inv) =>
        BindingEmitterHelpers.FormatTwoWayExtraMethodParams(inv, ForwardConverterName, ReverseConverterName);

    /// <summary>Emits the two-way subscription and <c>MultipleDisposable</c> return block, and closes the worker.</summary>
    /// <param name="sb">The writer, inside the worker's body.</param>
    /// <param name="inv">The call site, naming the paths written and the expressions a faulting write is reported against.</param>
    /// <param name="sourceVar">The source observable variable name to subscribe to.</param>
    /// <param name="targetVar">The target observable variable name to subscribe to.</param>
    /// <remarks>
    /// The target's own first value is weighed rather than dropped by position. Both observations report what
    /// they hold when subscribed, and the source is subscribed first, so by the time the target reports the
    /// target already carries the source's value and the equality guard drops it. Dropping the first value
    /// positionally instead would eat a real change wherever the target reports nothing to begin with - a chain
    /// through a null intermediate does exactly that.
    /// </remarks>
    private static void EmitTwoWaySubscription(
        SourceWriter sb,
        BindingInvocationInfo inv,
        string sourceVar,
        string targetVar)
    {
        BindingEmitterHelpers.AppendWriteSubscription(
            sb.BlankLine().BeginVar("d1"),
            sourceVar,
            TargetParameterName,
            inv.TargetPropertyPath,
            inv.TargetExpressionText);
        BindingEmitterHelpers.AppendWriteSubscription(
            sb.BlankLine().BeginVar("d2"),
            targetVar,
            SourceParameterName,
            inv.SourcePropertyPath,
            inv.SourceExpressionText);
        _ = sb.BlankLine()
            .Return($"new {GeneratedTypeNames.MultipleDisposable}(d1, d2)")
            .CloseBlock()
            .BlankLine();
    }
}
