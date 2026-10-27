// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.CommandBinding;

/// <summary>Emits native event and UIKit touch command bindings.</summary>
internal static class NativeCommandEmitter
{
    /// <summary>The shared event implementation, called directly after native selection.</summary>
    private static readonly EventEnabledBindingPlugin EventBinding = new();

    /// <summary>Uses the verified native event instead of a generic default-event guess.</summary>
    /// <param name="sb">The output builder.</param>
    /// <param name="inv">The binding with its native event.</param>
    /// <param name="controlAccess">The concrete control expression.</param>
    /// <param name="supportsNullable">Whether nullable annotations are supported.</param>
    internal static void EmitEvent(SourceWriter sb, BindCommandInvocationInfo inv, string controlAccess, bool supportsNullable)
    {
        var native = inv.NativeCommand!;
        EventBinding.EmitBinding(sb, inv with { ResolvedEventName = native.EventName, ResolvedEventArgsTypeFullName = native.EventArgsType }, controlAccess, supportsNullable);
    }

    /// <summary>Attaches UIKit's native touch target and keeps enabled state in step with the command.</summary>
    /// <param name="sb">The output builder.</param>
    /// <param name="inv">The selected binding.</param>
    /// <param name="controlAccess">The concrete control expression.</param>
    internal static void EmitTouch(SourceWriter sb, BindCommandInvocationInfo inv, string controlAccess)
    {
        if (CommandParameterEmitter.HasParameter(inv))
        {
            CommandParameterEmitter.EmitCapture(sb, inv);
        }

        var parameter = CommandParameterEmitter.Read(inv);
        _ = sb.BeginVar("__nativeControl").Append("(global::UIKit.UIControl)").Append(controlAccess).EndStatement();
        _ = CommandBindingSyntax.CloseCommandMissing(CommandBindingSyntax.OpenCommandSubscription(sb).Line("__nativeControl.Enabled = false;"))
            .Line($"{GeneratedTypeNames.EventHandler} __action = (__sender, __args) =>")
            .OpenBlock()
            .BeginVar("__parameter").Append("(object)").Append(parameter).EndStatement();
        _ = CommandBindingSyntax.AppendGuardedExecute(sb, "__parameter")
            .CloseBlock(";")
            .Append($"{GeneratedTypeNames.EventHandler} __enabled = (__sender, __args) => __nativeControl.Enabled = cmd.CanExecute(").Append(parameter).Line(");")
            .Append("__nativeControl.Enabled = cmd.CanExecute(").Append(parameter).Line(");")
            .Line("__nativeControl.AddTarget(__action, global::UIKit.UIControlEvent.TouchUpInside);")
            .Line("cmd.CanExecuteChanged += __enabled;");
        _ = CommandBindingSyntax.CloseCommandSubscription(
            CommandBindingSyntax.OpenSerialDetach(sb)
                .Line("__nativeControl.RemoveTarget(__action, global::UIKit.UIControlEvent.TouchUpInside);")
                .Line("cmd.CanExecuteChanged -= __enabled;")
                .CloseBlock(");"));
        AppendReturn(sb, inv);
    }

    /// <summary>Returns every native, command, and parameter subscription owned by the binding, and closes the worker.</summary>
    /// <param name="sb">The writer, inside the worker's body.</param>
    /// <param name="inv">The command binding.</param>
    internal static void AppendReturn(SourceWriter sb, BindCommandInvocationInfo inv)
    {
        _ = sb.BeginReturn().Append($"new {GeneratedTypeNames.MultipleDisposable}(__cmdSub, serial");
        if (CommandParameterEmitter.HasParameter(inv))
        {
            _ = sb.Append(", __paramSub");
        }

        _ = sb.Line(");").CloseBlock();
    }
}
