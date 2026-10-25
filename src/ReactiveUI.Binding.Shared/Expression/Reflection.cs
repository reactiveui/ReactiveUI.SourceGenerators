// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Expressions;
#else
namespace ReactiveUI.Binding.Expressions;
#endif

/// <summary>Reads and writes members along a property expression chain by reflection.</summary>
public static class Reflection
{
    /// <summary>Reported when an expression yields no chain to walk.</summary>
    private const string EmptyExpressionChainMessage = "Expression chain must contain at least one element.";

    /// <summary>The shared rewriter instance used to simplify expressions before inspection.</summary>
    /// <remarks>
    /// Built on first use rather than in a static initializer. The rewriter reads runtime types by
    /// reflection and says so, and a static constructor has nowhere to carry that annotation.
    /// </remarks>
    private static ExpressionRewriter? _expressionRewriter;

    /// <summary>Simplifies an expression with the shared expression rewriter.</summary>
    /// <param name="expression">The expression to rewrite.</param>
    /// <returns>The rewritten expression, or <see langword="null"/> when <paramref name="expression"/> is <see langword="null"/>.</returns>
    [RequiresUnreferencedCode(
        "Expression rewriting uses reflection over runtime types which may be removed by trimming.")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Expression Rewrite(Expression? expression) =>
        (_expressionRewriter ??= new()).Visit(expression);

    /// <summary>Converts an expression that points to a property chain into a dotted path string, such as <c>A.B[0].C</c>.</summary>
    /// <param name="expression">The expression to generate the property names from; an indexer's arguments must be constants.</param>
    /// <returns>The member names joined by dots, with an indexer written as its name followed by its arguments in brackets.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="expression"/> is <see langword="null"/>.</exception>
    /// <exception cref="NotSupportedException">The chain contains a node that is neither a member access nor an index.</exception>
    public static string ExpressionToPropertyNames(Expression? expression)
    {
        ArgumentExceptionHelper.ThrowIfNull(expression);

        var sb = new StringBuilder();
        var firstSegment = true;

        foreach (var exp in expression!.GetExpressionChain())
        {
            if (exp.NodeType == ExpressionType.Parameter)
            {
                continue;
            }

            if (!firstSegment)
            {
                _ = sb.Append('.');
            }

            // Anything other than an indexer or member access contributes no name segment.
            if (exp is IndexExpression { Indexer: not null } indexExpression)
            {
                _ = sb.Append(indexExpression.Indexer.Name).Append('[');

                var args = indexExpression.Arguments;
                for (var i = 0; i < args.Count; i++)
                {
                    if (i != 0)
                    {
                        _ = sb.Append(',');
                    }

                    _ = sb.Append(((ConstantExpression)args[i]).Value);
                }

                _ = sb.Append(']');
            }
            else if (exp is MemberExpression memberExpression)
            {
                _ = sb.Append(memberExpression.Member.Name);
            }

            firstSegment = false;
        }

        return sb.ToString();
    }

    /// <summary>Converts a <see cref="MemberInfo"/> into a delegate which fetches the value for the member.</summary>
    /// <param name="member">The member info to convert.</param>
    /// <returns>
    /// A delegate that fetches the value, or null when the member is neither a field nor a property. The delegate for
    /// a field throws <see cref="InvalidOperationException"/> when the field holds null.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="member"/> is <see langword="null"/>.</exception>
    public static Func<object?, object?[]?, object?>? GetValueFetcherForProperty(MemberInfo? member)
    {
        ArgumentExceptionHelper.ThrowIfNull(member);

        if (member is FieldInfo field)
        {
            return (obj, _) =>
            {
                var value = field.GetValue(obj);
                return value ?? throw new InvalidOperationException();
            };
        }

        return member is not PropertyInfo property ? null : property.GetValue;
    }

    /// <summary>Converts a <see cref="MemberInfo"/> into a delegate which fetches the value for the member. Throws if the member is not a field or property.</summary>
    /// <param name="member">The member info to convert.</param>
    /// <returns>A delegate that fetches the value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="member"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="member"/> is neither a <see cref="FieldInfo"/> nor a <see cref="PropertyInfo"/>, so no fetcher can be built for it.</exception>
    public static Func<object?, object?[]?, object?> GetValueFetcherOrThrow(MemberInfo? member)
    {
        ArgumentExceptionHelper.ThrowIfNull(member);

        var ret = GetValueFetcherForProperty(member);
        return ret
               ?? throw new ArgumentException($"Type '{member!.DeclaringType}' must have a property '{member.Name}'");
    }

    /// <summary>Converts a <see cref="MemberInfo"/> into a delegate which sets the value for the member.</summary>
    /// <param name="member">The member info to convert.</param>
    /// <returns>A delegate that sets the value, or null when the member is neither a field nor a property.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="member"/> is <see langword="null"/>.</exception>
    public static Action<object?, object?, object?[]?>? GetValueSetterForProperty(MemberInfo? member)
    {
        ArgumentExceptionHelper.ThrowIfNull(member);

        if (member is FieldInfo field)
        {
            return (obj, val, _) => field.SetValue(obj, val);
        }

        return member is not PropertyInfo property ? null : property.SetValue;
    }

    /// <summary>Converts a <see cref="MemberInfo"/> into a delegate which sets the value for the member. Throws if the member is not a field or property.</summary>
    /// <param name="member">The member info to convert.</param>
    /// <returns>A delegate that sets the value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="member"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="member"/> is neither a <see cref="FieldInfo"/> nor a <see cref="PropertyInfo"/>, so no setter can be built for it.</exception>
    public static Action<object?, object?, object?[]?> GetValueSetterOrThrow(MemberInfo? member)
    {
        ArgumentExceptionHelper.ThrowIfNull(member);

        var ret = GetValueSetterForProperty(member);
        return ret
               ?? throw new ArgumentException($"Type '{member!.DeclaringType}' must have a property '{member.Name}'");
    }

    /// <summary>Attempts to get the value of the last property in an expression chain.</summary>
    /// <typeparam name="TValue">The expected type of the final value.</typeparam>
    /// <param name="changeValue">Receives the value if the chain can be evaluated; otherwise the default.</param>
    /// <param name="current">The object that starts the property chain.</param>
    /// <param name="expressionChain">A sequence of expressions that point to properties/fields.</param>
    /// <returns>True if the value was retrieved; false when <paramref name="current"/> or a property partway along the chain is null.</returns>
    /// <exception cref="InvalidOperationException"><paramref name="expressionChain"/> is empty, so there is no member to read a value from.</exception>
    [RequiresUnreferencedCode("Evaluates expression-based member chains via reflection; members may be trimmed.")]
    public static bool TryGetValueForPropertyChain<TValue>(
        out TValue changeValue,
        object? current,
        IEnumerable<Expression> expressionChain)
    {
        var expressions = MaterializeExpressions(expressionChain);
        var count = expressions.Length;

        if (count == 0)
        {
            throw new InvalidOperationException(EmptyExpressionChainMessage);
        }

        for (var i = 0; i < count - 1; i++)
        {
            if (current is null)
            {
                changeValue = default!;
                return false;
            }

            var expression = expressions[i];
            current = GetValueFetcherOrThrow(expression.GetMemberInfo())(current, expression.GetArgumentsArray());
        }

        if (current is null)
        {
            changeValue = default!;
            return false;
        }

        var lastExpression = expressions[count - 1];
        changeValue =
            (TValue)GetValueFetcherOrThrow(lastExpression.GetMemberInfo())(
                current,
                lastExpression.GetArgumentsArray())!;
        return true;
    }

    /// <summary>Attempts to get all intermediate values in a property chain as observed changes.</summary>
    /// <param name="changeValues">Receives an array with one entry per expression in the chain; the entries from the first null link onward are null.</param>
    /// <param name="current">The object that starts the property chain.</param>
    /// <param name="expressionChain">A sequence of expressions that point to properties/fields.</param>
    /// <returns>True if all values were retrieved; false when <paramref name="current"/> or a property partway along the chain is null.</returns>
    /// <exception cref="InvalidOperationException"><paramref name="expressionChain"/> is empty, so there is no member to read a value from.</exception>
    [RequiresUnreferencedCode("Evaluates expression-based member chains via reflection; members may be trimmed.")]
    public static bool TryGetAllValuesForPropertyChain(
        out IObservedChange<object, object?>[] changeValues,
        object? current,
        IEnumerable<Expression> expressionChain)
    {
        var expressions = MaterializeExpressions(expressionChain);
        var count = expressions.Length;

        changeValues = new IObservedChange<object, object?>[count];

        if (count == 0)
        {
            throw new InvalidOperationException(EmptyExpressionChainMessage);
        }

        var currentIndex = 0;

        for (; currentIndex < count - 1; currentIndex++)
        {
            if (current is null)
            {
                changeValues[currentIndex] = null!;
                return false;
            }

            var expression = expressions[currentIndex];
            var sender = current;
            current = GetValueFetcherOrThrow(expression.GetMemberInfo())(current, expression.GetArgumentsArray());
            changeValues[currentIndex] = new ObservedChange<object, object?>(sender, expression, current);
        }

        if (current is null)
        {
            changeValues[currentIndex] = null!;
            return false;
        }

        var lastExpression = expressions[count - 1];
        changeValues[currentIndex] = new ObservedChange<object, object?>(
            current,
            lastExpression,
            GetValueFetcherOrThrow(lastExpression.GetMemberInfo())(current, lastExpression.GetArgumentsArray()));

        return true;
    }

    /// <summary>Attempts to set the value of the last property in an expression chain, throwing when a chain member is not a field or property.</summary>
    /// <typeparam name="TValue">The type of the end value being set.</typeparam>
    /// <param name="target">The object that starts the property chain.</param>
    /// <param name="expressionChain">A sequence of expressions that point to properties/fields.</param>
    /// <param name="value">The value to set on the last property in the chain.</param>
    /// <returns>True if the value was set; false when the object owning the last property is null.</returns>
    /// <exception cref="InvalidOperationException"><paramref name="expressionChain"/> is empty, so there is no member to set a value on.</exception>
    /// <exception cref="ArgumentException">A member in the chain is neither a field nor a property.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="target"/> or a property before the last link's owner is <see langword="null"/>.</exception>
    [RequiresUnreferencedCode("Evaluates expression-based member chains via reflection; members may be trimmed.")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TrySetValueToPropertyChain<TValue>(
        object? target,
        IEnumerable<Expression> expressionChain,
        TValue value) =>
        TrySetValueToPropertyChain(target, expressionChain, value, true);

    /// <summary>Attempts to set the value of the last property in an expression chain.</summary>
    /// <typeparam name="TValue">The type of the end value being set.</typeparam>
    /// <param name="target">The object that starts the property chain.</param>
    /// <param name="expressionChain">A sequence of expressions that point to properties/fields.</param>
    /// <param name="value">The value to set on the last property in the chain.</param>
    /// <param name="shouldThrow">If true, throw when a chain member is not a field or property; otherwise, an unreadable link is skipped and an unwritable last member returns false.</param>
    /// <returns>True if the value was set; false when the object owning the last property is null or the last member cannot be written.</returns>
    /// <exception cref="InvalidOperationException"><paramref name="expressionChain"/> is empty, so there is no member to set a value on.</exception>
    /// <exception cref="ArgumentException"><paramref name="shouldThrow"/> is true and a member in the chain is neither a field nor a property.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="target"/> or a property before the last link's owner is <see langword="null"/>.</exception>
    [RequiresUnreferencedCode("Evaluates expression-based member chains via reflection; members may be trimmed.")]
    public static bool TrySetValueToPropertyChain<TValue>(
        object? target,
        IEnumerable<Expression> expressionChain,
        TValue value,
        bool shouldThrow)
    {
        var expressions = MaterializeExpressions(expressionChain);
        var count = expressions.Length;

        if (count == 0)
        {
            throw new InvalidOperationException(EmptyExpressionChainMessage);
        }

        for (var i = 0; i < count - 1; i++)
        {
            var expression = expressions[i];

            var getter = shouldThrow
                ? GetValueFetcherOrThrow(expression.GetMemberInfo())
                : GetValueFetcherForProperty(expression.GetMemberInfo());

            if (getter is null)
            {
                continue;
            }

            ArgumentExceptionHelper.ThrowIfNull(target);
            target = getter(target, expression.GetArgumentsArray());
        }

        if (target is null)
        {
            return false;
        }

        var lastExpression = expressions[count - 1];

        var setter = shouldThrow
            ? GetValueSetterOrThrow(lastExpression.GetMemberInfo())
            : GetValueSetterForProperty(lastExpression.GetMemberInfo());

        return TryInvokeSetter(setter, target, value, lastExpression.GetArgumentsArray());
    }

    /// <summary>Materializes an expression chain into an array for indexed access, reusing the backing array when possible.</summary>
    /// <param name="expressionChain">The expression chain to materialize.</param>
    /// <returns>An array of expressions representing the chain.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Expression[] MaterializeExpressions(IEnumerable<Expression> expressionChain)
    {
        ArgumentExceptionHelper.ThrowIfNull(expressionChain);

        switch (expressionChain)
        {
            case Expression[] arr:
                return arr;
            case ICollection<Expression> coll:
                {
                    if (coll.Count == 0)
                    {
                        return [];
                    }

                    var result = new Expression[coll.Count];
                    coll.CopyTo(result, 0);
                    return result;
                }

            default:
                return [.. expressionChain];
        }
    }

    /// <summary>
    /// Invokes the setter if it is not null. Returns false when the setter is null,
    /// which occurs when <see cref="GetValueSetterForProperty"/> receives a <see cref="MemberInfo"/>
    /// that is neither a <see cref="FieldInfo"/> nor a <see cref="PropertyInfo"/>.
    /// This is a defensive guard for synthetic expression chains; standard lambda-derived chains
    /// always produce PropertyInfo or FieldInfo members.
    /// </summary>
    /// <param name="setter">The setter delegate, or null.</param>
    /// <param name="target">The target object.</param>
    /// <param name="value">The value to set.</param>
    /// <param name="arguments">The indexer arguments, if any.</param>
    /// <returns>True if the setter was invoked; false if the setter was null.</returns>
    [ExcludeFromCodeCoverage]
    private static bool TryInvokeSetter(
        Action<object?, object?, object?[]?>? setter,
        object? target,
        object? value,
        object?[]? arguments)
    {
        if (setter is null)
        {
            return false;
        }

        setter(target, value, arguments);
        return true;
    }
}
