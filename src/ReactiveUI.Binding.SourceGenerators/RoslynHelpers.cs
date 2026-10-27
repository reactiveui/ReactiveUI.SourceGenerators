// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ReactiveUI.Binding.SourceGenerators;

/// <summary>
/// Fast syntax-only predicates for the incremental generator pipeline.
/// These run on every keystroke and must be extremely cheap (no semantic model).
/// </summary>
internal static class RoslynHelpers
{
    /// <summary>Detects class declarations that can participate in view registration.</summary>
    /// <param name="node">The syntax node to check.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>true if the node is a class with a base list; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsClassWithBaseList(SyntaxNode node, CancellationToken ct) =>
        node is ClassDeclarationSyntax { BaseList.Types.Count: > 0 };

    /// <summary>Pipeline B predicate: detects WhenChanged invocations.</summary>
    /// <param name="node">The syntax node to check.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>true if the node is a WhenChanged invocation; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsWhenChangedInvocation(SyntaxNode node, CancellationToken ct) =>
        node is InvocationExpressionSyntax invocation
        && invocation.Expression is MemberAccessExpressionSyntax
        {
            Name.Identifier.Text: Constants.WhenChangedMethodName
        };

    /// <summary>Pipeline B predicate: detects WhenChanging invocations.</summary>
    /// <param name="node">The syntax node to check.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>true if the node is a WhenChanging invocation; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsWhenChangingInvocation(SyntaxNode node, CancellationToken ct) =>
        node is InvocationExpressionSyntax invocation
        && invocation.Expression is MemberAccessExpressionSyntax
        {
            Name.Identifier.Text: Constants.WhenChangingMethodName
        };

    /// <summary>Pipeline B predicate: detects BindOneWay or BindTwoWay invocations.</summary>
    /// <param name="node">The syntax node to check.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>true if the node is a bind invocation; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsBindInvocation(SyntaxNode node, CancellationToken ct) =>
        node is InvocationExpressionSyntax invocation
        && invocation.Expression is MemberAccessExpressionSyntax
        {
            Name.Identifier.Text: Constants.BindOneWayMethodName or Constants.BindTwoWayMethodName
            or Constants.OneWayBindMethodName or Constants.BindMethodName
        };

    /// <summary>Checks if a syntax node is a Bind (view-first two-way) invocation specifically.</summary>
    /// <param name="node">The syntax node to check.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>true if the node is a Bind invocation; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsBindSpecificInvocation(SyntaxNode node, CancellationToken ct) =>
        IsBindInvocation(node, ct) && GetMemberAccessName(node) == Constants.BindMethodName;

    /// <summary>Checks if a syntax node is a BindOneWay invocation specifically.</summary>
    /// <param name="node">The syntax node to check.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>true if the node is a BindOneWay invocation; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsBindOneWaySpecificInvocation(SyntaxNode node, CancellationToken ct) =>
        IsBindInvocation(node, ct) && GetMemberAccessName(node) == Constants.BindOneWayMethodName;

    /// <summary>Checks if a syntax node is a BindTwoWay invocation specifically.</summary>
    /// <param name="node">The syntax node to check.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>true if the node is a BindTwoWay invocation; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsBindTwoWaySpecificInvocation(SyntaxNode node, CancellationToken ct) =>
        IsBindInvocation(node, ct) && GetMemberAccessName(node) == Constants.BindTwoWayMethodName;

    /// <summary>Checks if a syntax node is a OneWayBind invocation specifically.</summary>
    /// <param name="node">The syntax node to check.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>true if the node is a OneWayBind invocation; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsOneWayBindSpecificInvocation(SyntaxNode node, CancellationToken ct) =>
        IsBindInvocation(node, ct) && GetMemberAccessName(node) == Constants.OneWayBindMethodName;

    /// <summary>
    /// Extracts the method name from a member access invocation expression.
    /// Returns null if the node is not the expected shape.
    /// </summary>
    /// <param name="node">The syntax node (expected to be an InvocationExpressionSyntax).</param>
    /// <returns>The method name, or null.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string? GetMemberAccessName(SyntaxNode node) =>
        node is not InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax memberAccess }
            ? null
            : memberAccess.Name.Identifier.Text;

    /// <summary>Pipeline B predicate: detects WhenAnyValue invocations only.</summary>
    /// <param name="node">The syntax node to check.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>true if the node is a WhenAnyValue invocation; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsWhenAnyValueInvocation(SyntaxNode node, CancellationToken ct) =>
        node is InvocationExpressionSyntax invocation
        && invocation.Expression is MemberAccessExpressionSyntax
        {
            Name.Identifier.Text: Constants.WhenAnyValueMethodName
        };

    /// <summary>Pipeline B predicate: detects WhenAny invocations (with IObservedChange selector).</summary>
    /// <param name="node">The syntax node to check.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>true if the node is a WhenAny invocation; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsWhenAnyInvocation(SyntaxNode node, CancellationToken ct) =>
        node is InvocationExpressionSyntax invocation
        && invocation.Expression is MemberAccessExpressionSyntax
        {
            Name.Identifier.Text: Constants.WhenAnyMethodName
        };

    /// <summary>Pipeline B predicate: detects WhenAnyObservable invocations.</summary>
    /// <param name="node">The syntax node to check.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>true if the node is a WhenAnyObservable invocation; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsWhenAnyObservableInvocation(SyntaxNode node, CancellationToken ct) =>
        node is InvocationExpressionSyntax invocation
        && invocation.Expression is MemberAccessExpressionSyntax
        {
            Name.Identifier.Text: Constants.WhenAnyObservableMethodName
        };

    /// <summary>Pipeline B predicate: detects BindInteraction invocations.</summary>
    /// <param name="node">The syntax node to check.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>true if the node is a BindInteraction invocation; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsBindInteractionInvocation(SyntaxNode node, CancellationToken ct) =>
        node is InvocationExpressionSyntax invocation
        && invocation.Expression is MemberAccessExpressionSyntax
        {
            Name.Identifier.Text: Constants.BindInteractionMethodName
        };

    /// <summary>Pipeline B predicate: detects BindCommand invocations.</summary>
    /// <param name="node">The syntax node to check.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>true if the node is a BindCommand invocation; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsBindCommandInvocation(SyntaxNode node, CancellationToken ct) =>
        node is InvocationExpressionSyntax invocation
        && invocation.Expression is MemberAccessExpressionSyntax
        {
            Name.Identifier.Text: Constants.BindCommandMethodName
        };

    /// <summary>Pipeline B predicate: detects BindTo invocations (observable stream to target property).</summary>
    /// <param name="node">The syntax node to check.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>true if the node is a BindTo invocation; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsBindToInvocation(SyntaxNode node, CancellationToken ct) =>
        node is InvocationExpressionSyntax invocation
        && invocation.Expression is MemberAccessExpressionSyntax
        {
            Name.Identifier.Text: Constants.BindToMethodName
        };

    /// <summary>Pipeline B predicate: detects InvokeCommand invocations (observable stream to a command).</summary>
    /// <param name="node">The syntax node to check.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>true if the node is an InvokeCommand invocation; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsInvokeCommandInvocation(SyntaxNode node, CancellationToken ct) =>
        node is InvocationExpressionSyntax invocation
        && invocation.Expression is MemberAccessExpressionSyntax
        {
            Name.Identifier.Text: Constants.InvokeCommandMethodName
        };

    /// <summary>Pipeline B predicate: detects ToProperty invocations (observable backing a read-only property).</summary>
    /// <param name="node">The syntax node to check.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>true if the node is a ToProperty invocation; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsToPropertyInvocation(SyntaxNode node, CancellationToken ct) =>
        node is InvocationExpressionSyntax invocation
        && invocation.Expression is MemberAccessExpressionSyntax
        {
            Name.Identifier.Text: Constants.ToPropertyMethodName
        };
}
