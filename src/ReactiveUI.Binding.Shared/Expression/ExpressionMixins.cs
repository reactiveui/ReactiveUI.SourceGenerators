// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using System.Text;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Expressions;
#else
namespace ReactiveUI.Binding.Expressions;
#endif

/// <summary>Extension methods that decompose property-access expression trees.</summary>
public static class ExpressionMixins
{
    /// <summary>Provides GetExpressionChain extension members for <paramref name="expression"/>.</summary>
    /// <param name="expression">The expression.</param>
    extension(Expression expression)
    {
        /// <summary>Gets the member accesses and indexer accesses that make up an expression, ordered from the root parameter outward.</summary>
        /// <returns>
        /// The links in order, each rebased onto a fresh parameter of its parent's type; empty when the expression is a
        /// parameter or <see langword="null"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">The expression contains a node that is neither a member access, an index nor the root parameter.</exception>
        public IEnumerable<Expression> GetExpressionChain()
        {
            var expressions = new List<Expression>();
            var node = expression;

            while (node is not null && node.NodeType != ExpressionType.Parameter)
            {
                if (node is IndexExpression indexExpression)
                {
                    expressions.Add(RebaseOnParameter(indexExpression));
                    node = indexExpression.Object;
                }
                else if (node is MemberExpression memberExpression)
                {
                    expressions.Add(RebaseOnParameter(memberExpression));
                    node = memberExpression.Expression;
                }
                else
                {
                    throw CreateUnsupportedExpressionException(node);
                }
            }

            expressions.Reverse();
            return expressions;
        }

        /// <summary>Gets the member an index or member-access expression names, looking through conversions.</summary>
        /// <returns>The property, field or indexer the expression names.</returns>
        /// <exception cref="ArgumentNullException">The expression is <see langword="null"/>.</exception>
        /// <exception cref="NotSupportedException">The expression is not an index, member access, or conversion node, so it names no member.</exception>
        public MemberInfo? GetMemberInfo()
        {
            while (true)
            {
                ArgumentExceptionHelper.ThrowIfNull(expression);

                MemberInfo? info;
                switch (expression.NodeType)
                {
                    case ExpressionType.Index when expression is IndexExpression indexExpression:
                        {
                            info = indexExpression.Indexer;
                            break;
                        }

                    case ExpressionType.MemberAccess when expression is MemberExpression memberExpression:
                        {
                            info = memberExpression.Member;
                            break;
                        }

                    case ExpressionType.Convert or ExpressionType.ConvertChecked when expression is UnaryExpression unaryExpression:
                        {
                            expression = unaryExpression.Operand;
                            continue;
                        }

                    default:
                        throw new NotSupportedException($"Unsupported {nameof(expression)} type: '{expression.NodeType}'");
                }

                return info;
            }
        }

        /// <summary>Gets the expression an index or member-access expression is read from.</summary>
        /// <returns>The object expression, or null for a static member.</returns>
        /// <exception cref="ArgumentNullException">The expression is <see langword="null"/>.</exception>
        /// <exception cref="NotSupportedException">The expression is not an index or member access node, so nothing precedes it in a chain.</exception>
        public Expression? GetParent()
        {
            ArgumentExceptionHelper.ThrowIfNull(expression);

            return expression.NodeType switch
            {
                ExpressionType.Index when expression is IndexExpression indexExpression => indexExpression.Object,
                ExpressionType.MemberAccess when expression is MemberExpression memberExpression => memberExpression
                    .Expression,
                _ => throw new NotSupportedException($"Unsupported expression type: '{expression.NodeType}'")
            };
        }

        /// <summary>Gets the constant arguments passed to an indexer expression.</summary>
        /// <returns>The argument values, or null when the expression is not an index expression.</returns>
        /// <exception cref="ArgumentNullException">The expression is <see langword="null"/>.</exception>
        /// <exception cref="InvalidCastException">An indexer argument is not a constant expression.</exception>
        public object?[]? GetArgumentsArray()
        {
            ArgumentExceptionHelper.ThrowIfNull(expression);

            if (expression.NodeType != ExpressionType.Index)
            {
                return null;
            }

            var arguments = ((IndexExpression)expression).Arguments;
            var values = new object?[arguments.Count];
            for (var i = 0; i < arguments.Count; i++)
            {
                values[i] = ((ConstantExpression)arguments[i]).Value;
            }

            return values;
        }
    }

    /// <summary>Rewrites an indexer access so it hangs off a parameter of the parent's type.</summary>
    /// <param name="indexExpression">The indexer access to rebase.</param>
    /// <returns>The rebased expression, or the original when it already sits on a parameter.</returns>
    private static IndexExpression RebaseOnParameter(IndexExpression indexExpression)
    {
        var parent = indexExpression.GetParent();
        return indexExpression.Object is not null && parent is not null
               && indexExpression.Object.NodeType != ExpressionType.Parameter
            ? indexExpression.Update(Expression.Parameter(parent.Type), indexExpression.Arguments)
            : indexExpression;
    }

    /// <summary>Rewrites a member access so it hangs off a parameter of the parent's type.</summary>
    /// <param name="memberExpression">The member access to rebase.</param>
    /// <returns>The rebased expression, or the original when it already sits on a parameter.</returns>
    private static MemberExpression RebaseOnParameter(MemberExpression memberExpression)
    {
        var parent = memberExpression.GetParent();
        return parent is not null && memberExpression.Expression is not null
               && memberExpression.Expression.NodeType != ExpressionType.Parameter
            ? memberExpression.Update(Expression.Parameter(parent.Type))
            : memberExpression;
    }

    /// <summary>Builds the exception thrown for an expression node the chain walker cannot decompose.</summary>
    /// <param name="node">The unsupported node.</param>
    /// <returns>The exception to throw.</returns>
    private static NotSupportedException CreateUnsupportedExpressionException(Expression node)
    {
        var errorMessageBuilder = new StringBuilder($"Unsupported expression of type '{node.NodeType}'.");

        if (node is ConstantExpression)
        {
            _ = errorMessageBuilder.Append(" Did you miss the member access prefix in the expression?");
        }

        return new(errorMessageBuilder.ToString());
    }
}
