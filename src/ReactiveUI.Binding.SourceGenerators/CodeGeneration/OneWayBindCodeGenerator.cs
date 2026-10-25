// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Plugins;
using ReactiveUI.Binding.SourceGenerators.Plugins.SetMethod;

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>
/// Generates concrete typed extension method overloads and binding methods for OneWayBind (view-first) invocations.
/// The generated methods return <c>IReactiveBinding&lt;TView, TValue&gt;</c> and use view-first parameter ordering.
/// </summary>
internal static class OneWayBindCodeGenerator
{
    /// <summary>Gets what distinguishes this API's generated dispatch overload from the other three.</summary>
    internal static readonly BindingEmitterHelpers.BindingDispatchApi DispatchApi = new()
    {
        Name = "OneWayBind",
        ReceiverParameterName = "view",
        OtherParameterName = "viewModel",
        ReceiverIsTarget = true,
        SourceSelectorName = "viewModelProperty",
        TargetSelectorName = "viewProperty",
        WorkerMethodPrefix = "__OneWayBind_",
        WorkerSourceParameterName = "viewModel",
        WorkerTargetParameterName = "view",
        IsTwoWay = false,
        HookRefusalValue = "null",
        SourceObservableName = SourceObservableVariable,
        SourceConvertedName = "__selected",
        SourceScheduledName = "bindObs",
        ForwardConverterArgument = ConversionParameterName,
        OverrideForwardName = "converter",
        FormatReturnType = FormatReturnType,
        FormatWorkerReturnType = FormatMethodReturnType,
        AppendExtraParameters = AppendExtraParameters,
        FormatWorkerParameters = FormatExtraMethodParams,
        FormatExtraArguments = FormatExtraArgs,
    };

    /// <summary>What this API calls the projection argument in its generated signatures.</summary>
    private const string ConversionParameterName = "selector";

    /// <summary>Name of the emitted local holding the source property observation, before conversion or scheduling.</summary>
    private const string SourceObservableVariable = "sourceObs";

    /// <summary>Generates a private OneWayBind method for a specific invocation.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The binding invocation info.</param>
    /// <param name="sourceClassInfo">The source type class binding info.</param>
    /// <param name="targetClassInfo">The view type class binding info, which says whether it exposes a view model.</param>
    /// <param name="suffix">The stable method name suffix.</param>
    internal static void GenerateOneWayBindMethod(
        SourceWriter sb,
        BindingInvocationInfo inv,
        ClassBindingInfo? sourceClassInfo,
        ClassBindingInfo? targetClassInfo,
        string suffix)
    {
        BindingEmitterHelpers.AppendWorkerMethodHeader(sb, DispatchApi, inv, suffix);

        // Emit inline observation code instead of delegating to WhenChanged dispatch
        var observation = BindingEmitterHelpers.ResolveViewModelObservation(inv, sourceClassInfo, targetClassInfo);

        ObservationCodeGenerator.EmitInlineObservation(
            sb,
            observation.RootVariable,
            observation.Path,
            inv.SourcePropertyTypeFullName,
            observation.RootClassInfo,
            SourceObservableVariable,
            NullParentObservationBehavior.SuppressEmission);

        var currentVar = BindingEmitterHelpers.EmitSingleStreamStages(sb, DispatchApi, inv);

        currentVar = BindingEmitterHelpers.EmitViewThreadStage(sb, inv, currentVar, "viewThreadObs", "view", inv.TargetViewThreadInvoker);

        if (inv.SetMethod is { } setMethod)
        {
            CollectionSetMethodEmitter.EmitSubscription(sb, new("view", inv.TargetPropertyPath, inv.SourcePropertyTypeFullName, setMethod, inv.TargetExpressionText, true), currentVar);
            _ = sb.BeginReturn().Append($"new {GeneratedTypeNames.ReactiveBinding}<").Append(inv.TargetTypeFullName).Append(", ")
                .Append(inv.TargetPropertyTypeFullName).Line($">(view, __setChanges, {GeneratedTypeNames.BindingDirection}.OneWay,")
                .Indent()
                .Line($"new {GeneratedTypeNames.MultipleDisposable}(__setSubscription, __setChanges));")
                .Outdent()
                .CloseBlock();
            return;
        }

        BindingEmitterHelpers.AppendWriteSubscription(sb.BlankLine().BeginVar("sub"), currentVar, "view", inv.TargetPropertyPath, inv.TargetExpressionText);
        _ = sb.BlankLine()
            .BeginReturn().Append($"new {GeneratedTypeNames.ReactiveBinding}<").Append(inv.TargetTypeFullName).Append(", ")
            .Append(inv.TargetPropertyTypeFullName).Append(">(").OpenContinuation()
            .Line("view,")
            .Append(currentVar).Line(",")
            .Line($"{GeneratedTypeNames.BindingDirection}.OneWay,")
            .Line("sub);")
            .Outdent()
            .CloseBlock()
            .BlankLine();
    }

    /// <summary>Appends extra parameters (selector, scheduler) to the concrete overload signature.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The binding type group.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AppendExtraParameters(SourceWriter sb, BindingTypeGroup group, bool supportsNullable) =>
        BindingEmitterHelpers.AppendExtraParameters(sb, group, ConversionParameterName, supportsNullable);

    /// <summary>Formats extra arguments (selector, scheduler) for forwarding to the binding method.</summary>
    /// <param name="group">The binding type group.</param>
    /// <returns>Extra arguments string or empty.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string FormatExtraArgs(BindingTypeGroup group) =>
        BindingEmitterHelpers.FormatExtraArgs(group, ConversionParameterName);

    /// <summary>Formats extra method parameters for the private binding method signature.</summary>
    /// <param name="inv">The binding invocation info.</param>
    /// <returns>Extra parameters string for selector and scheduler parameters.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string FormatExtraMethodParams(BindingInvocationInfo inv) =>
        BindingEmitterHelpers.FormatExtraMethodParams(inv, ConversionParameterName);

    /// <summary>Formats the return type for a concrete OneWayBind overload.</summary>
    /// <param name="group">The binding type group.</param>
    /// <returns>The fully qualified return type string.</returns>
    /// <remarks>
    /// Always the view property's type, which is what the runtime stub declares. Using the source type when no
    /// converter was supplied only agrees with the stub while the two sides happen to match.
    /// </remarks>
    internal static string FormatReturnType(BindingTypeGroup group) =>
        $"{GeneratedTypeNames.IReactiveBinding}<{group.TargetTypeFullName}, {group.TargetPropertyTypeFullName}>";

    /// <summary>Formats the return type for a private OneWayBind method.</summary>
    /// <param name="inv">The binding invocation info.</param>
    /// <returns>The fully qualified return type string.</returns>
    internal static string FormatMethodReturnType(BindingInvocationInfo inv) =>
        $"{GeneratedTypeNames.IReactiveBinding}<{inv.TargetTypeFullName}, {inv.TargetPropertyTypeFullName}>";
}
