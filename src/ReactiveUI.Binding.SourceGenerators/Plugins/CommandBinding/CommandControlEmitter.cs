// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.CommandBinding;

/// <summary>Owns a command binding for each concrete control reached by the view's property path.</summary>
internal static class CommandControlEmitter
{
    /// <summary>Closes the observing worker and opens the worker that captures one control instance.</summary>
    /// <param name="sb">The writer, inside the observing worker's body; left inside the capturing worker's body.</param>
    /// <param name="inv">The command binding.</param>
    /// <param name="viewInfo">The view's observation capabilities.</param>
    /// <param name="suffix">The call site's stable identifier.</param>
    internal static void EmitRebinding(SourceWriter sb, BindCommandInvocationInfo inv, ClassBindingInfo? viewInfo, string suffix)
    {
        var hasParameter = CommandParameterEmitter.HasParameter(inv);

        ObservationCodeGenerator.EmitInlineObservation(sb, "view", inv.ControlPropertyPath, inv.ControlTypeFullName, viewInfo, "__controlChanges");
        _ = BindingEmitterHelpers.AppendViewThreadCall(sb.BeginVar("__controls"), "__controlChanges", "view", inv.ViewThreadInvoker)
            .EndStatement()
            .Var("__controlBinding", $"new {GeneratedTypeNames.SwapDisposable}()")
            .Line($"var __controlSub = {GeneratedTypeNames.Subscribe}(__controls, __control =>")
            .OpenBlock()
            .Append("__controlBinding.Disposable = ").Append(GeneratedTypeNames.EmptyDisposableInstance).EndStatement()
            .If("__control != null")
            .Append("__controlBinding.Disposable = __BindCommandCore_").Append(suffix).Append("(__control, commandObs")
            .Append(hasParameter ? ", withParameter" : string.Empty).Line(");")
            .CloseBlock()
            .CloseBlock(");")
            .Return($"new {GeneratedTypeNames.MultipleDisposable}(__controlSub, __controlBinding)")
            .CloseBlock()
            .BlankLine()
            .Append("private static ").Append(GeneratedTypeNames.IDisposable).Append(" __BindCommandCore_").Append(suffix).Append('(')
            .Append(inv.ControlTypeFullName).Append(" __control, ").Append(GeneratedTypeNames.ObservableOf(inv.CommandTypeFullName)).Append(" commandObs");
        if (hasParameter)
        {
            _ = sb.Append(", ").Append(GeneratedTypeNames.ObservableOf(inv.ParameterTypeFullName ?? GeneratedTypeNames.ObjectType)).Append(" withParameter");
        }

        _ = sb.Line(")").OpenBlock();

        // The view is not thread-affine but the control is: each command lands on the control's thread.
        if (inv.ControlOnlyViewThreadInvoker is { } controlInvoker)
        {
            _ = BindingEmitterHelpers.AppendViewThreadCall(sb.Append("commandObs = "), "commandObs", "__control", controlInvoker)
                .EndStatement();
        }
    }
}
