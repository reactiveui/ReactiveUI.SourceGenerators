// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>One property of a bound path, as a generated binding describes it to <see cref="BindingHooks"/>.</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[DebuggerDisplay("BindingHookProperty")]
public sealed class BindingHookProperty
{
    /// <summary>Reads the property and describes the read as a change.</summary>
    private readonly ChangeReader _read;

    /// <summary>Reads the property and hands it to a visitor in its own types.</summary>
    private readonly Func<object, IBindingHookPropertyVisitor, bool, object?> _visit;

    /// <summary>Initializes a new instance of the <see cref="BindingHookProperty"/> class.</summary>
    /// <param name="read">Reads the property and describes the read as a change.</param>
    /// <param name="visit">Reads the property and hands it to a visitor in its own types.</param>
    private BindingHookProperty(ChangeReader read, Func<object, IBindingHookPropertyVisitor, bool, object?> visit)
    {
        _read = read;
        _visit = visit;
    }

    /// <summary>Reads a property from its owner, describing the read as a change.</summary>
    /// <param name="owner">The object that owns the property.</param>
    /// <param name="value">The value read, which owns the next property of the path.</param>
    /// <returns>The owner, the expression naming the property and the value read.</returns>
    private delegate ObservedChange<object, object> ChangeReader(object owner, out object? value);

    /// <summary>Describes a property through a typed reader and the expression that names it.</summary>
    /// <typeparam name="TOwner">The type that declares the property.</typeparam>
    /// <typeparam name="TValue">The property's type.</typeparam>
    /// <param name="read">Reads the property from the object that owns it.</param>
    /// <param name="expression">Builds the lambda that names the property.</param>
    /// <returns>The property.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="read"/> or <paramref name="expression"/> is <see langword="null"/>.</exception>
    /// <remarks>The expression is built on request, so a binding no hook inspects builds none.</remarks>
    public static BindingHookProperty Create<TOwner, TValue>(
        Func<TOwner, TValue> read,
        Func<Expression<Func<TOwner, TValue>>> expression)
    {
        ArgumentExceptionHelper.ThrowIfNull(read);
        ArgumentExceptionHelper.ThrowIfNull(expression);

        var accessors = new Accessors<TOwner, TValue>(read, expression);
        return new(accessors.Read, accessors.Visit);
    }

    /// <summary>Reads the property from its owner, describing the read as a change.</summary>
    /// <param name="owner">The object that owns the property.</param>
    /// <param name="value">The value read, which owns the next property of the path.</param>
    /// <returns>The owner, the expression naming the property and the value read.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ObservedChange<object, object> Read(object owner, out object? value) => _read(owner, out value);

    /// <summary>Reads the property from its owner and hands it to a visitor in its own types.</summary>
    /// <param name="owner">The object that owns the property.</param>
    /// <param name="visitor">The visitor that receives the property.</param>
    /// <param name="ownsNext">Whether the value read owns the next property of the path.</param>
    /// <returns>The value read when it owns the next property; otherwise <see langword="null"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal object? Visit(object owner, IBindingHookPropertyVisitor visitor, bool ownsNext) => _visit(owner, visitor, ownsNext);

    /// <summary>The typed accessors of one property, which the untyped property calls through delegates.</summary>
    /// <typeparam name="TOwner">The type that declares the property.</typeparam>
    /// <typeparam name="TValue">The property's type.</typeparam>
    /// <param name="read">Reads the property from the object that owns it.</param>
    /// <param name="expression">Builds the lambda that names the property.</param>
    private sealed class Accessors<TOwner, TValue>(Func<TOwner, TValue> read, Func<Expression<Func<TOwner, TValue>>> expression)
    {
        /// <summary>Reads the property, describing the read as a change.</summary>
        /// <param name="owner">The object that owns the property.</param>
        /// <param name="value">The value read.</param>
        /// <returns>The change.</returns>
        public ObservedChange<object, object> Read(object owner, out object? value)
        {
            // IPropertyBindingHook's contract carries each value as an object, and this runs only when such a hook
            // reads the path. A hook that implements ITypedPropertyBindingHook goes through Visit, which keeps TValue.
            value = read((TOwner)owner);
            return new(owner, expression().Body, value!);
        }

        /// <summary>Reads the property and hands it to a visitor in its own types.</summary>
        /// <param name="owner">The object that owns the property.</param>
        /// <param name="visitor">The visitor that receives the property.</param>
        /// <param name="ownsNext">Whether the value read owns the next property of the path.</param>
        /// <returns>The value read when it owns the next property; otherwise <see langword="null"/>.</returns>
        public object? Visit(object owner, IBindingHookPropertyVisitor visitor, bool ownsNext)
        {
            var typedOwner = (TOwner)owner;
            var value = read(typedOwner);
            visitor.Visit(typedOwner, expression(), value);
            return ownsNext ? value : null;
        }
    }
}
