// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.CommandBinding;

/// <summary>
/// Command binding plugin for controls that have a <c>Command</c> property (ICommand)
/// and optionally a <c>CommandParameter</c> property.
/// Replaces the runtime <c>CreatesCommandBindingViaCommandParameter</c> binder.
/// Affinity 5 (highest priority).
/// </summary>
/// <remarks>
/// Platforms covered: WPF Button, WinUI Button, MAUI Button, and any control
/// with Command + CommandParameter properties.
/// No Enabled synchronization is needed because these frameworks handle it
/// internally through the Command property binding.
/// </remarks>
internal sealed class CommandPropertyBindingPlugin : ICommandBindingPlugin
{
    /// <summary>The affinity score for the Command property binder (highest priority among command binding plugins).</summary>
    private static readonly int CommandPropertyAffinity = BindingAffinity.Explicit;

    /// <inheritdoc/>
    public int Affinity => CommandPropertyAffinity;

    /// <inheritdoc/>
    public bool RequiresCustomBinderFallback => false;

    /// <inheritdoc/>
    /// <remarks>
    /// Both properties, not just the command. This binder drives execution by assigning them, so a control that
    /// takes a command but no parameter cannot carry a parameter the call site supplied - and binding it anyway
    /// would drop that parameter silently. Stepping aside sends the binding to a mechanism that can take it,
    /// which is what the runtime engine does when either property is missing.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanHandle(BindCommandInvocationInfo inv) =>
        !inv.HasExplicitEvent && inv.HasCommandProperty && inv.HasCommandParameterProperty;

    /// <inheritdoc/>
    public void EmitBinding(
        SourceWriter sb,
        BindCommandInvocationInfo inv,
        string controlAccess,
        bool supportsNullable)
    {
        if (inv.HasObservableParameter)
        {
            EmitObservableParameterBinding(sb, inv, controlAccess, supportsNullable);
            return;
        }

        var tracksParameter = inv is { HasExpressionParameter: true, ParameterPropertyPath: not null };

        _ = sb.BlankLine();
        AppendCapturedOriginals(sb, controlAccess);
        _ = CommandBindingSyntax.OpenUntypedCommandSubscription(sb);

        if (tracksParameter)
        {
            // The parameter is a stream, so the control follows every value the property takes for as long as
            // the command is bound. Subscribing lands the current value before the command is assigned.
            _ = sb.Line($"serial.Disposable = {GeneratedTypeNames.Subscribe}(")
                .Indent()
                .Append("withParameter, __p => ").Append(controlAccess).Line(".CommandParameter = __p);")
                .Outdent();
        }

        // The command is assigned last, so the control never sees one with a parameter still to arrive.
        _ = CommandBindingSyntax.CloseCommandSubscription(sb.Append(controlAccess).Line(".Command = cmd;"))
            .BlankLine();

        AppendRestoringReturn(sb, controlAccess, $"new {GeneratedTypeNames.MultipleDisposable}(__cmdSub, serial)");
    }

    /// <summary>
    /// Checks if a control type has a settable <c>Command</c> property (ICommand)
    /// and optionally a settable <c>CommandParameter</c> property.
    /// Walks the type hierarchy.
    /// </summary>
    /// <param name="controlType">The control type symbol to inspect.</param>
    /// <param name="hasCommandParameter">
    /// Set to <see langword="true"/> when the type also has a settable <c>CommandParameter</c> property.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the type or one of its base types has a settable <c>Command</c> property.
    /// </returns>
    internal static bool HasCommandProperties(INamedTypeSymbol controlType, out bool hasCommandParameter)
    {
        hasCommandParameter = false;
        var hasCommand = false;

        var current = (ITypeSymbol?)controlType;
        while (current is INamedTypeSymbol namedCurrent)
        {
            var members = namedCurrent.GetMembers();
            for (var i = 0; i < members.Length; i++)
            {
                if (members[i] is not IPropertySymbol property)
                {
                    continue;
                }

                if (IsSettableICommandProperty(property))
                {
                    hasCommand = true;
                }

                if (IsSettableCommandParameterProperty(property))
                {
                    hasCommandParameter = true;
                }
            }

            if (hasCommand && hasCommandParameter)
            {
                return true;
            }

            current = namedCurrent.BaseType;
        }

        return hasCommand;
    }

    /// <summary>Determines whether a property is a settable public instance <c>Command</c> property typed as ICommand.</summary>
    /// <param name="property">The property to inspect.</param>
    /// <returns><see langword="true"/> if the property is a settable ICommand-typed Command property.</returns>
    internal static bool IsSettableICommandProperty(IPropertySymbol property)
    {
        if (property.Name != "Command" || property.IsReadOnly || property.IsStatic
            || property.DeclaredAccessibility != Accessibility.Public)
        {
            return false;
        }

        var typeName = property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        return typeName.EndsWith("ICommand", StringComparison.Ordinal);
    }

    /// <summary>Determines whether a property is a settable public instance <c>CommandParameter</c> property.</summary>
    /// <param name="property">The property to inspect.</param>
    /// <returns><see langword="true"/> if the property is a settable CommandParameter property.</returns>
    internal static bool IsSettableCommandParameterProperty(IPropertySymbol property) =>
        property.Name == "CommandParameter" && !property.IsReadOnly && !property.IsStatic
        && property.DeclaredAccessibility == Accessibility.Public;

    /// <summary>Writes the reads that remember what the control carried before the binding touched it.</summary>
    /// <param name="sb">The writer, inside the worker's body.</param>
    /// <param name="controlAccess">The access chain to the bound control.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AppendCapturedOriginals(SourceWriter sb, string controlAccess) =>
        sb.BeginVar("__originalCommand").Append(controlAccess).Line(".Command;")
            .BeginVar("__originalParameter").Append(controlAccess).Line(".CommandParameter;");

    /// <summary>Writes the return that disposes the binding and puts the control back as it was found, and closes the worker.</summary>
    /// <param name="sb">The writer, inside the worker's body.</param>
    /// <param name="controlAccess">The access chain to the bound control.</param>
    /// <param name="subscriptions">The expression producing the binding's own subscriptions.</param>
    /// <remarks>
    /// A binding that is disposed has to leave the control as it found it, or a view rebound to a second view
    /// model keeps executing the first one's command. The parameter is restored before the command, so the
    /// control never briefly holds the old command against the new parameter.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AppendRestoringReturn(SourceWriter sb, string controlAccess, string subscriptions) =>
        sb.Line($"return new {GeneratedTypeNames.MultipleDisposable}(")
            .Indent()
            .Append(subscriptions).Line(",")
            .Line($"new {GeneratedTypeNames.ActionDisposable}(() =>")
            .OpenBlock()
            .Append(controlAccess).Line(".CommandParameter = __originalParameter;")
            .Append(controlAccess).Line(".Command = __originalCommand;")
            .CloseBlock("));")
            .Outdent()
            .CloseBlock();

    /// <summary>
    /// Emits the binding for the variant that has both a CommandParameter property and an observable
    /// parameter, which must track the latest parameter value and re-subscribe per command.
    /// </summary>
    /// <param name="sb">The writer, inside the worker's body.</param>
    /// <param name="inv">The BindCommand invocation info.</param>
    /// <param name="controlAccess">The access chain to the bound control.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    /// <remarks>
    /// The parameter is written before the command. Assigning the command is what makes a control ask whether
    /// it can execute, and it asks with whatever parameter it is holding at that moment, so setting the command
    /// first decides the control's enabled state from the parameter belonging to the command it just replaced.
    /// </remarks>
    private static void EmitObservableParameterBinding(
        SourceWriter sb,
        BindCommandInvocationInfo inv,
        string controlAccess,
        bool supportsNullable)
    {
        _ = sb.BlankLine();
        AppendCapturedOriginals(sb, controlAccess);

        var nullableSuffix = supportsNullable && inv.ParameterIsReferenceType ? "?" : string.Empty;
        var writeLatest = CommandBindingSyntax.WriteLatestParameter(inv, "p");

        _ = sb.Append(inv.ParameterTypeFullName).Append(nullableSuffix).Line(" __latestParam = default;")
            .Line($"var __paramSub = {GeneratedTypeNames.Subscribe}(")
            .Indent()
            .Append("withParameter, p => ").Append(writeLatest).Line(");")
            .Outdent()
            .BlankLine();
        _ = CommandBindingSyntax.OpenUntypedCommandSubscription(sb)
            .Var("param", CommandBindingSyntax.ReadLatestParameter(inv))
            .Append(controlAccess).Line(".CommandParameter = param;")
            .Append(controlAccess).Line(".Command = cmd;")
            .If("cmd != null")
            .Line($"serial.Disposable = {GeneratedTypeNames.Subscribe}(")
            .Indent()
            .Line("withParameter, p =>")
            .OpenBlock()
            .Append(writeLatest).EndStatement()
            .Append(controlAccess).Line(".CommandParameter = p;")
            .CloseBlock(");")
            .Outdent()
            .CloseBlock();
        _ = CommandBindingSyntax.CloseCommandSubscription(sb).BlankLine();

        AppendRestoringReturn(
            sb,
            controlAccess,
            $"new {GeneratedTypeNames.MultipleDisposable}("
            + $"new {GeneratedTypeNames.MultipleDisposable}(__cmdSub, __paramSub), serial)");
    }
}
