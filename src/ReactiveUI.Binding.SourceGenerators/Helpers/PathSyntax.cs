// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ReactiveUI.Binding.SourceGenerators.Helpers;

/// <summary>
/// Walks the links of a property path as it is written in a selector lambda, from the last member back to the
/// lambda's parameter. The generator and the analyzers both read paths through this, so they agree on what a path is.
/// </summary>
/// <remarks>
/// A selector is a delegate, so it may use <c>?.</c> as well as <c>!</c> and parentheses. None of them changes
/// which member a link names: <c>x.Child?.Name</c>, <c>x.Child!.Name</c> and <c>(x.Child).Name</c> are one path.
/// </remarks>
internal static class PathSyntax
{
    /// <summary>Reads through the wrappers around a link to the link itself.</summary>
    /// <param name="expression">The expression to read through.</param>
    /// <returns>
    /// The first expression that is not a null-forgiving suppression, parenthesized, or a conditional access. A
    /// conditional access is read as the access after its <c>?</c>, which is where its last link is.
    /// </returns>
    internal static ExpressionSyntax Link(ExpressionSyntax expression)
    {
        while (true)
        {
            var kind = expression.Kind();
            if (kind == SyntaxKind.ParenthesizedExpression)
            {
                expression = ((ParenthesizedExpressionSyntax)expression).Expression;
            }
            else if (kind == SyntaxKind.SuppressNullableWarningExpression)
            {
                expression = ((PostfixUnaryExpressionSyntax)expression).Operand;
            }
            else if (kind == SyntaxKind.ConditionalAccessExpression)
            {
                expression = ((ConditionalAccessExpressionSyntax)expression).WhenNotNull;
            }
            else
            {
                return expression;
            }
        }
    }

    /// <summary>Determines whether an expression names a member, written after <c>.</c> or after <c>?.</c>.</summary>
    /// <param name="link">An expression <see cref="Link"/> returned.</param>
    /// <returns><see langword="true"/> for a member access and for a member binding.</returns>
    internal static bool IsMember(ExpressionSyntax link) =>
        link.IsKind(SyntaxKind.SimpleMemberAccessExpression) || link.IsKind(SyntaxKind.MemberBindingExpression);

    /// <summary>Gets the name a member link is written with.</summary>
    /// <param name="link">A link <see cref="IsMember"/> accepted.</param>
    /// <returns>The member's name.</returns>
    internal static SimpleNameSyntax Name(ExpressionSyntax link) =>
        link is MemberAccessExpressionSyntax access ? access.Name : ((MemberBindingExpressionSyntax)link).Name;

    /// <summary>Gets the expression a member link reads its member from, as it is written.</summary>
    /// <param name="link">A link <see cref="IsMember"/> accepted.</param>
    /// <returns>
    /// The expression before the <c>.</c>, or the expression before the <c>?</c> for a member written after
    /// <c>?.</c>. Pass it to <see cref="Link"/> to reach the link before this one.
    /// </returns>
    internal static ExpressionSyntax Receiver(ExpressionSyntax link) =>
        link is MemberAccessExpressionSyntax access ? access.Expression : ConditionalReceiver(link);

    /// <summary>Determines whether a member link is written after <c>?.</c>.</summary>
    /// <param name="link">A link <see cref="IsMember"/> accepted.</param>
    /// <returns><see langword="true"/> when the member is only read if what comes before it is not null.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsConditional(ExpressionSyntax link) =>
        link.IsKind(SyntaxKind.MemberBindingExpression);

    /// <summary>Finds the expression before the <c>?</c> that a member binding hangs from.</summary>
    /// <param name="binding">The member binding.</param>
    /// <returns>The expression tested for null.</returns>
    /// <remarks>
    /// In <c>a?.b?.c</c> the binding <c>.b</c> is the tested expression of the inner conditional access and the
    /// result of the outer one. The one it hangs from is the nearest that holds it on the result side.
    /// </remarks>
    private static ExpressionSyntax ConditionalReceiver(ExpressionSyntax binding)
    {
        // A member binding only parses inside a conditional access, so the walk always finds one.
        SyntaxNode child = binding;
        var parent = binding.Parent!;
        while (parent is not ConditionalAccessExpressionSyntax conditional || conditional.WhenNotNull != child)
        {
            child = parent;
            parent = parent.Parent!;
        }

        return ((ConditionalAccessExpressionSyntax)parent).Expression;
    }
}
