// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>
/// Generates concrete typed extension method overloads and binding methods for BindOneWay invocations.
/// Supports basic bindings, inline Func converters, and scheduler overloads.
/// </summary>
internal static class BindOneWayCodeGenerator
{
    /// <summary>Gets what distinguishes this API's generated dispatch overload from the other three.</summary>
    internal static readonly BindingEmitterHelpers.BindingDispatchApi DispatchApi = new()
    {
        Name = "BindOneWay",
        ReceiverParameterName = "source",
        OtherParameterName = "target",
        ReceiverIsTarget = false,
        SourceSelectorName = "sourceProperty",
        TargetSelectorName = "targetProperty",
        WorkerMethodPrefix = "__BindOneWay_",
        WorkerSourceParameterName = "source",
        WorkerTargetParameterName = "target",
        IsTwoWay = false,
        HookRefusalValue = GeneratedTypeNames.EmptyDisposableInstance,
        SourceObservableName = SourceObservableVariable,
        SourceConvertedName = "__selected",
        SourceScheduledName = "bindObs",
        ForwardConverterArgument = ConversionParameterName,
        OverrideForwardName = "converter",
        AppendExtraParameters = AppendExtraParameters,
        FormatWorkerParameters = FormatExtraMethodParams,
        FormatExtraArguments = FormatExtraArgs,
    };

    /// <summary>What this API calls the conversion argument in its generated signatures.</summary>
    private const string ConversionParameterName = "conversionFunc";

    /// <summary>Name of the emitted local holding the source property observation, before conversion or scheduling.</summary>
    private const string SourceObservableVariable = "sourceObs";

    /// <summary>
    /// Generates the BindOneWay method used for binding a source property to a target property with optional conversion and scheduler.
    /// </summary>
    /// <param name="sb">The SourceWriter for appending the generated code.</param>
    /// <param name="inv">The invocation information containing details about the binding.</param>
    /// <param name="sourceClassInfo">The class binding information of the source, or null if not applicable.</param>
    /// <param name="suffix">The suffix to append to the generated method name for uniqueness.</param>
    internal static void GenerateBindOneWayMethod(
        SourceWriter sb,
        BindingInvocationInfo inv,
        ClassBindingInfo? sourceClassInfo,
        string suffix)
    {
        BindingEmitterHelpers.AppendWorkerMethodHeader(sb, DispatchApi, inv, suffix);

        // Emit inline observation code instead of delegating to WhenChanged dispatch
        ObservationCodeGenerator.EmitInlineObservation(
            sb,
            "source",
            inv.SourcePropertyPath,
            inv.SourcePropertyTypeFullName,
            sourceClassInfo,
            SourceObservableVariable);

        var subscribeVar = BindingEmitterHelpers.EmitSingleStreamStages(sb, DispatchApi, inv);

        subscribeVar = BindingEmitterHelpers.EmitViewThreadStage(sb, inv, subscribeVar, "targetThreadObs", "target", inv.TargetViewThreadInvoker);

        BindingEmitterHelpers.AppendWriteSubscription(sb.BlankLine().BeginReturn(), subscribeVar, "target", inv.TargetPropertyPath, inv.TargetExpressionText);
        _ = sb.CloseBlock().BlankLine();
    }

    /// <summary>Appends extra parameters (converter, scheduler) to the concrete overload signature.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The binding type group.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AppendExtraParameters(SourceWriter sb, BindingTypeGroup group, bool supportsNullable) =>
        BindingEmitterHelpers.AppendExtraParameters(sb, group, ConversionParameterName, supportsNullable);

    /// <summary>Formats extra arguments (converter, scheduler) for forwarding to the binding method.</summary>
    /// <param name="group">The binding type group.</param>
    /// <returns>Extra arguments string like ", conversionFunc, scheduler" or empty.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string FormatExtraArgs(BindingTypeGroup group) =>
        BindingEmitterHelpers.FormatExtraArgs(group, ConversionParameterName);

    /// <summary>Formats extra method parameters for the private binding method signature.</summary>
    /// <param name="inv">The binding invocation info.</param>
    /// <returns>Extra parameters string like ", Func&lt;int, string&gt; conversionFunc, ISequencer scheduler" or empty.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string FormatExtraMethodParams(BindingInvocationInfo inv) =>
        BindingEmitterHelpers.FormatExtraMethodParams(inv, ConversionParameterName);
}
