// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.CommandBinding;

/// <summary>Emits Cocoa target/action routing with concrete setters and a native selector bridge.</summary>
internal static class AppKitCommandEmitter
{
    /// <summary>Installs and removes the native target while commands and parameters change.</summary>
    /// <param name="sb">The output builder.</param>
    /// <param name="inv">The verified native binding.</param>
    /// <param name="controlAccess">The concrete target expression.</param>
    internal static void EmitBinding(SourceWriter sb, BindCommandInvocationInfo inv, string controlAccess)
    {
        if (CommandParameterEmitter.HasParameter(inv))
        {
            CommandParameterEmitter.EmitCapture(sb, inv);
        }

        var native = inv.NativeCommand!;
        var parameter = CommandParameterEmitter.Read(inv);
        _ = CommandBindingSyntax.OpenCommandSubscription(sb);
        if (native.HasEnabled)
        {
            _ = sb.Append(controlAccess).Line(".Enabled = false;");
        }

        _ = CommandBindingSyntax.CloseCommandMissing(sb)
            .Append("var __target = new ").Append(GeneratedTypeNames.AppKitCommandTarget).Append("(cmd, () => ").Append(parameter).Line(");");
        if (native.HasAction)
        {
            _ = sb.Append(controlAccess).Line(".Action = new global::ObjCRuntime.Selector(\"theAction:\");");
        }

        _ = sb.Append(controlAccess).Line(".Target = __target;");
        AppendEnabled(sb, native, controlAccess, parameter);
        AppendDetach(sb, native, controlAccess);
        NativeCommandEmitter.AppendReturn(sb, inv);
    }

    /// <summary>Synchronizes the native enabled property and menu-validation result.</summary>
    /// <param name="sb">The output builder.</param>
    /// <param name="native">The verified native capabilities.</param>
    /// <param name="control">The concrete control expression.</param>
    /// <param name="parameter">The current command argument expression.</param>
    private static void AppendEnabled(SourceWriter sb, NativeCommandInfo native, string control, string parameter)
    {
        if (!native.HasEnabled)
        {
            return;
        }

        _ = sb.Line($"{GeneratedTypeNames.EventHandler} __enabled = (__sender, __args) =>")
            .OpenBlock()
            .Append("__target.IsEnabled = cmd.CanExecute(").Append(parameter).Line(");")
            .Append(control).Line(".Enabled = __target.IsEnabled;")
            .CloseBlock(";")
            .Line($"__enabled(null, {GeneratedTypeNames.EventArgs}.Empty);")
            .Line("cmd.CanExecuteChanged += __enabled;");
    }

    /// <summary>Clears native target/action references and releases the bridge.</summary>
    /// <param name="sb">The writer, inside the command subscription's callback.</param>
    /// <param name="native">The verified native capabilities.</param>
    /// <param name="control">The concrete control expression.</param>
    private static void AppendDetach(SourceWriter sb, NativeCommandInfo native, string control)
    {
        _ = CommandBindingSyntax.OpenSerialDetach(sb)
            .Append(control).Line(".Target = null;");
        if (native.HasAction)
        {
            _ = sb.Append(control).Line(".Action = null;");
        }

        if (native.HasEnabled)
        {
            _ = sb.Line("cmd.CanExecuteChanged -= __enabled;");
        }

        _ = CommandBindingSyntax.CloseCommandSubscription(sb.Line("__target.Dispose();").CloseBlock(");"));
    }
}
