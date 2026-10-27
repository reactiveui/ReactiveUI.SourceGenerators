// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

/// <summary>Opens the choice that lets a registration observe one link of a chain in a mechanism's place.</summary>
/// <remarks>
/// Every mechanism offers its links on the same terms, so the arguments are written once here and each plugin
/// supplies only the observation that follows them. All of it is fixed at compile time - the declaring type,
/// the property name, the accessor, and a lambda the compiler turns into member tokens - so a link honours a
/// registration without resolving anything by name, and a consumer publishing ahead-of-time carries no
/// expression engine for it.
/// </remarks>
internal static class ChainRegistrationEmitter
{
    /// <summary>The opening a link inside a switch expression is written with.</summary>
    internal const string TernaryOpening = "? ";

    /// <summary>
    /// Writes the chooser and its arguments, leaving the writer one level deeper on a fresh line for the
    /// mechanism's observation, which <see cref="AppendChoiceClose"/> then closes.
    /// </summary>
    /// <param name="sb">The writer, at the level of the line the choice starts on.</param>
    /// <param name="sourceExpression">The expression naming the object the property is read from.</param>
    /// <param name="segment">The property path segment being observed.</param>
    /// <param name="generatedAffinity">The affinity of the mechanism this link was built from.</param>
    /// <param name="isBeforeChange">Whether before-change notifications are being observed.</param>
    /// <param name="opening">What the call is written after, which differs by the position it sits in.</param>
    /// <remarks>
    /// A link inside a switch expression is written after a <c>?</c>; the first link of a chain and a
    /// single-property observation are written after a <c>return</c>. The arguments are the same either way, so
    /// the position is handed in rather than the call being written twice.
    /// </remarks>
    internal static void AppendChoiceOpen(
        SourceWriter sb,
        string sourceExpression,
        PropertyPathSegment segment,
        int generatedAffinity,
        bool isBeforeChange,
        string opening = TernaryOpening)
    {
        var valueType = segment.PropertyTypeFullName;
        var registration = $"__registration_{sourceExpression}";

        _ = sb.Append(opening).Append($"({GeneratedTypeNames.ObservationAffinityChecker}.FindHigherAffinityPlugin(")
            .Append(sourceExpression).Append(".GetType(), \"").Append(segment.PropertyName).Append("\", ")
            .Append(generatedAffinity).Append(", ").AppendLiteral(isBeforeChange)
            .Append(") is global::ReactiveUI.Binding.ICreatesObservableForProperty ").Append(registration)
            .OpenContinuation()
            .Append($"? ({GeneratedTypeNames.IObservable}<").Append(valueType)
            .Append($">)new {GeneratedTypeNames.PluginPropertyObservable}<").Append(valueType).Append(">(")
            .OpenContinuation();
        _ = AppendPluginObservableArguments(sb, registration, sourceExpression, segment, valueType, isBeforeChange)
            .Line(", false)")
            .Outdent()
            .Append($": ({GeneratedTypeNames.IObservable}<").Append(valueType).Line(">)");
    }

    /// <summary>Closes the choice <see cref="AppendChoiceOpen"/> opened, once the mechanism's observation is written.</summary>
    /// <param name="sb">The writer, part way through the line the mechanism's observation ended on.</param>
    /// <returns>The writer, back at the level the choice started on, with the line left open.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static SourceWriter AppendChoiceClose(SourceWriter sb) => sb.Append(')').Outdent();

    /// <summary>
    /// Opens the stage that switches one link of a chain onto its parent's latest value, up to the test that the
    /// parent is present, leaving the writer two levels deeper for the branch taken while it is.
    /// </summary>
    /// <param name="sb">The writer, at the level of the statement the stage is declared in.</param>
    /// <param name="stage">The parent stage, this stage, and the lambda's parent parameter.</param>
    /// <param name="segment">The link the stage observes, whose declaring type the parent is typed as.</param>
    /// <param name="valueType">The type the stage's values are observed as.</param>
    /// <remarks>
    /// The parent is typed as the declaring type: <c>IObservable</c> is covariant, so a parent of a more derived
    /// type still converts.
    /// </remarks>
    internal static void AppendStageOpen(SourceWriter sb, in ChainStageVariables stage, PropertyPathSegment segment, string valueType) =>
        _ = sb.BeginVar(stage.CurrentObservable).Append("new ").Append(GeneratedTypeNames.SwitchMapSignal).Append('<')
            .Append(segment.DeclaringTypeFullName).Append(", ").Append(valueType).Append(">(").Append(stage.PreviousObservable).Append(',')
            .OpenContinuation()
            .Append(stage.ParentParameter).Append(" => ").Append(stage.ParentParameter).Append(" != null")
            .OpenContinuation();

    /// <summary>Closes a chain stage with the branch taken while the parent is null, ending the statement.</summary>
    /// <param name="sb">The writer, on a fresh line after the branch taken while the parent is present.</param>
    /// <param name="valueType">The type the stage's values are observed as.</param>
    /// <param name="nullParentBehavior">What the stage emits while its parent is null.</param>
    /// <remarks>
    /// An inner link emits its default so the stage below re-parents onto null and drops its subscription on the
    /// detached subtree; a leaf that suppresses leaves its consumer as it was until the path is whole again.
    /// </remarks>
    internal static void AppendStageClose(SourceWriter sb, string valueType, NullParentObservationBehavior nullParentBehavior)
    {
        _ = sb.Append($": ({GeneratedTypeNames.IObservable}<").Append(valueType).Append(">)");
        _ = nullParentBehavior == NullParentObservationBehavior.EmitDefault
            ? sb.Append($"new {GeneratedTypeNames.ImmediateReturnSignal}<").Append(valueType)
                .Append(">(default(").Append(valueType).Append("))")
            : sb.Append($"{GeneratedTypeNames.ImmutableEmptySignal}<").Append(valueType).Append(">.Instance");
        _ = sb.Line(");").Outdent().Outdent();
    }

    /// <summary>
    /// Writes the arguments that hand one link to a registration: the registration, the source, the member, its
    /// name, its accessor and the notification timing, one per line. The caller writes what follows the timing.
    /// </summary>
    /// <param name="sb">The writer, at the level the arguments are written at.</param>
    /// <param name="registration">The expression naming the registration that won the link.</param>
    /// <param name="sourceExpression">The expression naming the object the property is read from.</param>
    /// <param name="segment">The property path segment being observed.</param>
    /// <param name="valueType">The type the link's values are observed as.</param>
    /// <param name="isBeforeChange">Whether before-change notifications are being observed.</param>
    /// <returns>The same writer, for chaining.</returns>
    internal static SourceWriter AppendPluginObservableArguments(
        SourceWriter sb,
        string registration,
        string sourceExpression,
        PropertyPathSegment segment,
        string valueType,
        bool isBeforeChange)
    {
        var declaringType = segment.DeclaringTypeFullName;
        return sb.Append(registration).Line(",")
            .Append(sourceExpression).Line(",")
            .Append($"(({GeneratedTypeNames.Expression}<{GeneratedTypeNames.Func}<").Append(declaringType).Append(", ")
            .Append(valueType).Append(">>)(__e => __e.").Append(segment.PropertyName).Line(")).Body,")
            .AppendQuoted(segment.PropertyName).Line(",")
            .Append("(object __o) => ((").Append(declaringType).Append(")__o).").Append(segment.PropertyName).Line(",")
            .AppendLiteral(isBeforeChange);
    }
}
