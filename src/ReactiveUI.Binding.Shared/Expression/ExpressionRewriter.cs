// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Expressions;
#else
namespace ReactiveUI.Binding.Expressions;
#endif

/// <summary>
/// Rewrites and validates expression trees used by binding infrastructure, normalizing
/// supported constructs into a consistent shape.
/// </summary>
/// <remarks>
/// <para>
/// This visitor intentionally supports a constrained set of expression node types. Unsupported shapes
/// are rejected with actionable exceptions to help callers correct their expressions.
/// </para>
/// <para>
/// Supported rewrites include:
/// </para>
/// <list type="bullet">
/// <item><description><see cref="ExpressionType.ArrayIndex"/> is rewritten into an indexer access.</description></item>
/// <item><description><see cref="ExpressionType.Call"/> to a special-name indexer method is rewritten into an <see cref="IndexExpression"/>.</description></item>
/// <item><description><see cref="ExpressionType.ArrayLength"/> is rewritten into member access to <c>Length</c>.</description></item>
/// <item><description><see cref="ExpressionType.Convert"/> is stripped.</description></item>
/// </list>
/// </remarks>
[RequiresUnreferencedCode(
    "Expression rewriting uses reflection over runtime types which may be removed by trimming.")]
internal sealed class ExpressionRewriter : ExpressionVisitor
{
    /// <inheritdoc/>
    public override Expression Visit(Expression? node)
    {
        ArgumentExceptionHelper.ThrowIfNull(node);

        return node!.NodeType switch
        {
            ExpressionType.ArrayIndex => VisitBinary((BinaryExpression)node),
            ExpressionType.ArrayLength or ExpressionType.Convert => VisitUnary((UnaryExpression)node),
            ExpressionType.Call => VisitMethodCall((MethodCallExpression)node),
            ExpressionType.Index => VisitIndex((IndexExpression)node),
            ExpressionType.MemberAccess => VisitMember((MemberExpression)node),
            ExpressionType.Parameter => VisitParameter((ParameterExpression)node),
            ExpressionType.Constant => VisitConstant((ConstantExpression)node),
            _ => throw CreateUnsupportedNodeException(node)
        };
    }

    /// <summary>Creates an exception for an unsupported expression node type, with an actionable error message.</summary>
    /// <param name="node">The unsupported expression node.</param>
    /// <returns>A <see cref="NotSupportedException"/> describing the unsupported node.</returns>
    internal static Exception CreateUnsupportedNodeException(Expression node)
    {
        const int MessageBuilderCapacity = 96;

        var sb = new StringBuilder(MessageBuilderCapacity);
        _ = sb.Append("Unsupported expression of type '")
            .Append(node.NodeType)
            .Append("' ")
            .Append(node)
            .Append('.');

        if (node is BinaryExpression be)
        {
            _ = sb.Append(" Did you meant to use expressions '")
                .Append(be.Left)
                .Append("' and '")
                .Append(be.Right)
                .Append("'?");
        }

        return new NotSupportedException(sb.ToString());
    }

    /// <summary>Gets the indexer property named "Item" from the specified type.</summary>
    /// <param name="type">The type to retrieve the indexer property from.</param>
    /// <returns>The <see cref="PropertyInfo"/> for the "Item" indexer property.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no indexer property named "Item" is found.</exception>
    internal static PropertyInfo GetItemProperty(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties
                                    | DynamicallyAccessedMemberTypes.NonPublicProperties)]
        Type type)
    {
        var property = type.GetRuntimeProperty("Item");
        return property ?? throw new InvalidOperationException("Could not find a valid indexer property named 'Item'.");
    }

    /// <summary>Gets the "Length" property from the specified type.</summary>
    /// <param name="type">The type to retrieve the Length property from.</param>
    /// <returns>The <see cref="PropertyInfo"/> for the "Length" property.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no "Length" property is found.</exception>
    internal static PropertyInfo GetLengthProperty(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties
                                    | DynamicallyAccessedMemberTypes.NonPublicProperties)]
        Type type)
    {
        var property = type.GetRuntimeProperty("Length");
        return property
               ?? throw new InvalidOperationException("Could not find valid information for the array length operator.");
    }

    /// <summary>Determines whether all expressions in the collection are <see cref="ConstantExpression"/> instances.</summary>
    /// <param name="expressions">The collection of expressions to check.</param>
    /// <returns><see langword="true"/> if every expression is a <see cref="ConstantExpression"/>; otherwise <see langword="false"/>.</returns>
    internal static bool AllConstant(ReadOnlyCollection<Expression> expressions)
    {
        for (var i = 0; i < expressions.Count; i++)
        {
            if (expressions[i] is not ConstantExpression)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>Visits each expression in an argument list and returns the visited results as an array.</summary>
    /// <param name="arguments">The argument expressions to visit.</param>
    /// <returns>An array of visited argument expressions.</returns>
    internal Expression[] VisitArgumentList(ReadOnlyCollection<Expression> arguments)
    {
        var count = arguments.Count;
        if (count == 0)
        {
            return [];
        }

        var visited = new Expression[count];
        for (var i = 0; i < count; i++)
        {
            visited[i] = Visit(arguments[i]);
        }

        return visited;
    }

    /// <inheritdoc/>
    protected override Expression VisitBinary(BinaryExpression node)
    {
        if (node.Right is not ConstantExpression)
        {
            throw new NotSupportedException("Array index expressions are only supported with constants.");
        }

        var instance = Visit(node.Left);
        var index = (ConstantExpression)Visit(node.Right);

        // ArrayIndex expressions are only produced by the C# compiler for actual arrays,
        // so instance.Type.IsArray is always true here.
        return Expression.ArrayAccess(instance, index);
    }

    /// <inheritdoc/>
    protected override Expression VisitUnary(UnaryExpression node)
    {
        // Visit() only routes Convert and ArrayLength here, so no fallthrough is needed.
        // UnaryExpression always has a non-null Operand for these node types.
        if (node.NodeType == ExpressionType.Convert)
        {
            return Visit(node.Operand);
        }

        // Must be ArrayLength
        var operand = Visit(node.Operand);
        var lengthProperty = GetLengthProperty(operand.Type);

        return Expression.MakeMemberAccess(operand, lengthProperty);
    }

    /// <inheritdoc/>
    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        if (!node.Method.IsSpecialName || !AllConstant(node.Arguments))
        {
            throw new NotSupportedException("Index expressions are only supported with constants.");
        }

        // Instance method calls routed here always have a non-null Object.
        var instance = Visit(node.Object!);
        var args = VisitArgumentList(node.Arguments);

        return Expression.MakeIndex(instance, GetItemProperty(instance.Type), args);
    }

    /// <inheritdoc/>
    protected override Expression VisitIndex(IndexExpression node)
    {
        if (!AllConstant(node.Arguments))
        {
            throw new NotSupportedException("Index expressions are only supported with constants.");
        }

        return base.VisitIndex(node);
    }
}
