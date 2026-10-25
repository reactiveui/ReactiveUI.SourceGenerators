// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.CommandBinding;

/// <summary>
/// The shapes of a generated command binding, named once so every plugin that drives a command from a
/// control writes the same lines.
/// </summary>
/// <remarks>
/// The bindings differ in what supplies the command parameter and how the control runs the command; the
/// subscription around them is the same in each. Each member writes at the writer's level.
/// </remarks>
internal static class CommandBindingSyntax
{
    /// <summary>Subscribes the generated handler to the control's event.</summary>
    internal const string HandlerSubscribe = " += __Handler;";

    /// <summary>Unsubscribes the generated handler from the control's event.</summary>
    internal const string HandlerUnsubscribe = " -= __Handler;";

    /// <summary>The disposable holding whatever the current command subscribed to.</summary>
    private const string SerialDeclaration = $"new {GeneratedTypeNames.SwapDisposable}()";

    /// <summary>
    /// Opens the subscription that rebinds whenever the command property changes, up to the test that a command
    /// arrived at all, leaving the writer inside the branch taken when none did.
    /// </summary>
    /// <param name="sb">The writer, inside the worker's body.</param>
    /// <returns>The writer, inside the <c>cmd == null</c> branch.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static SourceWriter OpenCommandSubscription(SourceWriter sb) =>
        sb.Var("serial", SerialDeclaration)
            .Line($"var __cmdSub = {GeneratedTypeNames.Subscribe}(commandObs, ({GeneratedTypeNames.ICommand} cmd) =>")
            .OpenBlock()
            .Append("serial.Disposable = ").Append(GeneratedTypeNames.EmptyDisposableInstance).EndStatement()
            .If("cmd == null");

    /// <summary>Opens a subscription to the command that does not test for a missing command.</summary>
    /// <param name="sb">The writer, inside the worker's body.</param>
    /// <returns>The writer, inside the subscription's callback.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static SourceWriter OpenUntypedCommandSubscription(SourceWriter sb) =>
        sb.Var("serial", SerialDeclaration)
            .Line($"var __cmdSub = {GeneratedTypeNames.Subscribe}(commandObs, cmd =>")
            .OpenBlock()
            .Append("serial.Disposable = ").Append(GeneratedTypeNames.EmptyDisposableInstance).EndStatement();

    /// <summary>Leaves the binding inert until a command arrives, closing the <c>cmd == null</c> branch.</summary>
    /// <param name="sb">The writer, inside the <c>cmd == null</c> branch.</param>
    /// <returns>The writer, inside the subscription's callback.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static SourceWriter CloseCommandMissing(SourceWriter sb) => sb.Line("return;").CloseBlock();

    /// <summary>Closes the subscription to the command.</summary>
    /// <param name="sb">The writer, inside the subscription's callback.</param>
    /// <returns>The writer, back in the worker's body.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static SourceWriter CloseCommandSubscription(SourceWriter sb) => sb.CloseBlock(");");

    /// <summary>Opens the disposable that undoes what the current command attached, when the command changes.</summary>
    /// <param name="sb">The writer, inside the subscription's callback.</param>
    /// <returns>The writer, inside the detach callback, which <see cref="CloseCommandSubscription"/>'s shape closes with <c>});</c>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static SourceWriter OpenSerialDetach(SourceWriter sb) =>
        sb.Line($"serial.Disposable = new {GeneratedTypeNames.ActionDisposable}(() =>").OpenBlock();

    /// <summary>Opens the declaration of the handler the control's event runs the command from.</summary>
    /// <param name="sb">The writer, inside the subscription's callback.</param>
    /// <param name="eventArgsType">The event args type the control's event carries.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types.</param>
    /// <returns>The writer, inside the handler's body.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static SourceWriter OpenHandler(SourceWriter sb, string eventArgsType, bool supportsNullable) =>
        sb.Append("void __Handler(").Append(CommandEventBindingEmitter.SenderType(supportsNullable)).Append(" sender, ")
            .Append(eventArgsType).Line(" e)")
            .OpenBlock();

    /// <summary>Writes the guarded run of the command.</summary>
    /// <param name="sb">The writer, where the command is run.</param>
    /// <param name="argument">The expression the command is asked about and run with.</param>
    /// <returns>The writer.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static SourceWriter AppendGuardedExecute(SourceWriter sb, string argument) =>
        sb.BeginIf().Append("cmd.CanExecute(").Append(argument).Append(')').CloseCondition()
            .Append("cmd.Execute(").Append(argument).Line(");")
            .CloseBlock();

    /// <summary>Renders the write that records the parameter a later command emission will be given.</summary>
    /// <param name="inv">The BindCommand invocation info.</param>
    /// <param name="valueExpression">The expression producing the value to record.</param>
    /// <returns>The rendered write, without a trailing semicolon.</returns>
    /// <remarks>
    /// The volatile write is what makes a parameter arriving on one thread visible to a command arriving on
    /// another, and it is only available for a reference type - <c>Volatile</c> offers no overload for an
    /// arbitrary value type, so a parameter such as a <c>Guid</c> would not compile. Those are recorded by a
    /// plain write, which is what the runtime engine does for every parameter it boxes.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string WriteLatestParameter(BindCommandInvocationInfo inv, string valueExpression) =>
        inv.ParameterIsReferenceType
            ? $"{GeneratedTypeNames.Volatile}.Write(ref __latestParam, {valueExpression})"
            : $"__latestParam = {valueExpression}";

    /// <summary>Renders the read that recovers the parameter a command emission should be given.</summary>
    /// <param name="inv">The BindCommand invocation info.</param>
    /// <returns>The rendered read.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string ReadLatestParameter(BindCommandInvocationInfo inv) =>
        inv.ParameterIsReferenceType
            ? $"{GeneratedTypeNames.Volatile}.Read(ref __latestParam)"
            : "__latestParam";
}
