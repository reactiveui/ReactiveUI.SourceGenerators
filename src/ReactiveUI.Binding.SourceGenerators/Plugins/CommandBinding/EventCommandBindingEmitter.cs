// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.CommandBinding;

/// <summary>Shares command-event subscription fragments across binding mechanisms.</summary>
internal static class EventCommandBindingEmitter
{
    /// <summary>Writes the subscription that rebinds the control whenever the command property changes, up to the missing-command branch.</summary>
    /// <param name="sb">The writer, inside the worker's body; left inside the <c>cmd == null</c> branch.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AppendCommandSubscription(SourceWriter sb) =>
        _ = CommandBindingSyntax.OpenCommandSubscription(sb);

    /// <summary>Writes the capture of the latest parameter, and opens the command subscription over it.</summary>
    /// <param name="sb">The writer, inside the worker's body; left inside the <c>cmd == null</c> branch.</param>
    /// <param name="inv">The BindCommand invocation info.</param>
    internal static void AppendLatestParameterCapture(
        SourceWriter sb,
        BindCommandInvocationInfo inv)
    {
        CommandParameterEmitter.EmitCapture(sb, inv);
        AppendCommandSubscription(sb);
    }

    /// <summary>Writes the declaration of the handler the control's event runs the command from.</summary>
    /// <param name="sb">The writer, inside the subscription's callback; left inside the handler's body.</param>
    /// <param name="eventArgsType">The event args type the control's event carries.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AppendHandlerDeclaration(SourceWriter sb, string eventArgsType, bool supportsNullable) =>
        _ = CommandBindingSyntax.OpenHandler(sb.BlankLine(), eventArgsType, supportsNullable);

    /// <summary>Writes the guarded run of the command inside the control's event handler, and closes the handler.</summary>
    /// <param name="sb">The writer, inside the handler's body.</param>
    /// <param name="argument">The expression the command is asked about and run with.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AppendHandlerExecution(SourceWriter sb, string argument) =>
        _ = CommandBindingSyntax.AppendGuardedExecute(sb, argument).CloseBlock().BlankLine();

    /// <summary>Writes the handler that runs the command, reading the parameter from the view model when one is named.</summary>
    /// <param name="sb">The writer, inside the subscription's callback.</param>
    /// <param name="eventArgsType">The event args type the control's event carries.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types.</param>
    /// <param name="paramAccess">The typed access that reads the command parameter, or null to run the command with none.</param>
    internal static void AppendHandler(SourceWriter sb, string eventArgsType, bool supportsNullable, string? paramAccess)
    {
        AppendHandlerDeclaration(sb, eventArgsType, supportsNullable);
        if (paramAccess is null)
        {
            AppendHandlerExecution(sb, "null");
            return;
        }

        _ = sb.Var("param", paramAccess);
        AppendHandlerExecution(sb, "param");
    }

    /// <summary>Writes the return of the command subscription when no parameter stream was subscribed to, and closes the member.</summary>
    /// <param name="sb">The writer, inside the worker's body.</param>
    internal static void AppendCommandOnlyReturn(SourceWriter sb) =>
        _ = sb.Return($"new {GeneratedTypeNames.MultipleDisposable}(__cmdSub, serial)").CloseBlock();

    /// <summary>Writes the return of the binding's own subscriptions when a parameter stream was subscribed to, and closes the member.</summary>
    /// <param name="sb">The writer, inside the worker's body.</param>
    internal static void AppendParameterisedDisposableReturn(SourceWriter sb) =>
        _ = sb.Line($"return new {GeneratedTypeNames.MultipleDisposable}(")
            .Indent()
            .Line($"new {GeneratedTypeNames.MultipleDisposable}(__cmdSub, __paramSub), serial);")
            .Outdent()
            .CloseBlock();
}
